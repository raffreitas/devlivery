# Copilot Instructions for devlivery-webapp

## Project Overview
**Stack:** React 19 + TypeScript + Vite + TailwindCSS 4 + React Router 7 + React Query (TanStack)
**Purpose:** To provide an end-to-end delivery solution that connects customers and businesses, enabling online self-service ordering for customers and comprehensive operational management for merchants, including products, orders, payments, dashboards, and cash register control.

## Architecture: Feature-Based Organization
Each domain lives in `src/features/{dashboard,orders,products,auth,cash}` with:
- `components/` — Feature-specific UI
- `pages/` — Route pages
- `services/` — API calls and DTO mapping
- `hooks/` — React Query hooks (queries + mutations)
- `types/` — TypeScript interfaces

**Shared resources** in `src/shared/`:
- `components/` — Reusable UI (Layout, RequireAuth, bottom-sheet, cash-modal, etc.)
- `components/ui/` — shadcn/ui-style components (Button, Card, Input, Dialog, etc.) with Radix UI + CVA + TailwindCSS
- `contexts/` — AuthContext only (user/token state)
- `services/api.ts` — HTTP client with auto-auth headers
- `services/auth-events.ts` — Global auth event emitter for 401 handling
- `hooks/` — Shared hooks like `useDateRangeFilter`
- `utils/formatters.ts` — Date/currency formatting utilities
- `constants/ui-styles.ts` — Shared UI configuration constants

## Critical Patterns

### HTTP Client & API
**File:** `src/shared/services/api.ts`
- Wraps native `fetch` with base URL from `VITE_API_URL` env var (validated via Zod in `src/env.ts`)
- Auto-injects `Authorization: Bearer <token>` header by reading from localStorage key `devlivery@auth`
- Returns typed responses; throws `ApiError` (status/details) or `UnauthorizedError` (401) on failure
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
Always define `getAll`, `getById`, `create`, `update`, `delete` methods per resource. Services may include helpers like `getTodayOrders()` for domain-specific queries.

### React Query Hooks
**Pattern:** Each feature exposes a custom hook (e.g., `useProducts`, `useOrders`) that wraps:
- `useQuery` for fetching (with `staleTime: 30_000` to `60_000` for caching)
- `useMutation` for create/update/delete (with `onSuccess` invalidating cache)
- `placeholderData: (prev) => prev` to prevent UI flicker during refetch

**Example:** `src/features/orders/hooks/use-orders.ts`
```typescript
const ordersQuery = useQuery({
  queryKey: ["orders", { startDate, endDate, paymentMethod }],
  queryFn: () => orderService.getAll({ startDate, endDate, paymentMethod }),
  staleTime: 30_000,
  placeholderData: (previousData) => previousData,
});
const createMutation = useMutation({
  mutationFn: orderService.create,
  onSuccess: () => queryClient.invalidateQueries({ queryKey: ["orders"] }),
});
return { orders: ordersQuery.data ?? [], createOrder: createMutation.mutateAsync, ... };
```
**Key:** Mutations always invalidate queries to refetch updated data. Query keys include filter params for proper cache isolation.

### Authentication Flow
**Files:** `src/features/auth/services/auth-service.ts`, `src/shared/contexts/AuthContext.tsx`, `src/shared/components/RequireAuth.tsx`, `src/shared/services/auth-events.ts`
- Login POST returns `{ userId, userName, token }` → saved to localStorage as `devlivery@auth`
- `AuthContext` wraps app (in `src/app.tsx`), provides `{ user, token, login, logout, isAuthenticated, loading }`
- `RequireAuth` component (used in `src/app-routes.tsx`) guards protected routes → redirects to `/login` if not authenticated
- Token is read from localStorage on every API request by `api.ts`
- **Global 401 handling:** `api.ts` throws `UnauthorizedError` on 401 → `AuthContext` subscribes to `authEvents` → auto-logout on any 401 response

### Routing
**File:** `src/app-routes.tsx`
- Uses `createBrowserRouter` with nested routes under `<Layout />`
- Protected routes wrapped in `<RequireAuth />` element (nested under Layout)
- Routes: `/` (dashboard), `/products`, `/orders`, `/cash`, `/login`

### Cash Register Feature
**Files:** `src/features/cash/*` (see `src/features/cash/README.md` for details)
- **Purpose:** Open/close cash register sessions, track deposits, validate physical cash count
- **Flow:** Open session with initial amount → Add deposits as needed → View real-time sales breakdown → Close with actual cash count
- **Backend validation:** System auto-calculates expected cash (opening + deposits + cash sales) and compares with actual count
- **Key service methods:** `openCashSession`, `closeCashSession`, `createDeposit`, `getCurrentSession`
- Hook returns current session state + sales totals + payment breakdown for live monitoring

### User Feedback & Toasts
**Library:** `sonner` for toast notifications (setup in `src/main.tsx`)
- Import with `import { toast } from "sonner"`
- Common pattern: Show `toast.success()` after successful mutations (create/update/delete)
- Toaster component configured with `richColors` prop for semantic color coding

### Specialized Hooks
**Printing:** `src/features/orders/hooks/use-print-order.ts` uses `react-to-print` with thermal receipt styling (55mm width, Courier New, custom `@page` styles for thermal printers).

**Date filtering:** `src/shared/hooks/use-date-range-filter.ts` provides debounced date range inputs with validation (start ≤ end), returning both input state (instant) and applied state (debounced for API calls).

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
- **Formatting:** Biome enforces double quotes, 2-space indent, organizeImports on save (no ESLint/Prettier)
- **TypeScript:** Strict mode enabled, `erasableSyntaxOnly` for React compiler compatibility
- **React Compiler:** Enabled via `babel-plugin-react-compiler` in Vite config (automatic memoization)
- **Styling:** TailwindCSS 4 via `@tailwindcss/vite` plugin, no separate config file needed
- **Icons:** `lucide-react` for all icons
- **Error handling:** Services throw on API errors; components handle via try/catch or React Query `error` state
- **Naming:** kebab-case for files, camelCase for hooks (`use-products.ts` exports `useProducts`)

## Key Dependencies
- `react-router-dom` v7 (new data router API with `createBrowserRouter`)
- `@tanstack/react-query` v5 + devtools
- `zod` for runtime env validation
- `tailwindcss` v4 + `@tailwindcss/vite`
- `lucide-react` for icons
- `react-to-print` for thermal receipt printing
- Biome v2 for linting/formatting (no ESLint/Prettier)

## Adding a New Feature
1. Create `src/features/{feature}/` folder structure
2. Define types in `types/index.ts` (DTOs + domain types)
3. Create service in `services/{feature}-service.ts` with DTO mapping functions
4. Build React Query hook in `hooks/use-{feature}.ts` (wrap queries + mutations)
5. Create UI in `components/` (feature-specific) and page in `pages/`
6. Add route in `src/app-routes.tsx` (protected or public)
7. Update Layout navigation if needed (`src/shared/components/layout.tsx`)
