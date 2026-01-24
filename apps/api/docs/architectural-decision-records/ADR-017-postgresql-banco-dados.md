# PostgreSQL como Banco de Dados Principal

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Persistência de Dados

## Contexto e Problema

Aplicações precisam de um banco de dados relacional robusto para persistir dados transacionais com garantias ACID. A escolha do SGBD impacta performance, escalabilidade, custos operacionais e ecossistema de ferramentas disponíveis.

A estrutura do repositório revela esta decisão através da organização:

```
docker-compose.yml
└── postgres:                        # Container PostgreSQL

src/Devlivery/Devlivery.csproj
└── Npgsql.EntityFrameworkCore.PostgreSQL  # Driver EF Core

Shared/Infrastructure/Persistence/
└── Context/ApplicationDbContext.cs  # Configurado para PostgreSQL
```

**Problema:** Qual SGBD relacional escolher para uma aplicação multi-tenant que precisa de performance, escalabilidade e suporte robusto a operações transacionais?

## Opções Consideradas

* **SQL Server** - Microsoft SQL Server (licenciamento, custo em cloud)
* **MySQL** - MySQL/MariaDB (popular, mas menos recursos avançados)
* **PostgreSQL** - PostgreSQL (open-source, robusto, rico em features)
* **SQLite** - SQLite (apenas para desenvolvimento, não produção)

## Decisão

**Escolhida:** "PostgreSQL", porque:

1. Open-source: sem custos de licenciamento
2. Robusto: suporta operações transacionais complexas com garantias ACID
3. Performance: excelente performance para cargas de trabalho variadas
4. Features avançadas: JSON, arrays, full-text search, extensões
5. Ecossistema: amplamente suportado por ferramentas e bibliotecas .NET
6. Cloud-ready: suportado por principais provedores (AWS RDS, Azure Database, etc.)

### Implementação Técnica

A decisão se materializa em:

**Driver EF Core:**
```csharp
// Devlivery.csproj
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
```

**Configuração do DbContext:**
```csharp
// Shared/Infrastructure/Persistence/DatabaseFeature.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions
            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            .EnableRetryOnFailure(maxRetryCount: 3)));
```

**Connection String:**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=devlivery;Username=postgres;Password=postgres"
  }
}
```

**Docker Compose (Desenvolvimento):**
```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:latest
    container_name: devlivery-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: devlivery
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql
```

**Migrations:**
```bash
# Makefile
db-add:
    dotnet ef migrations add v$(V) -p src/Devlivery -o Shared/Infrastructure/Persistence/Migrations -c ApplicationDbContext

db-update:
    dotnet ef database update -p src/Devlivery -c ApplicationDbContext
```

**Testcontainers (Testes):**
```csharp
// test/Devlivery.Tests/Common/BaseWebApplicationFactory.cs
private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
    .WithImage("postgres:latest")
    .WithDatabase("devlivery")
    .Build();
```

**Features Utilizadas:**
- **Query Filters:** Filtros automáticos por `EstablishmentId` (multi-tenancy)
- **Transactions:** Suporte completo a transações ACID
- **JSON Support:** Suporte nativo a colunas JSON (se necessário)
- **Full-Text Search:** Suporte a busca textual (se necessário)
- **Extensions:** Extensões como `pg_trgm`, `uuid-ossp` (se necessário)

### Consequências

* ✅ **Bom:** Open-source: sem custos de licenciamento
* ✅ **Bom:** Robusto: suporta operações transacionais complexas
* ✅ **Bom:** Performance: excelente para cargas de trabalho variadas
* ✅ **Bom:** Features avançadas: JSON, arrays, full-text search
* ✅ **Bom:** Ecossistema: amplamente suportado por ferramentas .NET
* ✅ **Bom:** Cloud-ready: suportado por principais provedores
* ⚠️ **Neutro:** Requer conhecimento de SQL e PostgreSQL (mas bem documentado)
* ⚠️ **Ruim:** Pode ser mais complexo que SQL Server para desenvolvedores .NET acostumados com SQL Server
* ⚠️ **Ruim:** Algumas features específicas do SQL Server não estão disponíveis (geralmente não são necessárias)

