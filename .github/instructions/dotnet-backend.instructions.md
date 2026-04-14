---
description: "Use when writing or modifying C# source code in the .NET backend. Covers controller, service, and model conventions, DI registration, and API design patterns."
applyTo: "src/backend/**/*.cs"
---

# .NET Backend Conventions

## Architecture

The backend follows a **Controller → Service → Model** layered architecture with constructor-based dependency injection.

```
Controllers/    API endpoints — thin, delegate to services
Services/       Business logic — interface + implementation pairs
Models/         Data shapes — plain C# classes
```

## Controller Pattern

Every controller must:
- Inherit `ControllerBase`
- Be decorated with `[ApiController]` and `[Route("api/[controller]")]`
- Use **primary constructor** injection for dependencies
- Return `ActionResult<T>` for typed responses or `IActionResult` for status-only responses
- Validate inputs and return appropriate HTTP status codes (`Ok`, `NotFound`, `BadRequest`, `CreatedAtAction`, `NoContent`)

Reference: `src/backend/MockEcommerce.Api/Controllers/ProductsController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        return Ok(productService.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = productService.GetById(id);
        if (product is null)
            return NotFound();
        return Ok(product);
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
2. `app.UseAuthorization()` — authorization middleware
3. `app.MapControllers()` — endpoint routing

When adding middleware, maintain this order and add new middleware at the appropriate point.

## Namespace Convention

Follow folder-based namespaces: `MockEcommerce.Api.{Folder}` (e.g., `MockEcommerce.Api.Controllers`, `MockEcommerce.Api.Services`, `MockEcommerce.Api.Models`).

## General C# Style

- Use **file-scoped namespaces** (`namespace X;` not `namespace X { }`)
- Use **primary constructors** for dependency injection
- Use **nullable reference types** — handle `null` explicitly with `is null` / `is not null`
- Use **collection expressions** where appropriate (e.g., `List<T> items = [];`)
- Prefer `var` for local variables when the type is obvious from the right-hand side
