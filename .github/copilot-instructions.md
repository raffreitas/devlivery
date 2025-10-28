# Copilot Instructions for devlivery-webapp

## Project Overview
- **Stack:** React + TypeScript + Vite + TailwindCSS
- **Purpose:** A web application for managing products and orders in a delivery service.
- **State Management:** Context API with LocalStorage persistence.
- **Structure:** Modular feature folders under `src/features/` (dashboard, orders, products), shared components in `src/shared/components/`, and context providers in `src/shared/contexts/`.
- **Routing:** Uses `react-router-dom` for client-side routing, with routes defined in `src/routes/index.tsx`.

## Architecture & Patterns
- **Feature-based organization:** Each domain (dashboard, orders, products) has its own folder with `components/`, `hooks/`, `pages/`, `services/`, and `types/`.
- **Shared UI:** Common UI elements (Button, Card, Input, Layout, Modal) are in `src/shared/components/`.
- **Context API:** State management for orders and products via React Contexts in `src/shared/contexts/`.
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
- **Context Usage:**
  - `src/shared/contexts/OrderContext.tsx` provides order state and actions to components.
- **Shared Component Example:**
  - `src/shared/components/Button.tsx` is used across features for consistent UI.

## Key Files & Directories
- `src/features/` — Feature modules
- `src/shared/components/` — Reusable UI components
- `src/shared/contexts/` — Context providers
- `src/routes/index.tsx` — App routing
- `vite.config.ts` — Vite configuration
- `README.md` — Additional setup and linting details

---

**Feedback:** If any conventions or workflows are unclear or missing, please specify so this guide can be improved.
