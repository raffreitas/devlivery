# API Response Pattern

Este documento descreve o padrão de resposta padronizado implementado na API Devlivery.

## Visão Geral

A API utiliza um padrão **simples e consistente** para todas as respostas, tanto de sucesso quanto de erro, usando `ApiResponse<T>`. Os status codes HTTP são explícitos nos endpoints através de **Typed Results** do ASP.NET Core.

## Padrão de Resposta

### ApiResponse<T> - Sucesso

Estrutura para respostas bem-sucedidas com dados:

```json
{
  "success": true,
  "data": { /* objeto retornado */ }
}
```

**Exemplo prático:**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Pizza Margherita",
    "price": 35.00
  }
}
```

### ApiResponse<T> - Erro

Estrutura para respostas com erro:

```json
{
  "success": false,
  "errors": [
    "Mensagem de erro 1",
    "Mensagem de erro 2"
  ]
}
```

**Exemplo prático - Validação:**
```json
{
  "success": false,
  "errors": [
    "O campo 'Name' é obrigatório.",
    "O campo 'Price' deve ser maior que 0."
  ]
}
```

**Exemplo prático - Not Found:**
```json
{
  "success": false,
  "errors": [
    "Produto não foi encontrado"
  ]
}
```

### ApiResponse (sem tipo genérico)

Para operações sem retorno de dados (ex: DELETE, UPDATE):

**Sucesso:**
```json
{
  "success": true
}
```

**Erro:**
```json
{
  "success": false,
  "errors": [
    "Não é possível deletar este produto pois existem pedidos associados"
  ]
}
```

## Propriedades

- `success` (bool): Indica se a operação foi bem-sucedida
- `data` (T, opcional): Dados retornados em caso de sucesso
- `errors` (string[], opcional): Array de mensagens de erro em caso de falha

**Regras:**
- Campo `errors` só aparece quando `success = false`
- Campo `data` só aparece quando `success = true`
- Status HTTP já indica o tipo de erro (400, 404, 409, etc.)

## Status HTTP Utilizados

| Status | Uso | Exemplo |
|--------|-----|---------|
| 200 OK | Operação bem-sucedida (GET, UPDATE) | Buscar produto |
| 201 Created | Recurso criado com sucesso | Criar produto |
| 204 No Content | Operação sem retorno | DELETE bem-sucedido |
| 400 Bad Request | Erro de validação | Campos obrigatórios faltando |
| 401 Unauthorized | Não autenticado | Token inválido |
| 404 Not Found | Recurso não encontrado | Produto não existe |
| 409 Conflict | Conflito de regra de negócio | Já existe caixa aberto |

## Validação: mensagens em Português (regra do projeto)

Todas as mensagens de validação expostas pela API devem estar em Português (pt-BR). Para garantir consistência e clareza para os consumidores da API, adote as seguintes práticas:

- Sempre forneça mensagens personalizadas nos Validators usando `.WithMessage(...)` em vez de depender apenas das mensagens padrão em inglês do FluentValidation.
- Use os placeholders do FluentValidation para manter as mensagens reutilizáveis e informativas: `{PropertyName}`, `{PropertyValue}`, `{ComparisonValue}`, `{MinLength}`, `{MaxLength}`.
- Faça as mensagens curtas, claras e orientadas ao usuário (ex.: "O campo '{PropertyName}' é obrigatório.").

**Exemplo de Validator com mensagens em Português:**

```csharp
public sealed class Validator : AbstractValidator<CreateProductCommand>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(200).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O campo '{PropertyName}' deve ser maior que {ComparisonValue}.");
    }
}
```

## Implementação nos Endpoints

### Extension Methods Disponíveis

**Para Result<T>:**
```csharp
result.ToOk()           // 200 OK
result.ToCreated(uri)   // 201 Created
result.ToNoContent()    // 204 No Content
result.ToBadRequest()   // 400 Bad Request
result.ToNotFound()     // 404 Not Found
result.ToConflict()     // 409 Conflict
```

**Para ValidationResult (FluentValidation):**
```csharp
validationResult.ToBadRequest()     // Converte erros de validação para ApiResponse
validationResult.ToBadRequest<T>()  // Com tipo genérico
```

### Exemplo de Endpoint Completo

```csharp
public static class CreateProductEndpoint
{
    private static async Task<Results<Created<ApiResponse<CreateProductResponse>>, BadRequest<ApiResponse<CreateProductResponse>>>> Handle(
        Request request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(request.Name, request.Price);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/products/{result.Value.ProductId}")
            : result.ToBadRequest();
    }
}
```

### Exemplo com Pattern Matching

```csharp
return result.IsSuccess
    ? result.ToOk()
    : result.GetError() switch
    {
        ValidationError => result.ToBadRequest(),
        NotFoundError => result.ToNotFound(),
        DomainRuleError => result.ToConflict(),
        _ => result.ToBadRequest()
    };
