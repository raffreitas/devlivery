# Minimal APIs com Mapeamento de Endpoints por Feature

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Estilo de API e Organização de Endpoints HTTP

## Contexto e Problema

ASP.NET Core oferece duas abordagens para criar APIs: Controllers (padrão MVC tradicional) e Minimal APIs (introduzido no .NET 6). Controllers são familiares e bem estruturados, mas adicionam overhead de classes e convenções. Minimal APIs são mais leves e permitem definir endpoints com lambdas ou métodos locais.

A estrutura do código revela uso de Minimal APIs:

```csharp
// Features/Products/ProductFeature.cs
public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/products").WithTags("Products");

    CreateProductEndpoint.MapEndpoint(group);
    DeleteProductEndpoint.MapEndpoint(group);
    UpdateProductEndpoint.MapEndpoint(group);
    GetAllProductsEndpoint.MapEndpoint(group);
    GetProductByIdEndpoint.MapEndpoint(group);

    return app;
}
```

```csharp
// Features/Products/Commands/CreateProduct/CreateProductEndpoint.cs
public static class CreateProductEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            CreateProductCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("CreateProduct")
        .RequireAuthorization();
    }
}
```

**Problema:** Como estruturar endpoints HTTP de forma coesa com features, mantendo simplicidade e performance?

## Opções Consideradas

* **Controllers Tradicionais** - Classes com atributos `[ApiController]`, métodos com `[HttpGet]`, etc.
* **Minimal APIs com Lambdas na Startup** - Todos endpoints definidos em `Program.cs`
* **Minimal APIs por Feature** - Endpoints agrupados por feature em classes estáticas
* **REPR Pattern** - Request-Endpoint-Response pattern (endpoints como classes)

## Decisão

**Escolhida:** "Minimal APIs por Feature com classes de Endpoint separadas", porque:

1. **Coesão:** Endpoints ficam junto com Commands/Queries da feature
2. **Performance:** Minimal APIs têm menos overhead que Controllers
3. **Simplicidade:** Sem herança de classes, atributos ou convenções complexas
4. **Organização:** Um arquivo por endpoint facilita navegação
5. **Integração com OpenAPI:** Suporte nativo via `.WithOpenApi()`

### Implementação Técnica

**Estrutura de uma Feature Completa:**

```
Features/Products/
├── ProductFeature.cs                 # Bootstrap (DI + Endpoint Mapping)
│
├── Commands/
│   └── CreateProduct/
│       ├── CreateProductCommand.cs
│       ├── CreateProductHandler.cs
│       ├── CreateProductValidator.cs
│       ├── CreateProductEndpoint.cs  # ← Minimal API Endpoint
│       └── CreateProductResponse.cs
│
└── Queries/
    └── GetProductById/
        ├── GetProductByIdQuery.cs
        ├── GetProductByIdHandler.cs
        ├── GetProductByIdEndpoint.cs  # ← Minimal API Endpoint
        └── GetProductByIdResponse.cs
```

**Feature Bootstrap (Registro de Endpoints):**

```csharp
// Features/Products/ProductFeature.cs
public static class ProductFeature
{
    // Registro de Dependências
    public static IServiceCollection AddProductFeature(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }

    // Mapeamento de Endpoints
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")                    // Swagger/OpenAPI tag
            .WithOpenApi();                          // Gera documentação OpenAPI

        // Mapeia cada endpoint
        CreateProductEndpoint.MapEndpoint(group);
        UpdateProductEndpoint.MapEndpoint(group);
        DeleteProductEndpoint.MapEndpoint(group);
        GetAllProductsEndpoint.MapEndpoint(group);
        GetProductByIdEndpoint.MapEndpoint(group);

        return app;
    }
}
```

