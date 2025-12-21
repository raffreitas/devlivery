# FluentValidation para Validação Declarativa de Requests

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Estratégia de Validação de Entrada

## Contexto e Problema

Aplicações precisam validar dados de entrada antes de processar lógica de negócio. Validações podem ser implementadas com Data Annotations (atributos), validações manuais em código imperativo, ou bibliotecas declarativas como FluentValidation. A escolha impacta legibilidade, testabilidade e separação de responsabilidades.

A configuração do projeto revela uso de FluentValidation:

```xml
<!-- Devlivery.csproj -->
<PackageReference Include="FluentValidation" Version="12.1.1"/>
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1"/>
```

```csharp
// Startup.cs
services.AddValidatorsFromAssembly(typeof(Startup).Assembly);

services.AddMediator(options =>
{
    options.PipelineBehaviors =
    [
        typeof(DomainEventTenantBehavior<,>),
        typeof(ValidationPipelineBehavior<,>)  // ← Pipeline Behavior
    ];
});
```

**Problema:** Como validar requests de forma declarativa, testável e reutilizável sem poluir modelos de domínio?

## Opções Consideradas

* **Data Annotations** - Atributos nos DTOs (`[Required]`, `[Range]`)
* **Validação Manual** - Código imperativo em cada handler
* **FluentValidation** - Biblioteca declarativa com classes de validação separadas
* **Guard Clauses** - Validações inline no domínio (ex: `Guard.Against.Null()`)

## Decisão

**Escolhida:** "FluentValidation com Pipeline Behavior", porque:

1. **Separação de Responsabilidades:** Validações não poluem DTOs ou domínio
2. **Declaratividade:** Regras expressas em linguagem fluente e legível
3. **Testabilidade:** Validators são classes independentes, fáceis de testar
4. **Reutilização:** Validadores podem ser compostos e herdados
5. **Integração com Mediator:** Pipeline Behavior valida automaticamente antes de executar handler

### Implementação Técnica

**Estrutura de Validação:**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductValidator.cs
using FluentValidation;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThanOrEqualTo(999999.99m).WithMessage("Price exceeds maximum allowed value");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(BeValidCategory).WithMessage("Invalid category");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));  // Validação condicional
    }

    private static bool BeValidCategory(string category)
    {
        var validCategories = new[] { "Food", "Beverage", "Dessert", "Appetizer" };
        return validCategories.Contains(category);
    }
}
```

**Registro Automático (DI):**

```csharp
// Startup.cs - ConfigureBuilder()
services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
// Registra TODOS os validators automaticamente (busca classes que herdam AbstractValidator<T>)
```

**Pipeline Behavior (Execução Automática):**

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
        // Se não há validators para este request, pula validação
        if (!validators.Any())
        {
            return await next(message, cancellationToken);
        }

        // Executa TODOS os validators para o request
        var context = new ValidationContext<TRequest>(message);
        
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Coleta todos os erros
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Se há falhas, lança exceção (capturada por GlobalExceptionHandler)
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Validação passou — continua para o handler
        return await next(message, cancellationToken);
    }
}
```

**Global Exception Handler (ASP.NET Core):**

```csharp
// Shared/Application/Errors/GlobalExceptionHandler.cs
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            await httpContext.Response.WriteAsJsonAsync(new
            {
                Type = "ValidationError",
                Title = "One or more validation errors occurred",
                Status = 400,
                Errors = errors
            }, cancellationToken);

            return true;  // Exceção tratada
        }

        return false;  // Deixa outros handlers processarem
    }
}
```

**Resposta HTTP de Erro de Validação:**

```json
{
  "type": "ValidationError",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
    "Name": ["Product name is required"],
    "Price": ["Price must be greater than zero"],
    "Category": ["Invalid category"]
  }
}
```

**Fluxo de Execução:**

```
HTTP Request (CreateProductCommand)
    ↓
Mediator.Send()
    ↓
[Pipeline Behavior: DomainEventTenantBehavior] ✓
    ↓
[Pipeline Behavior: ValidationPipelineBehavior]
    ├─ CreateProductValidator.Validate() ✓
    └─ Se falhar: throw ValidationException → GlobalExceptionHandler → HTTP 400
    ↓
CreateProductHandler.Handle() (só executa se validação passou)
    ↓
Repository → Database
    ↓
HTTP 200 OK
```

### Exemplos de Validações Avançadas

**1. Validações Assíncronas (ex: verificar unicidade):**

