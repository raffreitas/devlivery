# Devlivery WebAPI — AI Agent Guide

Quick reference for AI coding agents working in this .NET 9 Minimal API using Vertical Slice Architecture + CQRS.

## Architecture Overview

**Pattern**: Vertical Slice Architecture (VSA) with CQRS — each feature is self-contained with its Commands, Queries, Handlers, and Endpoints.

**Structure**:
`
Features/[FeatureName]/
├── Commands/[CommandName]/
│   ├── [CommandName]Command.cs      # Input DTO + FluentValidation
│   ├── [CommandName]Handler.cs      # Business logic, returns Result<T>
│   ├── [CommandName]Endpoint.cs     # HTTP endpoint with typed results
│   └── [CommandName]Response.cs     # Output DTO
├── Queries/[QueryName]/             # Read-only operations (same structure)
├── Domain/                          # Entities (if needed)
└── [FeatureName]Feature.cs          # DI registration + endpoint mapping
`

**Database**: Two DbContexts (`ApplicationDbContext` for app data, `ApplicationIdentityDbContext` for auth) using the same PostgreSQL connection string (`DefaultConnection`) with snake_case naming via `UseSnakeCaseNamingConvention()`.

## Critical Patterns (Do NOT Deviate)

### 1. Multi-Tenancy Pattern
**All entities MUST have an `EstablishmentId`** for tenant isolation:

`csharp
// Entity definition
public sealed class Product : Entity
{
    public Guid EstablishmentId { get; private set; }
    // ... other properties
}

// In Handlers - inject ITenantAccessor and use tenant ID
public sealed class CreateProductHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateProductResponse>> HandleAsync(CreateProductCommand command, ...)
    {
        var product = new Product(..., tenantAccessor.Tenant.Id);  // Always pass tenant ID
    }
}

**How it works:**
- JWT token contains `establishment_id` claim (added during login via `ITokenService`)
- `TenantRegisterMiddleware` extracts tenant from JWT → stores in `ITenantAccessor`
- All handlers inject `ITenantAccessor` to get current tenant

**Critical rules:**
- Commands: Pass `tenantAccessor.Tenant.Id` when creating entities
- Login bypassed: Middleware skips `/login`, `/health`, `/scalar`, `/openapi` paths

### 2. API Response Pattern
All endpoints use **ASP.NET Core Typed Results** + **RFC 7807 Problem Details**:

`csharp
// Endpoint signature example
private static async Task<Results<Created<ApiResponse<ProductResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(...)

// Success responses (wrap in ApiResponse<T>)
return result.ToCreated($"/api/products/{result.Value.Id}", "Product created successfully");
return result.ToOk("Operation successful");
return result.ToNoContent();

// Error responses (Problem Details)
return validationResult.ToValidationProblem();  // 400 with validation errors
return result.ToBadRequestProblem();            // 400 business error
return result.ToNotFoundProblem();              // 404 not found
`

See `docs/API-RESPONSE-PATTERN.md` for complete examples.

### 3. Validation Pattern
Validators are **explicitly called** in endpoints (not automatic):

`csharp
public sealed class Validator : AbstractValidator<CreateProductCommand>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(200).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
    }
}

// In endpoint
var validationResult = await validator.ValidateAsync(request, ct);
if (!validationResult.IsValid)
    return validationResult.ToValidationProblem();
`

**All validation messages MUST be in Portuguese (PT-BR)** — use `.WithMessage(...)` with placeholders like `{PropertyName}`, `{MaxLength}`.

### 4. Error Handling Pattern
Use **FluentResults** — never throw business exceptions:

`csharp
// In Handler
if (product is null)
    return Result.Fail<ProductResponse>("Produto não foi encontrado");

