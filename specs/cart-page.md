# Feature Spec: Cart Page

## Overview

Users should be able to view their cart, see what they're paying, and manage their selections before checkout. The cart is accessible from the existing cart icon in the header.

## Current State

### What exists

| Layer | Asset | Status |
|-------|-------|--------|
| Backend model | `CartItem` (ProductId, ProductName, UnitPrice, Quantity, computed TotalPrice) | **Ready** |
| Backend interface | `ICartService` (GetAll, Add, GetByProductId, Remove, Clear) | **Ready** |
| Backend service | `InMemoryCartService` | **Stubbed** — all methods throw `NotImplementedException` |
| Backend endpoints | `CartEndpoints` (GET `/api/cart`, POST `/api/cart`, DELETE `/api/cart/{productId}`, DELETE `/api/cart`) | **Stubbed** — all handlers throw `NotImplementedException` |
| DI registration | `Program.cs` registers `ICartService` as Singleton | **Ready** |
| Frontend API | `addToCart()` in `src/api/index.ts` | **Ready** |
| Frontend types | `CartItem` interface in `src/api/index.ts` (local, not exported) | Needs to move to `src/types/index.ts` |
| Frontend Header | Cart icon button with `cartItemCount` badge | **Ready** — but does not navigate anywhere |
| Frontend routing | None | **Missing** |

### What is missing

- Backend: implementation of `InMemoryCartService` methods
- Backend: implementation of `CartEndpoints` handler methods
- Frontend: cart API functions (`fetchCart`, `removeFromCart`, `clearCart`)
- Frontend: `CartItem` type exported from `src/types/index.ts`
- Frontend: client-side routing (e.g. React Router)
- Frontend: `CartPage` component
- Frontend: `CartItemRow` component
- Frontend: `useCart` hook
- Frontend: header cart icon wired to navigate to `/cart`
- Tests: backend unit + integration tests for cart service and endpoints
- Tests: frontend unit tests for new components and hook

---

## Backend Work

### 1. Implement `InMemoryCartService`

File: [src/backend/MockEcommerce.Api/Services/InMemoryCartService.cs](../src/backend/MockEcommerce.Api/Services/InMemoryCartService.cs)

Implement every method using the existing `_cart` list and `_lock`:

| Method | Behaviour |
|--------|-----------|
| `GetAll()` | Return a snapshot copy of all cart items. |
| `GetByProductId(int)` | Return the matching `CartItem` or `null`. |
| `Add(CartItem)` | If the product already exists in the cart, increment its `Quantity`; otherwise add a new item. Return the added/updated item. |
| `Remove(int)` | Remove the item with the given `ProductId`. Return `true` if found and removed. |
| `Clear()` | Remove all items. |

All reads and writes must be guarded by `_lock` for thread safety.

### 2. Implement `CartEndpoints` handlers

File: [src/backend/MockEcommerce.Api/Endpoints/CartEndpoints.cs](../src/backend/MockEcommerce.Api/Endpoints/CartEndpoints.cs)

| Endpoint | Handler logic | Success | Failure |
|----------|--------------|---------|---------|
| `GET /api/cart` | Return `cartService.GetAll()` | `200 OK` with `IEnumerable<CartItem>` | — |
| `POST /api/cart` | Validate `Quantity > 0`. Look up product via `productService.GetById(request.ProductId)`. Build a `CartItem` from the product snapshot. Call `cartService.Add(item)`. | `201 Created` if new item, `200 OK` if quantity updated | `404 Not Found` if product doesn't exist; `ValidationProblem` if quantity ≤ 0 |
| `DELETE /api/cart/{productId}` | Call `cartService.Remove(productId)` | `204 No Content` | `404 Not Found` if item not in cart |
| `DELETE /api/cart` | Call `cartService.Clear()` | `204 No Content` | — |

### 3. Add `PATCH /api/cart/{productId}` endpoint (new)

To support changing quantity from the cart page without removing/re-adding, add:

| Endpoint | Handler logic | Success | Failure |
|----------|--------------|---------|---------|
| `PATCH /api/cart/{productId}` | Accept `UpdateQuantityRequest(int Quantity)`. Validate `Quantity > 0`. Find cart item by productId. Update its quantity. | `200 OK` with updated `CartItem` | `404 Not Found` if not in cart; `ValidationProblem` if quantity ≤ 0 |

Record: `public record UpdateQuantityRequest(int Quantity);`

