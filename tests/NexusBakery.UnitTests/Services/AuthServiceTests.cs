using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NexusBakery.Application.DTOs.Auth;
using NexusBakery.Application.Services;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;
using Xunit;

namespace NexusBakery.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockConfig = new Mock<IConfiguration>();

        _mockConfig.Setup(c => c["JWT_SECRET"]).Returns("SuperSecretTestKeyForUnitTestingOnly123456789!");
        _mockConfig.Setup(c => c["JWT_ISSUER"]).Returns("TestIssuer");
        _mockConfig.Setup(c => c["JWT_AUDIENCE"]).Returns("TestAudience");
        _mockConfig.Setup(c => c["JWT_EXPIRES_IN_MINUTES"]).Returns("60");

        _authService = new AuthService(_mockUserRepo.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Baker Bob",
            Email = "bob@bakery.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepo.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) =>
            {
                u.Id = "669f1a2b3c4d5e6f78901234";
                return u;
            });

        // Act
        var result = await _authService.RegisterAsync(request, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("bob@bakery.com");
        result.User.FullName.Should().Be("Baker Bob");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Baker Bob",
            Email = "existing@bakery.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync("existing@bakery.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "existing@bakery.com" });

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(request, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Id = "669f1a2b3c4d5e6f78901234",
            Email = "bob@bakery.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync("bob@bakery.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var loginReq = new LoginRequest { Email = "bob@bakery.com", Password = "WrongPassword" };

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(loginReq, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
