# Multi-Tenancy com Isolamento por Establishment (Row-Level Security)

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Estratégia de Isolamento de Dados entre Clientes

## Contexto e Problema

O sistema Devlivery serve múltiplos estabelecimentos (restaurantes, cafés, lojas) que não devem ter acesso aos dados uns dos outros. Multi-tenancy pode ser implementado de várias formas, cada uma com trade-offs de isolamento, custo e complexidade.

A estrutura do código revela isolamento via `EstablishmentId`:

```
Shared/Infrastructure/Tenancy/
├── TenancyFeature.cs            # Bootstrap
├── Tenant.cs                    # Model (EstablishmentId)
├── TenantAccessor.cs            # Acesso ao tenant atual
├── TenantLocator.cs             # Resolve tenant do HTTP request
├── Middleware/
│   └── TenantMiddleware.cs      # Intercepta requests
└── Behaviors/
    └── DomainEventTenantBehavior.cs

Shared/Infrastructure/Persistence/Context/ApplicationDbContext.cs
└── ApplyQueryFilters()          # Query Filters por EstablishmentId
```

**Problema:** Como garantir isolamento de dados entre estabelecimentos sem complexidade excessiva de infraestrutura?

## Opções Consideradas

* **Database per Tenant** - Um banco de dados PostgreSQL para cada establishment
* **Schema per Tenant** - Um schema PostgreSQL separado por establishment no mesmo banco
* **Row-Level Security (Discriminador)** - Coluna `EstablishmentId` em todas as tabelas + query filters
* **Tenant Identifier no Connection String** - Alternar databases dinamicamente via connection strings

## Decisão

**Escolhida:** "Row-Level Security com Query Filters", porque:

1. **Simplicidade Operacional:** Um único banco de dados, uma migration, um backup
2. **Custo Reduzido:** Não requer provisionamento de infra por tenant
3. **Isolamento Suficiente:** Adequado para SaaS B2B onde tenants não são adversariais
4. **Performance:** Query Filters aplicados automaticamente pelo EF Core
5. **Escalabilidade Inicial:** Centenas de tenants podem coexistir no mesmo DB

### Implementação Técnica

**Modelo de Tenant:**

```csharp
// Shared/Infrastructure/Tenancy/Tenant.cs
public sealed class Tenant(Guid id)
{
    public Guid Id { get; } = id;
}
```

**Tenant Accessor (Scoped Service):**

```csharp
// Shared/Infrastructure/Tenancy/TenantAccessor.cs
public interface ITenantAccessor
{
    Tenant Tenant { get; }
}

public sealed class TenantAccessor : ITenantAccessor
{
    public Tenant Tenant { get; private set; } = null!;

    public void SetTenant(Tenant tenant)
    {
        Tenant = tenant;
    }
}
```

**Tenant Locator (Resolve do Request):**

```csharp
// Shared/Infrastructure/Tenancy/TenantLocator.cs
public interface ITenantLocator
{
    Task<Tenant> LocateTenantAsync(HttpContext httpContext);
}

public sealed class TenantLocator : ITenantLocator
{
    public async Task<Tenant> LocateTenantAsync(HttpContext httpContext)
    {
        // Estratégia 1: Claim do JWT
        var userClaims = httpContext.User;
        var establishmentIdClaim = userClaims.FindFirst("EstablishmentId")?.Value;
        
        if (Guid.TryParse(establishmentIdClaim, out var establishmentId))
        {
            return new Tenant(establishmentId);
        }
        
        // Estratégia 2: Header HTTP (para APIs administrativas)
        if (httpContext.Request.Headers.TryGetValue("X-Establishment-Id", out var headerValue))
        {
            if (Guid.TryParse(headerValue, out var tenantId))
                return new Tenant(tenantId);
        }
        
        throw new UnauthorizedAccessException("Tenant not found in request");
    }
}
```

**Middleware (Pipeline ASP.NET Core):**

```csharp
// Shared/Infrastructure/Tenancy/Middleware/TenantMiddleware.cs
public sealed class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantLocator tenantLocator,
        ITenantAccessor tenantAccessor)
    {
        var tenant = await tenantLocator.LocateTenantAsync(context);
        
        // Define tenant no scope atual
        if (tenantAccessor is TenantAccessor accessor)
        {
            accessor.SetTenant(tenant);
        }
        
        await next(context);
    }
}
```

**Query Filters Globais (EF Core):**

