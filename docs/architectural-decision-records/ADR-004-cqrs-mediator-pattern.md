# Implementação de CQRS com Mediator Pattern

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Padrão de Design / Arquitetura de Aplicação

## Contexto e Problema

Em aplicações tradicionais, operações de leitura e escrita frequentemente compartilham os mesmos modelos e lógica, criando complexidade desnecessária. Separar comandos (writes) de queries (reads) permite otimizar cada operação independentemente, mas requer um mecanismo de desacoplamento entre endpoints HTTP e handlers.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/
├── Commands/                       # Operações de escrita (CUD)
│   └── CreateProduct/
│       ├── CreateProductCommand.cs
│       ├── CreateProductHandler.cs
│       └── CreateProductEndpoint.cs
└── Queries/                        # Operações de leitura (R)
    └── GetAllProducts/
        ├── GetAllProductsQuery.cs
        ├── GetAllProductsHandler.cs
        └── GetAllProductsEndpoint.cs
```

**Problema:** Como separar operações de leitura e escrita (CQRS) mantendo desacoplamento entre endpoints HTTP e lógica de negócio, sem criar acoplamento direto?

## Opções Consideradas

* **Controllers com Services** - Endpoints chamam services diretamente (acoplamento)
* **Mediator Pattern (MediatR)** - Biblioteca MediatR para desacoplamento (dependência externa pesada)
* **Mediator Pattern (Mediator.Abstractions)** - Biblioteca leve com source generators
* **Command/Query Handlers Manuais** - Implementar dispatcher próprio (complexidade)

## Decisão

**Escolhida:** "Mediator Pattern (Mediator.Abstractions)", porque:

1. Desacopla endpoints de handlers: endpoints apenas enviam commands/queries via `ISender`
2. Facilita separação CQRS: commands e queries são tipos distintos com handlers separados
3. Permite pipeline behaviors: validação, logging, tenancy podem ser aplicados globalmente
4. Biblioteca leve com source generators: gera código em compile-time, sem overhead de runtime
5. Type-safe: compilador garante que handlers correspondem a commands/queries

### Implementação Técnica

A decisão se materializa em:

**Command (Write):**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductCommand.cs
public sealed record CreateProductCommand(
    string Name,
    decimal Price) : ICommand<Result<CreateProductResponse>>;

// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository repo,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor)
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(
        CreateProductCommand command,
        CancellationToken ct)
    {
        var product = new Product(command.Name, tenantAccessor.Tenant.Id);
        await repo.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**Query (Read):**
```csharp
// Features/Products/Queries/GetAllProducts/GetAllProductsQuery.cs
public sealed record GetAllProductsQuery() : IQuery<Result<GetAllProductsResponse>>;

// Features/Products/Queries/GetAllProducts/GetAllProductsHandler.cs
public sealed class GetAllProductsHandler(IProductRepository repo)
    : IQueryHandler<GetAllProductsQuery, Result<GetAllProductsResponse>>
{
    public async ValueTask<Result<GetAllProductsResponse>> Handle(
        GetAllProductsQuery query,
        CancellationToken ct)
    {
        var products = await repo.GetAllAsync(ct);
        return Result.Ok(new GetAllProductsResponse(products));
    }
}
```

**Endpoint (Minimal API):**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductEndpoint.cs
public static class CreateProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<Results<Created<...>, BadRequest<...>>> Handle(
        CreateProductCommand command,
        ISender sender,  // ← Mediator
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? result.ToCreated($"/api/products/{result.Value.ProductId}")
            : result.ToBadRequest();
    }
}
```

**Registro no Startup:**
```csharp
// Startup.cs
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = [
        typeof(ValidationPipelineBehavior<,>),
        typeof(DomainEventTenantBehavior<,>)
    ];
});
```

### Consequências

* ✅ **Bom:** Desacopla endpoints de lógica de negócio, facilitando testes
* ✅ **Bom:** Facilita separação CQRS: commands e queries podem ser otimizados independentemente
* ✅ **Bom:** Permite pipeline behaviors globais (validação, logging, tenancy)
* ✅ **Bom:** Type-safe: compilador garante correspondência entre commands/queries e handlers
* ✅ **Bom:** Handlers são descobertos automaticamente (sem registro manual)
* ⚠️ **Neutro:** Adiciona uma camada de indireção (trade-off por desacoplamento)
* ⚠️ **Ruim:** Pode ser mais difícil rastrear fluxo de execução (requer debugger ou logging)
* ⚠️ **Ruim:** Source generators podem adicionar tempo de compilação (geralmente aceitável)

