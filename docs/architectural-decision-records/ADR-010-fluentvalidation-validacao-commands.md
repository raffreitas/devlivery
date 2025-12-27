# FluentValidation para Validação de Commands

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Validação de Dados

## Contexto e Problema

Validação de dados de entrada é crítica para segurança e consistência, mas implementá-la manualmente em cada handler é repetitivo e propenso a erros. Além disso, validação deve ocorrer antes que handlers executem, para evitar processamento desnecessário de dados inválidos.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/Commands/CreateProduct/
├── CreateProductCommand.cs
├── CreateProductCommandValidator.cs    # FluentValidation
└── CreateProductHandler.cs

Shared/Application/Behaviors/
└── ValidationPipelineBehavior.cs     # Executa validação antes do handler
```

**Problema:** Como validar commands de forma declarativa e reutilizável, garantindo que validação ocorra antes da execução de handlers, sem adicionar código repetitivo?

## Opções Consideradas

* **Validação Manual em Handlers** - Validar manualmente em cada handler (repetitivo, propenso a erros)
* **Data Annotations** - Usar atributos `[Required]`, `[MaxLength]`, etc. (limitado, não funciona bem com records)
* **FluentValidation** - Biblioteca declarativa para validação (flexível, type-safe)
* **Validação no Endpoint** - Validar no endpoint antes de enviar ao handler (duplicação)

## Decisão

**Escolhida:** "FluentValidation", porque:

1. Declarativo: regras de validação são expressas de forma clara e legível
2. Type-safe: validators são classes tipadas, facilitando IntelliSense e refatoração
3. Reutilizável: validators podem ser compostos e reutilizados
4. Integração com Mediator: `ValidationPipelineBehavior` executa validação automaticamente antes de handlers
5. Mensagens customizadas: permite mensagens de erro específicas e localizadas

### Implementação Técnica

A decisão se materializa em:

**Command com Validator:**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductCommand.cs
public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available) : ICommand<Result<CreateProductResponse>>;

// Features/Products/Commands/CreateProduct/CreateProductCommandValidator.cs
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(200).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(1000).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O campo '{PropertyName}' deve ser maior que {ComparisonValue}.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(100).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
    }
}
```

**Pipeline Behavior (Validação Automática):**
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

        return await next();
    }
}
```

**Registro:**
```csharp
// Startup.cs
services.AddValidatorsFromAssembly(typeof(Startup).Assembly);

services.AddMediator(options =>
{
    options.PipelineBehaviors = [
        typeof(ValidationPipelineBehavior<,>),  // ← Validação antes de handlers
        typeof(DomainEventTenantBehavior<,>)
    ];
});
```

**Tratamento de Erros:**
```csharp
// Shared/Infrastructure/WebServer/GlobalExceptionHandler.cs
// Captura ValidationException e retorna 400 Bad Request com erros
```

**Fluxo:**
1. Endpoint recebe `CreateProductCommand`
2. `ValidationPipelineBehavior` intercepta antes do handler
3. `CreateProductCommandValidator` valida o command
4. Se válido: handler executa
5. Se inválido: retorna 400 Bad Request com lista de erros

### Consequências

* ✅ **Bom:** Validação declarativa e legível
* ✅ **Bom:** Type-safe: validators são classes tipadas
* ✅ **Bom:** Automático: validação ocorre antes de handlers via pipeline behavior
* ✅ **Bom:** Reutilizável: validators podem ser compostos
* ✅ **Bom:** Mensagens customizadas e localizadas
* ⚠️ **Neutro:** Requer criar validator para cada command (trade-off por type-safety)
* ⚠️ **Ruim:** Validação de regras de negócio complexas pode ser limitada (devem estar em domain)
* ⚠️ **Ruim:** Validators são descobertos automaticamente, mas precisam seguir convenção de nomenclatura

