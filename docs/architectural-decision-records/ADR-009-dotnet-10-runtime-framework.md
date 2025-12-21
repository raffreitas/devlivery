# Uso de .NET 10.0 como Runtime e Framework Principal

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Escolha de Plataforma e Versão do Framework

## Contexto e Problema

Aplicações .NET podem ser desenvolvidas em diferentes versões do framework (.NET 6, 7, 8, 9, 10). Cada versão traz melhorias de performance, novas features e suporte a longo prazo (LTS vs STS - Standard Term Support). A escolha impacta disponibilidade de APIs, compatibilidade de bibliotecas e ciclo de atualizações.

A configuração do projeto revela a versão escolhida:

```xml
<!-- src/Devlivery/Devlivery.csproj -->
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>

<!-- test/Devlivery.Tests/Devlivery.Tests.csproj -->
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

```dockerfile
# src/Devlivery/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

**Problema:** Qual versão do .NET adotar balanceando modernidade, estabilidade e suporte de longo prazo?

## Opções Consideradas

* **.NET 6 (LTS)** - Suporte até novembro de 2024 (já expirado), versão estável
* **.NET 8 (LTS)** - Suporte até novembro de 2026, versão LTS mais recente
* **.NET 9 (STS)** - Suporte até maio de 2025, versão mais recente antes da 10
* **.NET 10 (Preview/STS)** - Versão de ponta com features mais modernas

## Decisão

**Escolhida:** ".NET 10.0", porque:

1. **Features Modernas:** Acesso às mais recentes melhorias de C# 13 e runtime
2. **Performance:** Melhorias incrementais de JIT, GC e throughput HTTP
3. **Ecossistema Atualizado:** Bibliotecas (EF Core 10, OpenTelemetry) estão atualizadas
4. **Projeto Novo:** Não há débito técnico de versões anteriores
5. **Alinhamento com Preview:** Projeto iniciado quando .NET 10 já estava disponível (dezembro 2025)

**Trade-off Consciente:** .NET 10 não é LTS, mas o projeto pode migrar para .NET 12 LTS (previsto para novembro 2027) quando necessário.

### Implementação Técnica

**Target Framework Moniker (TFM):**

```xml
<!-- Todos os projetos .csproj -->
<TargetFramework>net10.0</TargetFramework>
```

**Imagens Docker Oficiais:**

```dockerfile
# Runtime otimizado para produção (sem SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# SDK completo para build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

**Versões de Pacotes Compatíveis:**

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.1"/>
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.1"/>
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0"/>
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.1"/>
```

**Features do C# 13 Utilizadas:**

```csharp
// 1. Primary Constructors (classes e structs)
public sealed class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    // dbContext é automaticamente um field privado
}

// 2. Collection Expressions
var items = [item1, item2, item3];  // Ao invés de new List<T> { ... }

// 3. Inline Arrays (Value Objects)
public readonly record struct Money(decimal Amount, string Currency);
```

**APIs .NET 10 Específicas:**

