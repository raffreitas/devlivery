## Devlivery WebAPI — Quick AI Agent Guide

This short guide tells an AI coding agent what matters most to be productive in this repository.

1) Big picture
- .NET 9 Minimal API using Vertical Slice Architecture (VSA) + CQRS. Each feature lives under `src/Devlivery.WebApi/Features/[FeatureName]` and contains Commands, Queries, Handlers, Endpoints and a `*Feature.cs` for DI/endpoint registration.
- Two DbContexts: `ApplicationDbContext` (app data) and `ApplicationIdentityDbContext` (identity). Both use the same `DefaultConnection` (Postgres) and snake_case naming.

2) Key repo patterns (do not deviate)
- Endpoints: Minimal API typed results (e.g. `Task<Results<Ok<ApiResponse<T>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(...)`). See `docs/API-RESPONSE-PATTERN.md` for canonical examples.
- Validation: Validators live with the Command/Query file and are named `Validator`. Endpoints MUST call `validator.ValidateAsync()` explicitly.
- Error handling: Use `FluentResults` (return `Result.Ok()` / `Result.Fail()`), and map to ProblemDetails via shared `ResultExtensions`.
- Success responses: wrap payloads in `ApiResponse<T>` and use `ToOk`, `ToCreated`, `ToNoContent` helpers from `Shared/Extensions`.

3) Where to look (high-value files)
- `src/Devlivery.WebApi/Program.cs` and `Startup.cs` — app boot, feature registration, auto-migration in Development.
- `src/Devlivery.WebApi/Features/*` — examples: `Products`, `Orders`, `Auth`, `Dashboard`.
- `src/Devlivery.WebApi/Shared/Extensions/` — `ResultExtensions`, `ValidationExtensions`, `ConfigurationExtensions` (use these helpers). 
- `docs/API-RESPONSE-PATTERN.md` — required response shapes and typed-results examples.
- `scripts/apply-migrations.ps1` and `Makefile` — migration commands used in development.

4) Common workflows & exact commands
- Start local DB + app (recommended):
  - `docker-compose up -d` (root of repo)
  - `dotnet run --project src/Devlivery.WebApi`
- Create/apply migrations (Makefile helper):
  - `make migration-db VERSION=v002` (ApplicationDbContext)
  - `make migration-identity VERSION=v002` (ApplicationIdentityDbContext)
  - `make migration-update-db` / `make migration-update-identity` (apply)
- Alternate: use EF tooling with `-c ApplicationDbContext` or `-c ApplicationIdentityDbContext`.

5) Conventions to enforce in edits
- Do not throw business exceptions — return `Result.Fail()` and map it to ProblemDetails.
- Keep feature cohesion: add handlers/validators/endpoints inside the same feature folder.
- Register handlers in the feature `Add[Feature]Feature()` method and map endpoints in `Map[Feature]Endpoints()`.
- Validation messages must be user-friendly and in PT-BR.

6) CI/CD and migrations note
- GitHub Actions on `main` applies migrations in CI using the `DATABASE_CONNECTION_STRING` secret. Avoid ad-hoc production migration changes without CI updates.

7) Quick examples (where to copy patterns)
- Create product flow: `Features/Products/Commands/CreateProduct/*` — follow the Command → Handler → Endpoint → Feature.cs pattern.
- API shape examples: `Features/Products/*Endpoint.cs` and `Shared/Models/ApiResponse.cs`.

8) When you need to run tests or verify changes
- Use `dotnet build` / `dotnet run` for quick checks. Use `Devlivery.WebApi.http` (root of project) with the REST Client extension or run Postman to exercise endpoints.

If anything here seems missing or unclear, tell me which area you want expanded (migrations, endpoint examples, or feature wiring) and I will iterate.

-- end# Devlivery WebAPI - AI Coding Agent Instructions

## Architecture Overview

This is a **.NET 9 Web API** using **Vertical Slice Architecture (VSA)** with **CQRS pattern**. Each feature is self-contained in `Features/` with its own domain, commands, queries, and endpoints.

**Key principle**: High cohesion within slices, low coupling between them. Changes to one feature should rarely affect others.

## Project Structure Pattern

```
Features/
└── [FeatureName]/
    ├── Commands/[CommandName]/         # State-changing operations
    │   ├── [CommandName]Command.cs     # Input DTO + FluentValidation
    │   ├── [CommandName]Handler.cs     # Business logic, returns Result<T>
    │   ├── [CommandName]Endpoint.cs    # HTTP endpoint config
    │   └── [CommandName]Response.cs    # Output DTO
    ├── Queries/[QueryName]/            # Read-only operations
    ├── Domain/                         # Entities (if needed)
    └── [FeatureName]Feature.cs         # DI registration + endpoint mapping
```

