# Devlivery

[![API CI](https://github.com/raffreitas/devlivery/actions/workflows/api-ci.yaml/badge.svg?branch=main)](https://github.com/raffreitas/devlivery/actions/workflows/api-ci.yaml) [![API Sonar Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=devlivery-api&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=devlivery-api) [![API Coverage](https://sonarcloud.io/api/project_badges/measure?project=devlivery-api&metric=coverage)](https://sonarcloud.io/summary/new_code?id=devlivery-api)<br>
[![Web App CI](https://github.com/raffreitas/devlivery/actions/workflows/web-app-ci.yaml/badge.svg?branch=main)](https://github.com/raffreitas/devlivery/actions/workflows/web-app-ci.yaml) [![Web Sonar Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=devlivery-web&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=devlivery-web)<br>
[![License](https://img.shields.io/github/license/raffreitas/devlivery)](LICENSE)

Gestão operacional para negócios de delivery. O Devlivery reúne uma API .NET e uma aplicação web React para centralizar pedidos, produtos, caixa, despesas e indicadores.

## Arquitetura e tecnologias

A API usa arquitetura vertical slice, com regras de negócio próximas de cada feature. O frontend React também é organizado por feature e consome a API por contratos HTTP. PostgreSQL armazena os dados da aplicação e do ASP.NET Core Identity, com isolamento por tenant derivado do token autenticado.

- [API](apps/api/README.md): .NET 10, Minimal APIs, EF Core, Dapper, PostgreSQL e OpenTelemetry.
- [Web](apps/web/README.md): React 19, TypeScript, Vite, TanStack Query e Tailwind CSS.

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

Eventos de domínio mantêm os efeitos de pedidos e caixa dentro da mesma requisição. Consulte a [visão de arquitetura](docs/architecture/overview.md) para os limites atuais.

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
- [Visão do produto](docs/product/overview.md)
- [Modelo de domínio](docs/product/domain-model.md)
- [Arquitetura](docs/architecture/overview.md)
- [ADRs](docs/adrs/README.md)
- [Estratégia de testes](docs/development/testing-strategy.md)
- [Desenvolvimento local](docs/development/local-development.md)
- [Configuração](docs/development/configuration.md)
- [Publicação da API](docs/operations/deployment.md)
- [Backup e recuperação](docs/operations/backup-and-restore.md)
- [Segurança do login e do caixa](docs/operations/login-and-cash-security.md)
