# Adoção de Vertical Slice Architecture com Organização por Features

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Padrão de Organização Interna de Código

## Contexto e Problema

Aplicações podem ser organizadas horizontalmente (por tipo técnico: Controllers, Services, Repositories) ou verticalmente (por capacidade de negócio). A organização horizontal é tradicional em arquiteturas em camadas (Layered Architecture), mas pode criar distância semântica entre código que colabora funcionalmente.

A estrutura de cada feature no repositório mostra uma abordagem vertical:

```
Features/
├── Products/
│   ├── ProductFeature.cs        # Bootstrap e endpoints
│   ├── Commands/                # Casos de uso de escrita
│   │   ├── CreateProduct/
│   │   ├── UpdateProduct/
│   │   └── DeleteProduct/
│   ├── Queries/                 # Casos de uso de leitura
│   │   ├── GetAllProducts/
│   │   └── GetProductById/
│   ├── Domain/                  # Entidades e regras de negócio
│   │   ├── Product.cs
│   │   └── IProductRepository.cs
│   └── Infrastructure/          # Implementações técnicas
│       └── ProductRepository.cs
```

**Problema:** Como organizar código para maximizar coesão funcional e facilitar navegação durante desenvolvimento?

## Opções Consideradas

* **Horizontal (Layered Architecture)** - Pastas: `Controllers/`, `Services/`, `Domain/`, `Infrastructure/`
* **Vertical por Feature (Vertical Slice)** - Pastas: `Products/`, `Orders/`, cada uma com suas camadas internas
* **Híbrido (Features + Shared Layers)** - Features verticais + camada `Shared/` para infraestrutura comum

## Decisão

**Escolhida:** "Vertical por Feature com Shared Kernel", porque:

1. **Coesão Funcional:** Todo código relacionado a Products está em `Features/Products/`
2. **Autonomia de Features:** Cada feature pode evoluir independentemente
3. **Facilidade de Navegação:** Desenvolvedores encontram tudo relacionado a uma feature em um só lugar
4. **Redução de Acoplamento:** Features não compartilham Services ou Controllers — apenas infraestrutura essencial
5. **Preparação para Extração:** Features auto-contidas podem se tornar microserviços no futuro

### Implementação Técnica

**Estrutura de uma Feature Completa:**

```
Features/CashRegister/
├── CashRegisterFeature.cs       # Bootstrap (DI + Endpoint Mapping)
│
├── Commands/                    # Casos de Uso de Escrita (CUD)
│   ├── CreateCashSession/
│   │   ├── CreateCashSessionCommand.cs
│   │   ├── CreateCashSessionHandler.cs
│   │   ├── CreateCashSessionValidator.cs
│   │   ├── CreateCashSessionEndpoint.cs
│   │   └── CreateCashSessionResponse.cs
│   ├── CloseCashSession/
│   └── AddCashDeposit/
│
├── Queries/                     # Casos de Uso de Leitura (R)
│   ├── GetCashSessionById/
│   │   ├── GetCashSessionByIdQuery.cs
│   │   ├── GetCashSessionByIdHandler.cs
│   │   └── GetCashSessionByIdEndpoint.cs
│   ├── GetActiveCashSession/
│   └── GetCashSessionDeposits/
│
├── Domain/                      # Lógica de Negócio
│   ├── CashSession.cs           # Aggregate Root
│   ├── CashDeposit.cs           # Entity
│   └── CashSessionStatus.cs     # Value Object (enum)
│
├── Events/                      # Domain Events (se aplicável)
│   └── CashSessionClosedEvent.cs
│
└── Infrastructure/              # Implementações Técnicas
    ├── ICashSessionRepository.cs
    ├── CashSessionRepository.cs
    └── CashSessionConfiguration.cs  # EF Core mapping
```

**Bootstrap da Feature (CashRegisterFeature.cs):**

```csharp
public static class CashRegisterFeature
{
    // Registro de Dependências
    public static IServiceCollection AddCashRegisterFeature(this IServiceCollection services)
    {
        services.AddScoped<ICashSessionRepository, CashSessionRepository>();
        
        // Handlers são registrados automaticamente via Mediator Source Generator
        return services;
    }

    // Mapeamento de Endpoints
    public static IEndpointRouteBuilder MapCashRegisterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cash-register").WithTags("Cash Register");

        CreateCashSessionEndpoint.MapEndpoint(group);
        CloseCashSessionEndpoint.MapEndpoint(group);
        // ... outros endpoints
        
        return app;
    }
}
```

**Invocação no Startup:**

```csharp
// Startup.cs - ConfigureBuilder()
services.AddCashRegisterFeature();
services.AddOrderFeature();
services.AddProductFeature();

// Startup.cs - ConfigureApp()
app.MapCashRegisterEndpoints();
app.MapOrderEndpoints();
app.MapProductEndpoints();
```

**Shared Kernel (Infraestrutura Comum):**

```
Shared/
├── Infrastructure/
│   ├── Persistence/             # DbContext, Migrations, UnitOfWork
│   ├── Identity/                # ASP.NET Identity
│   ├── Tenancy/                 # Multi-tenancy
│   └── Authorization/           # Policies, Handlers
├── Application/
│   ├── Behaviors/               # Mediator Behaviors (Validation, Logging)
│   └── Errors/                  # GlobalExceptionHandler
└── SeedWork/                    # Base classes DDD
    ├── Entity.cs
    ├── IDomainEvent.cs
    └── Money.cs
```

### Consequências

* ✅ **Bom:** Alta coesão — código que muda junto está junto
* ✅ **Bom:** Fácil onboarding — novos devs navegam por feature, não por camadas técnicas
* ✅ **Bom:** Testes espelham a estrutura — `test/Features/Products/` testa `src/Features/Products/`
* ✅ **Bom:** Features podem ter implementações técnicas diferentes (ex: Products usa EF Core, Orders usa Dapper)
* ✅ **Bom:** Reduz merge conflicts — times diferentes trabalham em features diferentes
* ⚠️ **Neutro:** Leve duplicação de código entre features (aceitável se consciente)
* ⚠️ **Ruim:** Desenvolvedores podem criar dependências diretas entre features (mitigado por code review)
* ⚠️ **Ruim:** Refatorações cross-feature requerem tocar múltiplas pastas

### Regras de Design

1. **Features não devem referenciar outras Features diretamente**
   - ❌ `Orders/` importando `Products/Domain/Product.cs`
   - ✅ `Orders/` comunicando via Mediator: `mediator.Send(new GetProductByIdQuery(id))`

2. **Shared/ é para infraestrutura técnica, não domínio**
   - ✅ `Shared/Infrastructure/Persistence/UnitOfWork.cs`
   - ❌ `Shared/Domain/CommonEntities/` (anti-pattern)

3. **Cada Feature é responsável por seus próprios Endpoints**
   - Não há pasta global `Controllers/`

**Princípio:** "Code that changes together, lives together. Organize by feature (business capability), not by technical layer."
