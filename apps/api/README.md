# Devlivery API

API HTTP da plataforma Devlivery, construída em .NET 10 e organizada por vertical slices.

## Responsabilidades

- autenticação JWT e isolamento por tenant;
- produtos, pedidos, despesas e sessões de caixa;
- consultas de dashboard;
- persistência com EF Core e Dapper em PostgreSQL;
- health checks, OpenAPI em desenvolvimento e telemetria OpenTelemetry.

## Estrutura

```text
apps/api/
├── src/Devlivery/
│   ├── Domain/                  # agregados, entidades e regras de domínio
│   ├── Features/                # endpoints, comandos, consultas e handlers
│   └── Infrastructure/          # identidade, persistência, tenancy e HTTP
├── test/Devlivery.Tests/        # testes unitários e HTTP
├── compose.yaml                 # PostgreSQL e dashboard Aspire locais
└── Devlivery.slnx
```

Os endpoints são agrupados em `/api/v1/auth`, `/api/v1/products`, `/api/v1/orders`, `/api/v1/cash-register`, `/api/v1/expenses` e `/api/v1/dashboard`. Exceto login e health checks, a API exige um JWT válido.

## Executar

Pré-requisitos: .NET 10 SDK, Docker e certificado HTTPS de desenvolvimento configurado.

```powershell
docker compose up -d postgres
dotnet tool restore
dotnet restore Devlivery.slnx
dotnet ef database update --project src/Devlivery --context ApplicationDbContext
dotnet ef database update --project src/Devlivery --context ApplicationIdentityDbContext
dotnet run --project src/Devlivery
```

Em desenvolvimento, a inicialização também aplica migrações pendentes e cria os dados locais quando o banco está vazio. A conta seed é apenas para desenvolvimento e deve ser substituída em qualquer ambiente compartilhado.

Endereços locais:

- API HTTPS: `https://localhost:7141`
- API HTTP: `http://localhost:5052`
- Scalar: `https://localhost:7141/scalar`
- OpenAPI: `https://localhost:7141/openapi/v1.json`
- Prontidão: `/health`
- Vivacidade: `/alive`

## Documentação interativa

Em desenvolvimento, abra `/scalar/` para consultar o guia de início rápido, selecionar o contrato e testar requisições. Autentique-se pelo login e cole `data.token` no campo Bearer; o login é público e as demais operações indicam a exigência de JWT. A referência inclui descrições das áreas, resumos das operações e orientações sobre respostas e erros.

Ao adicionar ou alterar um endpoint, mantenha os metadados `.Produces(...)` alinhados ao comportamento real. A apresentação e o conteúdo geral ficam em `Infrastructure/Http/Configuration/OpenApiConfiguration.cs`.

## Testes

A suíte cobre domínio, validators, handlers e contratos HTTP (`ApiFactory`, em `Features/<Feature>/Api`). Os testes HTTP e de consultas usam PostgreSQL via Testcontainers e precisam do Docker em execução.

```powershell
dotnet test Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
```

Consulte a [estratégia de testes](../../docs/development/testing-strategy.md) para os níveis cobertos e as convenções.

## Configuração e operação

Não salve connection strings, chaves JWT ou credenciais do R2 no repositório. Use User Secrets localmente e variáveis de ambiente no ambiente hospedado.

- [Configuração](../../docs/development/configuration.md)
- [Modelo de domínio](../../docs/product/domain-model.md)
- [Publicação da API](../../docs/operations/deployment.md)
- [Backup e recuperação](../../docs/operations/backup-and-restore.md)
- [Segurança do login e do caixa](../../docs/operations/login-and-cash-security.md)
