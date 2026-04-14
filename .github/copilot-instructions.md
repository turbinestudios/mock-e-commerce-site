# Mock E-Commerce Site — Project Guidelines

## Overview

A mock e-commerce application with two stacks:

- **Backend**: ASP.NET Core Web API (.NET 10) — `src/backend/`
- **Frontend**: React 19 + TypeScript 6 + Vite 8 — `src/frontend/`

The API serves product catalog and cart endpoints. The frontend consumes them via `/api` proxy.

## Repository Structure

```
src/backend/              .NET API source (solution: MockEcommerce.slnx)
src/frontend/             React/TypeScript frontend
test/backend/             xUnit test projects (mirrors src/backend structure)
test/frontend/            Vitest + Testing Library tests (mirrors src/frontend/src structure)
```

Source and test trees are kept separate. Test folder structure mirrors the source folder it covers.

## Mandatory Testing Policy

Every code change MUST include corresponding tests. No exceptions.

- **New feature** → unit tests for every new class, method, component, or hook
- **Bug fix** → at least one test that reproduces the bug before the fix
- **Refactor** → existing tests must still pass; add tests if coverage gaps are discovered

Before considering any task complete, verify:
1. All new code has corresponding test files
2. `dotnet test` passes for backend changes
3. `npm run test` (from repo root) passes for frontend changes

## Build, Run & Test Commands

### Backend (.NET)

```bash
# From repo root
dotnet build src/backend/MockEcommerce.slnx
dotnet run --project src/backend/MockEcommerce.Api/MockEcommerce.Api.csproj    # http://localhost:5063
dotnet test test/backend/MockEcommerce.Api.Tests/
```

### Frontend (React/Vite)

```bash
# From src/frontend/
npm install
npm run dev          # Vite dev server at http://localhost:5173
npm run build        # Type-check + production build
npm run lint         # ESLint

# From repo root
npm install
npm run test         # Vitest single-run
```

## Cross-Cutting Conventions

### API Contract

All API routes use the `api/[controller]` prefix. The frontend fetches from `/api/*` which Vite proxies to `http://localhost:5063` in development.

### CORS

The backend allows `http://localhost:5173` (Vite dev server) with any header and method. When adding new origins, update the CORS policy in `Program.cs`.

### Type Synchronization

Backend C# models in `Models/` and frontend TypeScript interfaces in `src/frontend/src/types/index.ts` must stay in sync. When a model property changes on one side, update the other.
