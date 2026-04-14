---
description: "Use when writing or modifying C# test code for the .NET backend. Covers xUnit conventions, test naming, AAA pattern, and integration testing with WebApplicationFactory."
applyTo: "test/backend/**/*.cs"
---

# .NET Testing Conventions

## Framework & Dependencies

- **xUnit** for test framework (`[Fact]`, `[Theory]`)
- **Microsoft.AspNetCore.Mvc.Testing** for integration tests (`WebApplicationFactory<T>`)
- **coverlet.collector** for code coverage

## Project Structure

Test folder structure mirrors the source project:

```
test/backend/MockEcommerce.Api.Tests/
├── Controllers/          mirrors src/backend/.../Controllers/
│   └── ProductsControllerTests.cs
├── Services/             mirrors src/backend/.../Services/
│   └── MockProductServiceTests.cs
```

When adding a new source file, create an equivalent test file in the mirrored location.

## Test File Naming

- File: `{ClassName}Tests.cs`
- Namespace: `MockEcommerce.Api.Tests.{Folder}` (mirrors the source namespace)

## Test Method Naming

Use the pattern: `MethodName_Condition_ExpectedResult`

```csharp
[Fact]
public void GetAll_ReturnsOkWithProducts() { ... }

[Fact]
public void GetById_WithValidId_ReturnsOkWithProduct() { ... }

[Fact]
public void GetById_WithInvalidId_ReturnsNotFound() { ... }
```

## AAA Pattern (Arrange-Act-Assert)

Every test follows Arrange-Act-Assert with clear separation:

```csharp
[Fact]
public void GetById_WithValidId_ReturnsOkWithProduct()
{
    // Arrange — set up via constructor or inline

    // Act
    var result = _controller.GetById(1);

    // Assert
    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var product = Assert.IsType<Product>(ok.Value);
    Assert.Equal(1, product.Id);
}
```

## Unit Tests

Unit tests use **direct instantiation** with real or simple dependencies:

```csharp
public class ProductsControllerTests
{
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var service = new MockProductService();
        _controller = new ProductsController(service);
    }
}
```

- Instantiate the system under test in the constructor
- Use the existing mock service implementations when available
- For isolating dependencies, create simple test doubles or use a mocking library

## Integration Tests

Use `WebApplicationFactory<Program>` for HTTP-level integration tests:

```csharp
public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
    }
}
```

## Controller Test Assertions

When testing controllers, assert on both the `ActionResult` wrapper and the inner value:

```csharp
// Success with typed value
var ok = Assert.IsType<OkObjectResult>(result.Result);
var product = Assert.IsType<Product>(ok.Value);

// Not found
Assert.IsType<NotFoundResult>(result.Result);

// Created
Assert.IsType<CreatedAtActionResult>(result.Result);

// No content
Assert.IsType<NoContentResult>(result);
```

## Data-Driven Tests

Use `[Theory]` with `[InlineData]` for parameterized tests:

```csharp
[Theory]
[InlineData(1)]
[InlineData(2)]
[InlineData(3)]
public void GetById_WithValidId_ReturnsProduct(int id)
{
    var result = _controller.GetById(id);
    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var product = Assert.IsType<Product>(ok.Value);
    Assert.Equal(id, product.Id);
}
```

## What to Test

For every new class, cover at minimum:
- **Happy path** — expected inputs produce expected outputs
- **Edge cases** — boundary values, empty collections, zero/negative numbers
- **Error paths** — invalid inputs, missing data, null references
- **Validation** — required fields, constraints, business rules

## Running Tests

```bash
dotnet test test/backend/MockEcommerce.Api.Tests/
```
