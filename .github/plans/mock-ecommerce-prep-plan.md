# Mock E-Commerce Site — Exercise Preparation Plan

This plan describes how to prepare the `turbinestudios/mock-e-commerce-site` repository for use as the base codebase in the Agent Wars workshop exercise. The goal is to create a codebase where the cart backend and frontend are partially stubbed — enough structure exists to be discoverable, but teams must implement the missing pieces.

## Feature Request (revealed at Round 2)

> "Users should be able to view their cart, see what they're paying, and manage their selections before checkout. The cart should be accessible from the existing cart icon in the header."

---

## Changes by Layer

### Backend — Gut Cart Implementation, Keep the Contract

#### `src/backend/MockEcommerce.Api/Endpoints/CartEndpoints.cs`

- **Keep** the `MapCartEndpoints` method with all route registrations (`GET /`, `POST /`, `DELETE /{productId}`, `DELETE /`). The URL structure should be discoverable.
- **Keep** the `AddToCartRequest` record at the bottom of the file.
- **Replace** the method bodies of `GetCart`, `AddToCart`, `RemoveFromCart`, and `ClearCart` with `throw new NotImplementedException();`.

```csharp
internal static Ok<IEnumerable<CartItem>> GetCart(ICartService cartService)
{
    throw new NotImplementedException();
}

internal static Results<Created<CartItem>, Ok<CartItem>, NotFound<string>, ValidationProblem> AddToCart(
    AddToCartRequest request,
    IProductService productService,
    ICartService cartService)
{
    throw new NotImplementedException();
}

internal static Results<NoContent, NotFound> RemoveFromCart(int productId, ICartService cartService)
{
    throw new NotImplementedException();
}

internal static NoContent ClearCart(ICartService cartService)
{
    throw new NotImplementedException();
}
```

#### `src/backend/MockEcommerce.Api/Services/InMemoryCartService.cs`

- **Keep** the class declaration, the `ICartService` implementation, and the private fields (`_cart`, `_lock`).
- **Replace** all method bodies with `throw new NotImplementedException();`.

```csharp
public IEnumerable<CartItem> GetAll()
{
    throw new NotImplementedException();
}

public CartItem? GetByProductId(int productId)
{
    throw new NotImplementedException();
}

public CartItem Add(CartItem item)
{
    throw new NotImplementedException();
}

public bool Remove(int productId)
{
    throw new NotImplementedException();
}

public void Clear()
{
    throw new NotImplementedException();
}
```

#### `src/backend/MockEcommerce.Api/Services/ICartService.cs` — NO CHANGES

Keep the full interface with all XML doc comments. This is the contract teams should discover and implement against.

#### `src/backend/MockEcommerce.Api/Models/CartItem.cs` — NO CHANGES

Keep the model with `ProductId`, `ProductName`, `UnitPrice`, `Quantity`, and computed `TotalPrice`. Teams need this data shape.

#### `src/backend/MockEcommerce.Api/Program.cs` — NO CHANGES

Keep `AddSingleton<ICartService, InMemoryCartService>()` and `MapCartEndpoints()`. The DI registration and route mapping remain — the endpoints just return 501 until implemented.

---

### Frontend — Strip Cart API Functions, Keep Add-to-Cart Flow

#### `src/frontend/src/api/index.ts`

- **Remove** the `fetchCart`, `removeFromCart`, and `clearCart` functions.
- **Keep** `addToCart` — it is wired into the existing UI and demonstrates the working add-to-cart flow.
- **Keep** `fetchProducts` and `fetchProductById`.

After changes, the file should contain:

```typescript
import type { Product, CartItem, AddToCartRequest } from '../types';

const BASE_URL = '/api';

export async function fetchProducts(): Promise<Product[]> {
  const response = await fetch(`${BASE_URL}/products`);
  if (!response.ok) throw new Error('Failed to fetch products');
  return response.json();
}

export async function fetchProductById(id: number): Promise<Product> {
  const response = await fetch(`${BASE_URL}/products/${id}`);
  if (!response.ok) throw new Error(`Failed to fetch product ${id}`);
  return response.json();
}

export async function addToCart(request: AddToCartRequest): Promise<CartItem> {
  const response = await fetch(`${BASE_URL}/cart`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to add item to cart');
  return response.json();
}
```

#### `src/frontend/src/types/index.ts`

- **Remove** the `CartItem` interface.
- **Keep** `Product` and `AddToCartRequest`.

After changes:

```typescript
export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  stock: number;
  imageUrl: string;
}

export interface AddToCartRequest {
  productId: number;
  quantity: number;
}
```

#### `src/frontend/src/App.tsx` — NO CHANGES

Keep as-is. The `handleAddToCart` function works and shows teams the existing pattern. The cart counter increments but there is no cart view — this is exactly the gap teams will fill.

#### `src/frontend/src/components/Header/Header.tsx` — NO CHANGES

Keep as-is. The cart button renders with a count badge but has no `onClick` handler. This is the entry point teams will connect to their new cart view.

---

### Tests — Remove Cart-Specific Tests

#### Remove entirely:

- `test/backend/MockEcommerce.Api.Tests/Services/InMemoryCartServiceTests.cs`
- `test/backend/MockEcommerce.Api.Tests/Endpoints/CartEndpointTests.cs`

#### Keep:

- All product endpoint and service tests
- `test/frontend/App.test.tsx` — the add-to-cart test still works since `addToCart` is kept. **However**, the test imports `CartItem` from types — update the mock return type inline or remove the `CartItem` import if it produces a compile error after removing the type.
- All other frontend tests (ProductCard, ProductList, Header, useProducts)

---

## Verification Checklist

After applying these changes, verify:

1. **Backend compiles and starts** — `dotnet build` succeeds, `dotnet run` starts the server. Cart endpoints return HTTP 501 (NotImplementedException).
2. **Frontend builds** — `npm run build` in the frontend directory succeeds with no type errors.
3. **Frontend runs** — products load, "Add to cart" button works (increments counter, shows notification), cart icon is visible but clicking it does nothing.
4. **Product tests pass** — `dotnet test` runs product-related tests. Cart tests are gone so no failures from stubs.
5. **Frontend tests pass** — `npm test` in the frontend/test directory. Verify the App.test.tsx add-to-cart test still passes. If there's a type error from the removed `CartItem` interface, inline the mock type in the test.

## What Teams Must Do

With this prepared codebase, teams will need to:

1. **Round 1 (The Foundation):** Explore and document this codebase. They should discover the existing product listing, the stubbed cart endpoints, the `ICartService` interface, and the `CartItem` model. Good instruction files will note what's implemented vs. stubbed.

2. **Round 2 (The Blueprint):** Read the feature request and write a spec + plan. They must make decisions about: Does the cart open as a drawer, modal, or separate page? Can users change quantities from the cart? What happens when the cart is empty? Should there be a "clear cart" action? What about a checkout button?

3. **Round 3 (The Sprint):** Implement both backend (fill in the `NotImplementedException` stubs) and frontend (new cart view component, wire up the cart icon, call the API). This is a meaningful amount of work even with an AI agent — spanning both .NET and React.

4. **Round 4 (Sabotage):** The cart math (`UnitPrice × Quantity`, total calculation) is ripe for subtle bugs — off-by-one, wrong operator, flipped comparison, forgotten edge case.
