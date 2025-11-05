# Copilot Instructions for devlivery-webapp

## Project Overview
**Stack:** React 19 + TypeScript + Vite + TailwindCSS 4 + React Router 7 + React Query (TanStack)
**Purpose:** PDV (Point of Sale) for pizza delivery with products, orders, and dashboard management.

## Architecture: Feature-Based Organization
Each domain lives in `src/features/{dashboard,orders,products,auth}` with:
- `components/` — Feature-specific UI
- `pages/` — Route pages
- `services/` — API calls and DTO mapping
- `hooks/` — React Query hooks (queries + mutations)
- `types/` — TypeScript interfaces

**Shared resources** in `src/shared/`:
- `components/` — Reusable UI (Button, Card, Input, Layout, Modal, RequireAuth)
- `contexts/` — AuthContext only (user/token state)
- `services/api.ts` — HTTP client with auto-auth headers

## Critical Patterns

### HTTP Client & API
**File:** `src/shared/services/api.ts`
- Wraps native `fetch` with base URL from `VITE_API_URL` env var (validated via Zod in `src/env.ts`)
- Auto-injects `Authorization: Bearer <token>` header by reading from localStorage key `devlivery@auth`
- Returns typed responses; throws `ApiError` with status/details on failure
- Handles 204 No Content and failed JSON parsing gracefully

**Example usage in a service:**
```typescript
const res = await api.get<ApiResponse<ProductDto[]>>("/api/products");
return res.data.map(mapDto);
```

### Service Layer: DTO Mapping
**Pattern:** Services receive DTOs (snake_case dates as strings) and map to domain types (camelCase, Date objects).
**Example:** `src/features/products/services/product-service.ts`
```typescript
function mapProductDto(dto: ProductDto): Product {
  return { ...dto, createdAt: new Date(dto.createdAt), updatedAt: new Date(dto.updatedAt) };
}
```
Always define `getAll`, `getById`, `create`, `update`, `delete` methods per resource.

### React Query Hooks
**Pattern:** Each feature exposes a custom hook (e.g., `useProducts`, `useOrders`) that wraps:
- `useQuery` for fetching (with `staleTime: 60_000` for caching)
- `useMutation` for create/update/delete (with `onSuccess` invalidating cache)

**Example:** `src/features/products/hooks/useProducts.ts`
```typescript
const productsQuery = useQuery({ queryKey: ["products"], queryFn: productService.getAll, staleTime: 60_000 });
const createMutation = useMutation({
  mutationFn: productService.create,
  onSuccess: () => queryClient.invalidateQueries({ queryKey: ["products"] }),
});
return { products: productsQuery.data ?? [], createProduct: createMutation.mutateAsync, ... };
```
**Key:** Mutations always invalidate queries to refetch updated data.

### Authentication Flow
**Files:** `src/features/auth/services/authService.ts`, `src/shared/contexts/AuthContext.tsx`, `src/shared/components/RequireAuth.tsx`
- Login POST returns `{ userId, userName, token }` → saved to localStorage as `devlivery@auth`
- `AuthContext` wraps app (in `src/app.tsx`), provides `{ user, token, login, logout, isAuthenticated }`
- `RequireAuth` component (used in `src/app-routes.tsx`) guards protected routes → redirects to `/login` if not authenticated
- Token is read from localStorage on every API request by `api.ts`

### Routing
**File:** `src/app-routes.tsx`
- Uses `createBrowserRouter` with nested routes under `<Layout />`
- Protected routes wrapped in `<RequireAuth />` element
- Routes: `/` (dashboard), `/products`, `/orders`, `/login`

## Developer Workflows

**Start dev server:**
```pwsh
pnpm dev
```

**Build for production (runs TypeScript build first):**
```pwsh
pnpm build
```

**Lint/format (Biome):**
```pwsh
pnpm lint
pnpm format
```

**Environment setup:**
Create `.env.local` with:
```dotenv
VITE_API_URL=https://localhost:7141
```
Validated via Zod schema in `src/env.ts`.

## Conventions

- **Path aliases:** `@/*` maps to `src/*` (configured in `vite.config.ts` + `tsconfig.app.json`)
- **Formatting:** Biome enforces double quotes, 2-space indent, organizeImports on save
- **TypeScript:** Strict mode enabled, `erasableSyntaxOnly` for React compiler compatibility
- **React Compiler:** Enabled via `babel-plugin-react-compiler` in Vite config
- **Styling:** TailwindCSS 4 via `@tailwindcss/vite` plugin, no separate config file needed
- **Error handling:** Services throw on API errors; components handle via try/catch or React Query `error` state

## Key Dependencies
- `react-router-dom` v7 (new data router API)
- `@tanstack/react-query` v5 + devtools
- `zod` for runtime env validation
- `tailwindcss` v4 + `@tailwindcss/vite`
- Biome v2 for linting/formatting (no ESLint/Prettier)

## Adding a New Feature
1. Create `src/features/{feature}/` folder
2. Define types in `types/index.ts`
3. Create service in `services/{feature}Service.ts` with DTO mapping
4. Build React Query hook in `hooks/use{Feature}.ts`
5. Create UI in `components/` and page in `pages/`
6. Add route in `src/app-routes.tsx`
