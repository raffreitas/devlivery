# Guia de Implementação: API de Controle de Caixa

## Visão Geral

Este documento descreve a implementação backend necessária para o sistema de controle de caixa (cash register), atualmente implementado como POC usando `localStorage` no frontend.

O sistema gerencia **sessões de caixa** com abertura (valor inicial), vendas do período, e fechamento (valor real vs. esperado), incluindo resumo por forma de pagamento. Foi projetado para suportar **turnos noturnos que atravessam meia-noite** (ex: 18:00 até 01:00).

---

## Modelo de Dados

### Entidade: `CashSession`

Tabela: `cash_sessions` (PostgreSQL com snake_case)

| Coluna              | Tipo           | Descrição                                                        | Restrições                  |
|---------------------|----------------|------------------------------------------------------------------|-----------------------------|
| `id`                | `UUID`         | Identificador único da sessão                                    | PK, NOT NULL                |
| `establishment_id`  | `UUID`         | ID do estabelecimento (multi-tenancy)                            | FK, NOT NULL, indexed       |
| `attendant_id`      | `UUID`         | ID do usuário que abriu o caixa                                  | FK, NOT NULL                |
| `attendant_name`    | `VARCHAR(200)` | Nome do atendente (desnormalizado para histórico)                | NOT NULL                    |
| `opening_amount`    | `DECIMAL(10,2)`| Valor em dinheiro inicial no caixa                               | NOT NULL, >= 0              |
| `closing_amount`    | `DECIMAL(10,2)`| Valor real contado no fechamento                                 | NULL (aberto), >= 0         |
| `start_at`          | `TIMESTAMPTZ`  | Data/hora de abertura (UTC)                                      | NOT NULL                    |
| `end_at`            | `TIMESTAMPTZ`  | Data/hora de fechamento (UTC)                                    | NULL (aberto)               |
| `status`            | `VARCHAR(20)`  | Status da sessão: `open`, `closed`                               | NOT NULL, CHECK constraint  |
| `notes`             | `TEXT`         | Observações sobre abertura/fechamento                            | NULL                        |
| `total_revenue`     | `DECIMAL(10,2)`| Soma das vendas no período (cache)                               | DEFAULT 0, >= 0             |
| `total_orders`      | `INT`          | Quantidade de pedidos no período (cache)                         | DEFAULT 0, >= 0             |
| `payment_breakdown` | `JSONB`        | Totais por forma de pagamento (ver estrutura abaixo)             | NULL                        |
| `created_at`        | `TIMESTAMPTZ`  | Timestamp de criação do registro                                 | NOT NULL, DEFAULT NOW()     |
| `updated_at`        | `TIMESTAMPTZ`  | Timestamp de última atualização                                  | NOT NULL, DEFAULT NOW()     |

**Índices sugeridos:**
- `idx_cash_sessions_establishment_id` em `establishment_id`
- `idx_cash_sessions_status` em `status` (para buscar caixas abertos rapidamente)
- `idx_cash_sessions_start_at` em `start_at` (para filtros de período)

**Regras de negócio:**
1. **Um estabelecimento pode ter apenas uma sessão `open` por vez**
   → Adicionar constraint `UNIQUE (establishment_id) WHERE status = 'open'`
2. **`end_at` deve ser maior que `start_at`** (quando fechado)
   → Validar no Handler/Entity
3. **`closing_amount` é obrigatório ao fechar** (`status = 'closed'`)
   → Validar no `CloseCashSessionCommand`

**Estrutura do JSONB `payment_breakdown`:**
```json
[
  {
    "method": "cash",
    "amount": 450.00,
    "count": 12
  },
  {
    "method": "credit_card",
    "amount": 890.50,
    "count": 23
  },
  {
    "method": "debit_card",
    "amount": 320.00,
    "count": 8
  },
  {
    "method": "pix",
    "amount": 560.00,
    "count": 15
  }
]
```

---

## Endpoints da API

Todos os endpoints devem seguir o padrão **Vertical Slice Architecture + CQRS** atual, com:
- Multi-tenancy via `ITenantAccessor` (filtra por `EstablishmentId`)
- FluentValidation com mensagens em PT-BR
- Typed Results + Problem Details (RFC 7807)
- Handlers retornam `Result<T>` (FluentResults)

### 1. Criar Nova Sessão (Abrir Caixa)

**Endpoint:**
`POST /api/cash-sessions`

**Command:**
```csharp
public sealed record CreateCashSessionCommand(
    string AttendantName,
    decimal OpeningAmount,
    string? Notes
);

public sealed class Validator : AbstractValidator<CreateCashSessionCommand>
{
    public Validator()
    {
        RuleFor(x => x.AttendantName)
            .NotEmpty().WithMessage("O nome do atendente é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do atendente deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.OpeningAmount)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de abertura deve ser maior ou igual a zero.");
    }
}
```