```

## Benefícios da Abordagem Simplificada

✅ **Consistência**: Sempre a mesma estrutura para sucesso e erro  
✅ **Simplicidade**: Sem campos extras desnecessários (type, title, detail, instance)  
✅ **Frontend amigável**: Fácil de consumir - basta verificar `success` e `errors`  
✅ **Status HTTP semântico**: O status já indica o tipo de erro  
✅ **Menos código**: Sem necessidade de múltiplos tipos de Problem Details  
✅ **API interna**: Otimizado para cliente único que conhece a estrutura  

## Exemplo de Consumo no Frontend

```typescript
try {
  const response = await fetch('/api/products', {
    method: 'POST',
    body: JSON.stringify(product)
  });
  
  const apiResponse = await response.json();
  
  if (apiResponse.success) {
    // Sucesso - usar apiResponse.data
    console.log('Produto criado:', apiResponse.data);
  } else {
    // Erro - exibir apiResponse.errors
    apiResponse.errors.forEach(error => console.error(error));
  }
} catch (error) {
  // Erro de rede ou parsing
}
``` 


## Status Codes Utilizados

### Sucesso
- **200 OK**: Operação bem-sucedida com retorno de dados
- **201 Created**: Recurso criado com sucesso
- **204 No Content**: Operação bem-sucedida sem retorno de dados

### Erro do Cliente
- **400 Bad Request**: Dados inválidos ou erro de lógica de negócio
- **401 Unauthorized**: Credenciais inválidas
- **404 Not Found**: Recurso não encontrado

### Erro do Servidor
- **500 Internal Server Error**: Erro inesperado no servidor

## Como Usar nos Endpoints

### 1. GET - Listar recursos (200 OK)

```csharp
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapGet("", Handle)
        .Produces<ApiResponse<List<ProductResponse>>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
}

private static async Task<Results<Ok<ApiResponse<List<ProductResponse>>>, BadRequest<ProblemDetails>>> Handle(
    ProductHandler handler, 
    CancellationToken ct)
{
    var result = await handler.HandleAsync(ct);

    return result.IsSuccess
        ? result.ToOk("Products retrieved successfully")
        : result.ToBadRequestProblem();
}
```

### 2. GET by ID - Buscar recurso (200 OK / 404 Not Found)

```csharp
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapGet("{id:guid}", Handle)
        .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
}

private static async Task<Results<Ok<ApiResponse<ProductResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(
    Guid id,
    IValidator<GetProductQuery> validator,
    ProductHandler handler,
    CancellationToken ct)
{
    var query = new GetProductQuery(id);

    var validationResult = await validator.ValidateAsync(query, ct);
    if (!validationResult.IsValid)
    {
        return validationResult.ToValidationProblem();
    }

    var result = await handler.HandleAsync(query, ct);

    return result.IsSuccess
        ? result.ToOk("Product retrieved successfully")
        : result.ToNotFoundProblem();
}
```

### 3. POST - Criar recurso (201 Created)

```csharp
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapPost("", Handle)
        .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
}

