# Pipeline Behaviors no Mediator

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Padrão de Design / Cross-Cutting Concerns

## Contexto e Problema

Cross-cutting concerns como validação, logging, tenancy e tratamento de erros precisam ser aplicados a todos os commands e queries, mas implementá-los em cada handler é repetitivo e propenso a erros. Além disso, alguns comportamentos (como registro de tenant em domain events) precisam ocorrer em pontos específicos do pipeline.

A estrutura do repositório revela esta decisão através da organização:

```
Shared/Application/Behaviors/
└── ValidationPipelineBehavior.cs    # Validação antes de handlers

Shared/Infrastructure/Tenancy/Behaviors/
└── DomainEventTenantBehavior.cs     # Registra tenant em domain events
```

**Problema:** Como aplicar cross-cutting concerns (validação, logging, tenancy) a todos os commands e queries de forma centralizada, sem duplicar código em cada handler?

## Opções Consideradas

* **Implementação em Cada Handler** - Adicionar código em cada handler (repetitivo, propenso a erros)
* **Base Handler Class** - Criar classe base com comportamentos (herança, menos flexível)
* **Pipeline Behaviors** - Interceptar requests antes/depois de handlers (centralizado, flexível)
* **Middleware ASP.NET Core** - Usar middleware HTTP (não funciona para lógica de aplicação)

## Decisão

**Escolhida:** "Pipeline Behaviors", porque:

1. Centralizado: comportamentos são aplicados automaticamente a todos os requests
2. Flexível: permite adicionar/remover behaviors sem modificar handlers
3. Ordenável: behaviors podem ser executados em ordem específica
4. Testável: behaviors podem ser testados isoladamente
5. Alinha com Mediator Pattern: pipeline é parte natural do mediator

### Implementação Técnica

A decisão se materializa em:

**Validation Pipeline Behavior:**
```csharp
// Shared/Application/Behaviors/ValidationPipelineBehavior.cs
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async ValueTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        // Validação antes do handler
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();  // ← Executa handler se válido
    }
}
```

**Domain Event Tenant Behavior:**
```csharp
// Shared/Infrastructure/Tenancy/Behaviors/DomainEventTenantBehavior.cs
public sealed class DomainEventTenantBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ITenantAccessor _tenantAccessor;

    public async ValueTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        var response = await next();  // ← Executa handler primeiro

        // Após handler, registra tenant em domain events
        if (response is Result result && result.IsSuccess)
        {
            // Lógica para registrar tenant em domain events
        }

        return response;
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
        typeof(ValidationPipelineBehavior<,>),        // ← 1. Validação primeiro
        typeof(DomainEventTenantBehavior<,>)          // ← 2. Tenant depois
    ];
});
```

**Ordem de Execução:**
1. `ValidationPipelineBehavior` → valida request
2. `DomainEventTenantBehavior` → registra tenant
3. Handler → executa lógica de negócio
4. `DomainEventTenantBehavior` → pós-processamento (se necessário)
5. Retorna resposta

**Exemplo de Logging Behavior (futuro):**
```csharp
public sealed class LoggingPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("Executando {RequestType}", typeof(TRequest).Name);
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            stopwatch.Stop();
            _logger.LogInformation("Completado {RequestType} em {Elapsed}ms", 
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao executar {RequestType}", typeof(TRequest).Name);
            throw;
        }
    }
}
```

### Consequências

* ✅ **Bom:** Centralizado: comportamentos aplicados automaticamente a todos os requests
* ✅ **Bom:** Flexível: permite adicionar/remover behaviors sem modificar handlers
* ✅ **Bom:** Ordenável: behaviors executam em ordem específica
* ✅ **Bom:** Testável: behaviors podem ser testados isoladamente
* ✅ **Bom:** Alinha com Mediator Pattern: pipeline é parte natural do mediator
* ⚠️ **Neutro:** Adiciona camada de indireção (trade-off por centralização)
* ⚠️ **Ruim:** Debug pode ser mais difícil (múltiplas camadas)
* ⚠️ **Ruim:** Ordem de behaviors é importante e deve ser documentada

