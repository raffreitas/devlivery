# Domain-Driven Design Tático com Aggregates e Domain Events

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Modelagem de Domínio e Consistência de Negócio

## Contexto e Problema

Em sistemas complexos, lógica de negócio pode vazar para camadas de infraestrutura (Services, Controllers), tornando difícil garantir invariantes de domínio. Domain-Driven Design (DDD) propõe modelar o domínio usando blocos de construção táticos: Entities, Value Objects, Aggregates, Domain Events.

A estrutura do código revela a adoção de padrões DDD táticos:

```
Shared/SeedWork/
├── Entity.cs                    # Base class para entidades
├── IDomainEvent.cs              # Interface para eventos de domínio
├── DomainEventBase.cs           # Implementação base de eventos
├── Money.cs                     # Value Object
└── PhoneNumber.cs               # Value Object

Features/Orders/Domain/
├── Order.cs                     # Aggregate Root
├── OrderItem.cs                 # Entity (parte do aggregate)
└── OrderStatus.cs               # Value Object (enum)
```

**Problema:** Como garantir que regras de negócio sejam sempre respeitadas e que mudanças de estado sejam rastreáveis?

## Opções Consideradas

* **Modelo Anêmico (Anemic Domain Model)** - Entidades são POCOs com getters/setters, lógica em Services
* **DDD Tático Leve** - Entities com comportamento, Aggregates para consistência, Domain Events opcionais
* **DDD Completo + Event Sourcing** - Todo estado derivado de eventos, sem state direto no banco

## Decisão

**Escolhida:** "DDD Tático Leve com Domain Events", porque:

1. **Encapsulamento de Regras:** Lógica de negócio vive nas entidades, não em Services
2. **Invariantes Garantidas:** Aggregate Roots protegem consistência via métodos de negócio
3. **Rastreabilidade:** Domain Events registram **o que aconteceu** (ex: `OrderPlaced`, `CashSessionClosed`)
4. **Desacoplamento:** Features reagem a eventos sem acoplamento direto
5. **Complexidade Balanceada:** Benefícios do DDD sem overhead de Event Sourcing

### Implementação Técnica

**Base Class: Entity (Shared/SeedWork/Entity.cs):**

```csharp
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected init; } = Guid.CreateVersion7();

    /// <summary>
    /// Domain events que ocorreram nesta entidade.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adiciona evento de domínio a ser publicado quando a entidade for salva.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Limpa eventos de domínio. Chamado após dispatch.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Exemplo de Aggregate Root:**

```csharp
// Features/Orders/Domain/Order.cs
public sealed class Order : Entity
{
    private readonly List<OrderItem> _items = [];

    // Propriedades read-only (encapsulamento)
    public Guid EstablishmentId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Construtor privado (só criação via factory methods)
    private Order() { }

    // Factory Method (cria e valida)
    public static Order Create(Guid establishmentId, List<OrderItem> items)
    {
        if (items.Count == 0)
            throw new DomainException("Order must have at least one item");

        var order = new Order
        {
            EstablishmentId = establishmentId,
            Status = OrderStatus.Pending,
            _items = items,
            TotalAmount = items.Sum(i => i.TotalPrice)
        };

        // Dispara evento de domínio
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, establishmentId));
        
        return order;
    }

    // Método de comportamento (valida invariantes)
    public void Complete()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be completed");

        Status = OrderStatus.Completed;
        AddDomainEvent(new OrderCompletedEvent(Id));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
            throw new DomainException("Cannot cancel completed orders");

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }
}
```

**Value Objects:**

```csharp
// Shared/SeedWork/Money.cs
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        
        return new Money(Amount + other.Amount, Currency);
    }
}
```

**Domain Events:**

```csharp
// Features/CashRegister/Events/CashSessionClosedEvent.cs
public sealed record CashSessionClosedEvent(
    Guid CashSessionId,
    Guid EstablishmentId,
    decimal FinalBalance
) : DomainEventBase;
```

**Dispatch Automático de Eventos (UnitOfWork):**

```csharp
// Shared/Infrastructure/Persistence/UnitOfWork.cs
public async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    // 1. SaveChanges dispara DispatchDomainEventsInterceptor
    // 2. Interceptor coleta eventos das entidades
    // 3. Publica eventos via Mediator
    // 4. Limpa eventos das entidades
    
    return await dbContext.SaveChangesAsync(ct);
}
```

**Estrutura de Pastas DDD:**

```
Features/CashRegister/
├── Domain/                      # Modelo rico de domínio
│   ├── CashSession.cs           # Aggregate Root
│   ├── CashDeposit.cs           # Entity (parte do aggregate)
│   └── CashSessionStatus.cs     # Value Object
│
├── Events/                      # Domain Events
│   ├── CashSessionOpenedEvent.cs
│   ├── CashSessionClosedEvent.cs
│   └── CashDepositAddedEvent.cs
│
├── Commands/                    # Casos de uso que MUDAM estado
│   ├── CreateCashSession/
│   │   └── CreateCashSessionHandler.cs  # Usa CashSession.Open()
│   └── CloseCashSession/
│       └── CloseCashSessionHandler.cs   # Usa cashSession.Close()
│
└── Infrastructure/
    ├── ICashSessionRepository.cs
    └── CashSessionRepository.cs  # Persiste aggregates
```

### Consequências

* ✅ **Bom:** Lógica de negócio centralizada nas entidades (fácil de testar)
* ✅ **Bom:** Invariantes protegidas — impossível criar estado inválido
* ✅ **Bom:** Domain Events permitem reação a mudanças (ex: enviar email quando order completa)
* ✅ **Bom:** Modelo expressivo — `order.Complete()` vs `orderService.SetStatusToCompleted(order)`
* ✅ **Bom:** Preparado para Event Sourcing no futuro
* ⚠️ **Neutro:** Curva de aprendizado — requer entender conceitos DDD
* ⚠️ **Ruim:** Pode ser over-engineering para CRUDs simples (ex: cadastro de categorias)
* ⚠️ **Ruim:** Aggregates grandes podem ter performance issues (mitigado por design consciente)

### Regras de Design

1. **Aggregates são fronteiras de consistência:**
   - Um `Order` e seus `OrderItems` são um aggregate (salvos atomicamente)
   - `Product` é um aggregate separado (não parte de Order)

2. **Modificações SEMPRE via métodos de negócio:**
   - ❌ `order.Status = OrderStatus.Completed;`
   - ✅ `order.Complete();`

3. **Domain Events para comunicação entre Aggregates:**
   - ❌ `OrderHandler` chamando `InventoryService.DecrementStock()`
   - ✅ `OrderCompletedEvent` → `InventoryEventHandler` reage

4. **Repository opera em Aggregate Roots:**
   - `IOrderRepository.GetById()` retorna `Order` com seus `OrderItems`
   - Não existe `IOrderItemRepository` (items são parte do aggregate)

### Exemplos de Domain Events no Sistema

- `OrderCreatedEvent` → Notifica sistema de pagamento
- `CashSessionClosedEvent` → Registra auditoria, gera relatório
- `ProductCreatedEvent` → Invalida cache de listagem de produtos

**Princípio:** "Make invalid states unrepresentable. Use the type system and domain model to enforce business rules."

### Referências

- Eric Evans: Domain-Driven Design (Blue Book)
- Vaughn Vernon: Implementing Domain-Driven Design (Red Book)
