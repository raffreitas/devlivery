# ASP.NET Core Identity com JWT Authentication

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Autenticação e Autorização

## Contexto e Problema

Aplicações multi-tenant precisam de autenticação e autorização robustas para garantir que usuários acessem apenas recursos do seu estabelecimento. Implementar autenticação do zero é complexo e propenso a erros de segurança. ASP.NET Core Identity oferece infraestrutura completa, mas precisa ser integrada com JWT para APIs stateless.

A estrutura do repositório revela esta decisão através da organização:

```
Shared/Infrastructure/Identity/
├── Context/ApplicationIdentityDbContext.cs  # DbContext para Identity
├── Tokens/Service/JwtTokenService.cs       # Geração de JWT
├── Users/Services/IdentityService.cs        # Serviço de autenticação
└── Migrations/                              # Migrations do Identity

Features/Auth/
└── Commands/Login/                          # Endpoint de login
```

**Problema:** Como implementar autenticação e autorização seguras para API multi-tenant, garantindo que tokens JWT contenham informações de tenant e sejam validados automaticamente?

## Opções Consideradas

* **Autenticação Custom** - Implementar do zero (complexo, propenso a erros)
* **ASP.NET Core Identity (Cookies)** - Identity com cookies (não funciona bem para APIs)
* **ASP.NET Core Identity + JWT** - Identity para gerenciamento de usuários, JWT para tokens (híbrido)
* **IdentityServer/Duende** - Solução completa de autenticação (complexidade desnecessária para monólito)

## Decisão

**Escolhida:** "ASP.NET Core Identity + JWT", porque:

1. Seguro: Identity gerencia senhas, lockouts, e políticas de segurança
2. Flexível: JWT permite APIs stateless sem cookies
3. Multi-tenant: JWT pode conter claims de tenant (EstablishmentId)
4. Padrão: solução padrão do .NET, bem documentada e suportada
5. Integrado: funciona nativamente com autorização e policies do ASP.NET Core

### Implementação Técnica

A decisão se materializa em:

**Identity DbContext Separado:**
```csharp
// Shared/Infrastructure/Identity/Context/ApplicationIdentityDbContext.cs
public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
        : base(options)
    {
    }
}

// Startup.cs - Dois DbContexts separados
services.AddDbContext<ApplicationDbContext>(...);      // Dados da aplicação
services.AddDbContext<ApplicationIdentityDbContext>(...);  // Dados de autenticação
```

**JWT Token Service:**
```csharp
// Shared/Infrastructure/Identity/Tokens/Service/JwtTokenService.cs
public interface ITokenService
{
    string GenerateToken(ApplicationUser user, Guid establishmentId);
}

public sealed class JwtTokenService : ITokenService
{
    public string GenerateToken(ApplicationUser user, Guid establishmentId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new("establishment_id", establishmentId.ToString())  // ← Tenant claim
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_settings.ExpirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**Identity Service:**
```csharp
// Shared/Infrastructure/Identity/Users/Services/IdentityService.cs
public interface IIdentityService
{
    Task<Result<LoginResponse>> LoginAsync(string email, string password);
}

public sealed class IdentityService : IIdentityService
{
    public async Task<Result<LoginResponse>> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return Result.Fail("Credenciais inválidas.");
        }

        var establishmentId = user.EstablishmentId;  // ← Obtém tenant do usuário
        var token = _tokenService.GenerateToken(user, establishmentId);

        return Result.Ok(new LoginResponse(user.Id, user.UserName!, token));
    }
}
```

**Login Endpoint:**
```csharp
// Features/Auth/Commands/Login/LoginEndpoint.cs
app.MapPost("/api/auth/login", async (LoginCommand command, ISender sender, ct) =>
{
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Ok(new ApiResponse<LoginResponse>(result.Value))
        : Results.BadRequest(new ApiResponse(result.Errors));
});
```

**JWT Authentication Configuration:**
```csharp
// Startup.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
        };
    });
```

**Tenant Extraction from JWT:**
```csharp
// Shared/Infrastructure/Tenancy/Middleware/TenantRegisterMiddleware.cs
// Extrai EstablishmentId do JWT claim e registra no TenantAccessor
```

### Consequências

* ✅ **Bom:** Seguro: Identity gerencia senhas, lockouts, políticas de segurança
* ✅ **Bom:** Flexível: JWT permite APIs stateless sem cookies
* ✅ **Bom:** Multi-tenant: JWT contém claims de tenant (EstablishmentId)
* ✅ **Bom:** Padrão: solução padrão do .NET, bem documentada
* ✅ **Bom:** Integrado: funciona nativamente com autorização do ASP.NET Core
* ⚠️ **Neutro:** Requer dois DbContexts separados (aplicação + identity)
* ⚠️ **Ruim:** JWT tokens não podem ser revogados facilmente (requer blacklist ou refresh tokens)
* ⚠️ **Ruim:** Configuração inicial pode ser complexa (mas bem documentada)

