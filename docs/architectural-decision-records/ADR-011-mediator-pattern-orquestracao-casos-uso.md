# Mediator Pattern para Orquestração de Casos de Uso

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Padrão de Comunicação entre Camadas e Desacoplamento

## Contexto e Problema

Em aplicações tradicionais, Controllers chamam Services diretamente, criando acoplamento forte. Quando múltiplas features precisam reagir a uma ação (ex: enviar email após criar pedido), Controllers acumulam responsabilidades. O Mediator Pattern desacopla o remetente (endpoint) do receptor (handler), permitindo pipeline behaviors e extensibilidade.

A configuração do projeto revela uso do Mediator:

```xml
<!-- Devlivery.csproj -->
<PackageReference Include="Mediator.Abstractions" Version="3.1.0-preview.14"/>
<PackageReference Include="Mediator.SourceGenerator" Version="3.1.0-preview.14">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

```csharp
// Startup.cs
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors =
    [
        typeof(Shared.Infrastructure.Tenancy.Behaviors.DomainEventTenantBehavior<,>),
        typeof(Shared.Application.Behaviors.ValidationPipelineBehavior<,>)
    ];
});
```

**Problema:** Como orquestrar casos de uso mantendo baixo acoplamento e permitindo cross-cutting concerns (validação, logging, transações)?

## Opções Consideradas

* **Controllers → Services diretos** - Chamadas síncronas sem intermediação
* **MediatR (biblioteca popular)** - Pattern Mediator via reflection
* **Mediator (Source Generator)** - Pattern Mediator com geração de código em compile-time
* **Event Bus** - Comunicação assíncrona via mensageria (RabbitMQ, Azure Service Bus)

## Decisão

**Escolhida:** "Mediator com Source Generator", porque:

1. **Desacoplamento:** Endpoints não conhecem handlers — apenas enviam requests
2. **Pipeline Behaviors:** Validação, logging, transações aplicados uniformemente
3. **Performance:** Source Generator elimina overhead de reflection (vs MediatR)
4. **Testabilidade:** Handlers são classes independentes, fáceis de testar
5. **Extensibilidade:** Novos behaviors podem ser adicionados sem modificar handlers

### Implementação Técnica

**Estrutura de um Request/Response:**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductCommand.cs
public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available
) : ICommand<Result<CreateProductResponse>>;  // ← Interface do Mediator

// Features/Products/Commands/CreateProduct/CreateProductResponse.cs
public sealed record CreateProductResponse(Guid ProductId);
```

**Handler Implementation:**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
using Mediator;

public sealed class CreateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = new Product(
            command.Name,
            command.Description,
            command.Price,
            command.Category,
            command.Available,
            tenantAccessor.Tenant.Id
        );

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**Endpoint (Minimal API):**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductEndpoint.cs
public static class CreateProductEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            CreateProductCommand command,
            IMediator mediator,  // ← Injetado automaticamente
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);  // ← Dispara handler
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : Results.BadRequest(result.Errors);
        })
        .WithName("CreateProduct")
        .RequireAuthorization();
    }
}
```

**Registro Automático (Source Generator):**

```csharp
// Startup.cs - ConfigureBuilder()
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    
    // Behaviors aplicados na ordem definida
    options.PipelineBehaviors =
    [
        typeof(DomainEventTenantBehavior<,>),      // 1. Multi-tenancy
        typeof(ValidationPipelineBehavior<,>)      // 2. FluentValidation
    ];
});

// Handlers são registrados AUTOMATICAMENTE pelo Source Generator
// Não é necessário services.AddScoped<CreateProductHandler>()
```

**Pipeline Behaviors (Cross-Cutting Concerns):**

**1. Validation Behavior:**

```csharp
// Shared/Application/Behaviors/ValidationPipelineBehavior.cs
using FluentValidation;
using Mediator;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next)
    {
        // Executa ANTES do handler
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(message);
            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                // Retorna erro de validação (Response precisa ser compatível)
                throw new ValidationException(failures);
            }
        }

        // Chama próximo behavior ou handler
        return await next(message, cancellationToken);
    }
}
```

**2. Domain Event Tenant Behavior:**

```csharp
// Shared/Infrastructure/Tenancy/Behaviors/DomainEventTenantBehavior.cs
public sealed class DomainEventTenantBehavior<TRequest, TResponse>(
    ITenantAccessor tenantAccessor
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next)
    {
        // Garante que tenant está configurado antes de processar
        if (tenantAccessor.Tenant == null)
        {
            throw new UnauthorizedAccessException("Tenant not configured");
        }

        // Continua pipeline
        return await next(message, cancellationToken);
    }
}
```

**Fluxo de Execução:**

```
HTTP Request
    ↓
Endpoint (CreateProductEndpoint)
    ↓
mediator.Send(command)
    ↓
[Pipeline Behavior 1: DomainEventTenantBehavior]
    ↓
[Pipeline Behavior 2: ValidationPipelineBehavior]
    ↓
CreateProductHandler.Handle()
    ↓
