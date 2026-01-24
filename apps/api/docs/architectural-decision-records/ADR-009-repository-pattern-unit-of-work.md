# Repository Pattern com Unit of Work

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Padrão de Design / Persistência de Dados

## Contexto e Problema

Acesso direto ao `DbContext` em handlers cria acoplamento com Entity Framework Core e dificulta testes. Além disso, operações que envolvem múltiplas entidades precisam ser transacionais, e domain events precisam ser disparados após commit bem-sucedido.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/
├── Domain/IProductRepository.cs     # Abstração do repositório
└── Infrastructure/ProductRepository.cs  # Implementação EF Core

Shared/Infrastructure/Persistence/
├── IUnitOfWork.cs                   # Interface Unit of Work
├── UnitOfWork.cs                    # Implementação
└── Interceptors/DispatchDomainEventsInterceptor.cs  # Dispara eventos após SaveChanges
```

**Problema:** Como abstrair acesso a dados, gerenciar transações e garantir que domain events sejam disparados após commit bem-sucedido, sem acoplar handlers ao EF Core?

## Opções Consideradas

* **Acesso Direto ao DbContext** - Handlers usam `DbContext` diretamente (acoplamento, difícil testar)
* **Repository Pattern** - Abstrair acesso a dados via interfaces (testável, mas sem transações)
* **Repository + Unit of Work** - Repository para acesso, Unit of Work para transações e eventos
* **Specification Pattern** - Adicionar camada de especificações (complexidade desnecessária para este contexto)

## Decisão

**Escolhida:** "Repository + Unit of Work", porque:

1. Abstrai acesso a dados: handlers não dependem diretamente do EF Core
2. Facilita testes: repositories podem ser mockados facilmente
3. Gerencia transações: Unit of Work garante atomicidade de operações
4. Dispara domain events: `DispatchDomainEventsInterceptor` garante eventos após commit
5. Alinha com DDD: repositories representam agregações, Unit of Work gerencia consistência

### Implementação Técnica

A decisão se materializa em:

**Repository Interface (Domain):**
```csharp
// Features/Products/Domain/IProductRepository.cs
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Delete(Product product);
}
```

**Repository Implementation (Infrastructure):**
```csharp
// Features/Products/Infrastructure/ProductRepository.cs
public sealed class ProductRepository(ApplicationDbContext dbContext)
    : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await dbContext.Products.AddAsync(product, ct);
    }

    // ... outros métodos
}
```

**Unit of Work:**
```csharp
// Shared/Infrastructure/Persistence/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

// Shared/Infrastructure/Persistence/UnitOfWork.cs
public sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // DispatchDomainEventsInterceptor dispara eventos automaticamente
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

**Domain Events Interceptor:**
```csharp
// Shared/Infrastructure/Persistence/Interceptors/DispatchDomainEventsInterceptor.cs
// Intercepta SaveChanges e dispara domain events após commit bem-sucedido
```

**Uso em Handler:**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository repo,        // ← Repository (abstração)
    IUnitOfWork unitOfWork,         // ← Unit of Work (transações)
    ITenantAccessor tenantAccessor)
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(...)
    {
        var product = new Product(command.Name, tenantAccessor.Tenant.Id);
        product.AddDomainEvent(new ProductCreatedEvent(product.Id));  // ← Domain event
        
        await repo.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);  // ← Commit + dispara eventos
        
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**Registro:**
```csharp
// Features/Products/ProductFeature.cs
services.AddScoped<IProductRepository, ProductRepository>();

// Shared/Infrastructure/Persistence/DatabaseFeature.cs
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Consequências

* ✅ **Bom:** Abstrai acesso a dados, facilitando testes e troca de implementação
* ✅ **Bom:** Gerencia transações automaticamente via Unit of Work
* ✅ **Bom:** Dispara domain events após commit bem-sucedido
* ✅ **Bom:** Alinha com DDD: repositories representam agregações
* ✅ **Bom:** Facilita testes: repositories podem ser mockados
* ⚠️ **Neutro:** Adiciona uma camada de abstração (trade-off por testabilidade)
* ⚠️ **Ruim:** Pode ser tentador criar repositories genéricos (deve ser evitado, cada agregação tem seu repository)
* ⚠️ **Ruim:** Requer disciplina: sempre usar `IUnitOfWork.SaveChangesAsync()`, nunca `DbContext.SaveChanges()` diretamente