Add to `ICartService`:

```csharp
/// <summary>Updates the quantity of an existing cart item.</summary>
/// <returns>The updated item, or <c>null</c> if not found.</returns>
CartItem? UpdateQuantity(int productId, int quantity);
```

### 4. Backend tests

#### Unit tests — `InMemoryCartService`

File: `test/backend/MockEcommerce.Api.Tests/Services/InMemoryCartServiceTests.cs`

| Test case |
|-----------|
| `GetAll_ReturnsEmpty_WhenCartIsEmpty` |
| `Add_NewItem_ReturnsItemWithCorrectFields` |
| `Add_ExistingItem_IncrementsQuantity` |
| `GetByProductId_ReturnsNull_WhenNotFound` |
| `GetByProductId_ReturnsItem_WhenExists` |
| `Remove_ReturnsTrue_WhenItemExists` |
| `Remove_ReturnsFalse_WhenItemNotFound` |
| `Clear_RemovesAllItems` |
| `UpdateQuantity_ReturnsUpdatedItem` |
| `UpdateQuantity_ReturnsNull_WhenNotFound` |

#### Integration tests — `CartEndpoints`

File: `test/backend/MockEcommerce.Api.Tests/Endpoints/CartEndpointTests.cs`

Use `WebApplicationFactory<Program>` (same pattern as existing `ProductEndpointTests`).

| Test case |
|-----------|
| `GetCart_ReturnsEmptyList_Initially` |
| `AddToCart_ReturnsCreated_ForNewItem` |
| `AddToCart_ReturnsOk_WhenIncrementingExistingItem` |
| `AddToCart_ReturnsNotFound_ForInvalidProduct` |
| `AddToCart_ReturnsValidationProblem_WhenQuantityIsZero` |
| `RemoveFromCart_ReturnsNoContent_WhenItemExists` |
| `RemoveFromCart_ReturnsNotFound_WhenItemMissing` |
| `ClearCart_ReturnsNoContent` |
| `UpdateQuantity_ReturnsOk_WithUpdatedItem` |
| `UpdateQuantity_ReturnsNotFound_WhenItemMissing` |

---

## Frontend Work

### 5. Add `CartItem` type

File: [src/frontend/src/types/index.ts](../src/frontend/src/types/index.ts)

```typescript
export interface CartItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}
```

Remove the local `CartItem` interface from `src/api/index.ts` and import from `types/`.

### 6. Add cart API functions

File: [src/frontend/src/api/index.ts](../src/frontend/src/api/index.ts)

| Function | Method | Path | Returns |
|----------|--------|------|---------|
| `fetchCart()` | GET | `/api/cart` | `CartItem[]` |
| `removeFromCart(productId: number)` | DELETE | `/api/cart/{productId}` | `void` |
| `clearCart()` | DELETE | `/api/cart` | `void` |
| `updateCartItemQuantity(productId: number, quantity: number)` | PATCH | `/api/cart/{productId}` | `CartItem` |

### 7. Install React Router

```sh
npm install react-router
```

### 8. Add routing to `App.tsx`

File: [src/frontend/src/App.tsx](../src/frontend/src/App.tsx)

- Wrap the app in `<BrowserRouter>`.
- Define two routes: `/` (home/product listing) and `/cart` (cart page).
- Move existing product listing markup into a `HomePage` component or keep inline with a route.
- Cart state (`cartItemCount`) should be lifted or fetched so it's available on both pages (the Header needs it everywhere).

### 9. Wire the Header cart icon

File: [src/frontend/src/components/Header/Header.tsx](../src/frontend/src/components/Header/Header.tsx)

- Replace the `<button>` with a React Router `<Link to="/cart">` (keep the same classes and aria-label).
- The cart badge continues to show `cartItemCount`.

### 10. Create `useCart` hook

File: `src/frontend/src/hooks/useCart.ts`

Manages all cart state and exposes:

```typescript
interface UseCartResult {
  items: CartItem[];
  loading: boolean;
  error: string | null;
  totalItems: number;       // sum of all quantities
  totalPrice: number;       // sum of all item totalPrice values
  refresh: () => void;      // re-fetch cart
  remove: (productId: number) => Promise<void>;
  clear: () => Promise<void>;
  updateQuantity: (productId: number, quantity: number) => Promise<void>;
}
```

On mount, call `fetchCart()`. Expose loading/error state the same way `useProducts` does.

### 11. Create `CartPage` component

