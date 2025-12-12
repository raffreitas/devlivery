# Exemplos de Implementação - Melhorias Arquiteturais

Este documento contém exemplos práticos de como implementar as melhorias sugeridas na revisão arquitetural.

---

## 1. Criar Interfaces para Repositories (Prioridade ALTA)

### 1.1 Interface para OrderRepository

**Arquivo:** `src/Devlivery/Features/Orders/Infrastructure/IOrderRepository.cs`

```csharp
using Devlivery.Features.Orders.Domain;

namespace Devlivery.Features.Orders.Infrastructure;

/// <summary>
/// Repository interface for Order aggregate.
/// Provides abstraction for order persistence operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by ID, including its items.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds a new order to the database.
    /// </summary>
    Task AddAsync(Order order, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    void Update(Order order);

    /// <summary>
    /// Removes an order from the database.
    /// </summary>
    void Remove(Order order);

    /// <summary>
    /// Gets all orders in a specific time period with optional filters.
    /// Used for business analytics and reporting.
    /// </summary>
    Task<List<Order>> GetOrdersInPeriodAsync(
        DateTime start,
        DateTime end,
        CancellationToken ct = default);
}
```

**Atualizar implementação:**

```csharp
// src/Devlivery/Features/Orders/Infrastructure/OrderRepository.cs
public sealed class OrderRepository : IOrderRepository
{
    // ... implementação existente permanece igual
}
```

---

### 1.2 Interface para ProductRepository

**Arquivo:** `src/Devlivery/Features/Products/Infrastructure/IProductRepository.cs`

```csharp
using Devlivery.Features.Products.Domain;

namespace Devlivery.Features.Products.Infrastructure;

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

---

### 1.3 Interface para CashSessionRepository

**Arquivo:** `src/Devlivery/Features/CashRegister/Infrastructure/ICashSessionRepository.cs`

```csharp
using Devlivery.Features.CashRegister.Domain;

namespace Devlivery.Features.CashRegister.Infrastructure;

/// <summary>
/// Repository interface for CashSession aggregate.
/// Provides abstraction for cash session persistence operations.
/// </summary>
public interface ICashSessionRepository
{
    /// <summary>
    /// Gets a cash session by ID, including deposits.
    /// </summary>
    Task<CashSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the currently active (open) cash session.
    /// </summary>
    Task<CashSession?> GetActiveSessionAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new cash session to the database.
    /// </summary>
    Task AddAsync(CashSession session, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing cash session.
    /// </summary>
    void Update(CashSession session);
}
```

---

## 2. Criar Interface para UnitOfWork (Prioridade ALTA)

**Arquivo:** `src/Devlivery/Shared/Infrastructure/Persistence/IUnitOfWork.cs`

```csharp
using Microsoft.EntityFrameworkCore.Storage;

namespace Devlivery.Shared.Infrastructure.Persistence;

/// <summary>
/// Unit of Work interface for managing database transactions.
/// Ensures Domain Events are dispatched via the DispatchDomainEventsInterceptor.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes to the database.
    /// Domain Events are automatically dispatched by the DispatchDomainEventsInterceptor.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// Use this for explicit transaction control when needed.
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
```

**Atualizar implementação:**

```csharp
// src/Devlivery/Shared/Infrastructure/Persistence/UnitOfWork.cs
public sealed class UnitOfWork : IUnitOfWork
{
    // ... implementação existente permanece igual
}
```

---

## 3. Atualizar Registros de DI

### 3.1 OrdersFeature.cs

```csharp
public static IServiceCollection AddOrderFeature(this IServiceCollection services)
{
    // ✅ Registrar interface + implementação
    services.AddScoped<IOrderRepository, OrderRepository>();
    
    // Register Handlers
    services.AddScoped<CreateOrderHandler>();
    // ... resto permanece igual
    return services;
}
```

### 3.2 ProductFeature.cs

```csharp
public static IServiceCollection AddProductFeature(this IServiceCollection services)
{
    // ✅ Registrar interface + implementação
    services.AddScoped<IProductRepository, ProductRepository>();
    
    // Register Handlers
    services.AddScoped<CreateProductHandler>();
    // ... resto permanece igual
    return services;
}
```

### 3.3 CashRegisterFeature.cs

```csharp
public static IServiceCollection AddCashRegisterFeature(this IServiceCollection services)
{
    // ✅ Registrar interface + implementação
    services.AddScoped<ICashSessionRepository, CashSessionRepository>();
    
    // Register Handlers
    services.AddScoped<CreateCashSessionHandler>();
    // ... resto permanece igual
    return services;
}
```

### 3.4 DatabaseFeature.cs

```csharp
public static IServiceCollection AddDatabaseFeature(this IServiceCollection services, IConfiguration configuration)
{
    // ... código existente ...

    // ✅ Registrar interface + implementação
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
}
```

---

## 4. Atualizar Handlers para Usar Interfaces

### 4.1 CreateOrderHandler.cs

**Antes:**
```csharp
public sealed class CreateOrderHandler(
    OrderRepository orderRepository,  // ❌ Classe concreta
    ProductRepository productRepository,
    UnitOfWork unitOfWork)
```

**Depois:**
```csharp
public sealed class CreateOrderHandler(
    IOrderRepository orderRepository,  // ✅ Interface
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
```

### 4.2 Exemplo Completo Atualizado

```csharp
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Features.Products.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;
using FluentResults;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    IOrderRepository orderRepository,      // ✅ Interface
    IProductRepository productRepository,   // ✅ Interface
    IUnitOfWork unitOfWork,                 // ✅ Interface
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateOrderResponse>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        // ... resto da implementação permanece igual
    }
}
```

---

## 5. Migrar Queries para Dapper (Prioridade MÉDIA)

### 5.1 GetAllOrdersHandler com Dapper

**Arquivo:** `src/Devlivery/Features/Orders/Queries/GetAllOrders/GetAllOrdersHandler.cs`

```csharp
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Infrastructure.Persistence.Abstractions;
using Devlivery.Shared.Infrastructure.Tenancy;
using Dapper;
using FluentResults;

