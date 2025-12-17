# ASP.NET Core Identity para Autenticação e Autorização

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Estratégia de Segurança e Gestão de Usuários

## Contexto e Problema

Aplicações precisam de autenticação (quem é o usuário) e autorização (o que ele pode fazer). Implementações customizadas de auth são propensas a falhas de segurança. ASP.NET Core oferece Identity (framework full-featured) ou autenticação JWT minimalista. A escolha impacta gestão de usuários, reset de senha, two-factor authentication, e integração com providers externos.

A configuração do projeto revela uso de ASP.NET Identity:

```xml
<!-- Devlivery.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.1"/>
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.1"/>
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.15.0"/>
```

```csharp
// Startup.cs
services.AddIdentityFeature(configuration);
services.AddAuthorizationFeature();

// Makefile
CONTEXT_IDENTITY = ApplicationIdentityDbContext
```

**Problema:** Como implementar autenticação robusta sem reinventar a roda, mantendo segurança e flexibilidade?

## Opções Consideradas

* **Autenticação JWT Minimalista** - Implementação custom de geração/validação de tokens
* **ASP.NET Core Identity** - Framework completo com user management, roles, claims
* **IdentityServer/Duende** - OAuth 2.0 / OpenID Connect server
* **Auth0 / Azure AD B2C** - Serviços gerenciados de identidade

## Decisão

**Escolhida:** "ASP.NET Core Identity com JWT Bearer", porque:

1. **Segurança Comprovada:** Battle-tested framework com best practices embutidas
2. **Gestão Completa de Usuários:** Registration, login, password reset, email confirmation
3. **Extensibilidade:** Suporte a roles, claims, two-factor authentication
4. **Integração com EF Core:** User store baseado em PostgreSQL
5. **JWT Tokens:** Stateless authentication adequado para APIs RESTful

### Implementação Técnica

**DbContext Separado para Identity:**

```csharp
// Shared/Infrastructure/Identity/Context/ApplicationIdentityDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Devlivery.Shared.Infrastructure.Identity.Users.Models;

public sealed class ApplicationIdentityDbContext(
    DbContextOptions<ApplicationIdentityDbContext> options
) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Customização de tabelas Identity
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<ApplicationRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
    }
}
```

**Modelo de Usuário Customizado:**

```csharp
// Shared/Infrastructure/Identity/Users/Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    // Propriedades customizadas
    public string? FullName { get; set; }
    public Guid EstablishmentId { get; set; }  // Multi-tenancy
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
```

**Configuração de Identity:**

```csharp
// Shared/Infrastructure/Identity/IdentityFeature.cs
public static class IdentityFeature
{
    public static IServiceCollection AddIdentityFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext separado para Identity
        services.AddDbContext<ApplicationIdentityDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationIdentityDbContext).Assembly.FullName)
            );
            options.UseSnakeCaseNamingConvention();
        });

        // Identity Core
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;  // Configurável
        })
        .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
        .AddDefaultTokenProviders();

        // JWT Bearer Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero  // Remove margem de expiração
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
```

**Geração de Token JWT:**

```csharp
// Features/Auth/Commands/Login/LoginHandler.cs
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public sealed class LoginHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration
) : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        // Busca usuário
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null || !user.IsActive)
        {
            return Result.Fail("Invalid credentials");
        }

        // Valida senha
        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Result.Fail("Invalid credentials");
        }

        // Atualiza last login
        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        // Gera JWT Token
        var token = GenerateJwtToken(user);

        return Result.Ok(new LoginResponse(token, user.FullName, user.Email));
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("EstablishmentId", user.EstablishmentId.ToString()),  // Multi-tenancy
            new("FullName", user.FullName ?? ""),
        };

        // Adiciona roles como claims
        var userRoles = userManager.GetRolesAsync(user).Result;
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),  // Expiração do token
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**Configuração (appsettings.json):**

```json
{
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key-here-replace-in-production",
    "Issuer": "devlivery-api",
    "Audience": "devlivery-clients",
    "ExpirationHours": 8
  }
}
```

**Autorização por Roles:**

```csharp
// Shared/Infrastructure/Authorization/AuthorizationFeature.cs
public static class AuthorizationFeature
{
    public static IServiceCollection AddAuthorizationFeature(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
            .AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager", "Admin"))
            .AddPolicy("RequireEstablishmentAccess", policy => 
                policy.RequireAssertion(context =>
                {
                    var establishmentIdClaim = context.User.FindFirst("EstablishmentId")?.Value;
                    return !string.IsNullOrEmpty(establishmentIdClaim);
                }));

        return services;
    }

