# Repository Pattern para Abstração de Persistência de Aggregates

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Padrão de Acesso a Dados para Operações de Escrita

## Contexto e Problema

Em arquiteturas DDD, Aggregate Roots são fronteiras de consistência que encapsulam regras de negócio. A camada de domínio não deve depender de detalhes de infraestrutura (Entity Framework, SQL, APIs externas). O Repository Pattern fornece uma abstração de "coleção em memória" para persistência.

A estrutura do código mostra implementação do padrão:

```
Features/Products/
├── Domain/
│   ├── Product.cs               # Aggregate Root
│   └── IProductRepository.cs    # Interface (abstração)
│
└── Infrastructure/
    └── ProductRepository.cs     # Implementação (EF Core)
```

**Problema:** Como isolar o domínio de detalhes de persistência mantendo testabilidade e permitindo múltiplas implementações?

## Opções Consideradas

* **Acesso direto ao DbContext** - Domain injeta `ApplicationDbContext` diretamente
* **Generic Repository** - Um repositório genérico `IRepository<T>` para todas entidades
* **Repository por Aggregate** - Uma interface específica por Aggregate Root
* **Especificação Pattern** - Repositories + Specification objects para queries complexas

## Decisão

**Escolhida:** "Repository por Aggregate", porque:

1. **Abstração Direcionada:** Interface expõe apenas operações relevantes para aquele aggregate
2. **Testabilidade:** Fácil mockar `IProductRepository` em testes unitários
3. **Domínio Isolado:** `Domain/` não conhece Entity Framework ou SQL
4. **Flexibilidade:** Implementação pode mudar (EF Core → Dapper → MongoDB) sem afetar domínio
5. **Clareza:** `IOrderRepository.GetByIdAsync()` é mais expressivo que `IRepository<Order>.GetById()`

### Implementação Técnica

**Interface do Repository (Domain Layer):**

```csharp
// Features/Products/Domain/IProductRepository.cs
namespace Devlivery.Features.Products.Domain;

/// <summary>
/// Repository interface for Product aggregate.
/// Provides abstraction for product persistence operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets multiple products by their IDs.
    /// Used when creating orders to validate and fetch products.
    /// </summary>
    Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    Task AddAsync(Product product, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    void Update(Product product);

    /// <summary>
    /// Removes a product from the database.
    /// </summary>
    void Remove(Product product);
}
```

**Implementação (Infrastructure Layer):**

```csharp
// Features/Products/Infrastructure/ProductRepository.cs
using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Infrastructure;

/// <summary>
/// Repository for Product aggregate.
/// Handles write operations and complex queries for Products.
/// </summary>
public sealed class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Products.FindAsync([id], ct);
    }

    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()  // Read-only para performance
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await dbContext.Products.AddAsync(product, ct);
    }

    public void Update(Product product)
    {
        dbContext.Products.Update(product);
    }

    public void Remove(Product product)
    {
        dbContext.Products.Remove(product);
    }
}
```

**Registro de Dependência (Feature Bootstrap):**

```csharp
// Features/Products/ProductFeature.cs
public static IServiceCollection AddProductFeature(this IServiceCollection services)
{
    // Registra implementação concreta para interface
    services.AddScoped<IProductRepository, ProductRepository>();
    
    // Handlers são registrados automaticamente via Mediator
    return services;
}
```

**Uso em Command Handler:**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository productRepository,  // ← Injeção da INTERFACE
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(...)
    {
        var product = new Product(...);
        
        // Usa abstração — handler não sabe que é EF Core
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**Estrutura Completa de Repositórios no Sistema:**

```
Features/
├── Products/
│   ├── Domain/IProductRepository.cs
│   └── Infrastructure/ProductRepository.cs
│
├── Orders/
│   ├── Domain/IOrderRepository.cs
│   └── Infrastructure/OrderRepository.cs
│
└── CashRegister/
    ├── Infrastructure/
    │   ├── ICashSessionRepository.cs
    │   └── CashSessionRepository.cs
```

### Consequências

* ✅ **Bom:** Domínio testável sem banco de dados (mocks de repositories)
* ✅ **Bom:** Implementação de persistência pode mudar sem afetar lógica de negócio
* ✅ **Bom:** Interface documenta operações possíveis no aggregate
* ✅ **Bom:** Preparado para múltiplas implementações (ex: InMemoryRepository para testes)
* ⚠️ **Neutro:** Queries de leitura NÃO usam repositories (injetam DbContext diretamente — ver ADR-005 CQRS)
* ⚠️ **Ruim:** Mais código (interface + implementação) comparado a acesso direto
* ⚠️ **Ruim:** Pode criar abstrações desnecessárias se aggregate é trivial

### Assimetria com Queries (Por Design)

**Commands (Write Side):**
```csharp
public sealed class CreateProductHandler(
    IProductRepository productRepository  // ← Interface abstrata
) { }
```

**Queries (Read Side):**
```csharp
public sealed class GetAllProductsHandler(
    ApplicationDbContext dbContext  // ← Implementação concreta
) { }
```

**Razão:** Queries são otimizadas por feature, não precisam trocar de implementação. Commands operam em Aggregates que podem ter lógica complexa de persistência (ex: salvar Order + OrderItems atomicamente).

### Regras de Design

1. **Repository opera em Aggregate Roots:**
   - ✅ `IOrderRepository` (Order é aggregate root)
   - ❌ `IOrderItemRepository` (OrderItem é parte do aggregate Order)

2. **Métodos do Repository são orientados a negócio:**
   - ✅ `GetActiveOrdersForUser(Guid userId)`
   - ❌ `ExecuteRawSql(string sql)` (detalhes de infraestrutura vazam)

3. **Repository não retorna IQueryable:**
   - ❌ `IQueryable<Product> GetAll()` (expõe LINQ que pode gerar SQL inesperado)
   - ✅ `Task<List<Product>> GetByIdsAsync(List<Guid> ids)` (contrato claro)

4. **UnitOfWork gerencia transação, Repository gerencia coleção:**
   - Repository: `AddAsync()`, `Update()`, `Remove()`
   - UnitOfWork: `SaveChangesAsync()` (commit atômico)

### Teste Unitário com Mock

```csharp
[Fact]
public async Task Handle_Should_Create_Product_Successfully()
{
    // Arrange
    var productRepository = Substitute.For<IProductRepository>();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    var tenantAccessor = fixture.CreateTenantAccessorMock();
    
    var handler = new CreateProductHandler(productRepository, unitOfWork, tenantAccessor);
    var command = new CreateProductCommand(...);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.ShouldBeTrue();
    await productRepository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
}
```

**Princípio:** "Repositories are collection-like abstractions for Aggregate Roots. The domain shouldn't know about databases."

### Referências

- Martin Fowler: [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- Eric Evans: DDD — Repositories são parte do modelo tático
