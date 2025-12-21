# Entity Framework Core com PostgreSQL e Dapper para Otimização de Queries

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Estratégia de Persistência e Otimização de Leitura

## Contexto e Problema

Sistemas podem usar um único ORM para todas operações de dados, ou combinar ferramentas especializadas. Entity Framework Core oferece produtividade e rastreamento de mudanças para operações de escrita, mas pode gerar queries subótimas para leituras complexas. Dapper oferece controle fino sobre SQL mas requer escrever queries manualmente.

A configuração do projeto revela uma estratégia híbrida:

```xml
<!-- Devlivery.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.1"/>
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0"/>
<PackageReference Include="Dapper" Version="2.1.66"/>
<PackageReference Include="EFCore.NamingConventions" Version="10.0.0-rc.2"/>
```

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:latest
    environment:
      POSTGRES_DB: devlivery
```

**Problema:** Como balancear produtividade de desenvolvimento com performance de queries otimizadas?

## Opções Consideradas

* **EF Core exclusivamente** - Toda persistência via Entity Framework Core
* **Dapper exclusivamente** - Toda persistência via micro-ORM com SQL manual
* **Híbrido (EF Core + Dapper)** - EF Core para writes, Dapper para reads complexas
* **PostgreSQL raw ADO.NET** - Controle total via NpgsqlConnection direto

## Decisão

**Escolhida:** "Híbrido (EF Core para writes, Dapper disponível para reads)", porque:

1. **Produtividade em Writes:** EF Core tracked entities facilitam updates e validações
2. **Performance em Reads:** Dapper disponível para queries complexas que EF geraria SQL subótimo
3. **Melhor dos Dois Mundos:** Change tracking quando necessário, SQL otimizado quando crítico
4. **Migrations Unificadas:** EF Core Migrations gerenciam schema, Dapper consome
5. **Flexibilidade:** Times podem escolher a ferramenta adequada por caso de uso

### Implementação Técnica

**1. Entity Framework Core (Write Side):**

```csharp
// Shared/Infrastructure/Persistence/Context/ApplicationDbContext.cs
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ITenantAccessor tenantAccessor)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações de entidades
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        
        // Convenção PostgreSQL (snake_case)
        modelBuilder.UseSnakeCaseNamingConvention();
        
        // Query Filters (Multi-tenancy)
        ApplyQueryFilters(modelBuilder);
    }
}
```

**Exemplo de Uso (Command Handler):**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(...)
    {
        // EF Core rastreia mudanças automaticamente
        var product = new Product(...);
        
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);  // EF Core SaveChanges
        
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**2. Dapper (Read Side Otimizado - Opcional):**

```csharp
// Exemplo de Query Handler com Dapper (se necessário)
using Dapper;
using Npgsql;

public sealed class GetOrderReportHandler(
    IConfiguration configuration
) : IQueryHandler<GetOrderReportQuery, Result<OrderReportDto>>
{
    public async ValueTask<Result<OrderReportDto>> Handle(...)
    {
        // SQL otimizado manualmente para performance
        const string sql = """
            SELECT 
                o.id,
                o.total_amount,
                COUNT(oi.id) as item_count,
                STRING_AGG(p.name, ', ') as product_names
            FROM orders o
            INNER JOIN order_items oi ON oi.order_id = o.id
            INNER JOIN products p ON p.id = oi.product_id
            WHERE o.establishment_id = @EstablishmentId
                AND o.created_at >= @StartDate
            GROUP BY o.id, o.total_amount
            ORDER BY o.created_at DESC
            """;

        await using var connection = new NpgsqlConnection(
            configuration.GetConnectionString("DefaultConnection"));
        
        var results = await connection.QueryAsync<OrderReportDto>(
            sql, 
            new { EstablishmentId = query.EstablishmentId, StartDate = query.StartDate }
        );
        
        return Result.Ok(results.ToList());
    }
}
```

**3. PostgreSQL como Banco Principal:**

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=devlivery;Username=postgres;Password=postgres"
  }
}
```

**Registro no DI:**

```csharp
// Startup.cs
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        }
    );
    
    // Convenção snake_case para PostgreSQL
    options.UseSnakeCaseNamingConvention();
    
    // Interceptors (Domain Events, Auditing)
    options.AddInterceptors(
        serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>()
    );
});
```

**4. Migrations (EF Core):**