return Result.Ok(response);
`

### 5. Primary Constructors
Always use C# primary constructors for dependency injection:

`csharp
public sealed class CreateProductHandler(ApplicationDbContext dbContext, ILogger<CreateProductHandler> logger)
{
    public async Task<Result<CreateProductResponse>> HandleAsync(...) { }
}
`

### 6. Query Filtering Pattern
For list queries with optional filters, use nullable parameters in the Query record:

`csharp
// Query with optional filters
public sealed record GetAllOrdersQuery(DateTime? StartDate, DateTime? EndDate, string? PaymentMethod);

// Handler applies filters conditionally
var query = dbContext.Orders.AsQueryable();
if (request.StartDate.HasValue)
    query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
`

## Development Workflow

### Start App Locally
`ash
docker-compose up -d                    # Start PostgreSQL (port 5432)
dotnet run --project src/Devlivery  # Run API (auto-migrates in Dev)
`

**Auto-migration**: In Development, `Startup.ConfigureApp()` runs migrations and seeds data automatically on startup.

### Database Migrations
**Two contexts**: Always specify `-c ApplicationDbContext` or `-c ApplicationIdentityDbContext`.

**Using Makefile** (preferred on Linux/macOS):
`ash
make migration-db VERSION=002              # Create application migration
make migration-identity VERSION=002        # Create identity migration
make migration-update-db                   # Apply application migrations
make migration-update-identity             # Apply identity migrations
`

**Using PowerShell/Windows**:
`ash
.\scripts\apply-migrations.ps1             # Apply all migrations locally
`

**Using EF Core directly**:
```bash
dotnet ef migrations add v002 -o ./Shared/Database/Migrations -c ApplicationDbContext
dotnet ef database update -c ApplicationDbContext
```

**Migration naming**: Use `vXXX` format (v001, v002, v003).

### CI/CD
GitHub Actions on `main` branch (`.github/workflows/main-build-deploy.yml`):
1. Builds and tests solution
2. **Applies migrations to production** using `DATABASE_CONNECTION_STRING` secret for both contexts

**Important**: Migrations run automatically on main branch — no manual production migrations needed.

**Note**: Docker image building and version tagging are currently disabled in the workflow (configured with `if: false`). To enable, remove the `if: false` conditions from the Docker-related steps in `.github/workflows/main-build-deploy.yml`.

## Adding a New Feature

Follow this exact sequence:

### 1. Create folder structure
`
Features/MyFeature/Commands/DoSomething/
`

### 2. Create Command with validation
`csharp
// DoSomethingCommand.cs
public sealed record DoSomethingCommand(string Name, decimal Value);

public sealed class Validator : AbstractValidator<DoSomethingCommand>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}
`

### 3. Create Handler
`csharp
// DoSomethingHandler.cs
using FluentResults;

public sealed class DoSomethingHandler(ApplicationDbContext dbContext, ITenantAccessor tenantAccessor)
{
    public async Task<Result<DoSomethingResponse>> HandleAsync(
        DoSomethingCommand command,
        CancellationToken cancellationToken = default)
    {
        // Business logic here - always pass tenant ID when creating entities
        var entity = new MyEntity(..., tenantAccessor.Tenant.Id);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok(new DoSomethingResponse());
    }
}
`

### 4. Create Endpoint with Typed Results
`csharp
// DoSomethingEndpoint.cs
using Devlivery.Shared.Extensions;

public static class DoSomethingEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", Handle)
            .Produces<ApiResponse<DoSomethingResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<DoSomethingResponse>>, ValidationProblem, BadRequest<ProblemDetails>>> Handle(
        DoSomethingCommand request,
        IValidator<DoSomethingCommand> validator,
        DoSomethingHandler handler,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var result = await handler.HandleAsync(request, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/my-feature/{result.Value.Id}", "Resource created successfully")
            : result.ToBadRequestProblem();
    }
}
`

### 5. Create Feature registration
`csharp
// MyFeature.cs
public static class MyFeature
{
    public static IServiceCollection AddMyFeature(this IServiceCollection services)
    {
        services.AddScoped<DoSomethingHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapMyFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/my-feature").WithTags("MyFeature");
        DoSomethingEndpoint.MapEndpoint(group);
        return app;
    }
}
`

### 6. Register in Startup.cs
`csharp
// In ConfigureBuilder:
builder.Services.AddMyFeature();

