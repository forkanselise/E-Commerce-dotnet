using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NexusBakery.Api.Controllers;
using NexusBakery.Application.Services;
using NexusBakery.Domain.Entities;
using Xunit;

namespace NexusBakery.UnitTests.Controllers;

public class MobilePhonesControllerTests
{
    private readonly Mock<IMobilePhoneService> _mockService;
    private readonly MobilePhonesController _controller;

    public MobilePhonesControllerTests()
    {
        _mockService = new Mock<IMobilePhoneService>();
        _controller = new MobilePhonesController(_mockService.Object);
    }

    [Fact]
    public async Task Get_ShouldReturnOkResult_WithPagedData()
    {
        // Arrange
        var phones = new List<MobilePhone>
        {
            new MobilePhone { Id = "1", Title = "Phone 1", Brand = "Brand 1" },
            new MobilePhone { Id = "2", Title = "Phone 2", Brand = "Brand 2" }
        };

        _mockService.Setup(s => s.GetPagedPhonesAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((phones, 2));

        // Act
        var result = await _controller.Get(1, 10, null, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        
        var expectedValue = new {
            Items = phones,
            TotalCount = 2L,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        okResult.Value.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public async Task Get_WithFilters_ShouldPassFiltersToService()
    {
        // Arrange
        var phones = new List<MobilePhone>();

        _mockService.Setup(s => s.GetPagedPhonesAsync(2, 5, "iphone", "apple", It.IsAny<CancellationToken>()))
            .ReturnsAsync((phones, 0));

        // Act
        var result = await _controller.Get(2, 5, "iphone", "apple", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var expectedValue = new {
            Items = phones,
            TotalCount = 0L,
            Page = 2,
            PageSize = 5,
            TotalPages = 0
        };

        okResult.Value.Should().BeEquivalentTo(expectedValue);
        _mockService.Verify(s => s.GetPagedPhonesAsync(2, 5, "iphone", "apple", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenPhoneExists_ShouldReturnOkResult_WithPhone()
    {
        // Arrange
        var phoneId = "123";
        var phone = new MobilePhone { Id = phoneId, Title = "Test Phone" };

        _mockService.Setup(s => s.GetPhoneByIdAsync(phoneId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phone);

        // Act
        var result = await _controller.GetById(phoneId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPhone = okResult.Value.Should().BeOfType<MobilePhone>().Subject;
        returnedPhone.Should().BeEquivalentTo(phone);
    }

    [Fact]
    public async Task GetById_WhenPhoneDoesNotExist_ShouldReturnNotFoundResult()
    {
        // Arrange
        var phoneId = "nonexistent";

        _mockService.Setup(s => s.GetPhoneByIdAsync(phoneId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MobilePhone?)null);

        // Act
        var result = await _controller.GetById(phoneId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtActionResult_WithCreatedPhone()
    {
        // Arrange
        var newPhone = new MobilePhone { Title = "New Phone", Brand = "Brand X" };
        var createdPhone = new MobilePhone { Id = "new-id", Title = "New Phone", Brand = "Brand X" };

        _mockService.Setup(s => s.CreatePhoneAsync(newPhone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPhone);

        // Act
        var result = await _controller.Create(newPhone, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(MobilePhonesController.GetById));
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(createdPhone.Id);
        
        var returnedPhone = createdResult.Value.Should().BeOfType<MobilePhone>().Subject;
        returnedPhone.Should().BeEquivalentTo(createdPhone);
    }
}