File: `src/frontend/src/components/CartPage/CartPage.tsx`  
Barrel: `src/frontend/src/components/CartPage/index.ts`

**Layout:**

```
┌────────────────────────────────────────────────────────┐
│  ← Back to shopping                  (link to "/")     │
│                                                        │
│  Your Cart (3 items)                                   │
│                                                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Wireless Headphones     $79.99  [-] 2 [+]  $159… │  │
│  │ Running Shoes           $59.99  [-] 1 [+]  $59.… │  │
│  │ Yoga Mat                $34.99  [-] 1 [+]  $34.… │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│                              Subtotal: $253.97         │
│                              [Clear cart]              │
│                                                        │
│  Empty state: "Your cart is empty."                    │
│               [Continue shopping] (link to "/")        │
└────────────────────────────────────────────────────────┘
```

**Behaviour:**

| Action | Effect |
|--------|--------|
| Click **`+`** on an item | Call `updateQuantity(productId, quantity + 1)`. Refresh totals. |
| Click **`-`** on an item | If quantity > 1: `updateQuantity(productId, quantity - 1)`. If quantity = 1: `remove(productId)`. |
| Click **Remove** on an item | Call `remove(productId)`. Remove the row. |
| Click **Clear cart** | Call `clear()`. Show empty state. |
| Click **Back to shopping** | Navigate to `/`. |

**States:**

| State | Display |
|-------|---------|
| Loading | Skeleton or "Loading cart…" text |
| Error | "Failed to load cart. Try again." with a retry button |
| Empty | "Your cart is empty." with a "Continue shopping" link |
| Populated | Item list + subtotal |

### 12. Create `CartItemRow` component

File: `src/frontend/src/components/CartItemRow/CartItemRow.tsx`  
Barrel: `src/frontend/src/components/CartItemRow/index.ts`

Props:

```typescript
interface CartItemRowProps {
  item: CartItem;
  onIncrement: (productId: number) => void;
  onDecrement: (productId: number) => void;
  onRemove: (productId: number) => void;
}
```

Renders one row: product name, unit price, quantity controls (`-`/`+`), line total, remove button.

### 13. Frontend tests

#### `useCart` hook

File: `test/frontend/hooks/useCart.test.ts`

| Test case |
|-----------|
| fetches cart items on mount |
| exposes totalItems and totalPrice |
| remove() calls API and refreshes |
| clear() calls API and empties items |
| updateQuantity() calls API and refreshes |
| sets error state on fetch failure |

#### `CartPage` component

File: `test/frontend/components/CartPage/CartPage.test.tsx`

| Test case |
|-----------|
| shows loading state initially |
| renders cart items after load |
| shows empty state when cart is empty |
| increments quantity on + click |
| decrements quantity on − click |
| removes item when quantity is 1 and − is clicked |
| clears cart on "Clear cart" click |
| navigates to home on "Back to shopping" click |
| shows error state when fetch fails |

#### `CartItemRow` component

File: `test/frontend/components/CartItemRow/CartItemRow.test.tsx`

| Test case |
|-----------|
| renders product name, price, and quantity |
| calls onIncrement when + is clicked |
| calls onDecrement when − is clicked |
| calls onRemove when remove is clicked |

#### Header (updated)

File: `test/frontend/components/Header/Header.test.tsx` (update existing)

| Test case |
|-----------|
| cart icon links to /cart |

---

## Implementation Order

```
Phase 1 — Backend (no frontend dependency)
  1.1  Implement InMemoryCartService methods
  1.2  Implement CartEndpoints handlers
  1.3  Add PATCH endpoint + UpdateQuantity to service/interface
  1.4  Write backend unit + integration tests
  1.5  Verify: dotnet test passes

Phase 2 — Frontend foundation
  2.1  Export CartItem type from types/
  2.2  Add cart API functions (fetchCart, removeFromCart, clearCart, updateCartItemQuantity)
  2.3  Install React Router; add BrowserRouter + routes in App.tsx
  2.4  Wire Header cart icon to <Link to="/cart">

Phase 3 — Cart UI
  3.1  Create useCart hook
  3.2  Create CartItemRow component
  3.3  Create CartPage component
  3.4  Write frontend tests
  3.5  Verify: npm test passes
```

## Out of Scope

- Checkout / payment flow
- User authentication or per-user carts
- Persistent cart storage (stays in-memory)
- Product stock validation on quantity change
- Animations or transitions
