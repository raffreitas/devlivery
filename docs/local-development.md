# Desenvolvimento local

## Pré-requisitos

- .NET 10 SDK;
- Node.js 24 e pnpm 10;
- Docker Desktop ou outro daemon Docker compatível;
- certificado HTTPS de desenvolvimento do ASP.NET Core.

Se necessário, confie no certificado local:

```powershell
dotnet dev-certs https --trust
```

## Banco e API

```powershell
cd apps/api
docker compose up -d postgres
dotnet tool restore
dotnet restore Devlivery.slnx
dotnet ef database update --project src/Devlivery --context ApplicationDbContext
dotnet ef database update --project src/Devlivery --context ApplicationIdentityDbContext
dotnet run --project src/Devlivery
```

O perfil de desenvolvimento usa `https://localhost:7141` e `http://localhost:5052`. Em desenvolvimento, a API aplica migrações e executa o seed quando o banco de negócio está vazio.

## Frontend

```powershell
cd apps/web
Copy-Item .env.example .env
pnpm install --frozen-lockfile
pnpm dev
```

Confirme que `VITE_API_URL` corresponde ao endereço HTTPS da API. O navegador pode exigir que o certificado local seja aceito na primeira execução.

## Verificação

```powershell
dotnet test apps/api/Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
pnpm --dir apps/web lint
pnpm --dir apps/web build
git diff --check
```

Os testes HTTP iniciam PostgreSQL com Testcontainers. Falhas de conexão com o Docker devem ser resolvidas antes de interpretar o resultado da suíte.

## Migrações

O repositório mantém migrações separadas para negócio e identidade. Crie a migração no contexto que possui a alteração e revise o SQL gerado antes de aplicar:

```powershell
cd apps/api
dotnet ef migrations add NomeDaMigracao --project src/Devlivery --context ApplicationDbContext --output-dir Infrastructure/Persistence/Migrations
dotnet ef migrations add NomeDaMigracao --project src/Devlivery --context ApplicationIdentityDbContext --output-dir Infrastructure/Identity/Migrations
```

Não coloque credenciais reais em `appsettings*.json`, `.env.example`, logs ou documentação.
