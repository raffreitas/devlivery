# Devlivery Web

Interface web da plataforma Devlivery para autenticação, dashboard e gestão de pedidos, produtos, despesas e caixa.

## Tecnologias

- React 19 e TypeScript;
- Vite 7;
- React Router 7;
- TanStack Query;
- React Hook Form e Zod;
- Tailwind CSS 4 e Radix UI;
- Biome para lint e formatação.

## Estrutura

```text
src/
├── features/                    # auth, cash, dashboard, expenses, orders e products
├── shared/                      # componentes, serviços, contextos e utilitários
├── app-routes.tsx
└── main.tsx
```

Cada feature concentra páginas, componentes, hooks, contratos e acesso à API relacionados ao seu fluxo.

## Executar

Pré-requisitos: Node.js 24 e pnpm 10. Copie o arquivo de exemplo e aponte a aplicação para a API local:

```powershell
Copy-Item .env.example .env
pnpm install --frozen-lockfile
pnpm dev
```

O frontend fica disponível em `http://localhost:5173`. A variável `VITE_API_URL` define a origem da API e não deve terminar com `/`.

## Verificações

```powershell
pnpm lint
pnpm build
```

O projeto não possui uma suíte de testes frontend configurada atualmente. O build executa a verificação TypeScript antes de gerar `dist/`.

Para publicar, configure `VITE_API_URL` durante o build com a URL HTTPS pública da API e inclua a origem pública do frontend em `ALLOWED_ORIGINS` na API. Consulte o [guia de publicação](../../docs/deployment.md).