Repository → UnitOfWork → Database
    ↓
Response (CreateProductResponse)
    ↓
HTTP Response
```

### Tipos de Messages

**1. Commands (ICommand\<TResponse\>):**
```csharp
// Alteram estado do sistema
public sealed record CreateOrderCommand(...) : ICommand<Result<CreateOrderResponse>>;
```

**2. Queries (IQuery\<TResponse\>):**
```csharp
// Apenas leitura, sem side effects
public sealed record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductDto>>;
```

**3. Notifications (INotification):**
```csharp
// Eventos que múltiplos handlers podem consumir
public sealed record OrderCreatedNotification(Guid OrderId) : INotification;

// Múltiplos handlers para mesma notificação
public class SendEmailHandler : INotificationHandler<OrderCreatedNotification> { }
public class UpdateInventoryHandler : INotificationHandler<OrderCreatedNotification> { }
```

### Consequências

* ✅ **Bom:** Desacoplamento total entre endpoints e handlers
* ✅ **Bom:** Pipeline behaviors aplicam cross-cutting concerns uniformemente
* ✅ **Bom:** Source Generator = zero overhead de reflection (performance)
* ✅ **Bom:** Handlers testáveis isoladamente (não dependem de HTTP context)
* ✅ **Bom:** Fácil adicionar novos behaviors (logging, caching, retry)
* ✅ **Bom:** Suporta notificações (1 evento → N handlers)
* ⚠️ **Neutro:** Indireção — pode dificultar "Go to Definition" para iniciantes
* ⚠️ **Ruim:** Curva de aprendizado — padrão menos familiar que Services
* ⚠️ **Ruim:** Debug pode ser mais complexo (pipeline behaviors intermediários)

### Comparação: MediatR vs Mediator

| Aspecto                | MediatR (Reflection)      | Mediator (Source Gen) |
|------------------------|---------------------------|-----------------------|
| **Performance**        | Overhead de reflection    | ✅ Zero overhead       |
| **Compile-time Safety**| ❌ Erros em runtime       | ✅ Erros em compile   |
| **Tamanho do Package** | Maior                     | ✅ Menor              |
| **Maturidade**         | Muito maduro (10+ anos)   | Mais recente (2022+)  |
| **Ecossistema**        | Extenso                   | Crescente             |

**Escolha:** Mediator oferece melhor performance para projeto novo.

### Convenções de Nomenclatura

**Commands:**
- `CreateProductCommand`, `UpdateOrderCommand`, `DeleteCashSessionCommand`
- Verbo imperativo (Create, Update, Delete, Close, Open)

**Queries:**
- `GetProductByIdQuery`, `GetAllOrdersQuery`, `GetActiveCashSessionQuery`
- Verbo Get + descrição do que retorna

**Handlers:**
- `CreateProductHandler`, `GetProductByIdHandler`
- Nome do Command/Query + "Handler"

**Responses:**
- `CreateProductResponse`, `GetProductByIdResponse`
- Nome do Command/Query + "Response"

### Teste Unitário

```csharp
[Fact]
public async Task Handle_Should_Create_Product_Successfully()
{
    // Arrange
    var productRepository = Substitute.For<IProductRepository>();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    var tenantAccessor = CreateTenantAccessorMock();
    
    // Handler testado DIRETAMENTE (sem mediator)
    var handler = new CreateProductHandler(
        productRepository, 
        unitOfWork, 
        tenantAccessor);
    
    var command = new CreateProductCommand("Test Product", "Description", 10.0m, "Food", true);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.ShouldBeTrue();
    await productRepository.Received(1).AddAsync(
        Arg.Any<Product>(), 
        Arg.Any<CancellationToken>());
}
```

### Exemplo de Notificação (Domain Events)

```csharp
// Domain Event
public sealed record OrderCreatedEvent(Guid OrderId, Guid EstablishmentId) : INotification;

// Handler 1: Enviar Email
public class SendOrderConfirmationEmailHandler(IEmailService emailService) 
    : INotificationHandler<OrderCreatedEvent>
{
    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        await emailService.SendOrderConfirmationAsync(notification.OrderId, ct);
    }
}

// Handler 2: Atualizar Dashboard
public class UpdateDashboardHandler(IDashboardService dashboard) 
    : INotificationHandler<OrderCreatedEvent>
{
    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        await dashboard.RefreshOrderCountAsync(notification.EstablishmentId, ct);
    }
}

// Publicação (dentro do UnitOfWork após SaveChanges)
await mediator.Publish(new OrderCreatedEvent(orderId, establishmentId), ct);
```

**Princípio:** "Mediate all use case execution through a single point. Apply cross-cutting concerns via pipeline behaviors."

### Referências

- [Mediator Pattern (GoF)](https://refactoring.guru/design-patterns/mediator)
- [Mediator Library](https://github.com/martinothamar/Mediator)
- Jimmy Bogard: [CQRS with MediatR](https://lostechies.com/jimmybogard/2015/05/05/cqrs-with-mediatr-and-automapper/)
