# Padrão de Test Builders e Fixtures

Este documento descreve o padrão adotado para testes unitários no projeto Devlivery, separando responsabilidades entre **Builders** e **Fixtures**.

## Visão Geral

O padrão combina dois conceitos:

1. **Builders** - Constroem objetos de domínio com valores padrão sensatos
2. **Fixtures** - Criam mocks de dependências e fornecem métodos convenientes

## Princípios

### 1. Builders: Valores Padrão Sempre Válidos

Os builders devem **sempre** produzir objetos válidos sem necessidade de configuração adicional.

**Correto:**
```csharp
var order = new OrderBuilder().Build(); // ✅ Funciona sem configuração
```

**Incorreto:**
```csharp
var order = new OrderBuilder()
    .WithItems(items)      // ❌ Não deve exigir configuração obrigatória
    .WithEstablishmentId(id)
    .Build();
```

### 2. Fixtures: Apenas Mocks e Helpers

Fixtures devem focar em:
- Criar mocks de dependências (repositories, UoW, etc.)
- Fornecer métodos convenientes que **usam builders internamente**
- Fornecer dados de teste comuns (como `_defaultTenantId`)

**Não devem:**
- Duplicar lógica de construção de objetos
- Conter lógica de negócio

## Estrutura dos Builders

### OrderBuilder

**Características:**
- Valores padrão para todos os campos
- Cria automaticamente um `OrderItem` padrão se nenhum for fornecido
- `EstablishmentId` padrão gerado automaticamente

```csharp
public class OrderBuilder
{
    private Guid _establishmentId;
    private OrderItem[] _orderItems;
    
    public OrderBuilder()
    {
        _establishmentId = Guid.NewGuid(); // ✅ Valor padrão
        _orderItems = [];                  // ✅ Pode estar vazio
    }
    
    public Order Build()
    {
        // Se não houver items, criar um padrão
        var items = _orderItems.Length == 0
            ? new[] { CreateDefaultOrderItem() }
            : _orderItems;
        
        return new Order(..., items: items.ToList(), ...);
    }
    
    private OrderItem CreateDefaultOrderItem()
    {
        return new OrderItem(
            productId: Guid.NewGuid(),
            establishmentId: _establishmentId,
            quantity: _faker.Random.Int(1, 5),
            unitPrice: _faker.Random.Decimal(10, 100),
            notes: null
        );
    }
}
```

### OrderItemBuilder

**Características:**
- Pode ser usado sem `Product` - usa IDs e preços fictícios
- Quando `Product` é fornecido, usa seus dados
- `EstablishmentId` padrão gerado automaticamente

```csharp
public class OrderItemBuilder
{
    private Product? _product;
    private Guid _productId;
    private decimal _unitPrice;
    private Guid _establishmentId;
    
    public OrderItemBuilder()
    {
        _productId = Guid.NewGuid();        // ✅ Valor padrão
        _unitPrice = _faker.Random.Decimal(10, 200);
        _establishmentId = Guid.NewGuid();  // ✅ Valor padrão
    }
    
    public OrderItem Build()
    {
        // Se um produto foi fornecido, usar seus dados
        var productId = _product?.Id ?? _productId;
        var unitPrice = _product?.Price ?? _unitPrice;
        
        return new OrderItem(productId, _establishmentId, ...);
    }
}
```

## Estrutura das Fixtures

### OrdersUnitTestFixture

**Características:**
- Cria mocks de todas as dependências
- Métodos helper que **usam builders internamente**
- Define valores padrão comuns (como `_defaultTenantId`)

```csharp
public sealed class OrdersUnitTestFixture : IDisposable
{
    public Faker Faker { get; } = new("pt_BR");
    
    private readonly Guid _defaultTenantId = Guid.NewGuid();
    
    // Criação de Mocks
    public ITenantAccessor CreateTenantAccessorMock(Guid? tenantId = null)
    {
        var mock = Substitute.For<ITenantAccessor>();
        var tenant = new Tenant(tenantId ?? _defaultTenantId);
        mock.Tenant.Returns(tenant);
        return mock;
    }
    
    public IOrderRepository CreateOrderRepositoryMock()
    {
        return Substitute.For<IOrderRepository>();
    }
    
    // Helper que USA o builder
    public Order CreateOrder(
        string? customerName = null,
        OrderStatus? status = null,
        IEnumerable<OrderItem>? orderItems = null,
        Guid? establishmentId = null)
    {
        var orderBuilder = new OrderBuilder();
        
        if (!string.IsNullOrEmpty(customerName))
            orderBuilder.WithCustomerName(customerName);
        
        if (orderItems != null)
            orderBuilder.WithItems(orderItems.ToArray());
        
        // Sempre garantir estabelecimento
        if (establishmentId.HasValue)
            orderBuilder.WithEstablishmentId(establishmentId.Value);
        else
            orderBuilder.WithEstablishmentId(_defaultTenantId);
        
        var order = orderBuilder.Build();
        
        if (status.HasValue)
            order.UpdateStatus(status.Value);
        
        return order;
    }
    
    public OrderItem CreateOrderItem(...)
    {
        // Criação direta, pois OrderItem é simples
        return new OrderItem(...);
    }
}
```

