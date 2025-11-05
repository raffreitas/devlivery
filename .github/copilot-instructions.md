# Copilot Instructions for devlivery-webapp

## Project Overview
- **Stack:** React + TypeScript + Vite + TailwindCSS
- **Purpose:** A web application for managing products and orders in a delivery service.
- **State Management:** React Query for server state (products, orders, dashboard) + AuthContext for auth state.
- **Structure:** Modular feature folders under `src/features/` (dashboard, orders, products), shared components in `src/shared/components/`, and shared services in `src/shared/services/`.
- **Routing:** Uses `react-router-dom` for client-side routing, with routes defined in `src/routes/index.tsx`.

## Architecture & Patterns
- **Feature-based organization:** Each domain (dashboard, orders, products) has its own folder with `components/`, `hooks/`, `pages/`, `services/`, and `types/`.
- **Shared UI:** Common UI elements (Button, Card, Input, Layout, Modal) are in `src/shared/components/`.
- **Server State:** Managed via React Query hooks inside each feature (e.g., `features/products/hooks/useProducts.ts`).
- **Service Layer:** API/data logic is separated into service files (e.g., `orderService.ts`, `productService.ts`).
- **Type Safety:** Types are defined per feature in `types/index.ts` files.

## Developer Workflows
- **Start Dev Server:**
  ```pwsh
  pnpm dev
  ```
- **Build for Production:**
  ```pwsh
  pnpm build
  ```
- **Install Dependencies:**
  ```pwsh
  pnpm add <package>
  ```
- **Linting:**
  - Biome is configured for TypeScript and React. For stricter/type-aware rules, update `biome.js`.
- **Hot Module Reloading:** Enabled via Vite for fast feedback.

## Conventions & Integration
- **TypeScript Configs:** Multiple tsconfig files (`tsconfig.app.json`, `tsconfig.node.json`) for app and node environments.
- **External Libraries:**
  - `react-router-dom` for routing
  - Vite plugins for React

## Example Patterns
- **Feature Service Example:**
  - `src/features/orders/services/orderService.ts` handles API/data logic for orders.
- **Feature Hook Example:**
  - `src/features/orders/hooks/useOrders.ts` encapsulates queries and mutations.
- **Shared Component Example:**
  - `src/shared/components/button.tsx` is used across features for consistent UI.

## Key Files & Directories
- `src/features/` — Feature modules
- `src/shared/components/` — Reusable UI components
- `src/shared/contexts/` — Context providers (Auth somente)
- `src/shared/services/` — HTTP client e utilidades
- `src/routes/index.tsx` — App routing
- `vite.config.ts` — Vite configuration
- `README.md` — Additional setup and linting details

---

**Feedback:** If any conventions or workflows are unclear or missing, please specify so this guide can be improved.
