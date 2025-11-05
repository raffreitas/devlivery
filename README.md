# Devlivery Webapp

Aplicação web para gestão de produtos e pedidos de delivery.

## Stack

- React 19 + TypeScript + Vite
- TailwindCSS
- React Router
- React Query (TanStack) para estado do servidor e cache
- Biome para lint/format

## Backend / API

- Base URL configurável via variável de ambiente: `VITE_API_URL`
- Endpoints (resumo, conforme OpenAPI):
	- Auth: `POST /api/auth/login`
	- Products: `GET/POST /api/products`, `GET/PUT/DELETE /api/products/{id}`
	- Orders: `GET/POST /api/orders`, `GET/DELETE /api/orders/{id}`, `PATCH /api/orders/{id}/status`
	- Dashboard: `GET /api/dashboard/stats`

## Configuração

1) Instalar dependências

```pwsh
pnpm install
```

2) Configurar a URL da API no arquivo `.env` (ou `.env.local`)

```dotenv
VITE_API_URL=https://localhost:7141
```

3) Rodar em desenvolvimento

```pwsh
pnpm dev
```

4) Build de produção

```pwsh
pnpm build
```

5) Lint/format

```pwsh
pnpm lint
pnpm format
```

## Arquitetura

- Organização por feature: `src/features/{products,orders,dashboard,auth}`
- Cada feature contém `components/`, `pages/`, `services/`, `types/`, `hooks/`
- `src/shared/` contém componentes UI, serviços compartilhados e contextos (Auth)
- Cliente HTTP em `src/shared/services/api.ts` (fetch + baseURL + Authorization)
- React Query configurado em `src/main.tsx` com `QueryClientProvider`

## Estado

- Estado do servidor (produtos/pedidos/dashboard) via React Query (hooks por feature)
- Autenticação: `AuthContext` mantém `{ user, token }` após login e persiste no localStorage

## Notas

- Páginas e componentes usam hooks de feature: `useProducts`, `useOrders`
- Atualizações (create/update/delete) invalidam automaticamente os caches
