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
├── Endpoints/            mirrors src/backend/.../Endpoints/
│   └── ProductEndpointTests.cs
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
public class MockProductServiceTests
{
    private readonly MockProductService _service = new();

    [Fact]
    public void GetAll_ReturnsAllProducts()
    {
        var products = _service.GetAll().ToList();
        Assert.NotEmpty(products);
    }
}
```

- Instantiate the system under test in the constructor or as a field initializer
- Use the existing mock service implementations when available
- For isolating dependencies, create simple test doubles or use a mocking library

## Integration Tests (Endpoint Tests)

Endpoint tests use `WebApplicationFactory<Program>` for HTTP-level integration testing:

```csharp
public class ProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/products/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

## Endpoint Test Assertions

When testing minimal API endpoints via HTTP, assert on status codes and deserialized response bodies:

```csharp
// Success with typed value
response.EnsureSuccessStatusCode();
var product = await response.Content.ReadFromJsonAsync<Product>();
Assert.NotNull(product);

// Not found
Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

// Created
Assert.Equal(HttpStatusCode.Created, response.StatusCode);

// No content
Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

// Bad request
Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
