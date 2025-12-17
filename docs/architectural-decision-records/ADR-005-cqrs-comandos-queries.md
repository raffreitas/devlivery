# Segregação de Responsabilidades em Comandos e Queries (CQRS)

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Padrão de Separação entre Leitura e Escrita

## Contexto e Problema

Em sistemas CRUD tradicionais, operações de leitura e escrita compartilham o mesmo modelo e caminho de código (Controllers → Services → Repositories). Isso pode criar trade-offs: otimizações para leitura (DTOs projetados, queries desnormalizadas) conflitam com validações de escrita (aggregate roots, invariantes de negócio).

O padrão CQRS (Command Query Responsibility Segregation) propõe separar intencionalmente esses caminhos.

A estrutura de cada feature mostra essa segregação:

```
Features/Products/
├── Commands/                    # Operações de ESCRITA (CUD)
│   ├── CreateProduct/
│   │   ├── CreateProductCommand.cs
│   │   ├── CreateProductHandler.cs
│   │   └── CreateProductValidator.cs
│   ├── UpdateProduct/
│   └── DeleteProduct/
│
└── Queries/                     # Operações de LEITURA (R)
    ├── GetAllProducts/
    │   ├── GetAllProductsQuery.cs
    │   ├── GetAllProductsHandler.cs
    │   └── GetAllProductsResponse.cs
    └── GetProductById/
```

**Problema:** Como balancear consistência de escrita com performance de leitura sem criar um modelo monolítico?

## Opções Consideradas

* **Modelo único (CRUD tradicional)** - Um Service com métodos Create/Read/Update/Delete
* **CQRS leve (separação lógica)** - Commands e Queries em pastas separadas, mesmo banco/modelo
* **CQRS completo (separação física)** - Write DB + Read DB com event sourcing

## Decisão

**Escolhida:** "CQRS leve (separação lógica)", porque:

1. **Clareza de Intenção:** Commands representam **ações de negócio** (CreateProduct, CloseOrder), Queries representam **consultas** (GetOrderById)
2. **Otimização Independente:** Queries podem usar Dapper/raw SQL, Commands usam EF Core com tracking
3. **Validação Assimétrica:** Commands têm validators complexos, Queries não precisam validar (apenas parâmetros simples)
4. **Escalabilidade Futura:** Preparado para read replicas sem reescrever código
5. **Complexidade Moderada:** Sem overhead de event sourcing ou bancos separados

### Implementação Técnica

**Estrutura de um Command:**

```
Commands/CreateProduct/
├── CreateProductCommand.cs      # Request (input)
├── CreateProductHandler.cs      # Lógica de negócio
├── CreateProductValidator.cs    # FluentValidation rules
├── CreateProductEndpoint.cs     # HTTP endpoint
└── CreateProductResponse.cs     # Output (ID do produto criado)
```

**Exemplo de Command Handler:**

```csharp
// CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository productRepository,  // ← Usa Repository (abstração)
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor
) : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Criar Aggregate Root (validações de domínio no construtor)
        var product = new Product(
            command.Name,
            command.Description,
            command.Price,
            command.Category,
            command.Available,
            tenantAccessor.Tenant.Id
        );

        // 2. Persistir via Repository
        await productRepository.AddAsync(product, cancellationToken);
        
        // 3. Commit da transação (dispara domain events)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Retornar resposta
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**Estrutura de uma Query:**

```
Queries/GetAllProducts/
├── GetAllProductsQuery.cs       # Request (filtros, paginação)
├── GetAllProductsHandler.cs     # Lógica de leitura
├── GetAllProductsEndpoint.cs    # HTTP endpoint
└── GetAllProductsResponse.cs    # DTO otimizado para UI
```

**Exemplo de Query Handler:**

```csharp
// GetAllProductsHandler.cs
public sealed class GetAllProductsHandler(
    ApplicationDbContext dbContext  // ← Injeta DbContext DIRETAMENTE
) : IQueryHandler<GetAllProductsQuery, Result<List<GetAllProductsResponse>>>
{
    public async ValueTask<Result<List<GetAllProductsResponse>>> Handle(
        GetAllProductsQuery query,
        CancellationToken cancellationToken)
    {
        // Query otimizada: AsNoTracking + projeção direta para DTO
        var products = await dbContext.Products
            .AsNoTracking()                              // Sem tracking = mais rápido
            .Where(p => p.Available == query.AvailableOnly)
            .Select(p => new GetAllProductsResponse(    // Projeção para DTO
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Category
            ))
            .ToListAsync(cancellationToken);

        return Result.Ok(products);
    }
}
```

**Assimetrias Intencionais:**

| Aspecto                | Commands                          | Queries                            |
|------------------------|-----------------------------------|------------------------------------|
| **Injeção de Deps**    | `IRepository` (abstração)         | `ApplicationDbContext` (concreto)  |
| **Tracking EF**        | Enabled (para change tracking)    | Disabled (`AsNoTracking()`)        |
| **Validação**          | `FluentValidation` + domain rules | Parâmetros simples (primitivos)    |
| **Transações**         | Sim (`UnitOfWork.SaveChanges()`)  | Não (read-only)                    |
| **Domain Events**      | Sim (disparados no SaveChanges)   | Não (queries não mudam estado)     |
| **Retorno**            | `Result<XxxResponse>` (sucesso/erro) | `Result<List<DTO>>` ou `DTO` direto |
| **Otimização**         | Foco em consistência              | Foco em performance (ex: Dapper)   |

**Mediator como Orquestrador:**

```csharp
// Endpoint invoca Mediator, que roteia para Handler correto
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapPost("/", async (CreateProductCommand command, IMediator mediator) =>
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
    });
}
```

### Consequências

* ✅ **Bom:** Queries podem ser otimizadas agressivamente sem impactar writes
* ✅ **Bom:** Commands têm validação robusta sem penalizar reads
* ✅ **Bom:** Preparado para escalar reads independentemente (read replicas)
* ✅ **Bom:** Código auto-documentado — `CreateOrderCommand` é claramente uma ação
* ✅ **Bom:** Testes mais focados — CommandTests validam negócio, QueryTests validam projeções
* ⚠️ **Neutro:** Mais arquivos por funcionalidade (trade-off aceitável)
* ⚠️ **Ruim:** Eventual duplicação de DTOs entre Commands e Queries
* ⚠️ **Ruim:** Curva de aprendizado para devs acostumados com CRUD tradicional

### Evolução Futura

Se necessário, CQRS leve pode evoluir para:

1. **Read Replicas:** Queries apontam para replica read-only do PostgreSQL
2. **CQRS com Event Sourcing:** Commands geram eventos, Queries consomem projections
3. **Bancos Separados:** Write DB (PostgreSQL) + Read DB (Elasticsearch/MongoDB)

**Princípio atual:** "Separate reads and writes logically; separate them physically only when performance demands it."

### Referências

- Martin Fowler: [CQRS](https://martinfowler.com/bliki/CQRS.html)
- Greg Young: Command Query Responsibility Segregation
