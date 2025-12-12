# Devlivery WebAPI

Backend desenvolvido com .NET 9 seguindo as melhores práticas de desenvolvimento e arquitetura limpa.

## 🏗️ Arquitetura

Este projeto implementa **Vertical Slice Architecture** (VSA), onde cada feature é completamente autocontida com sua própria lógica de negócio, validação, handlers e endpoints.

### Características principais:

- **Vertical Slice Architecture**: Organização por funcionalidade ao invés de camadas técnicas
- **CQRS Pattern**: Separação clara entre Commands (escrita) e Queries (leitura)
- **Minimal APIs**: APIs leves, performáticas e modernas do .NET 9
- **Typed Results**: Respostas HTTP tipadas e explícitas seguindo RFC 7807
- **FluentValidation**: Validação robusta e declarativa
- **FluentResults**: Tratamento de erros tipado e funcional
- **Entity Framework Core**: ORM para PostgreSQL

## 🎯 Princípios Aplicados

- **KISS** (Keep It Simple, Stupid): Código simples e direto ao ponto
- **YAGNI** (You Aren't Gonna Need It): Implementado apenas o necessário
- **DRY** (Don't Repeat Yourself): Evitar duplicação de código
- **SOLID**: Princípios de design orientado a objetos
- **Separation of Concerns**: Cada slice é independente e coesa

## 📁 Estrutura do Projeto

```
Devlivery/
├── Shared/                           # Componentes compartilhados
│   ├── Abstractions/                 # Interfaces e contratos
│   ├── Infrastructure/
│   │   ├── Database/                 # DbContext e configurações
│   │   ├── Identity/                 # Autenticação e autorização
│   │   └── Tokens/                   # Geração de tokens JWT
│   └── Presentation/                 # Exception handlers globais
│
├── Features/                         # Features organizadas por domínio
│   ├── Auth/                         # Autenticação
│   │   └── Commands/
│   │       └── Login/
│   │
│   ├── Products/                     # Gestão de produtos
│   │   ├── Commands/
│   │   │   ├── CreateProduct/
│   │   │   │   ├── CreateProductCommand.cs      # DTO + Validação
│   │   │   │   ├── CreateProductHandler.cs      # Lógica de negócio
│   │   │   │   ├── CreateProductEndpoint.cs     # Configuração do endpoint
│   │   │   │   └── CreateProductResponse.cs     # Response DTO
│   │   │   ├── UpdateProduct/
│   │   │   │   ├── UpdateProductCommand.cs
│   │   │   │   ├── UpdateProductHandler.cs
│   │   │   │   ├── UpdateProductEndpoint.cs
│   │   │   │   └── UpdateProductResponse.cs
│   │   │   └── DeleteProduct/
│   │   │       ├── DeleteProductCommand.cs
│   │   │       ├── DeleteProductHandler.cs
│   │   │       ├── DeleteProductEndpoint.cs
│   │   │       └── DeleteProductResponse.cs
│   │   ├── Queries/
│   │   │   ├── GetAllProducts/
│   │   │   │   ├── GetAllProductsQuery.cs
│   │   │   │   ├── GetAllProductsHandler.cs
│   │   │   │   ├── GetAllProductsEndpoint.cs
│   │   │   │   └── GetAllProductsResponse.cs
│   │   │   └── GetProductById/
│   │   │       ├── GetProductByIdQuery.cs
│   │   │       ├── GetProductByIdHandler.cs
│   │   │       ├── GetProductByIdEndpoint.cs
│   │   │       └── GetProductByIdResponse.cs
│   │   ├── Domain/
│   │   │   └── Product.cs                       # Entidade de domínio
│   │   └── ProductFeature.cs                    # Registros de DI e endpoints
│   │
│   ├── Orders/                       # Gestão de pedidos
│   │   ├── Commands/
│   │   │   ├── CreateOrder/
│   │   │   │   ├── CreateOrderCommand.cs
│   │   │   │   ├── CreateOrderHandler.cs
│   │   │   │   ├── CreateOrderEndpoint.cs
│   │   │   │   └── CreateOrderResponse.cs
│   │   │   ├── UpdateOrderStatus/
│   │   │   │   ├── UpdateOrderStatusCommand.cs
│   │   │   │   ├── UpdateOrderStatusHandler.cs
│   │   │   │   ├── UpdateOrderStatusEndpoint.cs
│   │   │   │   └── UpdateOrderStatusResponse.cs
│   │   │   └── DeleteOrder/
│   │   │       ├── DeleteOrderCommand.cs
│   │   │       ├── DeleteOrderHandler.cs
│   │   │       ├── DeleteOrderEndpoint.cs
│   │   │       └── DeleteOrderResponse.cs
│   │   ├── Queries/
│   │   │   ├── GetAllOrders/
│   │   │   │   ├── GetAllOrdersQuery.cs
│   │   │   │   ├── GetAllOrdersHandler.cs
│   │   │   │   ├── GetAllOrdersEndpoint.cs
│   │   │   │   └── GetAllOrdersResponse.cs
│   │   │   └── GetOrderById/
│   │   │       ├── GetOrderByIdQuery.cs
│   │   │       ├── GetOrderByIdHandler.cs
│   │   │       ├── GetOrderByIdEndpoint.cs
│   │   │       └── GetOrderByIdResponse.cs
│   │   ├── Domain/
│   │   │   ├── Order.cs                         # Entidade de domínio
│   │   │   └── OrderItem.cs                     # Entidade de domínio
│   │   └── OrdersFeature.cs                     # Registros de DI e endpoints

```

### 🔍 Padrão de cada Slice

Cada operação (Command ou Query) segue a estrutura:

1. **Command/Query**: Define o contrato de entrada com validações FluentValidation
2. **Handler**: Contém toda a lógica de negócio e retorna `Result<T>` (FluentResults)
3. **Endpoint**: Configura o endpoint HTTP (rota, método, validação)
4. **Response**: DTO de saída tipado

**Exemplo de fluxo:**
```
Request → Endpoint → Validation → Handler → Business Logic → Response
```

## � Padrão de Resposta da API

A API segue o padrão **RFC 7807 Problem Details** para erros e usa **Typed Results** para todas as respostas:

### Respostas de Sucesso

Todas as respostas de sucesso são envolvidas em `ApiResponse<T>`:

```json
{
  "success": true,
  "data": { /* dados retornados */ },
  "message": "Operação realizada com sucesso",
  "timestamp": "2025-11-04T10:30:00Z"
}
```

### Respostas de Erro

Erros seguem o padrão RFC 7807:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Um ou mais erros de validação ocorreram",
  "status": 400,
  "errors": {
    "name": ["O campo 'Name' é obrigatório."]
  }
}
```

📘 **Para detalhes completos sobre respostas da API, consulte:**
- [docs/API-RESPONSE-PATTERN.md](docs/API-RESPONSE-PATTERN.md)

## �🚀 Como Executar

### Pré-requisitos

- .NET 9 SDK
- PostgreSQL
- Docker (opcional)

### 1. Configurar o Banco de Dados

Execute o PostgreSQL via Docker:

```bash
docker run --name devlivery-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres
```

Ou use uma instância local do PostgreSQL.

### 2. Atualizar Connection String

Edite `appsettings.Development.json` se necessário:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=devlivery;Username=postgres;Password=postgres"
  }
}
```

### 3. Restaurar Dependências

```bash
dotnet restore
```

### 4. Executar o Projeto

```bash
dotnet run
```

O projeto irá:
- Criar o banco de dados automaticamente
- Aplicar as migrations
- Popular dados iniciais (seed)
- Iniciar em `http://localhost:5000` ou `https://localhost:5001`

## 📊 Migrations

O projeto utiliza **dois DbContexts separados**:
- `ApplicationDbContext` - Dados da aplicação (Products, Orders)
- `ApplicationIdentityDbContext` - Dados de autenticação (Users, Roles)

### ✅ Configuração Atual: Automatizado via CI/CD

As migrations são **aplicadas automaticamente** na branch `main` pelo GitHub Actions!

**⚠️ Pré-requisito:** Configure o secret `DATABASE_CONNECTION_STRING` no GitHub antes do primeiro deploy:
```bash
GitHub → Settings → Secrets and variables → Actions → New repository secret
Name: DATABASE_CONNECTION_STRING
Value: Host=prod-db;Port=5432;Database=devlivery;Username=...;Password=...
```

### Comandos via Makefile (Desenvolvimento Local)

```bash
# Criar nova migration
make migration-db VERSION=002           # ApplicationDbContext
make migration-identity VERSION=002     # ApplicationIdentityDbContext

# Aplicar migrations
make migration-update-db                # ApplicationDbContext
make migration-update-identity          # ApplicationIdentityDbContext

# Verificar status
make migration-status
```

### Scripts de Migration (Windows)

```powershell
# Aplicar todas as migrations localmente
.\scripts\apply-migrations.ps1
```

### EF Core direto (Alternativa)

```bash
# Criar migration
dotnet ef migrations add v002 -o ./Shared/Database/Migrations -c ApplicationDbContext

# Aplicar migration
dotnet ef database update -c ApplicationDbContext
```

📘 **Para estratégias completas de migration em diferentes ambientes, consulte:**
- [docs/MIGRATIONS.md](docs/MIGRATIONS.md) - Documentação completa
- [docs/MIGRATIONS-QUICKSTART.md](docs/MIGRATIONS-QUICKSTART.md) - Referência rápida

## ➕ Como Adicionar uma Nova Feature

Siga o padrão Vertical Slice Architecture para manter a consistência:

### 1. Criar a estrutura de pastas

```
Features/
└── MinhaFeature/
    ├── Commands/
    │   └── MeuCommand/
    │       ├── MeuCommand.cs
    │       ├── MeuCommandHandler.cs
    │       ├── MeuCommandEndpoint.cs
    │       └── MeuCommandResponse.cs
    ├── Queries/
    │   └── MinhaQuery/
    │       ├── MinhaQuery.cs
    │       ├── MinhaQueryHandler.cs
    │       ├── MinhaQueryEndpoint.cs
    │       └── MinhaQueryResponse.cs
    ├── Domain/
    │   └── MinhaEntidade.cs
    └── MinhaFeature.cs
```

### 2. Implementar o Command/Query

```csharp
// MeuCommand.cs
using FluentValidation;

namespace Devlivery.Features.MinhaFeature.Commands.MeuCommand;

public sealed record MeuCommand(string Propriedade);

public sealed class Validator : AbstractValidator<MeuCommand>
{
    public Validator()
    {
        RuleFor(x => x.Propriedade).NotEmpty();
    }
}
```

### 3. Implementar o Handler

```csharp
// MeuCommandHandler.cs
using FluentResults;
using Devlivery.Shared.Database.Context;

namespace Devlivery.Features.MinhaFeature.Commands.MeuCommand;

public sealed class MeuCommandHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<MeuCommandResponse>> HandleAsync(
        MeuCommand command,
        CancellationToken cancellationToken = default)
    {
        // Lógica de negócio aqui
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new MeuCommandResponse());
    }
}
```

### 4. Implementar o Endpoint

```csharp
// MeuCommandEndpoint.cs
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Features.MinhaFeature.Commands.MeuCommand;

public static class MeuCommandEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<MeuCommandResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<MeuCommandResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(
        MeuCommand request,
        IValidator<MeuCommand> validator,
        MeuCommandHandler handler,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var result = await handler.HandleAsync(request, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/minha-feature/{result.Value.Id}", "Recurso criado com sucesso")
            : result.ToBadRequestProblem();
    }
}
```

### 5. Registrar no Feature File

```csharp
// MinhaFeature.cs
using Devlivery.Features.MinhaFeature.Commands.MeuCommand;

namespace Devlivery.Features.MinhaFeature;

public static class MinhaFeature
{
    public static IServiceCollection AddMinhaFeature(this IServiceCollection services)
    {
        services.AddScoped<MeuCommandHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapMinhaFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/minha-feature").WithTags("MinhaFeature");
        
        MeuCommandEndpoint.MapEndpoint(group);
        
        return app;
    }
}
```

### 6. Registrar no Program.cs

```csharp
// Adicionar serviços
builder.Services.AddMinhaFeature();

// Mapear endpoints
app.MapMinhaFeatureEndpoints();
```

## 🔌 Endpoints

### 🔐 Auth
- `POST /api/auth/login` - Login de usuário

### 📦 Products
- `GET /api/products` - Listar todos os produtos
- `GET /api/products/{id}` - Buscar produto por ID
- `POST /api/products` - Criar novo produto
- `PUT /api/products/{id}` - Atualizar produto existente
- `DELETE /api/products/{id}` - Deletar produto

### 📋 Orders
- `GET /api/orders` - Listar todos os pedidos (com items e produtos)
  - **Query Parameters opcionais:**
    - `start` - Data inicial (filtrar por data de criação)
    - `end` - Data final (filtrar por data de criação)
    - `paymentMethod` - Filtrar por método de pagamento
- `GET /api/orders/{id}` - Buscar pedido por ID (com items e produtos)
- `POST /api/orders` - Criar novo pedido
- `PATCH /api/orders/{id}/status` - Atualizar status do pedido
- `DELETE /api/orders/{id}` - Deletar pedido

**Status de pedidos válidos:**
- `pending` - Pendente
- `preparing` - Em preparação
- `ready` - Pronto para entrega
- `delivered` - Entregue
- `cancelled` - Cancelado

**Métodos de pagamento válidos:**
- `Cash` - Dinheiro
- `CreditCard` - Cartão de Crédito
- `DebitCard` - Cartão de Débito
- `Pix` - PIX

## 🧪 Testando a API

Acesse a interface **Scalar** para testar a API de forma interativa:

- **Scalar UI**: `http://localhost:5000/scalar/v1`
- **Health Check**: `http://localhost:5000/health`

### Exemplos de Requisições

#### Criar Produto
```http
POST http://localhost:5000/api/products
Content-Type: application/json

{
  "name": "Pizza Margherita",
  "description": "Pizza tradicional italiana",
  "price": 35.90,
  "category": "Pizza",
  "available": true
}
```

#### Criar Pedido
```http
POST http://localhost:5000/api/orders
Content-Type: application/json

{
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2,
      "notes": "Sem cebola"
    }
  ],
  "customerName": "João Silva",
  "customerPhone": "(11) 98765-4321",
  "deliveryAddress": "Rua das Flores, 123 - São Paulo, SP",
  "paymentMethod": "Pix",
  "deliveryFee": 5.00
}
```

#### Listar Pedidos com Filtros
```http
GET http://localhost:5000/api/orders?start=2025-01-01&end=2025-12-31&paymentMethod=Pix
```

## 🔐 Credenciais de Teste

```
Email: admin@pizza.com
Senha: 123456
```

> **Nota**: O seed de dados é aplicado automaticamente no ambiente de desenvolvimento.

## 📦 Dependências Principais

- **Microsoft.EntityFrameworkCore** (9.0.10) - ORM para acesso a dados
- **Npgsql.EntityFrameworkCore.PostgreSQL** (9.0.10) - Provider PostgreSQL
- **FluentValidation** (12.1.0) - Validação de dados
- **FluentResults** (4.0.0) - Tratamento de erros funcional
- **ASP.NET Core Identity** (9.0.0) - Autenticação e autorização
- **JWT Bearer Authentication** - Tokens de acesso
- **EFCore.NamingConventions** - Convenção snake_case para PostgreSQL

## 🔧 Extension Methods Úteis

O projeto fornece extension methods para facilitar o trabalho com `Result<T>` e validações:

### ResultExtensions

Localizado em `Shared/Extensions/ResultExtensions.cs`:

- `ToOk<T>(message)` - Converte Result em 200 OK
- `ToCreated<T>(uri, message)` - Converte Result em 201 Created
- `ToNoContent()` - Retorna 204 No Content
- `ToBadRequestProblem()` - Converte erro em 400 Bad Request
- `ToNotFoundProblem()` - Converte erro em 404 Not Found

### ValidationExtensions

Localizado em `Shared/Extensions/ValidationExtensions.cs`:

- `ToValidationProblem()` - Converte ValidationResult em 400 Validation Problem

## 📊 Observabilidade

O projeto está preparado para observabilidade com:

- **OpenTelemetry**: Integração configurada
- **Grafana**: Suporte para métricas e traces
- **Health Checks**: Endpoint `/health` disponível
- **Scalar API Documentation**: Interface interativa em `/scalar/v1`

## 📋 Benefícios da Arquitetura

### Vertical Slice Architecture
- ✅ **Alta coesão**: Todo código relacionado a uma feature fica junto
- ✅ **Baixo acoplamento**: Features são independentes entre si
- ✅ **Fácil manutenção**: Mudanças ficam isoladas em um slice
- ✅ **Onboarding rápido**: Desenvolvedores entendem features completas
- ✅ **Testabilidade**: Cada slice pode ser testado isoladamente

### CQRS Pattern
- ✅ **Separação clara**: Commands mudam estado, Queries apenas leem
- ✅ **Otimização específica**: Queries podem usar projections otimizadas
- ✅ **Escalabilidade**: Commands e Queries podem escalar independentemente

## 🛠️ Tecnologias

- .NET 9
- PostgreSQL 16
- Entity Framework Core 9
- ASP.NET Core Minimal APIs
- FluentValidation 12.1.0
- FluentResults 4.0.0
- ASP.NET Core Identity
- JWT Bearer Authentication
- EFCore.NamingConventions (snake_case)
- OpenTelemetry
- Scalar (API Documentation)

---

## 📚 Recursos Adicionais

- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [FluentResults Documentation](https://github.com/altmann/FluentResults)
- [Minimal APIs in .NET](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

---

**Desenvolvido com ❤️ usando .NET 9**
