using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using NexusBakery.Api.Controllers;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.Interfaces;
using NexusBakery.Application.Services;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;
using Xunit;

namespace NexusBakery.UnitTests;

public class ProductTests
{
    [Fact]
    public void BaseEntity_NewInstance_HasValidObjectId()
    {
        var product = new Product { Title = "Croissant" };
        product.Id.Should().NotBeNullOrEmpty();
        ObjectId.TryParse(product.Id, out _).Should().BeTrue("BaseEntity.Id should always be initialized with a valid ObjectId");
    }

    [Fact]
    public void Product_ToBsonDocument_ShouldSerializeWithoutError()
    {
        var product = new Product
        {
            Title = "Test Product",
            Slug = "test-product",
            Sku = "SKU-12345",
            Category = ProductCategory.Bakery,
            Price = 100m,
            WarehouseStock = 10,
            RelatedProductIds = new List<string> { ObjectId.GenerateNewId().ToString() },
            CreatedBy = ObjectId.GenerateNewId().ToString(),
            UpdatedBy = ObjectId.GenerateNewId().ToString()
        };

        var doc = product.ToBsonDocument();
        doc.Should().NotBeNull();
    }

    [Fact]
    public void Product_SystemTextJson_ShouldSerializeWithoutError()
    {
        var product = new Product
        {
            Title = "Test Product",
            Slug = "test-product",
            Sku = "SKU-12345",
            Category = ProductCategory.Bakery,
            Price = 100m,
            WarehouseStock = 10
        };

        var json = JsonSerializer.Serialize(product);
        json.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("Ingredients", ProductCategory.Ingredients)]
    [InlineData("Bakery & Coffee", ProductCategory.Bakery)]
    [InlineData("Tools", ProductCategory.BakingTools)]
    [InlineData("Electronics", ProductCategory.Electronics)]
    [InlineData("Packaging", ProductCategory.Packaging)]
    [InlineData("Decorations", ProductCategory.Decorations)]
    [InlineData("Moulds", ProductCategory.Moulds)]
    [InlineData(null, ProductCategory.Bakery)]
    public void ParseProductCategory_ShouldParseCorrectly(string? categoryInput, ProductCategory expected)
    {
        var parsed = ProductService.ParseProductCategory(categoryInput);
        parsed.Should().Be(expected);
    }

    [Fact]
    public async Task ProductService_CreateAsync_ShouldGenerateValidProduct_AndInventory()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockWarehouseRepo = new Mock<IWarehouseRepository>();

        mockProductRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), default)).ReturnsAsync((Product?)null);
        mockProductRepo.Setup(r => r.GetBySkuAsync(It.IsAny<string>(), default)).ReturnsAsync((Product?)null);
        mockProductRepo.Setup(r => r.CreateAsync(It.IsAny<Product>(), default))
            .ReturnsAsync((Product p, System.Threading.CancellationToken ct) => p);

        mockWarehouseRepo.Setup(r => r.CreateAsync(It.IsAny<WarehouseInventory>(), default))
            .ReturnsAsync((WarehouseInventory w, System.Threading.CancellationToken ct) => w);

        var service = new ProductService(mockProductRepo.Object, mockWarehouseRepo.Object);
        var dto = new CreateProductDto
        {
            Title = "Belgian Chocolate 500g",
            Category = "Ingredients",
            Price = 1250m,
            WarehouseStock = 40,
            ImageUrl = "https://images.unsplash.com/photo-choco.jpg"
        };

        var created = await service.CreateAsync(dto);

        created.Should().NotBeNull();
        created.Title.Should().Be("Belgian Chocolate 500g");
        created.Slug.Should().Be("belgian-chocolate-500g");
        created.Category.Should().Be(ProductCategory.Ingredients);
        created.WarehouseStock.Should().Be(40);
        created.Images.Should().HaveCount(1);
        created.Images[0].Url.Should().Be("https://images.unsplash.com/photo-choco.jpg");

        mockWarehouseRepo.Verify(w => w.CreateAsync(It.Is<WarehouseInventory>(inv => 
            inv.ProductId == created.Id && 
            inv.Sku == created.Sku && 
            inv.TotalStock == 40), default), Times.Once);
    }

    [Fact]
    public async Task ProductsController_CreateProduct_NullDto_ReturnsBadRequest()
    {
        var mockService = new Mock<IProductService>();
        var mockFileStorage = new Mock<IFileStorageService>();
        var mockLogger = new Mock<ILogger<ProductsController>>();

        var controller = new ProductsController(mockService.Object, mockFileStorage.Object, mockLogger.Object);
        var result = await controller.CreateProduct(null!);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ProductsController_CreateProduct_EmptyTitle_ReturnsBadRequest()
    {
        var mockService = new Mock<IProductService>();
        var mockFileStorage = new Mock<IFileStorageService>();
        var mockLogger = new Mock<ILogger<ProductsController>>();

        var controller = new ProductsController(mockService.Object, mockFileStorage.Object, mockLogger.Object);
        var result = await controller.CreateProduct(new CreateProductDto { Title = "   " });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ProductsController_CreateProduct_ValidDto_ReturnsCreated201()
    {
        var mockService = new Mock<IProductService>();
        var mockFileStorage = new Mock<IFileStorageService>();
        var mockLogger = new Mock<ILogger<ProductsController>>();

        var expectedProduct = new Product
        {
            Title = "Vanilla Bean Paste",
            Slug = "vanilla-bean-paste",
            Sku = "SKU-VAN-01",
            Price = 950m
        };

        mockService.Setup(s => s.CreateAsync(It.IsAny<CreateProductDto>()))
            .ReturnsAsync(expectedProduct);

        var controller = new ProductsController(mockService.Object, mockFileStorage.Object, mockLogger.Object);
        var result = await controller.CreateProduct(new CreateProductDto
        {
            Title = "Vanilla Bean Paste",
            Category = "Ingredients",
            Price = 950m
        });

        var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().Be(expectedProduct);
    }
}
