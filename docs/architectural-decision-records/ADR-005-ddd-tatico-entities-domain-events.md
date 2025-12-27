# DDD Tático: Entities, Domain Events e Value Objects

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Padrão de Design / Domain-Driven Design

## Contexto e Problema

Em aplicações com lógica de negócio complexa, colocar regras de domínio em services ou handlers cria acoplamento e dificulta reutilização. Domain-Driven Design (DDD) tático oferece padrões para encapsular lógica de negócio em entidades ricas, mas requer disciplina para manter invariantes e comunicar mudanças através de eventos.

A estrutura do repositório revela esta decisão através da organização:

```
Shared/SeedWork/
├── Entity.cs                        # Base class para entidades
├── DomainEventBase.cs               # Base class para domain events
└── IDomainEvent.cs                  # Interface para domain events

Features/Products/Domain/
├── Product.cs                       # Entity com lógica de negócio
└── IProductRepository.cs

Features/Orders/Domain/
├── Order.cs                         # Entity com domain events
└── OrderItem.cs
```

**Problema:** Como modelar entidades de domínio ricas que encapsulam lógica de negócio, mantêm invariantes e comunicam mudanças através de eventos, sem acoplar a camada de aplicação?

## Opções Consideradas

* **Anemic Domain Model** - Entidades apenas com propriedades, lógica em services (anti-pattern)
* **Rich Domain Model (DDD Tático)** - Entidades com métodos, domain events, value objects
* **Event Sourcing** - Armazenar eventos ao invés de estado (complexidade desnecessária para este contexto)

## Decisão

**Escolhida:** "Rich Domain Model (DDD Tático)", porque:

1. Encapsula lógica de negócio em entidades, mantendo invariantes próximas aos dados
2. Domain events permitem comunicação desacoplada entre agregados
3. Value objects (Money, PhoneNumber) garantem consistência de dados
4. Facilita testes: lógica de negócio pode ser testada isoladamente
5. Alinha com princípios DDD: entidades representam conceitos do domínio, não apenas estruturas de dados

### Implementação Técnica

A decisão se materializa em:

**Entity Base Class:**
```csharp
// Shared/SeedWork/Entity.cs
public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.CreateVersion7();
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Rich Entity Example:**
```csharp
// Features/Products/Domain/Product.cs
public sealed class Product : Entity
{
    public string Name { get; private set; }  // ← private set!
    public decimal Price { get; private set; }
    public bool Available { get; private set; }
    public Guid EstablishmentId { get; private set; }

    public Product(string name, decimal price, Guid establishmentId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");
        if (price <= 0)
            throw new DomainException("Product price must be greater than zero");

        Name = name;
        Price = price;
        Available = true;
        EstablishmentId = establishmentId;
    }

    public void Update(string? name = null, decimal? price = null)
    {
        if (name != null && string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");
        if (price.HasValue && price.Value <= 0)
            throw new DomainException("Product price must be greater than zero");

        Name = name ?? Name;
        Price = price ?? Price;
        
        AddDomainEvent(new ProductUpdatedEvent(Id, Name, Price));
    }

    public void SetAsUnavailable()
    {
        if (!Available) return;
        Available = false;
        AddDomainEvent(new ProductUnavailableEvent(Id));
    }
}
```

**Domain Event:**
```csharp
// Features/Orders/Domain/Events/OrderCreatedEvent.cs
public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid EstablishmentId,
    decimal TotalAmount) : DomainEventBase;
```

**Value Objects:**
```csharp
// Shared/SeedWork/Money.cs
public sealed record Money(decimal Amount, string Currency = "BRL")
{
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot add money with different currencies");
        return new Money(left.Amount + right.Amount, left.Currency);
    }
}
```

**Domain Event Dispatch:**
```csharp
// Shared/Infrastructure/Persistence/Interceptors/DispatchDomainEventsInterceptor.cs
// Dispara eventos automaticamente após SaveChanges
```

### Consequências

* ✅ **Bom:** Encapsula lógica de negócio em entidades, mantendo invariantes
* ✅ **Bom:** Domain events permitem comunicação desacoplada entre agregados
* ✅ **Bom:** Facilita testes: lógica de negócio pode ser testada isoladamente
* ✅ **Bom:** Value objects garantem consistência e reutilização
* ✅ **Bom:** Private setters previnem modificação direta, forçando uso de métodos
* ⚠️ **Neutro:** Requer disciplina para não colocar lógica de aplicação em entidades
* ⚠️ **Ruim:** Pode ser mais verboso que anemic domain model
* ⚠️ **Ruim:** Domain events precisam ser gerenciados cuidadosamente para evitar duplicação