```csharp
public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MustAsync(async (command, name, ct) =>
            {
                var existingProduct = await productRepository.GetByNameAsync(name, ct);
                return existingProduct == null || existingProduct.Id == command.ProductId;
            })
            .WithMessage("Product name already exists");
    }
}
```

**2. Validações Condicionais:**

```csharp
RuleFor(x => x.DiscountPercentage)
    .InclusiveBetween(0, 100)
    .When(x => x.HasDiscount);  // Só valida se HasDiscount == true
```

**3. Validações Compostas:**

```csharp
// Reutilização de validadores
RuleFor(x => x.Address)
    .SetValidator(new AddressValidator());  // Valida objeto complexo
```

**4. Validações de Coleções:**

```csharp
RuleForEach(x => x.OrderItems)
    .ChildRules(item =>
    {
        item.RuleFor(i => i.Quantity).GreaterThan(0);
        item.RuleFor(i => i.ProductId).NotEmpty();
    });
```

**5. Validações com Mensagens Customizadas:**

```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("O email é obrigatório")
    .EmailAddress().WithMessage("Formato de email inválido")
    .WithName("E-mail");  // Nome customizado para a propriedade
```

### Consequências

* ✅ **Bom:** Validações declarativas e legíveis (DSL fluente)
* ✅ **Bom:** Separação clara — validators não poluem domínio ou DTOs
* ✅ **Bom:** Testabilidade — validators testáveis isoladamente
* ✅ **Bom:** Reutilização — validators podem ser compostos
* ✅ **Bom:** Validação automática via Pipeline Behavior (zero código nos handlers)
* ✅ **Bom:** Mensagens de erro estruturadas (JSON com erros por campo)
* ⚠️ **Neutro:** Mais classes (um validator por command)
* ⚠️ **Ruim:** Curva de aprendizado — sintaxe específica da biblioteca
* ⚠️ **Ruim:** Validações assíncronas podem impactar performance (mitigado por cache)

### Quando NÃO Usar FluentValidation

**Use Validação de Domínio (Guard Clauses) para:**
- Invariantes de negócio que SEMPRE devem ser verdadeiras
- Validações que são parte da lógica de domínio (não apenas input)

```csharp
// Domain/Order.cs
public void AddItem(OrderItem item)
{
    if (Status != OrderStatus.Pending)
        throw new DomainException("Cannot add items to non-pending orders");
    
    if (_items.Count >= 100)
        throw new DomainException("Maximum 100 items per order");
    
    _items.Add(item);
}
```

**Use FluentValidation para:**
- Validar formato de entrada (campos obrigatórios, tamanhos, regex)
- Validações de API/DTO (antes de chegar no domínio)
- Regras de negócio que podem mudar frequentemente

### Teste Unitário de Validators

```csharp
[Fact]
public void Should_Have_Error_When_Name_Is_Empty()
{
    // Arrange
    var validator = new CreateProductValidator();
    var command = new CreateProductCommand(
        Name: "",  // Inválido
        Description: "Test",
        Price: 10.0m,
        Category: "Food",
        Available: true
    );

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.ShouldBeFalse();
    result.Errors.ShouldContain(e => e.PropertyName == "Name");
    result.Errors.ShouldContain(e => e.ErrorMessage == "Product name is required");
}

[Fact]
public void Should_Not_Have_Error_When_Valid()
{
    // Arrange
    var validator = new CreateProductValidator();
    var command = new CreateProductCommand(
        Name: "Valid Product",
        Description: "Description",
        Price: 10.0m,
        Category: "Food",
        Available: true
    );

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.ShouldBeTrue();
}
```

### Convenções do Projeto

**Nomenclatura:**
- Validators: `{CommandName}Validator` (ex: `CreateProductValidator`)
- Localização: Mesma pasta do Command/Query

**Mensagens de Erro:**
- Inglês (padrão da API)
- Descritivas e acionáveis
- Evitar jargão técnico

**Validações por Camada:**

| Camada           | Tipo de Validação             | Ferramenta          |
|------------------|-------------------------------|---------------------|
| **API/DTO**      | Formato, obrigatoriedade      | FluentValidation    |
| **Domain**       | Invariantes de negócio        | Guard Clauses       |
| **Database**     | Constraints, unique, FK       | PostgreSQL          |

**Princípio:** "Validate input at the boundary. Use FluentValidation for API requests, domain logic for business invariants."

### Referências

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- Martin Fowler: [Replacing Throwing Exceptions with Notification in Validations](https://martinfowler.com/articles/replaceThrowWithNotification.html)