## Creating a New Feature (Step-by-Step)

### 1. Create the folder structure
```
Features/MyFeature/Commands/DoSomething/
```

### 2. Create Command with validation
```csharp
// DoSomethingCommand.cs
using FluentValidation;

public sealed record DoSomethingCommand(string Name, decimal Value);

public sealed class Validator : AbstractValidator<DoSomethingCommand>
{
    public Validator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).GreaterThan(0);
    }
}
```

### 3. Create Handler with FluentResults
```csharp
// DoSomethingHandler.cs
using FluentResults;
using Devlivery.WebApi.Shared.Infrastructure.Database.Context;

public sealed class DoSomethingHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<DoSomethingResponse>> HandleAsync(
        DoSomethingCommand command,
        CancellationToken cancellationToken = default)
    {
        // Business logic here
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok(new DoSomethingResponse());
    }
}
```

### 4. Create Endpoint with Typed Results and Problem Details
```csharp
// DoSomethingEndpoint.cs
using Devlivery.WebApi.Shared.Extensions;
using Devlivery.WebApi.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public static class DoSomethingEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/do-something", Handle)
            .Produces<ApiResponse<DoSomethingResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
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
        {
            return validationResult.ToValidationProblem();
        }

        var result = await handler.HandleAsync(request, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/my-feature/{result.Value.Id}", "Resource created successfully")
            : result.ToBadRequestProblem();
    }
}
```

### 5. Create Feature registration file
```csharp
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
```

### 6. Register in Startup.cs
```csharp
// In ConfigureBuilder:
builder.Services.AddMyFeature();

// In ConfigureApp:
app.MapMyFeatureEndpoints();
```

## Database Migrations (Dual DbContext)

**Two separate DbContexts**:
- `ApplicationDbContext` - Application data (Products, Orders)
- `ApplicationIdentityDbContext` - Identity/Auth data (Users, Roles)

Both use **PostgreSQL with snake_case naming** via `UseSnakeCaseNamingConvention()`.

**Same connection string for both**: Both contexts use `ConnectionStrings:DefaultConnection` from config. The separation is logical (different DbContexts), not physical (different databases).

### Using Makefile (preferred)
```bash
# Create application migration
make migration-db VERSION=002

# Create identity migration
make migration-identity VERSION=002

# Apply migrations
make migration-update-db
make migration-update-identity

# Check status
make migration-status
```

### Using dotnet ef directly
```bash
dotnet ef migrations add v002 -o ./Shared/Infrastructure/Database/Migrations -c ApplicationDbContext
dotnet ef database update -c ApplicationDbContext
```

**Migration naming convention**: Use `vXXX` format (v001, v002, v003)

## Development Workflow

### Running locally
```bash
# Start PostgreSQL (uses docker-compose.yml)
docker-compose up -d

# Run the API
dotnet run --project src/Devlivery.WebApi

# Access at http://localhost:5000
# Swagger/Scalar UI at /scalar/v1
```

**Auto-migration**: In Development mode, `Startup.ConfigureApp()` automatically runs migrations and seeds data on startup. Both contexts are migrated.

**Database**: PostgreSQL container `devlivery-postgres` on port 5432, database name `devlivery`.

### Test credentials
```
Email: admin@pizza.com
Password: 123456
```

### CI/CD Pipeline
`.github/workflows/main-build-deploy.yml` triggers on pushes to `main` branch:
1. Builds solution, runs tests (if any exist)
2. Creates version tag: `vYYYY.MM.DD-{short-sha}` (e.g., `v2025.11.04-a1b2c3d`)
3. Builds Docker image, pushes to GitHub Container Registry with tags: `latest`, version tag, and branch-SHA
4. Creates GitHub Release with version tag
5. **Applies migrations to production** using `DATABASE_CONNECTION_STRING` secret

**Critical**: Migrations run automatically on main branch. No manual migration needed for production.

## Critical Patterns & Conventions

### 1. API Response Pattern (CRITICAL)
All endpoints MUST follow the standardized response pattern:

**Typed Results**: Use ASP.NET Core Typed Results for explicit status codes
```csharp
Task<Results<Ok<ApiResponse<T>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(...)
```

**Success responses**: Wrap data in `ApiResponse<T>`
```csharp
return result.ToOk("Operation successful");
return result.ToCreated($"/api/resource/{id}", "Resource created");
return result.ToNoContent(); // for DELETE
```

**Error responses**: Use Problem Details (RFC 7807)
```csharp
return validationResult.ToValidationProblem();  // 400 with validation errors
return result.ToBadRequestProblem();            // 400 with business error
return result.ToNotFoundProblem();              // 404 resource not found
```