**Handler:**
```csharp
public sealed class CreateCashSessionHandler(
    ApplicationDbContext dbContext,
    ITenantAccessor tenantAccessor,
    ILogger<CreateCashSessionHandler> logger)
{
    public async Task<Result<CreateCashSessionResponse>> HandleAsync(
        CreateCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var establishmentId = tenantAccessor.Tenant.Id;

        // Verificar se já existe um caixa aberto
        var existingOpen = await dbContext.CashSessions
            .ForTenant(establishmentId)
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .AnyAsync(cancellationToken);

        if (existingOpen)
            return Result.Fail<CreateCashSessionResponse>(
                "Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo."
            );

        var cashSession = new CashSession(
            id: Guid.NewGuid(),
            establishmentId: establishmentId,
            attendantName: command.AttendantName,
            openingAmount: command.OpeningAmount,
            notes: command.Notes
        );

        dbContext.CashSessions.Add(cashSession);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(new CreateCashSessionResponse(
            Id: cashSession.Id,
            AttendantName: cashSession.AttendantName,
            OpeningAmount: cashSession.OpeningAmount,
            StartAt: cashSession.StartAt,
            Status: cashSession.Status.ToString().ToLowerInvariant()
        ));
    }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Caixa aberto com sucesso",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "attendantName": "João Silva",
    "openingAmount": 100.00,
    "startAt": "2025-12-08T18:00:00Z",
    "status": "open"
  }
}
```

**Status Codes:**
- `201 Created` → Sucesso
- `400 Bad Request` → Validação ou já existe caixa aberto
- `401 Unauthorized` → Token inválido/expirado

---

### 2. Buscar Sessão Ativa (Caixa Aberto)

**Endpoint:**
`GET /api/cash-sessions/active`

**Query:**
```csharp
public sealed record GetActiveCashSessionQuery();
```

**Handler:**
```csharp
public sealed class GetActiveCashSessionHandler(
    ApplicationDbContext dbContext,
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<GetActiveCashSessionResponse?>> HandleAsync(
        GetActiveCashSessionQuery query,
        CancellationToken cancellationToken = default)
    {
        var establishmentId = tenantAccessor.Tenant.Id;

        var session = await dbContext.CashSessions
            .ForTenant(establishmentId)
            .Where(cs => cs.Status == CashSessionStatus.Open)
            .OrderByDescending(cs => cs.StartAt) // Mais recente
            .Select(cs => new GetActiveCashSessionResponse(
                Id: cs.Id,
                AttendantName: cs.AttendantName,
                OpeningAmount: cs.OpeningAmount,
                StartAt: cs.StartAt,
                Status: cs.Status.ToString().ToLowerInvariant(),
                TotalRevenue: cs.TotalRevenue,
                TotalOrders: cs.TotalOrders,
                PaymentBreakdown: cs.PaymentBreakdown // JSONB
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Ok(session);
    }
}
```

**Response (200 OK se encontrado):**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "attendantName": "João Silva",
    "openingAmount": 100.00,
    "startAt": "2025-12-08T18:00:00Z",
    "status": "open",
    "totalRevenue": 1234.50,
    "totalOrders": 42,
    "paymentBreakdown": [
      { "method": "cash", "amount": 450.00, "count": 12 },
      { "method": "pix", "amount": 784.50, "count": 30 }
    ]
  }
}
```

**Response (200 OK se não encontrado):**
```json
{
  "success": true,
  "data": null
}
```

---

### 3. Fechar Sessão

**Endpoint:**
`PATCH /api/cash-sessions/{id}/close`

**Command:**
```csharp
public sealed record CloseCashSessionCommand(
    Guid Id,
    decimal ClosingAmount,
    string? Notes
);

