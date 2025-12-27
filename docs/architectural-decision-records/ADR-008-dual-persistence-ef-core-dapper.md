# Dual Persistence: Entity Framework Core + Dapper

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Persistência de Dados

## Contexto e Problema

Entity Framework Core é excelente para operações de escrita (writes) e queries simples, mas pode ter overhead de performance em queries complexas com múltiplos joins, agregações e subconsultas. Dapper oferece performance superior para queries SQL raw, mas requer mais código manual.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/
└── Infrastructure/ProductRepository.cs  # EF Core para writes

Features/Dashboard/Queries/
├── GetExpenseSummary/
│   └── GetExpenseSummaryHandler.cs      # Dapper para queries complexas
└── GetExpensesByCategory/
    └── GetExpensesByCategoryHandler.cs  # Dapper para agregações

Shared/Infrastructure/Persistence/
├── Factory/DbConnectionFactory.cs       # Factory para Dapper
└── Abstractions/IDbConnectionFactory.cs
```

**Problema:** Como otimizar performance de queries complexas (agregações, relatórios) sem sacrificar produtividade em operações de escrita e queries simples?

## Opções Consideradas

* **Apenas EF Core** - Usar EF Core para tudo (simplicidade, mas pode ter overhead em queries complexas)
* **Apenas Dapper** - Usar Dapper para tudo (performance, mas mais código manual)
* **Dual Persistence** - EF Core para writes/queries simples, Dapper para queries complexas (híbrido)
* **EF Core com SQL Raw** - Usar `FromSqlRaw` do EF Core (meio termo, mas ainda via EF Core)

## Decisão

**Escolhida:** "Dual Persistence (EF Core + Dapper)", porque:

1. Otimiza performance: Dapper é mais rápido para queries complexas com múltiplos joins
2. Mantém produtividade: EF Core continua sendo usado para writes e queries simples
3. Flexível: cada operação usa a ferramenta mais adequada
4. Alinha com CQRS: commands (writes) usam EF Core, queries complexas usam Dapper
5. Permite otimizações específicas: queries de relatórios podem ser otimizadas manualmente em SQL

### Implementação Técnica

A decisão se materializa em:

**EF Core para Writes (Commands):**
```csharp
// Features/Products/Infrastructure/ProductRepository.cs
public sealed class ProductRepository(ApplicationDbContext dbContext)
    : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct)
    {
        await dbContext.Products.AddAsync(product, ct);
    }
}

// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public async ValueTask<Result<...>> Handle(...)
{
    var product = new Product(...);
    await repo.AddAsync(product, ct);  // ← EF Core
    await unitOfWork.SaveChangesAsync(ct);
    return Result.Ok(...);
}
```

**Dapper para Queries Complexas:**
```csharp
// Features/Dashboard/Queries/GetExpenseSummary/GetExpenseSummaryHandler.cs
public sealed class GetExpenseSummaryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor)
    : IQueryHandler<GetExpenseSummaryQuery, Result<GetExpenseSummaryResponse>>
{
    public async ValueTask<Result<GetExpenseSummaryResponse>> Handle(...)
    {
        await using var connection = await dbConnectionFactory.CreateConnectionAsync(ct);
        
        var sql = @"
            SELECT 
                COUNT(*) as total_count,
                SUM(amount) as total_amount,
                AVG(amount) as average_amount,
                MIN(due_date) as earliest_due_date,
                MAX(due_date) as latest_due_date
            FROM expenses
            WHERE establishment_id = @EstablishmentId
                AND status = @Status";
        
        var parameters = new DynamicParameters();
        parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
        parameters.Add("Status", query.Status, DbType.String);
        
        var result = await connection.QueryFirstOrDefaultAsync<ExpenseSummaryDto>(
            sql, parameters);
        
        return Result.Ok(new GetExpenseSummaryResponse(result));
    }
}
```

**Factory para Dapper:**
```csharp
// Shared/Infrastructure/Persistence/Factory/DbConnectionFactory.cs
public interface IDbConnectionFactory
{
    ValueTask<DbConnection> CreateConnectionAsync(CancellationToken ct = default);
}

// Registrado no Startup
services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
```

**Padrão de Uso:**
- **Commands (Writes):** Sempre EF Core (tracking, domain events, transactions)
- **Queries Simples:** EF Core (produtividade, type-safety)
- **Queries Complexas (Dashboard, Relatórios):** Dapper (performance, SQL otimizado)

### Consequências

* ✅ **Bom:** Otimiza performance de queries complexas sem sacrificar produtividade
* ✅ **Bom:** Flexível: cada operação usa a ferramenta mais adequada
* ✅ **Bom:** Alinha com CQRS: commands usam EF Core, queries complexas usam Dapper
* ✅ **Bom:** Permite otimizações SQL específicas para relatórios
* ⚠️ **Neutro:** Requer decisão por operação: quando usar EF Core vs Dapper
* ⚠️ **Ruim:** Dapper requer SQL manual e filtros de tenant explícitos (não usa Query Filters)
* ⚠️ **Ruim:** Pode haver inconsistência: algumas queries em EF Core, outras em Dapper
* ⚠️ **Ruim:** Queries Dapper não se beneficiam de domain events ou tracking do EF Core

