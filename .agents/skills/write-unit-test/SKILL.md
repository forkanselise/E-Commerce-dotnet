---
name: write-unit-test
description: Workflow for scaffolding and writing an xUnit test for a backend C# service.
---

# Writing Unit Tests

When instructed to write a unit test for a service:

1. **Scaffold the Test Class:**
   - Location: `tests/NexusBakery.UnitTests/Services/`
   - Name the class `[ServiceName]Tests.cs`.
2. **Setup Dependencies:**
   - Create private readonly fields for the Service under test and the Mocks of its dependencies (e.g., `Mock<IProductRepository> _mockProductRepo;`).
   - Initialize them in the class constructor.
3. **Structure the Test (AAA Pattern):**
   - **Arrange:** Set up the mock returns (e.g., `_mockProductRepo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(product);`).
   - **Act:** Call the method on the service.
   - **Assert:** Use `FluentAssertions` to verify the result (e.g., `result.Should().NotBeNull();`).
   - **Verify:** Use `_mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Once);` to ensure critical methods were invoked.