```csharp
// Shared/Infrastructure/Persistence/Context/ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Configurações...
    
    ApplyQueryFilters(modelBuilder);
}

private void ApplyQueryFilters(ModelBuilder modelBuilder)
{
    // Filtra TODAS as queries automaticamente
    modelBuilder.Entity<User>()
        .HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    
    modelBuilder.Entity<Product>()
        .HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    
    modelBuilder.Entity<Order>()
        .HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    
    modelBuilder.Entity<OrderItem>()
        .HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    
    modelBuilder.Entity<CashSession>()
        .HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
    
    modelBuilder.Entity<CashDeposit>()
        .HasQueryFilter(x => x.EstablishmentId == tenantAccessor.Tenant.Id);
}
```

**Entidades com EstablishmentId:**

```csharp
// Features/Products/Domain/Product.cs
public sealed class Product : Entity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    public Guid EstablishmentId { get; private set; }  // ← Discriminador de Tenant
    
    public Product(string name, decimal price, Guid establishmentId)
    {
        Name = name;
        Price = price;
        EstablishmentId = establishmentId;  // Definido no construtor
    }
}
```

**Uso em Command Handler:**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor  // ← Injetado
) : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(...)
    {
        // Tenant é automaticamente injetado na entidade
        var product = new Product(
            command.Name,
            command.Description,
            command.Price,
            command.Category,
            command.Available,
            tenantAccessor.Tenant.Id  // ← EstablishmentId do tenant atual
        );
        
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**Estrutura de Tabelas (PostgreSQL):**

```sql
CREATE TABLE products (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    establishment_id UUID NOT NULL,  -- Discriminador
    ...
);

CREATE INDEX idx_products_establishment ON products(establishment_id);
```

**Comportamento em Runtime:**

```csharp
// Request 1: User do Establishment A
// JWT Claim: { "EstablishmentId": "aaaa-1111-..." }

var products = await dbContext.Products.ToListAsync();
// SQL gerado automaticamente:
// SELECT * FROM products WHERE establishment_id = 'aaaa-1111-...'

// Request 2: User do Establishment B  
// JWT Claim: { "EstablishmentId": "bbbb-2222-..." }

var products = await dbContext.Products.ToListAsync();
// SQL gerado:
// SELECT * FROM products WHERE establishment_id = 'bbbb-2222-...'
```

### Consequências

* ✅ **Bom:** Simplicidade de infraestrutura — um banco, uma migration, um deployment
* ✅ **Bom:** Query Filters protegem contra vazamento acidental de dados
* ✅ **Bom:** Custo reduzido — não requer provisionamento por tenant
* ✅ **Bom:** Backups e restores triviais (todo o sistema em um backup)
* ✅ **Bom:** Fácil agregação cross-tenant para analytics (admin queries)
* ⚠️ **Neutro:** Escalabilidade limitada (centenas de tenants OK, milhares requerem sharding)
* ⚠️ **Ruim:** Isolamento mais fraco que database-per-tenant (bug pode vazar dados)
* ⚠️ **Ruim:** Um tenant com alta carga afeta performance de todos (mitigado por connection pooling)
* ⚠️ **Ruim:** Não atende regulações que exigem isolamento físico (GDPR strict, HIPAA)

### Segurança

**Camadas de Proteção:**

1. **JWT Claims:** Establishment ID é parte do token (validado por middleware)
2. **Query Filters:** EF Core aplica WHERE automaticamente
3. **Testes de Integração:** Validam isolamento entre tenants

```csharp
[Fact]
public async Task GetProducts_Should_Return_Only_Tenant_Products()
{
    // Arrange
    var tenantA = Guid.NewGuid();
    var tenantB = Guid.NewGuid();
    
    await SeedProductForTenant(tenantA, "Product A");
    await SeedProductForTenant(tenantB, "Product B");
    
    // Act (simula request do Tenant A)
    var productsA = await GetProductsForTenant(tenantA);
    
    // Assert
    productsA.ShouldHaveSingleItem();
    productsA.First().Name.ShouldBe("Product A");
}
```

**Ignorar Query Filter (Admin Queries):**

```csharp
// APENAS para operações administrativas (auditoria, analytics)
var allProducts = await dbContext.Products
    .IgnoreQueryFilters()  // ⚠️ Bypass do tenant filter
    .ToListAsync();
```

### Estratégia de Migração Futura

Se necessário, migrar para Database-per-Tenant:

1. Script de export por `EstablishmentId`
2. Criar novo banco por tenant
3. Migrar dados filtrados
4. Atualizar `TenantLocator` para resolver connection string por tenant

**Princípio:** "Choose the simplest multi-tenancy strategy that meets your isolation and scale requirements. Evolve when pain justifies complexity."

### Referências

- Microsoft: [Multi-Tenancy in ASP.NET Core](https://docs.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)
- EF Core: [Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
