# Integration Tests — Structure Guide

## Overview

Os testes de integração usam **Testcontainers + Respawn** com **Collections por Feature** para garantir:

✅ **Isolamento entre features** - cada feature tem seu próprio container PostgreSQL  
✅ **Isolamento entre testes** - Respawn limpa dados rapidamente (~50-100ms)  
✅ **Performance no CI** - containers compartilhados por feature (não por teste)  
✅ **Alinhamento com VSA** - estrutura de testes segue Vertical Slice Architecture

## Architecture

```
test/Devlivery.Tests/
├── Common/
│   ├── BaseWebApplicationFactory.cs     # Factory genérica com Respawn
│   └── WebApiBaseFixture.cs             # Base class com helpers
├── Features/
│   ├── Auth/
│   │   ├── AuthWebApplicationFactory.cs     # Factory + Collection para Auth
│   │   └── Commands/Login/*EndpointTests.cs
│   ├── Orders/
│   │   ├── OrdersWebApplicationFactory.cs   # Factory + Collection para Orders
│   │   ├── Commands/*EndpointTests.cs
│   │   └── Queries/*EndpointTests.cs
│   └── Products/
│       ├── ProductsWebApplicationFactory.cs # Factory + Collection para Products
│       ├── Commands/*EndpointTests.cs
│       └── Queries/*EndpointTests.cs
```

## How It Works

### 1. BaseWebApplicationFactory<T>

Factory genérica que:
- Cria um container PostgreSQL via Testcontainers
- Aplica migrations de ambos os contexts (Application + Identity)
- Configura Respawn para limpar dados entre testes

```csharp
public abstract class BaseWebApplicationFactory<TEntryPoint> 
    : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    // Container compartilhado por todos os testes da collection
    private readonly PostgreSqlContainer _postgresContainer;
    private Respawner _respawner;

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection); // ~50-100ms
    }
}
```

### 2. Collection por Feature

Cada feature tem sua própria **factory + collection**:

```csharp
// AuthWebApplicationFactory.cs
public sealed class AuthWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Auth Tests")]
public sealed class AuthTestCollection : ICollectionFixture<AuthWebApplicationFactory>;
```

**Isso significa:**
- Todos os testes de `Auth` compartilham 1 container PostgreSQL
- Todos os testes de `Orders` compartilham outro container
- Todos os testes de `Products` compartilham outro container
- **Total: 3 containers no CI** (ao invés de 1 por classe de teste)

### 3. WebApiBaseFixture<TFactory>

Classe base genérica com métodos auxiliares:

```csharp
public abstract class WebApiBaseFixture<TFactory> 
    where TFactory : BaseWebApplicationFactory<Program>
{
    protected readonly TFactory Factory;

    protected async Task ResetDatabaseAsync() { }
    protected async Task<User> CreateUserAsync(...) { }
    protected async Task<string> GetAccessTokenAsync(...) { }
    protected async Task<HttpResponseMessage> PostAsync(...) { }
    // ... outros helpers HTTP
}
```

## Writing Tests

### Pattern

```csharp
[Collection("Products Tests")]  // Define qual container usar
[Trait("Category", "Integration Tests")]
public sealed class CreateProductEndpointTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture<ProductsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedAndProduct()
    {
        // Arrange
        await ResetDatabaseAsync();  // SEMPRE no início do teste!
        
        var token = await GetAccessTokenAsync();
        var command = new CreateProductCommand(...);

        // Act
        var response = await PostAsync("/api/products", command, token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }
}
```

### Key Rules

1. **SEMPRE** chamar `await ResetDatabaseAsync()` no início de cada teste
2. **NUNCA** usar `IAsyncLifetime` nos testes (não é mais necessário)
3. **Usar scoped DbContext** quando precisar manipular dados diretamente:

```csharp
using var scope = Factory.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await dbContext.Products.AddAsync(product);
await dbContext.SaveChangesAsync();
```

## Why Respawn?

**Sem Respawn:**
- Limpar banco: `EnsureDeletedAsync()` + `Migrate()` = **~2-5 segundos**
- 50 testes = **~100-250 segundos** só de limpeza

**Com Respawn:**
- Limpar banco: `ResetAsync()` = **~50-100ms**
- 50 testes = **~2.5-5 segundos** de limpeza
- **50x mais rápido** ⚡

## Running Tests

```bash
# Rodar todos os testes
dotnet test

# Rodar apenas testes de uma feature
dotnet test --filter "FullyQualifiedName~Orders"

# Rodar com output detalhado
dotnet test --logger "console;verbosity=detailed"
```

## CI/CD

No GitHub Actions, os testes rodam com:
- **3 containers PostgreSQL** (Auth, Orders, Products)
- **Respawn** para limpeza rápida
- **Execução paralela** de collections diferentes

Tempo esperado: **~30-60 segundos** para toda a suite de testes.

## Adding New Feature Tests

1. Criar a factory + collection:

```csharp
// Features/Payments/PaymentsWebApplicationFactory.cs
public sealed class PaymentsWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Payments Tests")]
public sealed class PaymentsTestCollection : ICollectionFixture<PaymentsWebApplicationFactory>;
```

2. Criar o teste:

```csharp
[Collection("Payments Tests")]
public sealed class CreatePaymentEndpointTests(PaymentsWebApplicationFactory factory)
    : WebApiBaseFixture<PaymentsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task Test()
    {
        await ResetDatabaseAsync();
        // ...
    }
}
```

## Troubleshooting

### "The database is locked"
- Certifique-se de que está usando `await ResetDatabaseAsync()` no início de cada teste
- Verifique se não há `SaveChangesAsync()` pendente sem `using var scope`

### "Container already exists"
- Limpe containers órfãos: `docker container prune -f`
- Testcontainers faz cleanup automático, mas pode falhar se o processo for interrompido

### Testes lentos
- Verifique se `ResetDatabaseAsync()` está sendo chamado (não `EnsureDeletedAsync`)
- Verifique se há muitos `CreateUserAsync()` desnecessários
- Use dados em memória quando possível (Builders) ao invés de sempre inserir no DB

---

**Estrutura criada em**: 2025-11-10  
**Padrão**: Collection por Feature + Respawn  
**Performance**: ~50-100ms por teste (limpeza de dados)
