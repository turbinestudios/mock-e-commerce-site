# Mock E-Commerce Site — Copilot Instructions

## Project Overview

A mock e-commerce application with a .NET backend API and a React frontend.

## Tech Stack

### Backend (`src/backend/MockEcommerce.Api/`)
- .NET 10 / C# 14 — ASP.NET Core Minimal APIs
- OpenAPI via `Microsoft.AspNetCore.OpenApi`
- In-memory data (no database) — services implement interfaces (`IProductService`, `ICartService`)
- Endpoints organised in static extension-method classes under `Endpoints/`
- Models in `Models/`, services in `Services/`
- `Program.cs` wires DI, CORS (localhost:5173), OpenAPI, and endpoint mapping

### Frontend (`src/frontend/`)
- React 19 with TypeScript 6 and Vite 8
- Functional components with hooks (`useState`, `useEffect`, custom hooks in `hooks/`)
- Components exported via barrel `index.ts` files
- API calls in `src/api/index.ts` — fetches from `http://localhost:5000`

### Testing
- **Backend**: xUnit with `Microsoft.AspNetCore.Mvc.Testing` (`test/backend/MockEcommerce.Api.Tests/`)
- **Frontend**: Vitest 4 + Testing Library + jsdom (`test/frontend/`)
- Root `vitest.config.ts` configures the frontend test suite

## Conventions

- Backend uses file-scoped namespaces, nullable reference types enabled, implicit usings
- Minimal API endpoints return `TypedResults` (e.g. `TypedResults.Ok(...)`, `TypedResults.NotFound()`)
- Endpoint methods are `internal static` on static classes
- Frontend components live under `src/frontend/src/components/<Name>/` with a co-located barrel index
- Frontend tests mirror the source tree under `test/frontend/`
- Backend tests mirror the source tree under `test/backend/`

## Running the Project

```sh
# Backend (from repo root)
dotnet run --project src/backend/MockEcommerce.Api

# Frontend (from src/frontend)
npm run dev

# Tests
dotnet test                          # backend
npm test                             # frontend (vitest)
```
