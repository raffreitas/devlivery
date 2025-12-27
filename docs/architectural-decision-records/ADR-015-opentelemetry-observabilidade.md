# OpenTelemetry para Observabilidade

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Observabilidade

## Contexto e Problema

Aplicações em produção precisam de observabilidade (logs, métricas, traces) para monitorar saúde, diagnosticar problemas e entender comportamento. Implementar observabilidade do zero é complexo e pode gerar overhead significativo. OpenTelemetry oferece padrão aberto e vendor-agnostic para instrumentação.

A estrutura do repositório revela esta decisão através da organização:

```
Shared/Infrastructure/Observability/
├── ObservabilityFeature.cs         # Configuração de OpenTelemetry
└── Middleware/RequestLoggingMiddleware.cs

Startup.cs
└── builder.AddObservabilityFeature()  # Registra instrumentação
```

**Problema:** Como implementar observabilidade (logs, métricas, traces) de forma padronizada e vendor-agnostic, sem acoplar a aplicação a ferramentas específicas de monitoramento?

## Opções Consideradas

* **Logging Apenas** - Apenas logs estruturados (limitado, sem métricas/traces)
* **APM Proprietário** - New Relic, Datadog, etc. (vendor lock-in)
* **OpenTelemetry** - Padrão aberto, vendor-agnostic (flexível, futuro-proof)
* **Sem Observabilidade** - Não implementar (não viável para produção)

## Decisão

**Escolhida:** "OpenTelemetry", porque:

1. Padrão aberto: não cria vendor lock-in, pode exportar para qualquer backend
2. Vendor-agnostic: funciona com Grafana, Prometheus, Jaeger, etc.
3. Completo: suporta logs, métricas e traces em um único padrão
4. Futuro-proof: padrão da CNCF, amplamente adotado
5. Instrumentação automática: instrumenta ASP.NET Core, EF Core, HTTP automaticamente

### Implementação Técnica

A decisão se materializa em:

**Observability Feature:**
```csharp
// Shared/Infrastructure/Observability/ObservabilityFeature.cs
public static class ObservabilityFeature
{
    public static IHostApplicationBuilder AddObservabilityFeature(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()      // ← Instrumenta HTTP requests
                .AddEntityFrameworkCoreInstrumentation()  // ← Instrumenta EF Core queries
                .AddHttpClientInstrumentation()      // ← Instrumenta HTTP clients
                .AddSource("Devlivery")              // ← Custom traces
                .AddOtlpExporter())                  // ← Exporta para OTLP (Grafana, etc.)
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()      // ← Métricas de HTTP
                .AddRuntimeInstrumentation()         // ← Métricas de runtime (.NET)
                .AddOtlpExporter())
            .WithLogging(logging => logging
                .AddOtlpExporter());

        return builder;
    }
}
```

**Registro no Startup:**
```csharp
// Startup.cs
builder.AddObservabilityFeature();  // ← Adiciona instrumentação

// ConfigureApp
app.UseObservabilityFeature();     // ← Middleware de logging
```

**Instrumentação Automática:**
- **ASP.NET Core:** Traces de HTTP requests, métricas de latência
- **EF Core:** Traces de queries SQL, métricas de duração
- **HTTP Client:** Traces de chamadas HTTP externas
- **Runtime:** Métricas de GC, threads, memória

**Custom Traces:**
```csharp
// Exemplo de trace customizado
using var activity = ActivitySource.StartActivity("CreateProduct");
activity?.SetTag("product.name", product.Name);
activity?.SetTag("product.price", product.Price);
```

**Exportação:**
- **OTLP (OpenTelemetry Protocol):** Exporta para backends compatíveis (Grafana, Jaeger, etc.)
- **Configurável:** Pode exportar para múltiplos backends simultaneamente

**Middleware de Logging:**
```csharp
// Shared/Infrastructure/Observability/Middleware/RequestLoggingMiddleware.cs
// Loga requests HTTP com informações estruturadas
```

### Consequências

* ✅ **Bom:** Padrão aberto: não cria vendor lock-in
* ✅ **Bom:** Vendor-agnostic: funciona com qualquer backend compatível
* ✅ **Bom:** Completo: suporta logs, métricas e traces
* ✅ **Bom:** Instrumentação automática: não requer código manual em cada operação
* ✅ **Bom:** Futuro-proof: padrão da CNCF, amplamente adotado
* ⚠️ **Neutro:** Requer backend de observabilidade (Grafana, Prometheus, etc.)
* ⚠️ **Ruim:** Pode adicionar overhead de performance (geralmente aceitável)
* ⚠️ **Ruim:** Configuração inicial pode ser complexa (mas bem documentada)