**Exemplo de Endpoint (POST):**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductEndpoint.cs
public static class CreateProductEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", Handler)
            .WithName("CreateProduct")               // Nome da operação (OpenAPI)
            .WithSummary("Create a new product")     // Descrição curta
            .WithDescription("Creates a new product in the establishment catalog")
            .Produces<CreateProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();                 // Requer autenticação

        static async Task<IResult> Handler(
            CreateProductCommand command,
            IMediator mediator,
            CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        }
    }
}
```

**Exemplo de Endpoint (GET com Route Parameter):**

```csharp
// Features/Products/Queries/GetProductById/GetProductByIdEndpoint.cs
public static class GetProductByIdEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handler)
            .WithName("GetProductById")
            .WithSummary("Get product by ID")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        static async Task<IResult> Handler(
            Guid id,                                  // Route parameter
            IMediator mediator,
            CancellationToken ct)
        {
            var query = new GetProductByIdQuery(id);
            var result = await mediator.Send(query, ct);
            
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { Message = "Product not found" });
        }
    }
}
```

**Exemplo de Endpoint (GET com Query Parameters):**

```csharp
// Features/Products/Queries/GetAllProducts/GetAllProductsEndpoint.cs
public static class GetAllProductsEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/", Handler)
            .WithName("GetAllProducts")
            .WithSummary("List all products")
            .Produces<List<GetAllProductsResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        static async Task<IResult> Handler(
            [AsParameters] GetAllProductsQuery query,  // ← Bind query string
            IMediator mediator,
            CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return Results.Ok(result.Value);
        }
    }
}
```

**Query com Parâmetros (Model Binding):**

```csharp
// Features/Products/Queries/GetAllProducts/GetAllProductsQuery.cs
public sealed record GetAllProductsQuery(
    [property: FromQuery] bool? AvailableOnly,        // ?availableOnly=true
    [property: FromQuery] string? Category,           // ?category=Food
    [property: FromQuery] int Page = 1,               // ?page=1
    [property: FromQuery] int PageSize = 20           // ?pageSize=20
) : IQuery<Result<List<GetAllProductsResponse>>>;
```

**Invocação no Startup:**

```csharp
// Startup.cs - ConfigureApp()
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapCashRegisterEndpoints();
app.MapAuthEndpoints();
```

**Configuração de OpenAPI (Swagger):**

```csharp
// Shared/Infrastructure/WebServer/OpenApiConfiguration.cs
public static class OpenApiConfiguration
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();  // .NET 10 built-in OpenAPI
        
        return services;
    }

    public static IApplicationBuilder UseOpenApiConfiguration(this IApplicationBuilder app)
    {
        app.MapOpenApi();                         // /openapi/v1.json
        app.MapScalarApiReference();              // Scalar UI (moderna alternativa ao Swagger UI)
        
        return app;
    }
}
```

**Scalar API Reference (UI Moderna):**

```xml
<PackageReference Include="Scalar.AspNetCore" Version="2.11.6"/>
```

```csharp
app.MapScalarApiReference(options =>
{
    options.Title = "Devlivery API";
    options.Theme = ScalarTheme.Purple;
    options.ShowSidebar = true;
});
// Acesso: http://localhost:8080/scalar/v1
```

### Vantagens sobre Controllers

**Minimal APIs:**
```csharp
group.MapPost("/", async (CreateProductCommand cmd, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(cmd, ct);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
});
```

**Controllers (Equivalente):**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ProductsController(IMediator mediator) => _mediator = mediator;
    
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}
```

**Comparação:**

| Aspecto               | Minimal APIs         | Controllers           |
|-----------------------|----------------------|-----------------------|
| **Boilerplate**       | ✅ Mínimo            | ❌ Herança, atributos |
| **Performance**       | ✅ Sem reflection    | ❌ Reflection pesada  |
| **Colocação**         | ✅ Por feature       | ❌ Pasta separada     |
| **Testabilidade**     | ✅ Handler isolado   | ⚠️ Precisa mockar ControllerBase |
| **Familiaridade**     | ⚠️ Novo (2021+)      | ✅ Tradicional        |
| **IntelliSense**      | ⚠️ Menos suporte IDE | ✅ Excelente          |

