using FluentAssertions;
using Moq;
using NexusBakery.Application.Services;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace NexusBakery.UnitTests.Services;

public class MobilePhoneServiceTests
{
    private readonly Mock<IMobilePhoneRepository> _mockRepository;
    private readonly MobilePhoneService _service;

    public MobilePhoneServiceTests()
    {
        _mockRepository = new Mock<IMobilePhoneRepository>();
        _service = new MobilePhoneService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllPhonesAsync_ShouldReturnAllPhones()
    {
        // Arrange
        var phones = new List<MobilePhone>
        {
            new MobilePhone { Id = "1", Title = "Phone 1", Brand = "Brand 1" },
            new MobilePhone { Id = "2", Title = "Phone 2", Brand = "Brand 2" }
        };

        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(phones);

        // Act
        var result = await _service.GetAllPhonesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(phones);
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedPhonesAsync_WithNoSearchOrBrand_ShouldReturnPagedResult()
    {
        // Arrange
        var phones = new List<MobilePhone>
        {
            new MobilePhone { Id = "1", Title = "Phone 1", Brand = "Brand 1" },
            new MobilePhone { Id = "2", Title = "Phone 2", Brand = "Brand 2" }
        };

        _mockRepository.Setup(r => r.GetPagedAsync(null, 1, 10, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((phones, 2));

        // Act
        var result = await _service.GetPagedPhonesAsync(1, 10, null, null);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        _mockRepository.Verify(r => r.GetPagedAsync(null, 1, 10, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedPhonesAsync_WithSearchOnly_ShouldApplySearchPredicate()
    {
        // Arrange
        var phones = new List<MobilePhone>
        {
            new MobilePhone { Id = "1", Title = "Phone 1", Brand = "Brand 1", Model = "Model 1" }
        };

        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<Expression<Func<MobilePhone, bool>>>(), 1, 10, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((phones, 1));

        // Act
        var result = await _service.GetPagedPhonesAsync(1, 10, "Phone", null);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        _mockRepository.Verify(r => r.GetPagedAsync(It.IsAny<Expression<Func<MobilePhone, bool>>>(), 1, 10, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedPhonesAsync_WithBrandOnly_ShouldApplyBrandPredicate()
    {
        // Arrange
        var phones = new List<MobilePhone>
        {
            new MobilePhone { Id = "1", Title = "Phone 1", Brand = "Brand 1" }
        };

        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<Expression<Func<MobilePhone, bool>>>(), 1, 10, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((phones, 1));

        // Act
        var result = await _service.GetPagedPhonesAsync(1, 10, null, "Brand 1");

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        _mockRepository.Verify(r => r.GetPagedAsync(It.IsAny<Expression<Func<MobilePhone, bool>>>(), 1, 10, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedPhonesAsync_WithSearchAndBrand_ShouldApplyBothPredicates()
    {
        // Arrange
        var phones = new List<MobilePhone>
        {
            new MobilePhone { Id = "1", Title = "Phone 1", Brand = "Brand 1", Model = "Model 1" }
        };

        _mockRepository.Setup(r => r.GetPagedAsync(It.IsAny<Expression<Func<MobilePhone, bool>>>(), 1, 10, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((phones, 1));

        // Act
        var result = await _service.GetPagedPhonesAsync(1, 10, "Phone", "Brand 1");

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        _mockRepository.Verify(r => r.GetPagedAsync(It.IsAny<Expression<Func<MobilePhone, bool>>>(), 1, 10, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPhoneByIdAsync_WhenPhoneExists_ShouldReturnPhone()
    {
        // Arrange
        var phoneId = "123";
        var phone = new MobilePhone { Id = phoneId, Title = "Test Phone" };

        _mockRepository.Setup(r => r.GetByIdAsync(phoneId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phone);

        // Act
        var result = await _service.GetPhoneByIdAsync(phoneId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(phone);
        _mockRepository.Verify(r => r.GetByIdAsync(phoneId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPhoneByIdAsync_WhenPhoneDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var phoneId = "nonexistent";

        _mockRepository.Setup(r => r.GetByIdAsync(phoneId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MobilePhone?)null);

        // Act
        var result = await _service.GetPhoneByIdAsync(phoneId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(phoneId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePhoneAsync_ShouldReturnCreatedPhone()
    {
        // Arrange
        var newPhone = new MobilePhone { Title = "New Phone", Brand = "Brand X" };
        var createdPhone = new MobilePhone { Id = "new-id", Title = "New Phone", Brand = "Brand X" };

        _mockRepository.Setup(r => r.CreateAsync(newPhone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPhone);

        // Act
        var result = await _service.CreatePhoneAsync(newPhone);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createdPhone);
        _mockRepository.Verify(r => r.CreateAsync(newPhone, It.IsAny<CancellationToken>()), Times.Once);
    }
}