## Uso nos Testes

### Cenário 1: Teste Simples de Domínio

```csharp
[Fact]
public void UpdateStatus_Should_Change_Status()
{
    // Arrange - Usa fixture que usa builder internamente
    var order = _fixture.CreateOrder();
    
    // Act
    order.UpdateStatus(OrderStatus.Preparing);
    
    // Assert
    order.Status.ShouldBe(OrderStatus.Preparing);
}
```

### Cenário 2: Teste com Builder Direto

```csharp
[Fact]
public void Should_Calculate_Total_With_Multiple_Items()
{
    // Arrange - Builder direto quando precisa de controle fino
    var item1 = new OrderItemBuilder()
        .WithQuantity(2)
        .WithUnitPrice(10.00m)
        .Build();
    
    var item2 = new OrderItemBuilder()
        .WithQuantity(3)
        .WithUnitPrice(5.00m)
        .Build();
    
    var order = new OrderBuilder()
        .WithItems(item1, item2)
        .WithDeliveryFee(5.00m)
        .Build();
    
    // Assert
    order.Total.ShouldBe(40.00m); // (2*10 + 3*5) + 5
}
```

### Cenário 3: Teste de Handler com Mocks

```csharp
[Fact]
public async Task Handle_Should_Update_Order_Status()
{
    // Arrange - Fixture para ordem E mocks
    var order = fixture.CreateOrder(status: OrderStatus.Pending);
    
    var orderRepository = fixture.CreateOrderRepositoryMock();
    var unitOfWork = fixture.CreateUnitOfWorkMock();
    
    orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
        .Returns(order);
    
    var handler = new UpdateOrderStatusHandler(orderRepository, unitOfWork);
    
    // Act & Assert...
}
```

## Checklist para Novos Builders

Ao criar um novo builder, certifique-se de:

- [ ] Todos os campos têm valores padrão no construtor
- [ ] O método `Build()` funciona sem configuração adicional
- [ ] Valores padrão são **sempre válidos** segundo regras de domínio
- [ ] Usar `Bogus.Faker` para gerar dados realistas
- [ ] Métodos fluentes retornam `this`
- [ ] Documentar regras especiais com comentários

## Checklist para Fixtures

Ao criar uma nova fixture:

- [ ] Implementa `IDisposable`
- [ ] Tem métodos para criar mocks de todas as dependências
- [ ] Métodos helper usam builders internamente (não duplicam lógica)
- [ ] Define valores padrão comuns (IDs de tenant, etc.)
- [ ] Usa `NSubstitute` para mocks
- [ ] Documentar cada método com XML docs

## Benefícios do Padrão

1. **DRY** - Sem duplicação entre builders e fixtures
2. **Facilidade** - Objetos válidos com `new Builder().Build()`
3. **Flexibilidade** - Builders para controle fino, fixtures para casos comuns
4. **Manutenibilidade** - Mudanças em construtores só afetam builders
5. **Legibilidade** - Testes expressam intenção claramente

## Anti-Padrões a Evitar

❌ **Builder que lança exceção sem configuração**
```csharp
public Order Build()
{
    if (_orderItems.Length == 0)
        throw new InvalidOperationException("No order items");
    // ...
}
```

❌ **Fixture duplicando lógica de builder**
```csharp
public Order CreateOrder(...)
{
    // Não fazer isso - usar OrderBuilder!
    return new Order(
        customer: new CustomerInfo(...),
        deliveryAddress: new DeliveryAddress(...),
        // ...
    );
}
```

❌ **Teste configurando TODOS os campos**
```csharp
var order = new OrderBuilder()
    .WithCustomerName("João")
    .WithPhone("11999999999")
    .WithAddress("Rua X")
    .WithPaymentMethod(PaymentMethod.Cash)
    // ... desnecessário se não é relevante para o teste
    .Build();
```

✅ **Configure apenas o que é relevante para o teste**
```csharp
var order = new OrderBuilder()
    .WithPaymentMethod(PaymentMethod.Cash)  // Só o que importa
    .Build();
```

## Evoluindo o Padrão

À medida que o projeto cresce:

1. **Novos builders** - Criar para entidades complexas (agregados)
2. **Builders específicos** - Criar variações para cenários comuns (ex: `OrderBuilder.ForDelivery()`)
3. **Fixtures por feature** - Manter fixtures separadas por bounded context
4. **Shared fixtures** - Criar para dependências compartilhadas (Auth, Tenancy)

## Referências

- [Test Data Builders Pattern](http://www.natpryce.com/articles/000714.html)
- [Object Mother vs Builder](https://martinfowler.com/bliki/ObjectMother.html)
- [xUnit Test Patterns - Test Fixture](http://xunitpatterns.com/test%20fixture%20-%20xUnit.html)
