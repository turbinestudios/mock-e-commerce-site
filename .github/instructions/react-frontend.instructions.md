---
description: "Use when writing or modifying React components, hooks, API functions, or TypeScript types in the frontend. Covers component structure, hooks, API layer, styling, and accessibility patterns."
applyTo: "src/frontend/src/**/*.{ts,tsx}"
---

# React/TypeScript Frontend Conventions

## Component Structure

Components live in `src/frontend/src/components/` using a folder-per-component pattern:

```
components/
├── ProductCard/
│   ├── ProductCard.tsx       # Component implementation
│   └── index.ts              # Barrel export
```

Every component folder must have:
1. `ComponentName.tsx` — the component file (named identically to the folder)
2. `index.ts` — barrel re-export: `export { ComponentName } from './ComponentName';`

## Component Pattern

Use **named function exports** (not default exports) with explicit props interfaces:

```tsx
import type { Product } from '../../types';

interface ProductCardProps {
  product: Product;
  onAddToCart: (product: Product) => void;
}

export function ProductCard({ product, onAddToCart }: ProductCardProps) {
  return (
    <article className="product-card">
      {/* ... */}
    </article>
  );
}
```

Rules:
- **Functional components only** — no class components
- **Named exports** — no `export default`
- **Props interface** defined in the component file, prefixed with component name (`ProductCardProps`)
- Use `import type` for type-only imports

## Custom Hooks

Hooks live in `src/frontend/src/hooks/` with `use*` naming:

```typescript
import { useState, useEffect } from 'react';
import type { Product } from '../types';
import { fetchProducts } from '../api';

interface UseProductsResult {
  products: Product[];
  loading: boolean;
  error: string | null;
}

export function useProducts(): UseProductsResult {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchProducts()
      .then(setProducts)
      .catch((err: unknown) => setError(err instanceof Error ? err.message : 'Unknown error'))
      .finally(() => setLoading(false));
  }, []);

  return { products, loading, error };
}
```

Rules:
- Return a typed result interface (`Use{Name}Result`)
- Handle loading, error, and data states
- Type caught errors as `unknown`

## API Layer

All API calls go through `src/frontend/src/api/index.ts`:

```typescript
const BASE_URL = '/api';

export async function fetchProducts(): Promise<Product[]> {
  const response = await fetch(`${BASE_URL}/products`);
  if (!response.ok) throw new Error('Failed to fetch products');
  return response.json();
}
```

Rules:
- Use native `fetch` — no external HTTP clients
- Every function has an explicit `Promise<T>` return type
- Throw on non-OK responses with descriptive messages
- Base URL is `/api` (Vite proxies to `http://localhost:5063` in dev)

When adding a new API endpoint:
1. Add the function to `api/index.ts`
2. Ensure the return type matches the TypeScript interface in `types/index.ts`
3. Ensure the TypeScript interface matches the C# model in the backend

## Types

Shared TypeScript interfaces live in `src/frontend/src/types/index.ts`:

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
```

Rules:
- Use `interface` (not `type` alias) for object shapes
- Property names use **camelCase** (matching JSON serialization from the .NET backend)
- Keep synchronized with backend C# models in `src/backend/MockEcommerce.Api/Models/`

## CSS & Styling

- Plain CSS files — no CSS-in-JS or CSS modules
- **BEM naming**: `.product-card`, `.product-card__body`, `.product-card__button`
- **CSS custom properties** for theming (defined in `index.css`): `--text`, `--bg`, `--accent`, `--shadow`
- Dark mode via `@media (prefers-color-scheme: dark)`
- Responsive layout with CSS Grid and Flexbox

## Accessibility

- Use semantic HTML: `<article>`, `<section>`, `<nav>`, `<ul>`, `<button>`
- Add `aria-label` attributes for screen reader context
- Ensure interactive elements are keyboard-accessible
- Disable buttons with clear label changes (e.g., "Out of Stock" instead of "Add to Cart")

## TypeScript Style

- **Strict mode** — no `any` types
- Use `import type` for type-only imports
- Enable `noUnusedLocals` and `noUnusedParameters`
- Prefer explicit function return types for exported functions
