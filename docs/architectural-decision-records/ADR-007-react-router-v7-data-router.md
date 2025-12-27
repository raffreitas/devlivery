# React Router v7 com Data Router API

**Data:** 2025-12-26
**Status:** Aceito
**Contexto:** Stack Tecnológica / Roteamento

## Contexto e Problema

O projeto precisa de roteamento client-side para navegação entre páginas. A decisão fundamental é: devemos usar React Router (qual versão?), Next.js Router, ou outra solução?

A estrutura do repositório revela esta decisão através da organização:

```
src/
├── app-routes.tsx         # ← createBrowserRouter
└── features/
    └── */pages/          # ← Páginas/rotas
```

**Problema:** Qual biblioteca de roteamento oferece melhor DX e suporta padrões modernos (data loading, nested routes)?

## Opções Consideradas

* **React Router v6** - Versão estável, mas API mais antiga
* **React Router v7** - Nova API com data router, suporte a nested routes melhorado
* **Next.js Router** - Requer Next.js (framework completo, não necessário aqui)
* **Wouter** - Biblioteca leve, mas com menos features

## Decisão

**Escolhida:** "React Router v7", porque:

1. **Data Router API:** Nova API mais declarativa e poderosa
2. **Nested Routes:** Suporte melhorado para rotas aninhadas
3. **Type Safety:** Melhor suporte a TypeScript
4. **Ecosystem:** Biblioteca mais popular e madura para React
5. **Future-Proof:** Versão mais recente com melhorias de performance

### Implementação Técnica

A decisão se materializa em:

1. **Router Configuration:** `createBrowserRouter` define todas as rotas
2. **Nested Routes:** Rotas protegidas e layout aninhados
3. **Route Protection:** Componente `RequireAuth` protege rotas

```typescript
// src/app-routes.tsx
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { Layout } from "./shared/components/layout";
import { RequireAuth } from "./shared/components/require-auth";

const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/",
    element: <RequireAuth />,        // ← Proteção de rota
    children: [
      {
        element: <Layout />,         // ← Layout compartilhado
        children: [
          { index: true, element: <DashboardPage /> },
          { path: "orders", element: <OrdersPage /> },
          { path: "products", element: <ProductsPage /> },
          // ...
        ],
      },
    ],
  },
]);

export function AppRoutes() {
  return <RouterProvider router={router} />;
}
```

**Padrão de Rotas:**
- Rotas públicas (ex: `/login`) fora de `RequireAuth`
- Rotas protegidas aninhadas sob `RequireAuth`
- Layout compartilhado (`Layout`) aplicado a rotas protegidas
- Páginas importadas de `features/*/pages/`

### Consequências

* ✅ **Bom:** API declarativa e fácil de entender
* ✅ **Bom:** Suporte a rotas aninhadas facilita organização
* ✅ **Bom:** TypeScript forte com inferência de tipos
* ✅ **Bom:** Integração nativa com React
* ⚠️ **Neutro:** v7 é relativamente nova (mas estável)
* ⚠️ **Ruim:** Migração de v6 requer ajustes (não aplicável - projeto novo)
* ⚠️ **Ruim:** Alguns padrões de v6 mudaram (não é problema aqui)

