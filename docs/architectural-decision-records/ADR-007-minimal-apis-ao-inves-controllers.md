# Minimal APIs ao invés de Controllers

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / ASP.NET Core

## Contexto e Problema

Controllers tradicionais do ASP.NET Core introduzem overhead desnecessário para APIs simples: requerem classes, herança, e convenções que podem não ser necessárias. Minimal APIs oferecem uma abordagem mais leve e explícita para definir endpoints, especialmente em arquiteturas Vertical Slice onde cada endpoint é autocontido.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/Commands/CreateProduct/
├── CreateProductEndpoint.cs        # Endpoint estático, não Controller
└── CreateProductHandler.cs

Features/Products/Queries/GetAllProducts/
├── GetAllProductsEndpoint.cs       # Endpoint estático
└── GetAllProductsHandler.cs
```

**Problema:** Como definir endpoints HTTP de forma leve e explícita, mantendo cada endpoint próximo à sua lógica de negócio, sem overhead de classes Controller?

## Opções Consideradas

* **Controllers Tradicionais** - Classes Controller com actions (overhead, convenções)
* **Minimal APIs** - Endpoints estáticos com `MapPost`, `MapGet`, etc. (leve, explícito)
* **Endpoints com Attributes** - Usar `[HttpGet]` em métodos estáticos (híbrido)

## Decisão

**Escolhida:** "Minimal APIs", porque:

1. Mais leve: endpoints são métodos estáticos, sem necessidade de classes Controller
2. Mais explícito: cada endpoint define claramente seu método HTTP, rota e tipos de resposta
3. Alinha com Vertical Slice: cada endpoint fica próximo ao seu handler (mesma pasta)
4. Menos boilerplate: não requer herança de `ControllerBase` ou atributos
5. Type-safe: parâmetros e respostas são tipados, facilitando IntelliSense e validação

### Implementação Técnica

A decisão se materializa em:

**Endpoint Pattern:**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductEndpoint.cs
public static class CreateProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("Products");
    }

    private static async Task<Results<Created<ApiResponse<CreateProductResponse>>, BadRequest<ApiResponse>>> Handle(
        CreateProductCommand command,
        ISender sender,  // ← Mediator
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created(
                $"/api/products/{result.Value.ProductId}",
                new ApiResponse<CreateProductResponse>(result.Value))
            : TypedResults.BadRequest(new ApiResponse(result.Errors));
    }
}
```

**Registro de Endpoints:**
```csharp
// Features/Products/ProductFeature.cs
public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/products").WithTags("Products");
    
    CreateProductEndpoint.MapEndpoint(group);
    UpdateProductEndpoint.MapEndpoint(group);
    DeleteProductEndpoint.MapEndpoint(group);
    GetAllProductsEndpoint.MapEndpoint(group);
    GetProductByIdEndpoint.MapEndpoint(group);
    
    return app;
}

// Startup.cs
app.MapProductEndpoints();
```

**Estrutura de Endpoint:**
- Classe estática com método `MapEndpoint(IEndpointRouteBuilder)`
- Método `Handle` privado com parâmetros tipados
- Usa `ISender` (Mediator) para enviar commands/queries
- Retorna `Results<T1, T2>` para múltiplos tipos de resposta
- Define tipos de resposta com `.Produces<T>()`

**Vantagens sobre Controllers:**
- Sem herança necessária
- Sem convenções de roteamento implícitas
- Endpoints explícitos e próximos aos handlers
- Menos arquivos: endpoint e handler na mesma feature

### Consequências

* ✅ **Bom:** Mais leve: menos overhead que Controllers tradicionais
* ✅ **Bom:** Mais explícito: cada endpoint define claramente sua rota e tipos
* ✅ **Bom:** Alinha com Vertical Slice: endpoints próximos aos handlers
* ✅ **Bom:** Type-safe: parâmetros e respostas tipados
* ✅ **Bom:** Facilita testes: endpoints são métodos estáticos testáveis
* ⚠️ **Neutro:** Requer registro manual de endpoints (trade-off por explícito)
* ⚠️ **Ruim:** Pode ser mais verboso para endpoints muito simples
* ⚠️ **Ruim:** Algumas funcionalidades de Controllers (filters, model binding avançado) podem requerer configuração adicional