**Available extension methods**:
- `ToOk<T>(message)` - 200 OK with ApiResponse
- `ToCreated<T>(uri, message)` - 201 Created with ApiResponse
- `ToNoContent()` - 204 No Content
- `ToBadRequestProblem()` - 400 Bad Request with ProblemDetails
- `ToNotFoundProblem()` - 404 Not Found with ProblemDetails
- `ToValidationProblem()` - 400 with validation errors

**Endpoint structure pattern**:
```csharp
public static void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapGet("{id:guid}", Handle)
        .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
}

private static async Task<Results<Ok<ApiResponse<ProductResponse>>, ValidationProblem, NotFound<ProblemDetails>>> Handle(...)
{
    // Implementation
}
```

See `docs/API-RESPONSE-PATTERN.md` for complete documentation.

### 2. Dependency Injection via Constructor
Always use primary constructors for handlers:
```csharp
public sealed class MyHandler(ApplicationDbContext dbContext, ILogger<MyHandler> logger)
```

### 3. FluentResults for error handling
Never throw exceptions for business logic failures. Use `Result<T>`:
```csharp
if (product is null)
    return Result.Fail<ProductResponse>("Product not found");

return Result.Ok(response);
```

### 4. Manual validation in endpoints
Unlike some CQRS frameworks, validation is **explicitly called** in each endpoint:
```csharp
var validationResult = await validator.ValidateAsync(request, ct);
if (!validationResult.IsValid)
    return validationResult.ToValidationProblem();
```

### 5. Configuration extensions
Use custom extensions for safe config retrieval:
```csharp
var connString = configuration.GetConnectionStringOrThrow("DefaultConnection");
var settings = configuration.GetOrThrow<JwtTokenSettings>(JwtTokenSettings.SectionName);
```

### 6. Feature-based service registration
Each feature has `Add[Feature]Feature()` and `Map[Feature]Endpoints()` extension methods.

## Shared Infrastructure

### Authentication
JWT tokens configured in `Features/Auth/AuthFeature.cs`:
- Uses ASP.NET Core Identity with custom `ApplicationUser`
- Token settings in `appsettings.json` under `JwtTokenSettings`
- Configured via `AddTokensConfiguration()` method

### Global Exception Handler
`Shared/Presentation/GlobalExceptionHandler.cs` catches all unhandled exceptions and returns RFC 7807 Problem Details with `traceId` for tracking.

**Example error response**:
```json
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

### Database Seeder
`Shared/Infrastructure/Database/Seeder/DatabaseSeeder.cs` runs automatically in Development to seed admin user and initial data.

## Common Gotchas

1. **Two DbContexts**: Remember to specify `-c ApplicationDbContext` or `-c ApplicationIdentityDbContext` in EF commands
2. **Same connection string**: Both contexts use `DefaultConnection` - they share the same database, not separate ones
3. **Validators must be registered**: FluentValidation auto-scans via `AddValidatorsFromAssemblyContaining<Program>()`
4. **Handler registration**: Each handler must be manually registered in the Feature file's `AddScoped<>()` calls
5. **Endpoint registration order**: Call `Add[Feature]Feature()` in `Startup.ConfigureBuilder()`, then `Map[Feature]Endpoints()` in `Startup.ConfigureApp()`
6. **CancellationToken**: Always pass through to async DB operations
7. **UTC timestamps**: Always use `DateTime.UtcNow` for `CreatedAt`/`UpdatedAt`
8. **Validation in endpoints**: Validation is NOT automatic - must explicitly call `validator.ValidateAsync()` in each endpoint
9. **FluentResults pattern**: Return `Result.Ok()` or `Result.Fail()`, never throw exceptions for business logic
10. **Typed Results**: Always use Typed Results (Results<Ok<T>, NotFound<P>>) for explicit status codes
11. **Problem Details**: All errors must return RFC 7807 Problem Details format
12. **ApiResponse wrapper**: All success responses must be wrapped in `ApiResponse<T>`
13. **Validation Messages**: All validation messages must be user-friendly and in Portuguese PT-BR.

## Example Features to Reference

- **Products** (`Features/Products/`) - Full CRUD implementation
- **Orders** (`Features/Orders/`) - Complex entity with relationships (OrderItems)
- **Auth** (`Features/Auth/`) - JWT authentication with UserManager
- **Dashboard** (`Features/Dashboard/`) - Simple read-only query without full CRUD

## File Naming Conventions

- Commands/Queries: `[Action][Entity]Command.cs` (e.g., `CreateProductCommand.cs`)
- Handlers: `[Action][Entity]Handler.cs`
- Endpoints: `[Action][Entity]Endpoint.cs`
- Responses: `[Action][Entity]Response.cs`
- Validators: Always named `Validator` class inside Command/Query file
