# Devlivery WebAPI - AI Coding Agent Instructions

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

### 4. Create Endpoint with manual validation
```csharp
// DoSomethingEndpoint.cs
using FluentValidation;

public static class DoSomethingEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/do-something", async (
            IValidator<DoSomethingCommand> validator,
            DoSomethingCommand request,
            DoSomethingHandler handler,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(request, ct);
            
            if (result.IsFailed)
            {
                return Results.Problem(result.Errors[0].Message);
            }

            return Results.Created($"/api/my-feature/{result.Value.Id}", result.Value);
        });
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

### 1. Dependency Injection via Constructor
Always use primary constructors for handlers:
```csharp
public sealed class MyHandler(ApplicationDbContext dbContext, ILogger<MyHandler> logger)
```

### 2. FluentResults for error handling
Never throw exceptions for business logic failures. Use `Result<T>`:
```csharp
if (product is null)
    return Result.Fail<ProductResponse>("Product not found");

return Result.Ok(response);
```

### 3. Manual validation in endpoints
Unlike some CQRS frameworks, validation is **explicitly called** in each endpoint:
```csharp
var validationResult = await validator.ValidateAsync(request, ct);
if (!validationResult.IsValid)
    return Results.ValidationProblem(validationResult.ToDictionary());
```

### 4. Configuration extensions
Use custom extensions for safe config retrieval:
```csharp
var connString = configuration.GetConnectionStringOrThrow("DefaultConnection");
var settings = configuration.GetOrThrow<JwtTokenSettings>(JwtTokenSettings.SectionName);
```

### 5. Feature-based service registration
Each feature has `Add[Feature]Feature()` and `Map[Feature]Endpoints()` extension methods.

## Shared Infrastructure

### Authentication
JWT tokens configured in `Features/Auth/AuthFeature.cs`:
- Uses ASP.NET Core Identity with custom `ApplicationUser`
- Token settings in `appsettings.json` under `JwtTokenSettings`
- Configured via `AddTokensConfiguration()` method

### Global Exception Handler
`Shared/Presentation/GlobalExceptionHandler.cs` catches all unhandled exceptions and returns ProblemDetails with `requestId` for tracking.

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