private static async Task<Results<Created<ApiResponse<ProductResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(
    CreateProductCommand command,
    IValidator<CreateProductCommand> validator,
    ProductHandler handler,
    CancellationToken ct)
{
    var validationResult = await validator.ValidateAsync(command, ct);
    if (!validationResult.IsValid)
    {
        return validationResult.ToValidationProblem();
    }

    var result = await handler.HandleAsync(command, ct);

    return result.IsSuccess
        ? result.ToCreated($"/api/products/{result.Value.Id}", "Product created successfully")
        : result.ToBadRequestProblem();
}
```

### 4. PUT - Atualizar recurso (200 OK / 404 Not Found)

```csharp
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapPut("{id:guid}", Handle)
        .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
}

private static async Task<Results<Ok<ApiResponse<ProductResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(
    Guid id,
    UpdateProductCommand command,
    IValidator<UpdateProductCommand> validator,
    ProductHandler handler,
    CancellationToken ct)
{
    var validationResult = await validator.ValidateAsync(command, ct);
    if (!validationResult.IsValid)
    {
        return validationResult.ToValidationProblem();
    }

    var result = await handler.HandleAsync(command, ct);

    return result.IsSuccess
        ? result.ToOk("Product updated successfully")
        : result.ToNotFoundProblem();
}
```

### 5. DELETE - Deletar recurso (204 No Content / 404 Not Found)

```csharp
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapDelete("{id:guid}", Handle)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
}

private static async Task<Results<NoContent, ValidationProblem, NotFound<ProblemDetails>>> Handle(
    Guid id,
    IValidator<DeleteProductCommand> validator,
    ProductHandler handler,
    CancellationToken ct)
{
    var command = new DeleteProductCommand(id);
    
    var validationResult = await validator.ValidateAsync(command, ct);
    if (!validationResult.IsValid)
    {
        return validationResult.ToValidationProblem();
    }

    var result = await handler.HandleAsync(command, ct);

    return result.IsSuccess
        ? result.ToNoContent()
        : result.ToNotFoundProblem();
}
```

## Extension Methods Disponíveis

### ResultExtensions

**Para sucesso:**
- `ToOk<T>(message)`: Retorna 200 OK com ApiResponse
- `ToCreated<T>(uri, message)`: Retorna 201 Created com ApiResponse
- `ToNoContent()`: Retorna 204 No Content

**Para erros:**
- `ToBadRequestProblem()`: Retorna 400 Bad Request com ProblemDetails
- `ToNotFoundProblem()`: Retorna 404 Not Found com ProblemDetails

### ValidationExtensions

- `ToValidationProblem()`: Converte ValidationResult em ValidationProblem (400)

## Benefícios do Padrão

1. **Clareza de Status Codes**: Typed Results tornam explícito os possíveis retornos
2. **Consistência**: Todas as respostas seguem o mesmo formato
3. **RFC 7807**: Padrão internacional para representação de erros
4. **OpenAPI/Swagger**: Documentação automática precisa dos endpoints
5. **Type Safety**: Compilador valida os tipos de retorno
6. **Manutenibilidade**: Fácil entender e modificar o comportamento dos endpoints

## Exemplo de Resposta Completa

### Sucesso (200 OK)
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Pizza Margherita",
    "price": 29.90
  },
  "message": "Product retrieved successfully",
  "timestamp": "2025-11-04T10:30:00Z"
}
```

### Erro de Validação (400 Bad Request)
```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "Um ou mais erros de validação ocorreram",
  "status": 400,
  "errors": {
        "Name": ["O campo 'Name' é obrigatório."],
        "Price": ["O campo 'Price' deve ser maior que 0."]
  }
}
```

### Recurso Não Encontrado (404 Not Found)
```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
    "title": "Recurso não encontrado",
    "status": 404,
    "detail": "Produto com ID 123e4567-e89b-12d3-a456-426614174000 não foi encontrado"
}
```

### Erro do Servidor (500 Internal Server Error)
```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
    "title": "Erro interno do servidor",
    "status": 500,
    "detail": "Ocorreu um erro inesperado. Por favor, contate o suporte com o trace ID fornecido.",
  "instance": "/api/products/123",
  "traceId": "00-1234567890abcdef-fedcba0987654321-00",
  "timestamp": "2025-11-04T10:30:00Z"
}
```

## Referências

- [RFC 7807 - Problem Details for HTTP APIs](https://tools.ietf.org/html/rfc7807)
- [ASP.NET Core Typed Results](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses)
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
