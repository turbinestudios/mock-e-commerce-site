---
description: "Use when writing or modifying frontend tests. Covers Vitest configuration, Testing Library patterns, behavioral testing, mocking strategies, and test file organization."
applyTo: "test/frontend/**/*.{test,spec}.{ts,tsx}"
---

# Frontend Testing Conventions

## Framework & Dependencies

- **Vitest** — test runner with `globals: true` (no explicit `describe`/`it`/`expect` imports)
- **jsdom** — browser DOM simulation environment
- **@testing-library/react** — `render`, `screen` queries
- **@testing-library/user-event** — user interaction simulation
- **@testing-library/jest-dom** — DOM assertion matchers (`toBeInTheDocument`, `toBeDisabled`, etc.)

## Test File Location

Test files mirror the source component path under `test/frontend/`:

```
src/frontend/src/components/ProductCard/ProductCard.tsx
→ test/frontend/components/ProductCard/ProductCard.test.tsx

src/frontend/src/hooks/useProducts.ts
→ test/frontend/hooks/useProducts.test.ts
```

File naming: `{SourceFileName}.test.tsx` (or `.test.ts` for non-JSX)

## Imports

Import components and types using relative paths from the test file to the source:

```tsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProductCard } from '../../../../src/frontend/src/components/ProductCard';
import type { Product } from '../../../../src/frontend/src/types';
```

- Import the component from its **barrel export** (`index.ts`)
- Use `import type` for type-only imports
- Vitest globals (`describe`, `it`, `expect`, `vi`) are available without imports

## Test Structure

Use `describe` blocks per component and `it` for each behavior:

```tsx
describe('ProductCard', () => {
  it('renders product name', () => {
    render(<ProductCard product={mockProduct} onAddToCart={() => {}} />);
    expect(screen.getByText('Test Headphones')).toBeInTheDocument();
  });
});
```

## Mock Data

Define mock data as `const` at the top of the test file, typed with the source interface:

```tsx
const mockProduct: Product = {
  id: 1,
  name: 'Test Headphones',
  description: 'Great sound quality.',
  price: 79.99,
  category: 'Electronics',
  stock: 10,
  imageUrl: 'https://example.com/headphones.jpg',
};
```

Provide all required fields — do not use `Partial<T>` or type assertions.

## Behavioral Testing

Test **what the user sees and does**, not implementation details:

```tsx
// GOOD — tests user-visible behavior
expect(screen.getByText('Test Headphones')).toBeInTheDocument();
await userEvent.click(screen.getByRole('button', { name: /add to cart/i }));
expect(onAddToCart).toHaveBeenCalledWith(mockProduct);

// BAD — tests internal state or implementation
expect(component.state.isClicked).toBe(true);
```

## Query Priority

Prefer queries in this order (most accessible first):
1. `getByRole` — buttons, headings, links with accessible names
2. `getByText` — visible text content
3. `getByLabelText` — form inputs
4. `getByPlaceholderText` — fallback for form inputs
5. `getByTestId` — last resort only

## User Interactions

Use `@testing-library/user-event` (not `fireEvent`) for realistic interactions:

```tsx
const user = userEvent;
await user.click(screen.getByRole('button', { name: /add to cart/i }));
await user.type(screen.getByRole('textbox'), 'search query');
```

## Mocking

- **Callback props**: `vi.fn()` for mock functions passed as props
- **Modules**: `vi.mock('../../api')` for mocking API layer
- **Timers**: `vi.useFakeTimers()` / `vi.useRealTimers()` when testing time-dependent behavior

```tsx
it('calls onAddToCart when button is clicked', async () => {
  const onAddToCart = vi.fn();
  render(<ProductCard product={mockProduct} onAddToCart={onAddToCart} />);
  await userEvent.click(screen.getByRole('button', { name: /add to cart/i }));
  expect(onAddToCart).toHaveBeenCalledWith(mockProduct);
});
```

## What to Test

For every component, cover:
- **Rendering** — correct content appears for given props
- **User interactions** — clicks, input, form submissions trigger expected callbacks
- **Conditional rendering** — empty states, loading states, error states
- **Edge cases** — zero values, empty strings, boundary conditions
- **Accessibility** — disabled states, ARIA attributes, semantic structure

## Running Tests

```bash
# From repo root
npm run test
```

Test configuration is in `vitest.config.ts` at the repo root. Setup file: `src/frontend/src/test-setup.ts`.
