# Multi-tenancy via Row-Level Security (EstablishmentId)

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Padrão de Design / Segurança e Isolamento de Dados

## Contexto e Problema

Aplicações multi-tenant precisam isolar dados entre diferentes estabelecimentos (tenants) para garantir segurança e privacidade. Sem isolamento adequado, um estabelecimento pode acessar dados de outro, causando violações de segurança e problemas legais.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/Domain/Product.cs
├── public Guid EstablishmentId { get; private set; }  // ← Em todas as entidades

Shared/Infrastructure/
├── Tenancy/
│   ├── TenantAccessor.cs            # Acessa tenant do JWT
│   ├── TenantLocator.cs
│   └── Middleware/TenantRegisterMiddleware.cs
└── Persistence/Context/ApplicationDbContext.cs
    └── ApplyQueryFilters()           # Filtros automáticos por EstablishmentId
```

**Problema:** Como garantir isolamento de dados entre estabelecimentos (tenants) de forma automática e segura, sem exigir que cada query manualmente filtre por tenant?

## Opções Consideradas

* **Database per Tenant** - Um banco de dados por tenant (complexidade operacional)
* **Schema per Tenant** - Um schema por tenant no mesmo banco (complexidade de migração)
* **Row-Level Security (RLS) no Banco** - Filtros no PostgreSQL (dependência de SGBD)
* **Application-Level Filtering (EstablishmentId)** - Filtros automáticos via EF Core Query Filters
* **Manual Filtering** - Cada query filtra manualmente (propenso a erros)

## Decisão

**Escolhida:** "Application-Level Filtering (EstablishmentId) com EF Core Query Filters", porque:

1. Simples de implementar: todas as entidades têm `EstablishmentId`, filtros aplicados automaticamente
2. Transparente: desenvolvedores não precisam lembrar de filtrar manualmente
3. Seguro: filtros são aplicados em todas as queries, prevenindo vazamento de dados
4. Flexível: permite queries cross-tenant quando necessário (desabilitando filtros)
5. Independente de SGBD: funciona com qualquer banco suportado pelo EF Core

### Implementação Técnica

A decisão se materializa em:

**Entity com EstablishmentId:**
```csharp
// Features/Products/Domain/Product.cs
public sealed class Product : Entity
{
    public Guid EstablishmentId { get; private set; }  // ← Obrigatório
    public string Name { get; private set; }
    // ...
    
    public Product(string name, Guid establishmentId)
    {
        Name = name;
        EstablishmentId = establishmentId;  // ← Sempre fornecido no construtor
    }
}
```

**Query Filters Automáticos:**
```csharp
// Shared/Infrastructure/Persistence/Context/ApplicationDbContext.cs
private void ApplyQueryFilters(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().HasQueryFilter(
        x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    modelBuilder.Entity<Order>().HasQueryFilter(
        x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    // ... todas as entidades multi-tenant
}
```

**Tenant Accessor (do JWT):**
```csharp
// Shared/Infrastructure/Tenancy/TenantAccessor.cs
public interface ITenantAccessor
{
    Tenant Tenant { get; }
}

// Middleware extrai EstablishmentId do JWT claim e registra no TenantAccessor
```

**Uso em Handlers:**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository repo,
    ITenantAccessor tenantAccessor)  // ← Injeta tenant accessor
    : ICommandHandler<...>
{
    public async ValueTask<Result<...>> Handle(...)
    {
        var product = new Product(
            command.Name,
            tenantAccessor.Tenant.Id);  // ← Usa tenant do JWT
        await repo.AddAsync(product, ct);
        // ...
    }
}
```

**Queries Automáticas (sem filtro manual):**
```csharp
// Features/Products/Queries/GetAllProducts/GetAllProductsHandler.cs
public async ValueTask<Result<...>> Handle(...)
{
    // Query filter aplicado automaticamente!
    var products = await repo.GetAllAsync(ct);  // ← Só retorna produtos do tenant atual
    return Result.Ok(products);
}
```

**Queries com Dapper (filtro manual necessário):**
```csharp
// Features/Dashboard/Queries/GetExpenseSummary/GetExpenseSummaryHandler.cs
var sql = @"
    SELECT SUM(amount) as total
    FROM expenses
    WHERE establishment_id = @EstablishmentId";  // ← Filtro manual em SQL

parameters.Add("EstablishmentId", tenantAccessor.Tenant.Id, DbType.Guid);
```

### Consequências

* ✅ **Bom:** Isolamento automático de dados, prevenindo vazamento entre tenants
* ✅ **Bom:** Transparente para desenvolvedores: não precisam lembrar de filtrar manualmente
* ✅ **Bom:** Seguro por padrão: todas as queries EF Core são filtradas automaticamente
* ✅ **Bom:** Flexível: permite desabilitar filtros quando necessário (queries administrativas)
* ✅ **Bom:** Independente de SGBD: funciona com qualquer banco suportado pelo EF Core
* ⚠️ **Neutro:** Queries com Dapper precisam filtrar manualmente (trade-off por performance)
* ⚠️ **Ruim:** Requer disciplina: todas as entidades multi-tenant devem ter `EstablishmentId`
* ⚠️ **Ruim:** Queries cross-tenant requerem desabilitar filtros explicitamente (pode ser esquecido)