public sealed class Validator : AbstractValidator<CloseCashSessionCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID da sessão é obrigatório.");

        RuleFor(x => x.ClosingAmount)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de fechamento deve ser maior ou igual a zero.");
    }
}
```

**Handler:**
```csharp
public sealed class CloseCashSessionHandler(
    ApplicationDbContext dbContext,
    ITenantAccessor tenantAccessor,
    ILogger<CloseCashSessionHandler> logger)
{
    public async Task<Result<CloseCashSessionResponse>> HandleAsync(
        CloseCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var session = await dbContext.CashSessions
            .ForTenant(tenantAccessor.Tenant.Id)
            .FirstOrDefaultAsync(cs => cs.Id == command.Id, cancellationToken);

        if (session is null)
            return Result.Fail<CloseCashSessionResponse>("Sessão de caixa não encontrada.");

        if (session.Status == CashSessionStatus.Closed)
            return Result.Fail<CloseCashSessionResponse>("Este caixa já está fechado.");

        // Fechar sessão
        session.Close(command.ClosingAmount, command.Notes);

        await dbContext.SaveChangesAsync(cancellationToken);

        var difference = command.ClosingAmount - (session.OpeningAmount + session.TotalRevenue);

        return Result.Ok(new CloseCashSessionResponse(
            Id: session.Id,
            EndAt: session.EndAt!.Value,
            ClosingAmount: session.ClosingAmount!.Value,
            ExpectedAmount: session.OpeningAmount + session.TotalRevenue,
            Difference: difference
        ));
    }
}
```

**Entity method:**
```csharp
public void Close(decimal closingAmount, string? notes)
{
    if (Status == CashSessionStatus.Closed)
        throw new InvalidOperationException("Este caixa já está fechado.");

    Status = CashSessionStatus.Closed;
    EndAt = DateTime.UtcNow;
    ClosingAmount = closingAmount;
    if (!string.IsNullOrWhiteSpace(notes))
        Notes = notes;
    UpdatedAt = DateTime.UtcNow;
}
```

**Response:**
```json
{
  "success": true,
  "message": "Caixa fechado com sucesso",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "endAt": "2025-12-09T01:30:00Z",
    "closingAmount": 1350.00,
    "expectedAmount": 1334.50,
    "difference": 15.50
  }
}
```

**Status Codes:**
- `200 OK` → Sucesso
- `404 Not Found` → Sessão não encontrada
- `400 Bad Request` → Validação ou caixa já fechado

---

### 4. Buscar Sessão por ID

**Endpoint:**
`GET /api/cash-sessions/{id}`

**Query:**
```csharp
public sealed record GetCashSessionByIdQuery(Guid Id);
```

**Handler:** Similar ao `GetActiveCashSessionHandler`, mas filtra por `Id` específico.

---

### 5. Listar Sessões (Histórico)

**Endpoint:**
`GET /api/cash-sessions?startDate=2025-12-01&endDate=2025-12-31&status=closed&page=1&pageSize=20`

**Query:**
```csharp
public sealed record GetAllCashSessionsQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Status, // "open" ou "closed"
    int Page = 1,
    int PageSize = 20
);
```

**Handler:** Aplica filtros opcionais com paginação, retorna lista de sessões.

---

## Cálculo de Vendas no Período

**Importante:** As vendas devem ser filtradas pelo intervalo `[start_at, end_at]` da sessão, **não por dia civil**.

### Quando recalcular?

**Opção 1 (Recomendada):** Cache + Recalcular ao fechar
- `total_revenue`, `total_orders`, `payment_breakdown` são **calculados e salvos apenas ao fechar a sessão**.
- Durante a sessão aberta, o frontend calcula em tempo real usando `GET /api/orders?startDate=X&endDate=Y`.
- Ao fechar, o backend recalcula e persiste os valores finais.

**Opção 2:** Recalcular em tempo real no GET
- Toda vez que `GET /api/cash-sessions/active` é chamado, o backend executa uma query agregada em `orders`.
- Mais lento, mas sempre atualizado.

**Implementação sugerida (Opção 1):**

```csharp
// No CloseCashSessionHandler, antes de fechar:
var sessionOrders = await dbContext.Orders
    .ForTenant(tenantAccessor.Tenant.Id)
    .Where(o => o.Status == OrderStatus.Completed) // Apenas completed
    .Where(o => o.CreatedAt >= session.StartAt && o.CreatedAt <= DateTime.UtcNow)
    .ToListAsync(cancellationToken);

var totalRevenue = sessionOrders.Sum(o => o.TotalAmount);
var totalOrders = sessionOrders.Count;

var paymentBreakdown = sessionOrders
    .GroupBy(o => o.PaymentMethod)
    .Select(g => new
    {
        method = g.Key,
        amount = g.Sum(o => o.TotalAmount),
        count = g.Count()
    })
    .ToList();

