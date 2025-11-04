# API Response Pattern

Este documento descreve o padrão de resposta padronizado implementado na API Devlivery, seguindo as melhores práticas de construção de APIs REST.

## Visão Geral

A API utiliza **RFC 7807 Problem Details** para erros e um padrão consistente de sucesso usando `ApiResponse<T>`. Todos os status codes são explícitos nos endpoints através de **Typed Results** do ASP.NET Core.

## Padrão de Resposta de Sucesso

### ApiResponse<T>

Estrutura padronizada para respostas bem-sucedidas:

```json
{
  "success": true,
  "data": { /* objeto retornado */ },
  "message": "Operation completed successfully",
  "timestamp": "2025-11-04T10:30:00Z"
}
```

**Propriedades:**
- `success`: Sempre `true` em respostas de sucesso
- `data`: Dados retornados pela operação
- `message`: Mensagem contextual opcional
- `timestamp`: Data/hora UTC da resposta

## Padrão de Resposta de Erro

### Problem Details (RFC 7807)

Estrutura padronizada para todos os erros:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Product with ID 123 was not found",
  "instance": "/api/products/123"
}
```

**Propriedades:**
- `type`: URI de referência do tipo de erro
- `title`: Título legível do erro
- `status`: Código de status HTTP
- `detail`: Descrição detalhada do erro
- `instance`: Caminho da requisição que gerou o erro

### Validation Problem Details

Para erros de validação (400):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Price": ["Price must be greater than 0."]
  }
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
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Price": ["Price must be greater than 0."]
  }
}
```

### Recurso Não Encontrado (404 Not Found)
```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Product with ID 123e4567-e89b-12d3-a456-426614174000 was not found"
}
```

### Erro do Servidor (500 Internal Server Error)
```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please contact support with the provided trace ID.",
  "instance": "/api/products/123",
  "traceId": "00-1234567890abcdef-fedcba0987654321-00",
  "timestamp": "2025-11-04T10:30:00Z"
}
```

## Referências

- [RFC 7807 - Problem Details for HTTP APIs](https://tools.ietf.org/html/rfc7807)
- [ASP.NET Core Typed Results](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses)
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
