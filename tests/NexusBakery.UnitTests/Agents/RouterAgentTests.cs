using FluentAssertions;
using Moq;
using NexusBakery.Agents.Agents;
using NexusBakery.Agents.Core;
using NexusBakery.Agents.Tools;
using NexusBakery.Domain.Interfaces;
using Xunit;

namespace NexusBakery.UnitTests.Agents;

public class RouterAgentTests
{
    private readonly RouterConciergeAgent _routerAgent;

    public RouterAgentTests()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockWarehouseRepo = new Mock<IWarehouseRepository>();
        var mockTutorialRepo = new Mock<ITutorialRepository>();

        var searchProdTool = new SearchProductsTool(mockProductRepo.Object);
        var checkStockTool = new CheckStockTool(mockProductRepo.Object, mockWarehouseRepo.Object);
        var addToCartTool = new AddToCartTool(mockProductRepo.Object);
        var searchTutorialsTool = new SearchTutorialsTool(mockTutorialRepo.Object);
        var adjustInvTool = new AdjustInventoryTool(mockWarehouseRepo.Object, mockProductRepo.Object);

        var storefrontAgent = new StorefrontInventoryAgent(searchProdTool, checkStockTool, addToCartTool);
        var bakingAgent = new BakingMasterclassAgent(searchTutorialsTool);
        var warehouseAgent = new WarehouseOpsAgent(adjustInvTool, checkStockTool);

        _routerAgent = new RouterConciergeAgent(storefrontAgent, bakingAgent, warehouseAgent);
    }

    [Theory]
    [InlineData("Write me a python script to parse json")]
    [InlineData("Who is the president of France?")]
    [InlineData("What is the weather in Dhaka?")]
    [InlineData("Calculate 2+2 in Javascript")]
    public async Task ExecuteAsync_OutOfContextQuery_RefusesWithStandardMessage(string outOfContextMsg)
    {
        // Arrange
        var context = new AgentContext();

        // Act
        var result = await _routerAgent.ExecuteAsync(context, outOfContextMsg);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("I can only answer questions related to Nexus Bakery & Tech products");
        result.Message.Should().Contain("I can't assist with queries outside my domain");
    }

    [Fact]
    public async Task ExecuteAsync_Greeting_RespondsDirectly()
    {
        // Arrange
        var context = new AgentContext { UserName = "Alice" };

        // Act
        var result = await _routerAgent.ExecuteAsync(context, "Hello");

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("Welcome to Nexus Bakery & Tech");
        result.RespondingAgent.Should().Be("RouterConcierge");
    }
}