### Consequências

* ✅ **Bom:** Endpoints coesos com features (arquitetura vertical slice)
* ✅ **Bom:** Menos boilerplate que Controllers
* ✅ **Bom:** Performance superior (sem reflection)
* ✅ **Bom:** Integração nativa com OpenAPI/Swagger
* ✅ **Bom:** Fácil testar handlers isoladamente (apenas funções)
* ✅ **Bom:** Route Groups facilitam prefixos e configurações comuns
* ⚠️ **Neutro:** Menos familiar para devs acostumados com MVC
* ⚠️ **Ruim:** IntelliSense menos robusto que Controllers (melhorias contínuas no .NET)
* ⚠️ **Ruim:** Sem convenções automáticas (precisa mapear tudo explicitamente)

### Padrões de Roteamento

**Route Groups (Prefixos Comuns):**

```csharp
var productsGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .RequireAuthorization();  // Aplica a TODOS os endpoints do grupo

productsGroup.MapGet("/", GetAll);
productsGroup.MapGet("/{id}", GetById);
productsGroup.MapPost("/", Create);
productsGroup.MapPut("/{id}", Update);
productsGroup.MapDelete("/{id}", Delete);
```

**Versionamento (Futuro):**

```csharp
var v1 = app.MapGroup("/api/v1");
v1.MapProductEndpoints();

var v2 = app.MapGroup("/api/v2");
v2.MapProductEndpointsV2();
```

**Subrotas:**

```csharp
var ordersGroup = app.MapGroup("/api/orders");
ordersGroup.MapGet("/", GetAllOrders);
ordersGroup.MapGet("/{id}", GetOrderById);

// Subrota: /api/orders/{orderId}/items
ordersGroup.MapGroup("/{orderId:guid}/items")
    .MapGet("/", GetOrderItems)
    .MapPost("/", AddOrderItem);
```

### Teste de Endpoints

**Teste de Integração (WebApplicationFactory):**

```csharp
[Fact]
public async Task CreateProduct_Should_Return_Ok_When_Valid()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreateProductCommand("Test Product", "Description", 10.0m, "Food", true);

    // Act
    var response = await client.PostAsJsonAsync("/api/products", command);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<CreateProductResponse>();
    result.ShouldNotBeNull();
    result.ProductId.ShouldNotBe(Guid.Empty);
}
```

**Teste Unitário (Handler):**

```csharp
[Fact]
public async Task Handler_Should_Create_Product()
{
    // Testa o handler diretamente (sem HTTP)
    var handler = new CreateProductHandler(repository, unitOfWork, tenantAccessor);
    var command = new CreateProductCommand(...);
    
    var result = await handler.Handle(command, CancellationToken.None);
    
    result.IsSuccess.ShouldBeTrue();
}
```

### OpenAPI/Swagger Output

```json
{
  "openapi": "3.0.1",
  "paths": {
    "/api/products": {
      "post": {
        "tags": ["Products"],
        "summary": "Create a new product",
        "operationId": "CreateProduct",
        "requestBody": {
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/CreateProductCommand" }
            }
          }
        },
        "responses": {
          "200": {
            "content": {
              "application/json": {
                "schema": { "$ref": "#/components/schemas/CreateProductResponse" }
              }
            }
          },
          "400": { "description": "Bad Request" }
        },
        "security": [{ "Bearer": [] }]
      }
    }
  }
}
```

**Princípio:** "Use Minimal APIs for modern, performance-focused APIs. Organize endpoints by feature for maximum cohesion."

### Referências

- [Minimal APIs Overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview)
- [Route Groups](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups)
- [OpenAPI Support](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi)
- [Scalar API Documentation](https://github.com/scalar/scalar)
