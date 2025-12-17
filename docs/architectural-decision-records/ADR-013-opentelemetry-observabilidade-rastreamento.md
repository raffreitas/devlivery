# OpenTelemetry para Observabilidade e Rastreamento Distribuído

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Estratégia de Monitoramento, Logging e Tracing

## Contexto e Problema

Aplicações em produção precisam de visibilidade para troubleshooting, análise de performance e detecção de anomalias. Existem múltiplas abordagens: logs tradicionais (file-based), Application Performance Monitoring (APM) proprietário (New Relic, Datadog), ou padrões abertos como OpenTelemetry.

A configuração do projeto revela adoção de OpenTelemetry:

```xml
<!-- Devlivery.csproj -->
<PackageReference Include="Grafana.OpenTelemetry" Version="1.3.0"/>
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.14.0"/>
<PackageReference Include="OpenTelemetry.Extensions" Version="1.14.0-beta.1"/>
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.14.0"/>
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.14.0"/>
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.14.0-beta.2"/>
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.14.0"/>
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.14.0"/>
```

```csharp
// Startup.cs
builder.AddObservabilityFeature();
```

**Problema:** Como implementar observabilidade de forma vendor-neutral e preparada para ambientes cloud-native?

## Opções Consideradas

* **Logging Tradicional (Serilog/NLog)** - Logs estruturados em arquivos ou stdout
* **APM Proprietário** - New Relic, Datadog, Application Insights
* **OpenTelemetry** - Padrão aberto para traces, metrics e logs
* **Elastic Stack (ELK)** - Elasticsearch + Logstash + Kibana

## Decisão

**Escolhida:** "OpenTelemetry com exportação OTLP", porque:

1. **Vendor Neutrality:** Não lock-in com fornecedor específico — pode exportar para qualquer backend
2. **Padrão da Indústria:** CNCF (Cloud Native Computing Foundation) standard
3. **Três Pilares de Observabilidade:** Traces (distributed tracing), Metrics (counters, gauges), Logs
4. **Instrumentação Automática:** Bibliotecas auto-instrumentam ASP.NET Core, EF Core, HTTP clients
5. **Integração com Grafana:** Visualização via Grafana Cloud/On-Prem

### Implementação Técnica

**Configuração de Observabilidade:**

```csharp
// Shared/Infrastructure/Observability/ObservabilityFeature.cs
public static class ObservabilityFeature
{
    public static WebApplicationBuilder AddObservabilityFeature(this WebApplicationBuilder builder)
    {
        var serviceName = "devlivery-api";
        var serviceVersion = "1.0.0";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment"] = builder.Environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName
                }))
            
            // TRACES (Distributed Tracing)
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("http.client_ip", httpRequest.HttpContext.Connection.RemoteIpAddress);
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;  // Captura queries SQL
                    options.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.query_time", command.CommandTimeout);
                    };
                })
                .AddHttpClientInstrumentation()
                .AddSource(serviceName)
                .AddOtlpExporter())
            
            // METRICS (Counters, Gauges, Histograms)
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()  // GC, threads, etc.
                .AddMeter("Devlivery.Orders")
                .AddMeter("Devlivery.Products")
                .AddOtlpExporter());

        return builder;
    }
}
```

**OTLP Exporter (OpenTelemetry Protocol):**

```json
// appsettings.json
{
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",  // Grafana Agent ou Collector
    "Protocol": "grpc"
  }
}
```

**Instrumentação Automática (ASP.NET Core):**

```
Requisição HTTP
    ↓
[OpenTelemetry AspNetCore Instrumentation]
    ├─ Cria Span: "POST /api/products"
    ├─ Captura: HTTP method, status code, route, duration
    └─ Propaga TraceId e SpanId via headers (W3C Trace Context)
    ↓
Mediator.Send()
    ↓
[OpenTelemetry EF Core Instrumentation]
    ├─ Cria Child Span: "SELECT * FROM products WHERE establishment_id = ?"
    ├─ Captura: SQL statement, duration, rows affected
    └─ Associa ao Parent Span (request HTTP)
    ↓
Resposta HTTP
    ↓
[Exportação OTLP para Grafana/Jaeger/Tempo]
```

