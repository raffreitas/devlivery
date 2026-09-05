# Devlivery

Devlivery é uma plataforma de gestão para operações de delivery. O monorepo reúne uma API .NET e uma aplicação web React para administrar pedidos, produtos, despesas, caixa e indicadores operacionais.

## Visão geral

A API segue arquitetura vertical slice e mantém as regras de negócio próximas de cada feature. O frontend também é organizado por feature e consome a API por contratos HTTP. PostgreSQL armazena os dados da aplicação e do ASP.NET Core Identity.

## Funcionalidades

- **Autenticação:** login JWT com bloqueio por falhas consecutivas e limitação por IP.
- **Pedidos:** criação, edição, mudança de status, pagamentos, troco e cancelamento.
- **Produtos:** catálogo, preços e disponibilidade.
- **Caixa:** abertura, aportes, fechamento e movimentações vinculadas ao operador autenticado.
- **Despesas:** categorias, vencimentos, pagamentos e acompanhamento operacional.
- **Dashboard:** vendas, pedidos, produtos e despesas consolidados.
- **Backup:** job separado para exportar o PostgreSQL e armazenar os artefatos no Cloudflare R2.

## Estrutura do monorepo

```text
apps/
├── api/                         # API, domínio, persistência, backup e testes
│   ├── src/Devlivery
│   ├── src/Devlivery.BackupJob
│   └── test/Devlivery.Tests
└── web/                         # SPA React organizada por features
    └── src/
        ├── features
        └── shared
```

- [API](apps/api/README.md): .NET 10, Minimal APIs, EF Core, Dapper, PostgreSQL e OpenTelemetry.
- [Web](apps/web/README.md): React 19, TypeScript, Vite, TanStack Query e Tailwind CSS.

O tenant vem do token autenticado e é aplicado aos dados da aplicação. Eventos de domínio mantêm os efeitos de pedidos e caixa dentro da mesma requisição. Consulte a [visão de arquitetura](docs/architecture.md) para os limites atuais.

## Executar localmente

Pré-requisitos: .NET 10 SDK, Node.js 24, pnpm 10 e Docker em execução.

```powershell
cd apps/api
docker compose up -d postgres
dotnet tool restore
dotnet restore Devlivery.slnx
dotnet ef database update --project src/Devlivery --context ApplicationDbContext
dotnet ef database update --project src/Devlivery --context ApplicationIdentityDbContext
dotnet run --project src/Devlivery
```

Em outro terminal:

```powershell
cd apps/web
pnpm install --frozen-lockfile
pnpm dev
```

A API usa `https://localhost:7141` e `http://localhost:5052`; o frontend usa `http://localhost:5173`. Scalar e o documento OpenAPI são disponibilizados somente em desenvolvimento.

Para executar as verificações principais:

```powershell
dotnet test apps/api/Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
pnpm --dir apps/web lint
pnpm --dir apps/web build
```

## Documentação

- [Índice da documentação](docs/README.md)
- [Arquitetura](docs/architecture.md)
- [Desenvolvimento local](docs/local-development.md)
- [Configuração](docs/configuration.md)
- [Publicação da API](docs/deployment.md)
- [Backup e recuperação](docs/backup-and-restore.md)
- [Segurança do login e do caixa](docs/login-and-cash-security.md)
