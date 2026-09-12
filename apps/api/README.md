# Devlivery API

API HTTP da plataforma Devlivery, construída em .NET 10 e organizada por vertical slices.

## Responsabilidades

- autenticação JWT e isolamento por tenant;
- produtos, pedidos, despesas e sessões de caixa;
- consultas de dashboard;
- persistência com EF Core e Dapper em PostgreSQL;
- health checks, OpenAPI em desenvolvimento e telemetria OpenTelemetry;
- job independente de backup do PostgreSQL para Cloudflare R2.

## Estrutura

```text
apps/api/
├── src/Devlivery/
│   ├── Domain/                  # agregados, entidades e regras de domínio
│   ├── Features/                # endpoints, comandos, consultas e handlers
│   └── Infrastructure/          # identidade, persistência, tenancy e HTTP
├── src/Devlivery.BackupJob/     # processo executável de backup
├── test/Devlivery.Tests/        # testes unitários e HTTP
├── compose.yaml                 # PostgreSQL e dashboard Aspire locais
└── Devlivery.slnx
```

Os endpoints são agrupados em `/api/auth`, `/api/products`, `/api/orders`, `/api/cash-register`, `/api/expenses` e `/api/dashboard`. Exceto login e health checks, a API exige um JWT válido.

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

## Testes

A suíte cobre principalmente domínio, validators e handlers. Cenários de integração que exercitam consultas PostgreSQL usam Testcontainers e precisam do Docker em execução.

```powershell
dotnet test Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
```

Consulte a [estratégia de testes](../../docs/development/testing-strategy.md) para os níveis cobertos e as lacunas atuais.

## Configuração e operação

Não salve connection strings, chaves JWT ou credenciais do R2 no repositório. Use User Secrets localmente e variáveis de ambiente no ambiente hospedado.

- [Configuração](../../docs/development/configuration.md)
- [Modelo de domínio](../../docs/product/domain-model.md)
- [Publicação da API](../../docs/operations/deployment.md)
- [Backup e recuperação](../../docs/operations/backup-and-restore.md)
- [Segurança do login e do caixa](../../docs/operations/login-and-cash-security.md)
