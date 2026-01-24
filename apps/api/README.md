# Devlivery

API Web para gestão de features relacionadas a entregas — monólito modular organizado por features (vertical slice).

**Visão geral**
- Projeto ASP.NET Core Web API (solução: Devlivery.slnx).
- Organização por features em `src/Devlivery/Features` com testes em `test/Devlivery.Tests`.
- Arquitetura orientada a features/vertical slice e princípios DDD tático (ver ADRs em `docs/architectural-decision-records`).

**Principais tecnologias**
- .NET 10 / ASP.NET Core
- Entity Framework Core, Dapper (persistência)
- Minimal APIs / Features por responsabilidade
- Docker / docker-compose para ambiente local

**Pré-requisitos**
- .NET SDK 10.0
- Docker (opcional para execução em containers)

**Como rodar localmente**
1. Restaurar dependências e compilar:

```bash
dotnet restore
dotnet build
```

2. Executar a API (diretamente):

```bash
dotnet run --project src/Devlivery
```

3. Ou via Docker (docker-compose):

```bash
docker compose up --build -d
```

**Testes**
- Executar testes unitários/integrados:

```bash
dotnet test test/Devlivery.Tests
```

**Estrutura importante**
- `src/Devlivery` — código da aplicação (Features, Shared, Infrastructure).
- `test/Devlivery.Tests` — suíte de testes.
- `docs/architectural-decision-records` — ADRs que documentam decisões arquiteturais.

**Observações de desenvolvimento**
- O projeto segue ADRs e padrões internos (vertical slice, mediator, repository quando aplicável).
- Configurações de ambiente encontram-se nos `appsettings*.json` no projeto `src/Devlivery`.
