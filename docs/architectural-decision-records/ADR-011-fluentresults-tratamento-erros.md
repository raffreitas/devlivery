# FluentResults para Tratamento de Erros

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Padrão de Design / Tratamento de Erros

## Contexto e Problema

Tratamento de erros em aplicações pode ser inconsistente: alguns métodos retornam `null`, outros lançam exceções, outros retornam códigos de status. Isso dificulta composição de operações e tratamento uniforme de erros. Além disso, exceções são caras em termos de performance e podem mascarar erros esperados do domínio.

A estrutura do repositório revela esta decisão através da organização:

```
Features/Products/Commands/CreateProduct/
└── CreateProductHandler.cs
    └── return Result<CreateProductResponse>  // ← FluentResults

Shared/Extensions/
└── ResultExtensions.cs              # Extensões para converter Result em HTTP responses
```

**Problema:** Como tratar erros de forma funcional e explícita, permitindo composição de operações e conversão uniforme para respostas HTTP, sem depender exclusivamente de exceções?

## Opções Consideradas

* **Exceções Apenas** - Lançar exceções para erros (simples, mas caro e difícil de compor)
* **Códigos de Retorno** - Retornar códigos numéricos (não type-safe, difícil de manter)
* **Result Pattern Manual** - Criar classe `Result<T>` própria (reinventar a roda)
* **FluentResults** - Biblioteca para Result pattern funcional (type-safe, composável)

## Decisão

**Escolhida:** "FluentResults", porque:

1. Type-safe: `Result<T>` é um tipo explícito que representa sucesso ou falha
2. Composável: permite encadear operações com `Map`, `Bind`, `OnSuccess`, etc.
3. Funcional: alinha com programação funcional, facilitando tratamento de erros
4. Explícito: erros são parte do tipo de retorno, não exceções ocultas
5. Performance: não usa exceções para fluxo de controle (mais eficiente)

### Implementação Técnica

A decisão se materializa em:

**Commands e Queries Retornam Result:**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductCommand.cs
public sealed record CreateProductCommand(...) 
    : ICommand<Result<CreateProductResponse>>;  // ← Result<T>

// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(...)
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(...)
    {
        // Validação de negócio
        if (await repo.ExistsByNameAsync(command.Name, ct))
        {
            return Result.Fail("Produto com este nome já existe.");  // ← Falha explícita
        }

        var product = new Product(command.Name, tenantAccessor.Tenant.Id);
        await repo.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(new CreateProductResponse(product.Id));  // ← Sucesso explícito
    }
}
```

**Extensões para HTTP Responses:**
```csharp
// Shared/Extensions/ResultExtensions.cs
public static class ResultExtensions
{
    public static Created<ApiResponse<T>> ToCreated<T>(
        this Result<T> result,
        string location)
    {
        return TypedResults.Created(
            location,
            new ApiResponse<T>(result.Value));
    }

    public static BadRequest<ApiResponse> ToBadRequest(this Result result)
    {
        return TypedResults.BadRequest(
            new ApiResponse(result.Errors.Select(e => e.Message)));
    }

    public static BadRequest<ApiResponse> ToBadRequest<T>(this Result<T> result)
    {
        return TypedResults.BadRequest(
            new ApiResponse(result.Errors.Select(e => e.Message)));
    }
}
```

**Uso em Endpoints:**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductEndpoint.cs
private static async Task<Results<Created<...>, BadRequest<...>>> Handle(
    CreateProductCommand command,
    ISender sender,
    CancellationToken ct)
{
    var result = await sender.Send(command, ct);
    
    return result.IsSuccess
        ? result.ToCreated($"/api/products/{result.Value.ProductId}")  // ← 201 Created
        : result.ToBadRequest();  // ← 400 Bad Request
}
```

**Composição de Operações:**
```csharp
// Exemplo de composição
var result = await GetProductAsync(id, ct)
    .Bind(product => ValidateProductAsync(product, ct))
    .Map(product => new ProductDto(product));

if (result.IsSuccess)
{
    return Result.Ok(result.Value);
}
else
{
    return Result.Fail(result.Errors);
}
```

**Padrão de Uso:**
- **Sucesso:** `Result.Ok<T>(value)` → retorna 200/201
- **Falha de Validação:** `Result.Fail("mensagem")` → retorna 400
- **Falha de Negócio:** `Result.Fail("regra violada")` → retorna 400
- **Erro Não Esperado:** Exceção → retorna 500 (via `GlobalExceptionHandler`)

### Consequências

* ✅ **Bom:** Type-safe: erros são parte do tipo de retorno, não exceções ocultas
* ✅ **Bom:** Composável: permite encadear operações com `Map`, `Bind`, etc.
* ✅ **Bom:** Funcional: alinha com programação funcional
* ✅ **Bom:** Performance: não usa exceções para fluxo de controle
* ✅ **Bom:** Explícito: sucesso/falha são claros no tipo de retorno
* ⚠️ **Neutro:** Requer disciplina: sempre retornar `Result<T>`, não lançar exceções para erros esperados
* ⚠️ **Ruim:** Pode ser verboso: cada operação precisa verificar `IsSuccess`
* ⚠️ **Ruim:** Erros não esperados (exceções) ainda precisam ser tratados separadamente

