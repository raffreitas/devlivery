# Testcontainers + Respawn para Testes de Integração

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Testes

## Contexto e Problema

Testes de integração precisam de um banco de dados real para validar comportamento end-to-end, mas configurar e limpar banco de dados entre testes é complexo. Usar banco compartilhado causa interferência entre testes, e usar mocks não valida comportamento real do sistema.

A estrutura do repositório revela esta decisão através da organização:

```
test/Devlivery.Tests/Common/
├── BaseWebApplicationFactory.cs     # Factory com Testcontainers
└── WebApiBaseFixture.cs             # Fixture base para testes HTTP

Features/Products/
└── ProductsWebApplicationFactory.cs  # Factory específica da feature
```

**Problema:** Como executar testes de integração com banco de dados real, garantindo isolamento entre testes e limpeza automática, sem depender de banco compartilhado ou mocks?

## Opções Consideradas

* **Banco Compartilhado** - Usar banco de dados compartilhado (interferência entre testes)
* **Mocks/Stubs** - Mockar repositórios e DbContext (não valida comportamento real)
* **In-Memory Database** - Usar banco em memória (comportamento diferente de banco real)
* **Testcontainers + Respawn** - Container Docker isolado + limpeza automática (isolado, real)
* **SQLite em Disco** - Usar SQLite para testes (comportamento diferente de PostgreSQL)

## Decisão

**Escolhida:** "Testcontainers + Respawn", porque:

1. Isolado: cada execução de testes tem seu próprio container PostgreSQL
2. Real: usa PostgreSQL real, validando comportamento do sistema
3. Automático: container é criado/destruído automaticamente
4. Limpeza: Respawn limpa banco entre testes sem recriar schema
5. Determinístico: testes são reproduzíveis e não interferem entre si

### Implementação Técnica

A decisão se materializa em:

**Base Factory com Testcontainers:**
```csharp
// test/Devlivery.Tests/Common/BaseWebApplicationFactory.cs
public abstract class BaseWebApplicationFactory<TEntryPoint> 
    : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("devlivery")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _dbConnection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", 
            _postgresContainer.GetConnectionString());
        base.ConfigureWebHost(builder);
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);  // ← Limpa dados, mantém schema
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();  // ← Inicia container

        using var scope = Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await appDbContext.Database.MigrateAsync();  // ← Aplica migrations
        var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        await identityDbContext.Database.MigrateAsync();

        _dbConnection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]  // ← Mantém histórico de migrations
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _postgresContainer.DisposeAsync();  // ← Remove container
    }
}
```

**Uso em Testes:**
```csharp
// test/Devlivery.Tests/Features/Products/Commands/CreateProductTests.cs
public class CreateProductTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture(factory)
{
    [Fact]
    public async Task CreateProduct_Should_Return_201()
    {
        // Arrange
        await ResetDatabaseAsync();  // ← Limpa banco antes do teste

        var command = new CreateProductCommand("Pizza", 25.00m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

**Fluxo de Execução:**
1. `InitializeAsync()` → inicia container PostgreSQL
2. Aplica migrations → cria schema
3. Teste executa → `ResetDatabaseAsync()` limpa dados
4. Teste valida comportamento
5. `DisposeAsync()` → remove container

**Vantagens:**
- **Isolamento:** Cada execução tem container próprio
- **Real:** PostgreSQL real, não mock
- **Rápido:** Respawn limpa dados sem recriar schema
- **Determinístico:** Testes não interferem entre si

### Consequências

* ✅ **Bom:** Isolado: cada execução tem container próprio, sem interferência
* ✅ **Bom:** Real: usa PostgreSQL real, validando comportamento do sistema
* ✅ **Bom:** Automático: container criado/destruído automaticamente
* ✅ **Bom:** Rápido: Respawn limpa dados sem recriar schema
* ✅ **Bom:** Determinístico: testes são reproduzíveis
* ⚠️ **Neutro:** Requer Docker instalado e rodando (pré-requisito)
* ⚠️ **Ruim:** Pode ser mais lento que mocks (trade-off por realismo)
* ⚠️ **Ruim:** Consome recursos (memória, CPU) durante execução de testes

