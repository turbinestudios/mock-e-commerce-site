# Plan: Cart Page Feature

## TL;DR

Implement a cart page where users can view, manage, and total their cart before checkout. The backend cart service and endpoints are stubbed — implement them first, then add React Router + cart UI on the frontend.

## Phases

### Phase 1 — Backend service + endpoints (no frontend dependency)

**Step 1.1** Implement `InMemoryCartService` methods
- File: `src/backend/MockEcommerce.Api/Services/InMemoryCartService.cs`
- Implement `GetAll`, `GetByProductId`, `Add`, `Remove`, `Clear` using the existing `_cart` list and `_lock`
- All methods must acquire `_lock` for thread safety
- `GetAll` returns a snapshot copy; `Add` increments quantity if product already in cart

**Step 1.2** Add `UpdateQuantity` to cart service
- Add `CartItem? UpdateQuantity(int productId, int quantity)` to `ICartService` interface
- Implement in `InMemoryCartService`: find item by productId, set quantity, return updated item (or null)
- Files: `ICartService.cs`, `InMemoryCartService.cs`

**Step 1.3** Implement `CartEndpoints` handlers
- File: `src/backend/MockEcommerce.Api/Endpoints/CartEndpoints.cs`
- `GetCart` → 200 OK with items
- `AddToCart` → validate quantity > 0, look up product, build CartItem snapshot, call Add. 201 Created (new) or 200 OK (updated). 404 if product missing, ValidationProblem if quantity ≤ 0
- `RemoveFromCart` → 204 or 404
- `ClearCart` → 204

**Step 1.4** Add `PATCH /api/cart/{productId}` endpoint
- File: `CartEndpoints.cs`
- Add `UpdateQuantityRequest` record and `UpdateQuantity` handler
- Register route in `MapCartEndpoints`
- Validate quantity > 0, return 200 OK or 404/ValidationProblem

**Step 1.5** Write backend tests
- `test/backend/MockEcommerce.Api.Tests/Services/InMemoryCartServiceTests.cs` — 10 unit tests
- `test/backend/MockEcommerce.Api.Tests/Endpoints/CartEndpointTests.cs` — 10 integration tests using `WebApplicationFactory<Program>` (follow `ProductEndpointTests` pattern)

**Step 1.6** Verify: `dotnet test` passes

### Phase 2 — Frontend foundation (*parallel with Phase 1*)

**Step 2.1** Export `CartItem` type
- File: `src/frontend/src/types/index.ts` — add `CartItem` interface
- File: `src/frontend/src/api/index.ts` — remove local `CartItem` interface, import from `types/`

**Step 2.2** Add cart API functions
- File: `src/frontend/src/api/index.ts`
- Add `fetchCart()`, `removeFromCart(productId)`, `clearCart()`, `updateCartItemQuantity(productId, quantity)`

**Step 2.3** Install React Router and add routing
- Run `npm install react-router` in `src/frontend/`
- File: `src/frontend/src/App.tsx` — wrap app in `<BrowserRouter>`, define `/` and `/cart` routes
- Extract existing product listing into the `/` route

**Step 2.4** Wire Header cart icon to navigate
- File: `src/frontend/src/components/Header/Header.tsx`
- Replace `<button>` with React Router `<Link to="/cart">`, keep styling and aria-label

### Phase 3 — Cart UI (*depends on Phase 2; can start before Phase 1 finishes*)

**Step 3.1** Create `useCart` hook
- File: `src/frontend/src/hooks/useCart.ts`
- Fetch cart on mount, expose items, loading, error, totalItems, totalPrice, refresh, remove, clear, updateQuantity
- Follow `useProducts` pattern for loading/error

**Step 3.2** Create `CartItemRow` component
- Files: `src/frontend/src/components/CartItemRow/CartItemRow.tsx` + `index.ts` barrel
- Props: item, onIncrement, onDecrement, onRemove
- Renders: product name, unit price, −/+ quantity controls, line total, remove button

**Step 3.3** Create `CartPage` component
- Files: `src/frontend/src/components/CartPage/CartPage.tsx` + `index.ts` barrel
- Uses `useCart` hook
- States: loading, error (with retry), empty (with "Continue shopping" link), populated (item list + subtotal + "Clear cart")
- "Back to shopping" link at top

**Step 3.4** Write frontend tests
- `test/frontend/hooks/useCart.test.ts` — 6 tests
- `test/frontend/components/CartPage/CartPage.test.tsx` — 9 tests
- `test/frontend/components/CartItemRow/CartItemRow.test.tsx` — 4 tests
- Update `test/frontend/components/Header/Header.test.tsx` — 1 new test (cart icon links to /cart)

**Step 3.5** Verify: `npm test` passes

## Relevant files

### Backend — modify
- `src/backend/MockEcommerce.Api/Services/ICartService.cs` — add `UpdateQuantity` method
- `src/backend/MockEcommerce.Api/Services/InMemoryCartService.cs` — implement all methods
- `src/backend/MockEcommerce.Api/Endpoints/CartEndpoints.cs` — implement handlers, add PATCH route

### Backend — create
- `test/backend/MockEcommerce.Api.Tests/Services/InMemoryCartServiceTests.cs`
- `test/backend/MockEcommerce.Api.Tests/Endpoints/CartEndpointTests.cs`

### Backend — reference (patterns to follow)
- `src/backend/MockEcommerce.Api/Endpoints/ProductEndpoints.cs` — endpoint pattern with `TypedResults`
- `src/backend/MockEcommerce.Api/Services/MockProductService.cs` — service pattern
- `test/backend/MockEcommerce.Api.Tests/Endpoints/ProductEndpointTests.cs` — integration test pattern
- `test/backend/MockEcommerce.Api.Tests/Services/MockProductServiceTests.cs` — unit test pattern

### Frontend — modify
- `src/frontend/src/types/index.ts` — add `CartItem` interface
- `src/frontend/src/api/index.ts` — add cart API functions, remove local `CartItem`
- `src/frontend/src/App.tsx` — add BrowserRouter + routes
- `src/frontend/src/components/Header/Header.tsx` — cart icon → `<Link>`

### Frontend — create
- `src/frontend/src/hooks/useCart.ts`
- `src/frontend/src/components/CartPage/CartPage.tsx` + `index.ts`
- `src/frontend/src/components/CartItemRow/CartItemRow.tsx` + `index.ts`
- `test/frontend/hooks/useCart.test.ts`
- `test/frontend/components/CartPage/CartPage.test.tsx`
- `test/frontend/components/CartItemRow/CartItemRow.test.tsx`

### Frontend — update test
- `test/frontend/components/Header/Header.test.tsx`

## Verification

1. `dotnet test` — all 20 backend tests pass (10 unit + 10 integration)
2. `npm test` — all frontend tests pass (20 new + existing unchanged)
3. Manual: run backend (`dotnet run`) + frontend (`npm run dev`), add items from product page, click cart icon, verify cart page renders with items, adjust quantities, clear cart

## Decisions

- **PATCH endpoint added** beyond spec stubs — needed for in-place quantity editing without remove/re-add
- **React Router** chosen as the routing solution (standard for React apps, no alternatives considered)
- **Shared singleton cart** — kept as-is per existing `InMemoryCartService` design (no auth)
- **No checkout flow** — out of scope
- **No stock validation** on quantity change — out of scope
- **No persistent storage** — stays in-memory per existing design