namespace Devlivery.Features.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<List<GetAllOrdersResponse>>> HandleAsync(
        GetAllOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var tenantId = tenantAccessor.Tenant.Id;
        
        // Construir query dinâmica baseada em filtros
        var sql = @"
            SELECT 
                o.id,
                o.customer_name,
                o.customer_phone,
                o.delivery_address,
                o.notes,
                o.status,
                o.total,
                o.delivery_fee,
                o.payment_method,
                o.created_at,
                o.updated_at,
                oi.id as item_id,
                oi.product_id,
                oi.quantity,
                oi.unit_price,
                oi.notes as item_notes,
                p.id as product_id,
                p.name as product_name,
                p.description as product_description,
                p.price as product_price,
                p.category as product_category,
                p.available as product_available,
                p.created_at as product_created_at,
                p.updated_at as product_updated_at
            FROM orders o
            INNER JOIN order_items oi ON o.id = oi.order_id
            INNER JOIN products p ON oi.product_id = p.id
            WHERE o.establishment_id = @TenantId
                AND (@StartDate IS NULL OR o.created_at >= @StartDate)
                AND (@EndDate IS NULL OR o.created_at <= @EndDate)
                AND (@PaymentMethod IS NULL OR o.payment_method = @PaymentMethod)
            ORDER BY o.created_at DESC";

        var parameters = new
        {
            TenantId = tenantId,
            StartDate = query.StartDate,
            EndDate = query.EndDate,
            PaymentMethod = !string.IsNullOrWhiteSpace(query.PaymentMethod) 
                && Enum.TryParse<PaymentMethod>(query.PaymentMethod, out var pm) 
                ? pm.ToString() 
                : (string?)null
        };

        var orderDict = new Dictionary<Guid, GetAllOrdersResponse>();
        
        await connection.QueryAsync<OrderRow, OrderItemRow, ProductRow, GetAllOrdersResponse>(
            sql,
            (order, item, product) =>
            {
                if (!orderDict.TryGetValue(order.OrderId, out var orderResponse))
                {
                    orderResponse = new GetAllOrdersResponse(
                        order.OrderId,
                        new List<OrderItemDto>(),
                        order.CustomerName,
                        order.CustomerPhone,
                        order.DeliveryAddress,
                        order.Notes,
                        order.Status,
                        order.Total,
                        order.DeliveryFee,
                        order.PaymentMethod,
                        order.CreatedAt,
                        order.UpdatedAt);
                    
                    orderDict[order.OrderId] = orderResponse;
                }

                var productDto = new ProductDto(
                    product.Id,
                    product.Name,
                    product.Description,
                    product.Price,
                    product.Category,
                    product.Available,
                    product.CreatedAt,
                    product.UpdatedAt);

                var orderItemDto = new OrderItemDto(
                    productDto,
                    item.Quantity,
                    item.Notes);

                orderResponse.Items.Add(orderItemDto);
                
                return orderResponse;
            },
            parameters,
            splitOn: "item_id,product_id");

        var response = orderDict.Values
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        return Result.Ok(response);
    }
}

