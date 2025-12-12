# Devlivery WebAPI — AI Agent Guide

Quick reference for AI coding agents working in this .NET 9 Minimal API using Vertical Slice Architecture + CQRS.

## Architecture Overview

**Pattern**: Vertical Slice Architecture (VSA) with CQRS — each feature is self-contained with its Commands, Queries, Handlers, and Endpoints.

**Write Side (Commands)**:
- **Pattern**: Repository + Unit of Work (encapsulating EF Core).
- **Goal**: Encapsulate persistence logic and ensure transactional consistency.
- **Rule**: NEVER inject `DbContext` directly into Command Handlers. Use `I[Entity]Repository` and `IUnitOfWork`.

**Read Side (Queries)**:
- **Pattern**: Dapper with raw SQL.
- **Goal**: High-performance reads, avoiding EF Core overhead for queries.
- **Rule**: Queries utilize `IDbConnectionFactory` to execute raw SQL.

**Structure**:
```
Features/[FeatureName]/
├── Commands/[CommandName]/
│   ├── [CommandName]Command.cs      # Input DTO + FluentValidation
│   ├── [CommandName]Handler.cs      # Business logic -> Repository -> UnitOfWork
│   ├── [CommandName]Endpoint.cs     # HTTP endpoint with typed results
│   └── [CommandName]Response.cs     # Output DTO
├── Queries/[QueryName]/
│   ├── [QueryName]Handler.cs        # Dapper Logic -> IDbConnectionFactory
│   └── ...
├── Infrastructure/                  # Feature-specific Repositories
│   └── [Feature]Repository.cs
├── Domain/                          # Entities (if needed)
└── [FeatureName]Feature.cs          # DI registration + endpoint mapping
```

**Database**: PostgreSQL. EF Core for Writes (via Repositories), Dapper for Reads.

## Critical Patterns (Do NOT Deviate)

### 1. Multi-Tenancy Pattern (CRITICAL)
**All entities MUST have an `EstablishmentId`** for tenant isolation.

**Write Model (EF Core/Repository)**:
- **Automatic Filtering**: EF Core Global Query Filters handle tenant isolation automatically.
- **Creation**: Always pass `tenantAccessor.Tenant.Id` when creating new entities.

```csharp
// In Handler
var product = new Product(..., tenantAccessor.Tenant.Id);
await repository.AddAsync(product, ct);
```

**Read Model (Dapper)**:
- **MANUAL Filtering**: Dapper bypasses EF Core filters. You **MUST** manually add the tenant filter to every query.

```sql
SELECT * FROM products 
WHERE establishment_id = @TenantId  -- CRITICAL: NEVER FORGET THIS
```

### 2. Command Pattern (Repository + UnitOfWork)
Write operations must use the Repository pattern and UnitOfWork.

```csharp
public sealed class CreateProductHandler(
    IProductRepository productRepository, // Inject specific repository
    IUnitOfWork unitOfWork,               // Inject UnitOfWork
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateProductResponse>> HandleAsync(...)
    {
        var product = new Product(..., tenantAccessor.Tenant.Id);
        
        // Add to repository
        await productRepository.AddAsync(product, ct);
        
        // Commit transaction
        await unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

### 3. Query Pattern (Dapper)
Read operations must use Dapper via `IDbConnectionFactory`.

```csharp
public sealed class GetAllOrdersHandler(
    IDbConnectionFactory dbConnectionFactory, // Inject factory
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<GetAllOrdersResponse>> HandleAsync(...)
    {
        using var connection = await dbConnectionFactory.OpenConnectionAsync(ct);
        
        const string sql = """
            SELECT id, total, status 
            FROM orders 
            WHERE establishment_id = @TenantId -- Manual filter required
            AND (@StartDate IS NULL OR created_at >= @StartDate)
        """;
        
        var orders = await connection.QueryAsync<OrderDto>(sql, new 
        { 
            TenantId = tenantAccessor.Tenant.Id, // Pass tenant ID
            StartDate = query.StartDate 
        });
        
        return new GetAllOrdersResponse(orders.ToList());
    }
}
```

### 4. API Response Pattern
All endpoints use **ASP.NET Core Typed Results** + **RFC 7807 Problem Details**:

```csharp
// Endpoint signature example
private static async Task<Results<Created<ApiResponse<ProductResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(...)

