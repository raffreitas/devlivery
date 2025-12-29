# Devlivery - AI Coding Instructions

## Architecture Overview

This is a **modular monolith** organized by **vertical slices** (features). Everything lives in one deployable (`src/Devlivery`), but features are self-contained modules. See ADRs in `docs/architectural-decision-records/` for architectural decisions.

**Key Principle:** Features own their entire vertical stack — from HTTP endpoint → handler → domain model → database repository.

## Feature Structure (Critical Pattern)

Every feature follows this exact structure:

```
Features/Products/
├── ProductFeature.cs              # DI registration + endpoint mapping
├── Commands/                      # Write operations (CUD)
│   └── CreateProduct/
│       ├── CreateProductCommand.cs
│       ├── CreateProductHandler.cs
│       ├── CreateProductValidator.cs     # FluentValidation
│       ├── CreateProductEndpoint.cs      # Minimal API endpoint
│       └── CreateProductResponse.cs
├── Queries/                       # Read operations (R)
│   └── GetAllProducts/
│       ├── GetAllProductsQuery.cs
│       ├── GetAllProductsHandler.cs
│       └── GetAllProductsEndpoint.cs
├── Domain/                        # Business logic
│   ├── Product.cs                 # Entity (inherits from Entity)
│   └── IProductRepository.cs
└── Infrastructure/
    └── ProductRepository.cs       # EF Core implementation
```

**When creating new features:**
1. Create `XxxFeature.cs` with `AddXxxFeature()` and `MapXxxEndpoints()` methods
2. Register in `Startup.cs` → `ConfigureBuilder()` and `ConfigureApp()`
3. Follow CQRS: separate Commands (writes) from Queries (reads)
4. One file per endpoint — `CreateProductEndpoint.cs`, not a Controller

## CQRS Implementation (Mediator Pattern)

**Commands (Writes):**
- Use `ICommand<Result<TResponse>>` from Mediator library
- Handler: `ICommandHandler<TCommand, Result<TResponse>>`
- **Always** inject `ITenantAccessor` to get `EstablishmentId`
- **Always** call `unitOfWork.SaveChangesAsync()` to commit

```csharp
public sealed record CreateProductCommand(...) : ICommand<Result<CreateProductResponse>>;

public sealed class CreateProductHandler(
    IProductRepository repo,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<...>
{
    public async ValueTask<Result<CreateProductResponse>> Handle(...)
    {
        var product = new Product(..., tenantAccessor.Tenant.Id); // ← Multi-tenancy
        await repo.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);  // ← Commits and dispatches domain events
        return Result.Ok(new CreateProductResponse(product.Id));
    }
}
```

**Queries (Reads):**
- Use `IQuery<Result<TResponse>>`
- Handler: `IQueryHandler<TQuery, Result<TResponse>>`
- Can use Dapper for raw SQL if needed (performance optimization)

## Multi-Tenancy (Row-Level Security)

**Every entity MUST have `EstablishmentId`:**
```csharp
public sealed class Product : Entity
{
    public Guid EstablishmentId { get; private set; }  // ← Required
    // ... other properties
}
```

**In handlers:**
```csharp
tenantAccessor.Tenant.Id  // ← Current establishment from JWT claim
```

**Query Filters automatically applied** by EF Core in `ApplicationDbContext.ApplyQueryFilters()` — no need to manually filter by `EstablishmentId` in queries.

## Domain Model (DDD Tactical)

**Entities:**
- Inherit from `Entity` base class (gives `Guid Id` via `Guid.CreateVersion7()`)
- Use **private setters** — only modify via methods
- Constructors enforce invariants (validation in constructor or throw `DomainException`)

```csharp
public sealed class Product : Entity
{
    public string Name { get; private set; }  // ← private set!
    
    public Product(string name, ...) {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("...");
        Name = name;
    }
    
    public void Update(string? name = null, ...) {
        Name = name ?? Name;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Domain Events:**
- Use `AddDomainEvent(new XxxEvent(...))` in entity methods
- Events dispatched automatically when `unitOfWork.SaveChangesAsync()` is called

## Minimal API Endpoints

**Pattern:**
```csharp
public static class CreateProductEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        CreateProductCommand command,
        ISender sender,  // ← Mediator
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess 
            ? result.ToCreated($"/api/products/{result.Value.ProductId}") 
            : result.ToBadRequest();
    }
}
```

**Register in Feature bootstrap:**
```csharp
public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/products").WithTags("Products");
    CreateProductEndpoint.MapEndpoint(group);  // ← One call per endpoint
    // ...
    return app;
}
```

## Validation (FluentValidation)

Every Command **should** have a Validator class:
```csharp
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

Validation runs automatically via `ValidationPipelineBehavior` pipeline behavior before handlers execute.

## Developer Workflow

**Local development:**
```bash
dotnet run --project src/Devlivery          # Run API (auto-migrates DB in Dev)
dotnet test test/Devlivery.Tests            # Run tests
```

**Using Makefile (Windows):**
```bash
make build                  # Build solution
make test                   # Run all tests
make db-add V=001           # Add migration for ApplicationDbContext
make id-add V=001           # Add migration for Identity context
make db-update              # Apply migrations
```

**Docker:**
```bash
docker compose up --build   # Run app + PostgreSQL container
```

## Testing

Tests use **Testcontainers** (PostgreSQL 16 in Docker) + **Respawn** (database reset between tests).

**Base class:** `BaseWebApplicationFactory<Program>` provides:
- Isolated PostgreSQL container per test run
- `ResetDatabaseAsync()` to clean DB between tests

**Pattern:**
```csharp
public class ProductTests(BaseWebApplicationFactory<Program> factory) 
    : WebApiBaseFixture(factory)
{
    [Fact]
    public async Task CreateProduct_Should_Return_201()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/products", command);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## Common Pitfalls

❌ **Don't** create Controllers — use Minimal API Endpoints  
❌ **Don't** forget `ITenantAccessor.Tenant.Id` when creating entities  
❌ **Don't** call `DbContext.SaveChanges()` directly — use `IUnitOfWork.SaveChangesAsync()`  
❌ **Don't** put business logic in handlers — it belongs in domain entities  
❌ **Don't** use public setters on entities — use methods like `Update()`, `SetAsAvailable()`  

✅ **Do** follow the feature folder structure exactly  
✅ **Do** use `Result<T>` from FluentResults for error handling  
✅ **Do** read ADRs for context on "why" decisions were made  
✅ **Do** register handlers in `XxxFeature.AddXxxFeature()` method