**Trace Distribuído (Exemplo):**

```
TraceId: 4bf92f3577b34da6a3ce929d0e0e4736

├─ Span: POST /api/orders (200ms)
│   ├─ http.method: POST
│   ├─ http.status_code: 200
│   ├─ http.route: /api/orders
│   └─ establishment.id: aaaa-1111-2222-3333
│
│   ├─ Child Span: Validate CreateOrderCommand (5ms)
│   │   └─ validation.result: success
│   │
│   ├─ Child Span: SELECT * FROM products WHERE id IN (...) (15ms)
│   │   ├─ db.system: postgresql
│   │   ├─ db.statement: SELECT * FROM products WHERE...
│   │   └─ db.rows_affected: 3
│   │
│   ├─ Child Span: INSERT INTO orders (...) (10ms)
│   │   └─ db.statement: INSERT INTO orders...
│   │
│   └─ Child Span: Publish OrderCreatedEvent (20ms)
│       └─ event.name: OrderCreatedEvent
```

**Custom Metrics (Business Logic):**

```csharp
// Features/Orders/Commands/CreateOrder/CreateOrderHandler.cs
using System.Diagnostics.Metrics;

public sealed class CreateOrderHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    Meter meter
) : ICommandHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly Counter<long> _ordersCreatedCounter = 
        meter.CreateCounter<long>("orders.created", "orders", "Total orders created");
    
    private readonly Histogram<double> _orderValueHistogram = 
        meter.CreateHistogram<double>("order.value", "BRL", "Distribution of order values");

    public async ValueTask<Result<CreateOrderResponse>> Handle(...)
    {
        var order = Order.Create(command.EstablishmentId, items);
        
        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Incrementa métricas de negócio
        _ordersCreatedCounter.Add(1, new KeyValuePair<string, object?>("establishment_id", order.EstablishmentId));
        _orderValueHistogram.Record((double)order.TotalAmount);

        return Result.Ok(new CreateOrderResponse(order.Id));
    }
}
```

**Registro de Meters:**

```csharp
// Features/Orders/OrdersFeature.cs
public static IServiceCollection AddOrderFeature(this IServiceCollection services)
{
    // Registra Meter para métricas customizadas
    services.AddSingleton(new Meter("Devlivery.Orders", "1.0.0"));
    
    services.AddScoped<IOrderRepository, OrderRepository>();
    return services;
}
```

**Grafana Dashboard (Query Exemplo):**

```promql
# Total de orders criadas por establishment
sum(rate(orders_created_total[5m])) by (establishment_id)

# Percentil 95 de duração de requests
histogram_quantile(0.95, sum(rate(http_server_duration_bucket[5m])) by (le, http_route))

# Taxa de erros (5xx)
sum(rate(http_server_requests_total{http_status_code=~"5.."}[5m])) / sum(rate(http_server_requests_total[5m]))
```

### Consequências

* ✅ **Bom:** Vendor-neutral — pode trocar backend (Grafana, Jaeger, New Relic) sem mudar código
* ✅ **Bom:** Distributed tracing nativo — rastreia requests através de microserviços (futuro)
* ✅ **Bom:** Instrumentação automática — zero boilerplate para HTTP, EF Core, HTTP clients
* ✅ **Bom:** Três pilares unificados (traces, metrics, logs) em um padrão
* ✅ **Bom:** Integração com CNCF ecosystem (Prometheus, Grafana, Jaeger, Tempo)
* ✅ **Bom:** Performance otimizada — exportação assíncrona, sampling configurável
* ⚠️ **Neutro:** Requer infraestrutura de coleta (Grafana Agent, OTEL Collector)
* ⚠️ **Ruim:** Curva de aprendizado — conceitos de spans, traces, meters
* ⚠️ **Ruim:** Overhead de performance (mitigado por sampling em produção)