// Success
return result.ToCreated($"/api/products/{result.Value.Id}", "Product created successfully");

// Error
return result.ToBadRequestProblem();
```

See `docs/API-RESPONSE-PATTERN.md` for complete examples.

### 5. Validation Pattern
Validators are **explicitly called** in endpoints.

```csharp
var validationResult = await validator.ValidateAsync(request, ct);
if (!validationResult.IsValid)
    return validationResult.ToValidationProblem();
```

**Validation Messages**: MUST be in Portuguese (PT-BR) using `.WithMessage(...)`.

### 6. Error Handling Pattern
Use **FluentResults** — never throw business exceptions.

```csharp
if (product is null)
    return Result.Fail<ProductResponse>("Produto não foi encontrado");
```

## Development Workflow

### Start App Locally
```bash
docker-compose up -d                    # Start PostgreSQL
dotnet run --project src/Devlivery      # Run API
```

### Database Migrations
**Contexts**: `ApplicationDbContext` and `ApplicationIdentityDbContext`.

**Using Makefile**:
```bash
make migration-db VERSION=002              # Create application migration
make migration-update-db                   # Apply application migrations
```

**Using PowerShell**:
```powershell
.\scripts\apply-migrations.ps1             # Apply all migrations locally
```

### CI/CD
GitHub Actions builds and tests. Migrations are applied automatically on deploy.

## Adding a New Feature

### 1. Create Repository Interface & Implementation
Define `I[Feature]Repository` in core implementation in `Infrastructure`.

```csharp
// IProductRepository.cs
public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken ct);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
}

// ProductRepository.cs
public sealed class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task AddAsync(Product product, CancellationToken ct) 
        => await context.Products.AddAsync(product, ct);
        
    // EF Core Global Filters handle tenant automatically here
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
        => await context.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
}
```

### 2. Create Handler (Write)
Inject Repository and UnitOfWork.

```csharp
public sealed class CreateProductHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor)
{
    // ... logic uses repository and unitOfWork.SaveChangesAsync()
}
```

### 3. Create Handler (Read)
Inject `IDbConnectionFactory`.

```csharp
public sealed class GetProductHandler(
    IDbConnectionFactory connectionFactory,
    ITenantAccessor tenantAccessor)
{
    // ... logic uses Dapper + SQL with WHERE establishment_id = @TenantId
}
```

### 4. Register Services
In `[Feature]Feature.cs`:

```csharp
public static IServiceCollection AddMyFeature(this IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<CreateProductHandler>();
    return services;
}
```

## Key Files to Reference

**Infrastructure**:
- `Shared/Infrastructure/Persistence/UnitOfWork.cs` — Transaction management
- `Shared/Infrastructure/Persistence/Factory/DbConnectionFactory.cs` — Dapper connection factory
- `Shared/Models/ApiResponse.cs` — Response wrapper

**Docs**:
- `docs/REPOSITORY-UNITOFWORK-IMPLEMENTATION.md` — Implementation details of Repos/UoW
- `docs/API-RESPONSE-PATTERN.md` — API response guide

## Common Gotchas

1.  **Dapper Tenant Filtering**: You **MUST** manually add `WHERE establishment_id = @TenantId` to ALL Dapper queries. Dapper does NOT respect EF Core global filters.
2.  **No Direct DbContext**: Do not inject `DbContext` into Handlers. Use Repositories and UnitOfWork.
3.  **Validation**: Manual validation check in endpoints is required.
4.  **Language**: All validation messages in PT-BR.
5.  **Integration Tests**: Always call `await ResetDatabaseAsync()` at start of tests.