```csharp
// Guid.CreateVersion7() — UUIDs ordenados temporalmente
public Guid Id { get; protected init; } = Guid.CreateVersion7();

// Improved JSON Serialization (Source Generators)
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

**Performance Improvements:**

- **JIT Tiered Compilation:** Otimizações progressivas de código hot-path
- **Native AOT Support:** Preparado para compilação ahead-of-time (futuro)
- **HTTP/3 por padrão:** Kestrel com QUIC habilitado
- **Arm64 Optimization:** Performance nativa em arquiteturas ARM (Apple Silicon, Graviton)

### Consequências

* ✅ **Bom:** Acesso às features mais modernas de C# e runtime
* ✅ **Bom:** Melhor performance comparado a versões anteriores
* ✅ **Bom:** Ecossistema de bibliotecas totalmente atualizado
* ✅ **Bom:** Imagens Docker menores e mais eficientes
* ✅ **Bom:** Suporte oficial da Microsoft (STS até maio 2026)
* ⚠️ **Neutro:** Não é versão LTS (trade-off aceitável para projeto novo)
* ⚠️ **Ruim:** Requer atualização para .NET 12 LTS até maio 2026 (antes do EOL)
* ⚠️ **Ruim:** Algumas bibliotecas de terceiros podem não ter versões compatíveis (mitigado: pacotes principais já suportam)

### Ciclo de Atualização

**Cronograma de Suporte .NET:**

| Versão  | Tipo | Lançamento      | End of Support  | Status Atual  |
|---------|------|-----------------|-----------------|---------------|
| .NET 6  | LTS  | Nov 2021        | Nov 2024        | ❌ Expirado   |
| .NET 7  | STS  | Nov 2022        | Mai 2024        | ❌ Expirado   |
| .NET 8  | LTS  | Nov 2023        | Nov 2026        | ✅ Ativo      |
| .NET 9  | STS  | Nov 2024        | Mai 2025        | ✅ Ativo      |
| .NET 10 | STS  | Nov 2025        | Mai 2026        | ✅ **Atual**  |
| .NET 12 | LTS  | Nov 2027 (prev) | Nov 2030 (prev) | 🔮 Futuro     |

**Estratégia de Migração:**

1. **Até maio de 2026:** Manter .NET 10 (STS)
2. **Nov 2027:** Migrar para .NET 12 LTS para suporte de longo prazo
3. **Monitoramento:** Acompanhar breaking changes e deprecations via GitHub Discussions

**Comandos de Atualização:**

```bash
# Atualizar SDK local
dotnet --list-sdks
winget upgrade Microsoft.DotNet.SDK.10

# Atualizar pacotes NuGet
dotnet list package --outdated
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.1
```

**CI/CD GitHub Actions:**

```yaml
# .github/workflows/main-build-deploy.yml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'  # Versão específica do .NET 10
```

### Recursos do .NET 10 Aproveitados

**1. Minimal APIs Improvements:**
```csharp
// Endpoint Filters, Typed Results, Request/Response Validation
app.MapPost("/api/products", CreateProduct)
   .WithName("CreateProduct")
   .WithOpenApi()
   .RequireAuthorization();
```

**2. Native AOT (Preparação Futura):**
```xml
<!-- Habilitável quando necessário -->
<PublishAot>false</PublishAot>
<InvariantGlobalization>false</InvariantGlobalization>
```

**3. Improved Observability:**
```csharp
// OpenTelemetry integrado nativamente
builder.Services.AddOpenTelemetry()
    .WithTracing(...)
    .WithMetrics(...);
```

### Compatibilidade de Bibliotecas

**Pacotes Principais Validados:**

- ✅ Entity Framework Core 10.0.1
- ✅ ASP.NET Core Identity 10.0.1
- ✅ Npgsql 10.0.0
- ✅ FluentValidation 12.1.1 (compatível)
- ✅ FluentResults 4.0.0 (compatível)
- ✅ Dapper 2.1.66 (compatível)
- ✅ Mediator 3.1.0-preview.14
- ✅ OpenTelemetry 1.14.0+

### Justificativa da Escolha (Não-LTS)

Para projetos **greenfield** (iniciando do zero) em dezembro de 2025:

- .NET 8 LTS é de **2023** — já tem 2 anos, faltam features modernas
- .NET 10 STS traz melhorias significativas de DX e performance
- Janela de suporte (mai 2026) é suficiente para MVP e early adoption
- Migração para .NET 12 LTS (2027) será planejada antes do EOL

**Para projetos enterprise críticos:** Recomendaria .NET 8 LTS.  
**Para este projeto (startup/MVP):** .NET 10 STS é apropriado.

**Princípio:** "Use the latest stable version for greenfield projects. Plan migrations before end-of-support dates."

### Referências

- [.NET Release Schedule](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [What's New in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview)
