# Feature-based Dependency Injection

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Padrão de Design / Inversão de Controle

## Contexto e Problema

Em arquiteturas Vertical Slice, cada feature é autocontida e possui suas próprias dependências (repositories, handlers, validators). Registrar todas as dependências em um único local (como `Startup.cs`) pode ficar desorganizado e difícil de manter conforme o projeto cresce.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/
└── ProductFeature.cs              # Bootstrap da feature
    ├── AddProductFeature()         # Registra dependências
    └── MapProductEndpoints()       # Mapeia endpoints

Startup.cs
├── services.AddProductFeature()    # Registra feature
└── app.MapProductEndpoints()       # Mapeia endpoints
```

**Problema:** Como organizar registro de dependências e mapeamento de endpoints de forma que cada feature seja responsável por seu próprio bootstrap, facilitando manutenção e descoberta?

## Opções Consideradas

* **Registro Centralizado** - Tudo em `Startup.cs` (desorganizado em projetos grandes)
* **Módulos por Assembly** - Usar bibliotecas como Scrutor (complexidade desnecessária)
* **Feature Bootstrap Classes** - Cada feature tem classe `XxxFeature` com métodos de extensão (explícito, organizado)
* **Auto-discovery** - Descoberta automática via reflection (menos explícito, mais mágico)

## Decisão

**Escolhida:** "Feature Bootstrap Classes", porque:

1. Explícito: cada feature declara claramente suas dependências e endpoints
2. Organizado: dependências ficam próximas ao código da feature
3. Fácil de manter: adicionar/remover features é simples
4. Descoberta: desenvolvedores encontram dependências facilmente
5. Alinha com Vertical Slice: cada feature é responsável por seu próprio bootstrap

### Implementação Técnica

A decisão se materializa em:

**Feature Bootstrap Class:**
```csharp
// Features/Products/ProductFeature.cs
public static class ProductFeature
{
    // Registra dependências da feature
    public static IServiceCollection AddProductFeature(this IServiceCollection services)
    {
        // Registra repositories
        services.AddScoped<IProductRepository, ProductRepository>();

        // Handlers são descobertos automaticamente pelo Mediator
        // Validators são descobertos automaticamente pelo FluentValidation
        // Não precisa registrar manualmente

        return services;
    }

    // Mapeia endpoints da feature
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // Registra cada endpoint
        CreateProductEndpoint.MapEndpoint(group);
        UpdateProductEndpoint.MapEndpoint(group);
        DeleteProductEndpoint.MapEndpoint(group);
        GetAllProductsEndpoint.MapEndpoint(group);
        GetProductByIdEndpoint.MapEndpoint(group);

        return app;
    }
}
```

**Registro no Startup:**
```csharp
// Startup.cs
public static void ConfigureBuilder(WebApplicationBuilder builder)
{
    var services = builder.Services;

    // Shared Features (infraestrutura)
    services.AddIdentityFeature(configuration);
    services.AddDatabaseFeature(configuration);
    services.AddAuthorizationFeature();
    services.AddTenancyFeature();

    // Business Features
    services.AddAuthFeature();
    services.AddOrderFeature();
    services.AddProductFeature();      // ← Registra dependências
    services.AddCashRegisterFeature();
    services.AddExpenseFeature();
    services.AddDashboardFeature();
}

public static void ConfigureApp(WebApplication app)
{
    // Endpoints
    app.MapAuthEndpoints();
    app.MapProductEndpoints();         // ← Mapeia endpoints
    app.MapOrderEndpoints();
    app.MapCashRegisterEndpoints();
    app.MapExpenseEndpoints();
    app.MapDashboardEndpoints();
}
```

**Padrão de Nomenclatura:**
- `AddXxxFeature()` → registra dependências (DI)
- `MapXxxEndpoints()` → mapeia endpoints HTTP
- Classe `XxxFeature` → bootstrap da feature

**Vantagens:**
- **Explícito:** Cada feature declara suas dependências
- **Organizado:** Dependências ficam próximas ao código
- **Fácil de Manter:** Adicionar/remover features é simples
- **Descoberta:** Desenvolvedores encontram dependências facilmente

**Exemplo de Nova Feature:**
```csharp
// 1. Criar estrutura da feature
Features/Inventory/
├── InventoryFeature.cs
├── Commands/
├── Queries/
└── ...

// 2. Implementar bootstrap
public static class InventoryFeature
{
    public static IServiceCollection AddInventoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory");
        // ... endpoints
        return app;
    }
}

// 3. Registrar no Startup
services.AddInventoryFeature();
app.MapInventoryEndpoints();
```

### Consequências

* ✅ **Bom:** Explícito: cada feature declara suas dependências claramente
* ✅ **Bom:** Organizado: dependências ficam próximas ao código da feature
* ✅ **Bom:** Fácil de manter: adicionar/remover features é simples
* ✅ **Bom:** Descoberta: desenvolvedores encontram dependências facilmente
* ✅ **Bom:** Alinha com Vertical Slice: cada feature é responsável por seu bootstrap
* ⚠️ **Neutro:** Requer criar classe `XxxFeature` para cada feature (trade-off por organização)
* ⚠️ **Ruim:** Registro manual no `Startup.cs` (mas explícito e fácil de encontrar)
* ⚠️ **Ruim:** Pode ser esquecido registrar feature no `Startup.cs` (mas compilador ajuda com métodos de extensão)