    public static IApplicationBuilder UseAuthorizationFeature(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
```

**Uso em Endpoints:**

```csharp
// Features/Products/Commands/CreateProduct/CreateProductEndpoint.cs
group.MapPost("/", CreateProduct)
    .RequireAuthorization()  // Requer autenticação
    .WithOpenApi();

// Autorização por role
group.MapDelete("/{id}", DeleteProduct)
    .RequireAuthorization("RequireAdminRole")  // Apenas Admin
    .WithOpenApi();
```

**Migrations Separadas:**

```bash
# Makefile
id-add V=001:
    dotnet ef migrations add v$(V) -p src/Devlivery -o Shared/Infrastructure/Identity/Migrations -c ApplicationIdentityDbContext

id-update:
    dotnet ef database update -p src/Devlivery -c ApplicationIdentityDbContext
```

### Consequências

* ✅ **Bom:** Framework maduro e seguro (password hashing, lockout, claims)
* ✅ **Bom:** Gestão completa de usuários (CRUD, roles, password reset)
* ✅ **Bom:** Integração nativa com EF Core e ASP.NET Core
* ✅ **Bom:** Extensível (two-factor auth, external logins como Google/Microsoft)
* ✅ **Bom:** JWT stateless adequado para APIs RESTful
* ✅ **Bom:** DbContext separado facilita migração de identity isoladamente
* ⚠️ **Neutro:** Complexidade inicial maior que JWT custom (trade-off aceitável)
* ⚠️ **Ruim:** Tokens JWT não podem ser revogados (mitigado por expiração curta + refresh tokens)
* ⚠️ **Ruim:** Schema de tabelas Identity é complexo (muitas tabelas relacionadas)

### Features de Segurança Implementadas

**1. Password Hashing:**
```csharp
// Identity usa PBKDF2 com salt aleatório automaticamente
await userManager.CreateAsync(user, password);
```

**2. Lockout após Tentativas Falhas:**
```csharp
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
```

**3. Claims-Based Authorization:**
```csharp
var establishmentId = User.FindFirst("EstablishmentId")?.Value;
```

**4. Token Expiration:**
```csharp
expires: DateTime.UtcNow.AddHours(8)
```

### Estrutura de Tabelas (PostgreSQL)

```sql
-- Gerado por Identity Migrations
users                   -- ApplicationUser
roles                   -- ApplicationRole
user_roles              -- Many-to-Many (users ↔ roles)
user_claims             -- Claims customizadas por usuário
role_claims             -- Claims por role
user_logins             -- External logins (Google, Microsoft)
user_tokens             -- Password reset, email confirmation tokens
```

### Fluxo de Autenticação

```
1. Cliente → POST /api/auth/login { email, password }
2. LoginHandler valida credenciais via UserManager
3. Gera JWT com claims (user id, email, establishment id, roles)
4. Cliente armazena token (localStorage/sessionStorage)
5. Requests subsequentes incluem: Authorization: Bearer <token>
6. Middleware valida token e popula User.Identity
7. Endpoint acessa claims: User.FindFirst("EstablishmentId")
```

### Refresh Tokens (Futuro)

```csharp
// Para tokens de longa duração
public record RefreshTokenResponse(string AccessToken, string RefreshToken);

// Armazenar refresh token no banco
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
```

**Princípio:** "Don't roll your own auth. Use battle-tested frameworks and follow security best practices."

### Referências

- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