### Estrutura de Telemetria

**1. Traces (Distributed Tracing):**
- Rastreiam fluxo de execução através de serviços
- Cada operação é um "Span" (unidade de trabalho)
- Spans formam uma árvore (parent-child relationship)

**2. Metrics (Counters, Gauges, Histograms):**
- `Counter`: Valores incrementais (ex: total de orders criadas)
- `Gauge`: Valores instantâneos (ex: memória em uso)
- `Histogram`: Distribuições (ex: latência de requests)

**3. Logs (Structured Logging - Futuro):**
- Integração com `Microsoft.Extensions.Logging`
- Logs correlacionados com TraceId e SpanId

### Configuração de Ambiente

**Development (docker-compose.yml - Futuro):**

```yaml
services:
  grafana-agent:
    image: grafana/agent:latest
    ports:
      - "4317:4317"  # OTLP gRPC
      - "4318:4318"  # OTLP HTTP
    volumes:
      - ./agent-config.yaml:/etc/agent/agent.yaml
```

**Production (Environment Variables):**

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp.grafana.com:4317
OTEL_EXPORTER_OTLP_HEADERS=Authorization=Basic <token>
OTEL_SERVICE_NAME=devlivery-api
OTEL_TRACES_SAMPLER=parentbased_traceidratio
OTEL_TRACES_SAMPLER_ARG=0.1  # Sample 10% dos traces
```

### Dashboards Recomendados

**1. Application Performance (APM):**
- Request rate (req/s)
- Error rate (%)
- Latency percentiles (P50, P95, P99)
- Database query duration

**2. Business Metrics:**
- Orders created per establishment
- Revenue per hour/day
- Top selling products
- Cash session statistics

**3. Infrastructure:**
- CPU usage
- Memory usage (GC heap, allocations)
- Thread pool saturation
- Database connection pool

### Alertas Exemplo

```yaml
# Grafana Alerting
- alert: HighErrorRate
  expr: sum(rate(http_server_requests_total{http_status_code=~"5.."}[5m])) / sum(rate(http_server_requests_total[5m])) > 0.05
  for: 5m
  annotations:
    summary: "Error rate above 5% for 5 minutes"

- alert: SlowDatabaseQueries
  expr: histogram_quantile(0.95, sum(rate(db_query_duration_bucket[5m])) by (le)) > 1
  for: 10m
  annotations:
    summary: "P95 database query duration above 1 second"
```

### Sampling Strategies

**Development:**
```csharp
.SetSampler(new AlwaysOnSampler())  // 100% dos traces
```

**Production:**
```csharp
.SetSampler(new TraceIdRatioBasedSampler(0.1))  // 10% dos traces
```

**Adaptive (Recomendado):**
```csharp
// Sample 100% de erros, 10% de sucessos
.SetSampler(new ParentBasedSampler(
    new TraceIdRatioBasedSampler(0.1),
    remoteParentSampled: new AlwaysOnSampler(),
    remoteParentNotSampled: new AlwaysOffSampler()
))
```

### Backends Suportados

OpenTelemetry pode exportar para:
- ✅ **Grafana Cloud** (Tempo + Loki + Prometheus)
- ✅ **Jaeger** (Distributed Tracing)
- ✅ **Zipkin** (Distributed Tracing)
- ✅ **Prometheus** (Metrics)
- ✅ **Azure Monitor** (Application Insights)
- ✅ **AWS X-Ray** (Tracing)
- ✅ **Google Cloud Trace**
- ✅ **Datadog** (via OTLP)
- ✅ **New Relic** (via OTLP)

**Princípio:** "Instrument once, export anywhere. Use OpenTelemetry as the standard for observability."

### Referências

- [OpenTelemetry Official Docs](https://opentelemetry.io/docs/)
- [.NET OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
- [Grafana LGTM Stack](https://grafana.com/oss/)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