```bash
# Makefile commands
make db-add V=001          # Adicionar migration
make db-update             # Aplicar migrations
make db-remove             # Remover última migration
```

```csharp
// Migrations geradas em:
// Shared/Infrastructure/Persistence/Migrations/
// Shared/Infrastructure/Identity/Migrations/
```

**Schema PostgreSQL Gerado:**

```sql
-- Exemplo: tabela products
CREATE TABLE products (
    id UUID NOT NULL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(10,2) NOT NULL,
    category VARCHAR(100),
    available BOOLEAN NOT NULL DEFAULT TRUE,
    establishment_id UUID NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE
);

CREATE INDEX ix_products_establishment_id ON products(establishment_id);
CREATE INDEX ix_products_category ON products(category);
```

**Convenções Adotadas:**

| Aspecto              | Convenção                          | Implementação                    |
|----------------------|------------------------------------|----------------------------------|
| **Naming**           | snake_case                         | `EFCore.NamingConventions`       |
| **Primary Keys**     | UUID (Guid.CreateVersion7)         | `Entity.Id`                      |
| **Timestamps**       | UTC                                | `DateTimeExtensions`             |
| **Soft Delete**      | Não implementado (hard delete)     | N/A                              |
| **Auditing**         | CreatedAt, UpdatedAt (se aplicável)| Interceptors (futuro)            |

### Consequências

* ✅ **Bom:** EF Core fornece produtividade para writes (change tracking, validations)
* ✅ **Bom:** Dapper disponível para queries críticas de performance
* ✅ **Bom:** PostgreSQL oferece features avançadas (JSONB, Full-Text Search, CTEs)
* ✅ **Bom:** Migrations versionadas facilitam evolução de schema
* ✅ **Bom:** Testcontainers permite testes de integração com PostgreSQL real
* ⚠️ **Neutro:** Curva de aprendizado — devs precisam saber quando usar cada ferramenta
* ⚠️ **Neutro:** Dois paradigmas de acesso a dados (ORM vs micro-ORM)
* ⚠️ **Ruim:** Risco de N+1 queries se EF Core mal utilizado (mitigado por code review)
* ⚠️ **Ruim:** Dapper queries não são type-safe (compilador não valida SQL)

### Diretrizes de Uso

**Use EF Core para:**
- ✅ Operações de escrita (Create, Update, Delete)
- ✅ Queries simples de leitura (Get by ID, List All)
- ✅ Operações que requerem change tracking
- ✅ Operações transacionais complexas

**Use Dapper para:**
- ✅ Relatórios complexos com múltiplos JOINs e agregações
- ✅ Queries com performance crítica (milhares de registros)
- ✅ Stored procedures ou funções PostgreSQL customizadas
- ✅ Queries que EF Core geraria SQL subótimo

**Evite:**
- ❌ Misturar EF Core e Dapper na mesma transação (complexidade desnecessária)
- ❌ Usar Dapper para writes simples (perde validações de domínio)
- ❌ Lazy loading no EF Core (sempre use eager loading explícito)

### Features PostgreSQL Aproveitadas

**1. UUIDs Ordenados (Version 7):**
```csharp
public Guid Id { get; protected init; } = Guid.CreateVersion7();
// Gera UUIDs com ordenação temporal nativa no PostgreSQL
```

**2. JSONB (Futuro):**
```sql
-- Para dados semi-estruturados (ex: metadata de produtos)
ALTER TABLE products ADD COLUMN metadata JSONB;
```

**3. Full-Text Search (Futuro):**
```sql
-- Para busca textual eficiente
CREATE INDEX idx_products_name_fts ON products USING GIN(to_tsvector('portuguese', name));
```

**4. Connection Pooling (Npgsql):**
```csharp
// Configurado automaticamente por Npgsql
options.UseNpgsql(..., npgsqlOptions => 
{
    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    npgsqlOptions.CommandTimeout(30);
});
```

### Testes de Integração

```csharp
// test/Devlivery.Tests/Common/BaseWebApplicationFactory.cs
public class BaseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("devlivery_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Substitui connection string por Testcontainer
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }
}
```

**Princípio:** "Use the right tool for the job. EF Core for writes, Dapper for complex reads. Let PostgreSQL handle what it does best."

### Referências

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [PostgreSQL Best Practices](https://wiki.postgresql.org/wiki/Don't_Do_This)