// In ConfigureApp:
app.MapMyFeatureEndpoints();
`

## Key Files to Reference

**Examples**:
- `Features/Products/Commands/CreateProduct/*` — complete Command pattern
- `Features/Orders/` — complex entity with relationships (OrderItems)
- `Features/Auth/` — JWT authentication with Identity

**Infrastructure**:
- `Shared/Extensions/ResultExtensions.cs` — ToOk, ToCreated, ToBadRequestProblem helpers
- `Shared/Extensions/ValidationExtensions.cs` — ToValidationProblem
- `Shared/Models/ApiResponse.cs` — response wrapper
- `Shared/Presentation/GlobalExceptionHandler.cs` — unhandled exception handler

**Configuration**:
- `Startup.cs` — app boot, feature registration, auto-migration
- `Program.cs` — minimal entry point
- `appsettings.json` — connection strings, JWT config

## Common Gotchas

1. **Two DbContexts**: Always specify `-c ApplicationDbContext` or `-c ApplicationIdentityDbContext` in EF commands
2. **Same connection string**: Both contexts use `DefaultConnection` — they share the same database
3. **Manual validation**: Validation is NOT automatic — must call `validator.ValidateAsync()` in endpoints
4. **FluentResults pattern**: Return `Result.Ok()` or `Result.Fail()`, never throw business exceptions
5. **Typed Results**: Always use explicit typed results (e.g., `Results<Ok<T>, NotFound<P>>`)
6. **Validation messages**: All validation messages must be in Portuguese PT-BR with `.WithMessage(...)`
7. **Handler registration**: Each handler must be manually registered in Feature's `Add[Feature]Feature()` method
8. **UTC timestamps**: Always use `DateTime.UtcNow` for `CreatedAt`/`UpdatedAt`
9. **CancellationToken**: Always pass through to async DB operations
10. **Query filtering**: Use nullable parameters in Query records for optional filters, apply conditionally in Handler
11. **Multi-tenancy**: ALL entities must have `EstablishmentId`; always inject `ITenantAccessor` in handlers;
12. **Integration tests**: Always call `await ResetDatabaseAsync()` first; use `Prepare()` helper for auth setup

## Testing

### Integration Tests
Use **Testcontainers + Respawn** pattern with **Collections per Feature**:

`csharp
[Collection("Products Tests")]  // Shares PostgreSQL container with all Products tests
[Trait("Category", "Integration Tests")]
public sealed class CreateProductEndpointTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture<ProductsWebApplicationFactory>(factory)
{
    [Fact]
    public async Task Test()
    {
        await ResetDatabaseAsync();  // ALWAYS call first - clears data in ~50-100ms
        
        var (user, establishment, token) = await Prepare();  // Creates test user + establishment
        var command = new CreateProductCommand(...);
        
        var response = await PostAsync("/api/products", command, token);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }
}
`

**Critical rules:**
- ALWAYS call `await ResetDatabaseAsync()` at the start of each test
- Each feature has its own `WebApplicationFactory` + Collection (e.g., `ProductsWebApplicationFactory`)
- Use `Prepare()` helper to create authenticated users with establishment context
- Respawn cleans data 50x faster than recreating the database (~50-100ms vs 2-5s)

See `docs/INTEGRATION-TESTS.md` for complete guide.

### Manual Testing
Use `Devlivery.http` (root of project) with REST Client extension in VS Code or Rider.

**Test credentials**:
`
Email: admin@pizza.com
Password: 123456
`

**Endpoints**: `/scalar/v1` for Scalar UI, `/health` for health checks.

---

For detailed API response patterns, see `docs/API-RESPONSE-PATTERN.md`.
