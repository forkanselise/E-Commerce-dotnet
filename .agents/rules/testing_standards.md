---
name: Nexus Bakery Testing Standards
description: Guidelines for writing unit, integration, and E2E tests for the Nexus Bakery & Tech platform.
trigger: always_on
---

# Nexus Bakery & Tech - Testing Standards

## 1. Backend Unit Tests
- **Framework:** Use `xUnit` as the primary test runner.
- **Assertions:** Use `FluentAssertions` for highly readable assertions (e.g., `result.Should().BeTrue();`).
- **Mocking:** Use `Moq` for mocking interfaces (e.g., `IMongoCollection<T>`, `IHubContext<T>`).
- **Coverage Goal:** Focus heavily on Application layer Domain Services (`AuthService`, `OrderService`, etc.).
- **Naming Convention:** Use `MethodName_StateUnderTest_ExpectedBehavior` (e.g., `CreateOrder_InsufficientStock_ThrowsBadRequest`).

## 2. Backend Integration Tests
- **Framework:** Use `WebApplicationFactory<Program>` to spin up the API in-memory.
- **Database:** Use `Mongo2Go` or a dedicated test MongoDB container to hit a real database instead of mocking.

## 3. Frontend Tests
- **Component Tests:** Use `Vitest` and `@testing-library/react`.
- **E2E Tests:** Use `Playwright`. Ensure tests cover critical user flows:
  - Add to cart -> Checkout -> Payment
  - Admin login -> Adjust Warehouse Stock -> Verify
  - Router Agent Delegation -> Agent Tools