// Classes auxiliares para mapeamento Dapper
internal record OrderRow(
    Guid OrderId,
    string CustomerName,
    string? CustomerPhone,
    string DeliveryAddress,
    string? Notes,
    string Status,
    decimal Total,
    decimal DeliveryFee,
    string PaymentMethod,
    DateTime CreatedAt,
    DateTime UpdatedAt);

internal record OrderItemRow(
    Guid ItemId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string? Notes);

internal record ProductRow(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available,
    DateTime CreatedAt,
    DateTime UpdatedAt);
```

**Nota:** Esta é uma implementação simplificada. Em produção, considere:
- Usar bibliotecas como Dapper.Contrib ou Slapper.AutoMapper
- Criar mappers dedicados
- Otimizar queries para evitar N+1

---

## 6. Criar Abstração para User Repository (Prioridade MÉDIA)

### 6.1 Interface IUserRepository

**Arquivo:** `src/Devlivery/Shared/Infrastructure/Identity/Abstractions/IUserRepository.cs`

```csharp
using Devlivery.Features.Users.Domain;

namespace Devlivery.Shared.Infrastructure.Identity.Abstractions;

/// <summary>
/// Repository interface for User aggregate.
/// Provides abstraction for user persistence operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by email, ignoring query filters (for authentication).
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
```

### 6.2 Implementação

**Arquivo:** `src/Devlivery/Shared/Infrastructure/Identity/UserRepository.cs`

```csharp
using Devlivery.Features.Users.Domain;
using Devlivery.Shared.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Shared.Infrastructure.Identity;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .IgnoreQueryFilters() // Para autenticação, ignorar filtro de tenant
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
```

### 6.3 Atualizar LoginHandler

```csharp
public sealed class LoginHandler(
    ILogger<LoginHandler> logger,
    IUserRepository userRepository,  // ✅ Interface
    IIdentityService identityService,
    ITokenService tokenService)
{
    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            logger.LogInformation("Failed login attempt.");
            return Result.Fail("Credenciais inválidas");
        }

        // ... resto da implementação permanece igual
    }
}
```

---

## 7. Checklist de Implementação

### Fase 1: Interfaces (Prioridade ALTA)

- [ ] Criar `IOrderRepository` e atualizar `OrderRepository`
- [ ] Criar `IProductRepository` e atualizar `ProductRepository`
- [ ] Criar `ICashSessionRepository` e atualizar `CashSessionRepository`
- [ ] Criar `IUnitOfWork` e atualizar `UnitOfWork`
- [ ] Atualizar registros de DI em todas as Features
- [ ] Atualizar todos os handlers para usar interfaces
- [ ] Executar testes para garantir que nada quebrou

### Fase 2: Queries Dapper (Prioridade MÉDIA)

- [ ] Migrar `GetAllOrdersHandler` para Dapper
- [ ] Migrar `GetOrderByIdHandler` para Dapper
- [ ] Migrar outras queries conforme necessário
- [ ] Testar performance e correção

### Fase 3: Abstrações Adicionais (Prioridade MÉDIA)

- [ ] Criar `IUserRepository`
- [ ] Atualizar `LoginHandler` para usar `IUserRepository`
- [ ] Remover injeções diretas de `ApplicationDbContext`

### Fase 4: Limpeza (Prioridade BAIXA)

- [ ] Remover projetos não utilizados
- [ ] Atualizar documentação
- [ ] Revisar código morto

---

## 8. Benefícios Esperados

### Testabilidade
- ✅ Handlers podem ser testados com mocks das interfaces
- ✅ Testes unitários mais fáceis de escrever
- ✅ Isolamento de dependências

### Manutenibilidade
- ✅ Código mais desacoplado
- ✅ Fácil trocar implementações (ex: cache, diferentes DBs)
- ✅ Melhor alinhamento com SOLID

### Consistência
- ✅ Padrão uniforme em todo o projeto
- ✅ Queries usando Dapper conforme planejado
- ✅ Sem injeções diretas de DbContext

---

**Próximos Passos:**
1. Revisar este documento
2. Priorizar implementação
3. Criar branches para cada fase
4. Implementar incrementalmente com testes

