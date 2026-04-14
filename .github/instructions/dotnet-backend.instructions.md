---
description: "Use when writing or modifying C# source code in the .NET backend. Covers controller, service, and model conventions, DI registration, and API design patterns."
applyTo: "src/backend/**/*.cs"
---

# .NET Backend Conventions

## Architecture

The backend follows a **Minimal API Endpoint → Service → Model** layered architecture with dependency injection via handler parameters.

```
Endpoints/      API route mappings — thin static classes, delegate to services
Services/       Business logic — interface + implementation pairs
Models/         Data shapes — plain C# classes
```

## Endpoint Pattern

Endpoints are organized as static classes in the `Endpoints/` folder. Each class:
- Contains a `Map*Endpoints(this WebApplication app)` extension method
- Groups related routes using `MapGroup()`
- Uses `TypedResults` for strongly-typed responses
- Uses `Results<T1, T2>` union types when multiple response types are possible
- Receives services via handler parameter injection
- Includes `WithName()` for operationIds and `WithSummary()` for OpenAPI docs

Reference: `src/backend/MockEcommerce.Api/Endpoints/ProductEndpoints.cs`

```csharp
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/products")
            .WithTags("Products");

        group.MapGet("/", GetAll)
            .WithName("GetAllProducts");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetProductById");
    }

    internal static Ok<IEnumerable<Product>> GetAll(IProductService productService)
    {
        return TypedResults.Ok(productService.GetAll());
    }

    internal static Results<Ok<Product>, NotFound> GetById(int id, IProductService productService)
    {
        var product = productService.GetById(id);
        if (product is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(product);
    }
}
```

## Service Pattern

Services are defined as an **interface + implementation** pair:
- Interface in `Services/I{Name}Service.cs` — defines the contract
- Implementation in `Services/{Name}Service.cs` — contains logic
- Register in `Program.cs` via `builder.Services.AddSingleton<IService, Implementation>()`

Reference: `src/backend/MockEcommerce.Api/Services/IProductService.cs`, `MockProductService.cs`

```csharp
public interface IProductService
{
    IEnumerable<Product> GetAll();
    Product? GetById(int id);
}
```

When adding a new service:
1. Create the interface in `Services/`
2. Create the implementation in `Services/`
3. Register in `Program.cs` with the appropriate lifetime (`Singleton`, `Scoped`, or `Transient`)

## Model Pattern

Models are plain C# classes in the `Models/` folder:
- Use auto-properties with `{ get; set; }`
- Initialize string properties to `string.Empty` to satisfy nullable reference types
- Computed properties use expression-bodied members (e.g., `public decimal TotalPrice => UnitPrice * Quantity;`)
- Request/response records can be defined as `record` types in the controller file when simple (e.g., `public record AddToCartRequest(int ProductId, int Quantity);`)

Reference: `src/backend/MockEcommerce.Api/Models/Product.cs`, `CartItem.cs`

## Middleware Pipeline (`Program.cs`)

The minimal hosting pipeline in order:
1. `app.UseCors()` — CORS middleware (origin: `http://localhost:5173`)
2. `app.MapOpenApi()` — OpenAPI document endpoint
3. `app.Map*Endpoints()` — minimal API endpoint mappings

When adding middleware, maintain this order and add new middleware at the appropriate point.

## Namespace Convention

Follow folder-based namespaces: `MockEcommerce.Api.{Folder}` (e.g., `MockEcommerce.Api.Controllers`, `MockEcommerce.Api.Services`, `MockEcommerce.Api.Models`).

## General C# Style

- Use **file-scoped namespaces** (`namespace X;` not `namespace X { }`)
- Use **primary constructors** for dependency injection
- Use **nullable reference types** — handle `null` explicitly with `is null` / `is not null`
- Use **collection expressions** where appropriate (e.g., `List<T> items = [];`)
- Prefer `var` for local variables when the type is obvious from the right-hand side