session.UpdateTotals(totalRevenue, totalOrders, paymentBreakdown);
```

**Entity method:**
```csharp
public void UpdateTotals(decimal totalRevenue, int totalOrders, List<PaymentMethodTotal> breakdown)
{
    TotalRevenue = totalRevenue;
    TotalOrders = totalOrders;
    PaymentBreakdown = JsonSerializer.Serialize(breakdown); // ou mapear para JSONB
    UpdatedAt = DateTime.UtcNow;
}
```

---

## Considerações: Turnos Noturnos (Cross-Midnight)

### Problema
Um estabelecimento pode abrir o caixa às 18:00 de um dia e fechar às 01:00 do dia seguinte. Precisamos garantir:
1. **Filtro de vendas por timestamp**, não por dia civil.
2. **Validação `start_at < end_at`** (sempre válido em UTC).
3. **Alerta no frontend** se a sessão estiver aberta há mais de 24h (possível esquecimento).

### Solução
- Sempre use `TIMESTAMPTZ` (UTC) no banco.
- Filtro: `WHERE order.created_at >= session.start_at AND order.created_at <= session.end_at`
- No frontend, exibir duração em horas/minutos e avisar se > 24h.

---

## Migrations

### Criar Migração

**Usando EF Core:**
```bash
dotnet ef migrations add v003_AddCashSessions -o ./Shared/Database/Migrations -c ApplicationDbContext
```

**Usando Makefile (Linux/macOS):**
```bash
make migration-db VERSION=003
```

### Aplicar Migração

**Localmente:**
```bash
dotnet ef database update -c ApplicationDbContext
```

**Produção (via GitHub Actions):**
- Migrations são aplicadas automaticamente no pipeline CI/CD ao fazer push na branch `main`.
- Usar secret `DATABASE_CONNECTION_STRING` no GitHub.

---

## Testes de Integração

Seguir padrão atual com **Testcontainers + Respawn**:

```csharp
[Collection("Cash Tests")]
[Trait("Category", "Integration Tests")]
public sealed class CreateCashSessionEndpointTests(
    CashWebApplicationFactory factory)
    : WebApiBaseFixture<CashWebApplicationFactory>(factory)
{
    [Fact]
    public async Task Should_Create_Cash_Session_Successfully()
    {
        await ResetDatabaseAsync(); // SEMPRE primeiro

        var (user, establishment, token) = await Prepare();

        var command = new CreateCashSessionCommand(
            AttendantName: "João Silva",
            OpeningAmount: 100.00m,
            Notes: "Abertura turno noite"
        );

        var response = await PostAsync("/api/cash-sessions", command, token);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateCashSessionResponse>>();
        result.ShouldNotBeNull();
        result.Data.AttendantName.ShouldBe("João Silva");
        result.Data.OpeningAmount.ShouldBe(100.00m);
    }

    [Fact]
    public async Task Should_Fail_When_Cash_Already_Open()
    {
        await ResetDatabaseAsync();

        var (user, establishment, token) = await Prepare();

        // Abrir primeiro caixa
        await PostAsync("/api/cash-sessions", new CreateCashSessionCommand("João", 100m, null), token);

        // Tentar abrir segundo caixa
        var response = await PostAsync("/api/cash-sessions", new CreateCashSessionCommand("Maria", 50m, null), token);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
```

---

## Checklist de Implementação

### Backend (devlivery-webapi)

- [ ] Criar entidade `CashSession` em `Features/Cash/Domain/`
- [ ] Adicionar `DbSet<CashSession>` no `ApplicationDbContext`
- [ ] Criar migration `v003_AddCashSessions` com constraint `UNIQUE (establishment_id) WHERE status = 'open'`
- [ ] Implementar `CreateCashSession` command/handler/endpoint
- [ ] Implementar `GetActiveCashSession` query/handler/endpoint
- [ ] Implementar `CloseCashSession` command/handler/endpoint
- [ ] Implementar `GetCashSessionById` query/handler/endpoint
- [ ] Implementar `GetAllCashSessions` query/handler/endpoint (com filtros e paginação)
- [ ] Adicionar método `UpdateTotals` na entidade para recalcular vendas
- [ ] Registrar feature em `Startup.cs` (`AddCashFeature()` e `MapCashEndpoints()`)
- [ ] Adicionar testes de integração em `test/Devlivery.WebApi.Tests/Features/Cash/`

### Frontend (devlivery-webapp)

- [x] **Implementado (POC)** — Remover `local-cash-store.ts` e substituir por chamadas API reais
- [x] **Implementado** — Atualizar hooks `use-cash-sessions.ts` para usar `api.ts` (substituir `localStorage`)
- [ ] Testar integração com backend quando endpoints estiverem prontos
- [ ] Validar cross-midnight behavior (abrir 18:00, fechar 01:00)
- [ ] Ajustar tipos para DTOs do backend (snake_case → camelCase)

---

## Recursos Adicionais

- **Padrão de API**: Ver `docs/API-RESPONSE-PATTERN.md` no repositório webapi
- **Testes**: Ver `docs/INTEGRATION-TESTS.md` no repositório webapi
- **Migrations**: Ver `docs/MIGRATIONS.md` no repositório webapi
- **Multi-tenancy**: Ver implementação em `Features/Orders/` e `Features/Products/`

---

## Suporte

Para dúvidas sobre implementação, consulte:
- Copilot Instructions (`.github/copilot-instructions.md`) em ambos os projetos
- Exemplos completos em `Features/Orders/` e `Features/Products/`
- Equipe de desenvolvimento via pull request comments

---

**Última atualização:** 08/12/2025
**Versão do documento:** 1.0
