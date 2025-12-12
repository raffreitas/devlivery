# Exemplo Prático: Refatoração do CashRegister

Este documento mostra exatamente como refatorar o CashRegister para uma estrutura mais limpa e organizada.

---

## 🎯 Objetivo

Simplificar a estrutura do CashRegister removendo pastas desnecessárias e organizando melhor os arquivos.

---

## 📋 Estrutura Atual vs Proposta

### Antes (Confuso)

```
CashRegister/
├── Commands/
│   ├── CreateCashSession/
│   ├── CloseCashSession/
│   └── CreateCashDeposit/
├── Queries/
│   ├── GetActiveCashSession/
│   ├── GetCashSessionById/
│   ├── GetCashSessionDeposits/
│   └── GetCashSessions/
├── Domain/
│   ├── CashSession.cs
│   ├── CashDeposit.cs
│   └── CashSessionStatus.cs
├── DTOs/                          ❌ Redundante
│   ├── CashSessionResponse.cs
│   ├── CashDepositResponse.cs
│   └── PaymentBreakdownDto.cs
├── Errors/                        ❌ Desnecessário
│   └── CashRegisterErrors.cs
├── EventHandlers/                 ⚠️ Pode melhorar nome
│   ├── OrderCreatedEventHandler.cs
│   └── OrderStatusChangedEventHandler.cs
└── Infrastructure/
    └── CashSessionRepository.cs
```

### Depois (Limpo)

```
CashRegister/
├── Commands/
│   ├── CreateCashSession/
│   ├── CloseCashSession/
│   └── CreateCashDeposit/
├── Queries/
│   ├── GetActiveCashSession/
│   │   └── GetActiveCashSessionResponse.cs    ← Movido de DTOs
│   ├── GetCashSessionById/
│   │   └── GetCashSessionByIdResponse.cs      ← Movido de DTOs
│   ├── GetCashSessionDeposits/
│   │   └── GetCashSessionDepositsResponse.cs   ← Movido de DTOs
│   └── GetCashSessions/
│       └── GetCashSessionsResponse.cs         ← Movido de DTOs
├── Domain/
│   ├── CashSession.cs
│   ├── CashDeposit.cs
│   ├── CashSessionStatus.cs
│   ├── CashRegisterErrors.cs                  ← Movido de Errors/
│   └── PaymentBreakdownDto.cs                  ← Movido de DTOs (ou pode ficar nas Responses)
├── Events/                                     ← Renomeado de EventHandlers
│   ├── OrderCreatedEventHandler.cs
│   └── OrderStatusChangedEventHandler.cs
└── Infrastructure/
    └── CashSessionRepository.cs
```

---

## 🔧 Passo a Passo da Refatoração

### Passo 1: Mover CashSessionResponse

**Arquivo:** `DTOs/CashSessionResponse.cs` → `Queries/GetCashSessionById/GetCashSessionByIdResponse.cs`

**Antes:**
```csharp
// DTOs/CashSessionResponse.cs
namespace Devlivery.Features.CashRegister.DTOs;

public sealed record CashSessionResponse(
    Guid Id,
    Guid AttendantId,
    string AttendantName,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal ExpectedCashAmount,
    decimal TotalRevenue,
    int TotalOrders,
    IReadOnlyCollection<PaymentBreakdownDto> PaymentBreakdown,
    DateTime StartAt,
    DateTime? EndAt,
    string Status,
    string? Notes)
{
    public static CashSessionResponse FromDomain(...)
    {
        // ...
    }
}
```

**Depois:**
```csharp
// Queries/GetCashSessionById/GetCashSessionByIdResponse.cs
using Devlivery.Features.CashRegister.Domain;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public sealed record GetCashSessionByIdResponse(
    Guid Id,
    Guid AttendantId,
    string AttendantName,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal ExpectedCashAmount,
    decimal TotalRevenue,
    int TotalOrders,
    IReadOnlyCollection<PaymentBreakdownDto> PaymentBreakdown,
    DateTime StartAt,
    DateTime? EndAt,
    string Status,
    string? Notes)
{
    public static GetCashSessionByIdResponse FromDomain(
        Domain.CashSession cashSession,
        decimal? expectedCashAmount = null)
    {
        var payments = cashSession.PaymentBreakdown
            .Select(pb => new PaymentBreakdownDto(pb.Method, pb.Amount, pb.Count))
            .ToList();

        var calculatedExpectedCash = expectedCashAmount ??
                                     cashSession.OpeningAmount + cashSession.PaymentBreakdown
                                         .Where(pb => pb.Method.Equals("cash", StringComparison.OrdinalIgnoreCase))
                                         .Sum(pb => pb.Amount);

        return new GetCashSessionByIdResponse(
            cashSession.Id,
            cashSession.AttendantId,
            cashSession.AttendantName,
            cashSession.OpeningAmount,
            cashSession.ClosingAmount,
            calculatedExpectedCash,
            cashSession.TotalRevenue,
            cashSession.TotalOrders,
            new ReadOnlyCollection<PaymentBreakdownDto>(payments),
            cashSession.StartAt,
            cashSession.EndAt,
            cashSession.Status.ToString().ToLowerInvariant(),
            cashSession.Notes);
    }
}
```

**Atualizar Handler:**
```csharp
// Queries/GetCashSessionById/GetCashSessionByIdHandler.cs
using Devlivery.Features.CashRegister.Domain;  // ← Mudar import
using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdHandler(ApplicationDbContext dbContext)
{
    public async Task<Result<GetCashSessionByIdResponse>> HandleAsync(  // ← Mudar tipo
        GetCashSessionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Id == query.Id, cancellationToken);

        return cashSession is null
            ? Result.Fail<GetCashSessionByIdResponse>(CashRegisterErrors.CashSessionNotFound)
            : Result.Ok(GetCashSessionByIdResponse.FromDomain(cashSession));  // ← Mudar chamada
    }
}
```

---

### Passo 2: Mover CashDepositResponse

**Arquivo:** `DTOs/CashDepositResponse.cs` → `Queries/GetCashSessionDeposits/GetCashSessionDepositsResponse.cs`

**Antes:**
```csharp
// DTOs/CashDepositResponse.cs
namespace Devlivery.Features.CashRegister.DTOs;
public sealed record CashDepositResponse(...)
```

**Depois:**
```csharp
// Queries/GetCashSessionDeposits/GetCashSessionDepositsResponse.cs
namespace Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;
public sealed record GetCashSessionDepositsResponse(...)
```

**Atualizar Handler correspondente.**

---

### Passo 3: Mover PaymentBreakdownDto

**Opção A: Mover para Domain (Recomendado se usado em múltiplos lugares)**

```csharp
// Domain/PaymentBreakdownDto.cs
namespace Devlivery.Features.CashRegister.Domain;

public sealed record PaymentBreakdownDto(string Method, decimal Amount, int Count);
```

**Opção B: Deixar junto com a Response (Se usado apenas em 1 lugar)**

```csharp
// Queries/GetCashSessionById/GetCashSessionByIdResponse.cs
namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;

// Dentro do mesmo arquivo ou arquivo separado na mesma pasta
public sealed record PaymentBreakdownDto(string Method, decimal Amount, int Count);

public sealed record GetCashSessionByIdResponse(
    // ...
    IReadOnlyCollection<PaymentBreakdownDto> PaymentBreakdown,
    // ...
)
```

**Recomendação:** Mover para `Domain/` se usado em múltiplas queries.

---

### Passo 4: Mover CashRegisterErrors

**Arquivo:** `Errors/CashRegisterErrors.cs` → `Domain/CashRegisterErrors.cs`

**Antes:**
```csharp
// Errors/CashRegisterErrors.cs
namespace Devlivery.Features.CashRegister.Errors;

public static class CashRegisterErrors
{
    public static BusinessRuleError CashSessionAlreadyOpen => ...
    public static BusinessRuleError CashSessionAlreadyClosed => ...
    public static NotFoundError CashSessionNotFound => ...
}
```

**Depois:**
```csharp
// Domain/CashRegisterErrors.cs
using Devlivery.Shared.SeedWork.Errors;

namespace Devlivery.Features.CashRegister.Domain;

public static class CashRegisterErrors
{
    public static BusinessRuleError CashSessionAlreadyOpen =>
        new("Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo.");

    public static BusinessRuleError CashSessionAlreadyClosed =>
        new("O caixa já está fechado.");

    public static NotFoundError CashSessionNotFound =>
        new("Caixa não encontrado.");
}
```

**Atualizar todos os imports:**
```csharp
// De:
using Devlivery.Features.CashRegister.Errors;

// Para:
using Devlivery.Features.CashRegister.Domain;
```

---

### Passo 5: Renomear EventHandlers para Events

**Renomear pasta:** `EventHandlers/` → `Events/`

**Não precisa mudar código, apenas renomear pasta e atualizar namespace (opcional):**

**Antes:**
```csharp
// EventHandlers/OrderCreatedEventHandler.cs
namespace Devlivery.Features.CashRegister.EventHandlers;
```

**Depois:**
```csharp
// Events/OrderCreatedEventHandler.cs
namespace Devlivery.Features.CashRegister.Events;
```

**Atualizar CashRegisterFeature.cs:**
```csharp
// De:
using Devlivery.Features.CashRegister.EventHandlers;

// Para:
using Devlivery.Features.CashRegister.Events;
```

---

## 📝 Checklist Completo

### Arquivos a Mover

- [ ] `DTOs/CashSessionResponse.cs` → `Queries/GetCashSessionById/GetCashSessionByIdResponse.cs`
- [ ] `DTOs/CashDepositResponse.cs` → `Queries/GetCashSessionDeposits/GetCashSessionDepositsResponse.cs`
- [ ] `DTOs/PaymentBreakdownDto.cs` → `Domain/PaymentBreakdownDto.cs` (ou junto com Response)
- [ ] `Errors/CashRegisterErrors.cs` → `Domain/CashRegisterErrors.cs`

### Pastas a Renomear

- [ ] `EventHandlers/` → `Events/`

### Pastas a Deletar (após mover arquivos)

- [ ] `DTOs/` (vazia)
- [ ] `Errors/` (vazia)

### Imports a Atualizar

- [ ] Todos os arquivos que usam `CashSessionResponse`
- [ ] Todos os arquivos que usam `CashDepositResponse`
- [ ] Todos os arquivos que usam `PaymentBreakdownDto`
- [ ] Todos os arquivos que usam `CashRegisterErrors`
- [ ] `CashRegisterFeature.cs` (imports de EventHandlers)

### Arquivos Específicos a Atualizar

- [ ] `Queries/GetCashSessionById/GetCashSessionByIdHandler.cs`
- [ ] `Queries/GetCashSessionById/GetCashSessionByIdEndpoint.cs`
- [ ] `Queries/GetCashSessions/GetCashSessionsHandler.cs` (se usar CashSessionResponse)
- [ ] `Queries/GetActiveCashSession/GetActiveCashSessionHandler.cs` (se usar CashSessionResponse)
- [ ] `Queries/GetCashSessionDeposits/GetCashSessionDepositsHandler.cs`
- [ ] `Commands/CloseCashSession/CloseCashSessionHandler.cs` (se usar Errors)
- [ ] `Commands/CreateCashSession/CreateCashSessionHandler.cs` (se usar Errors)
- [ ] `CashRegisterFeature.cs`

---

## 🧪 Testes Após Refatoração

### Verificar que Compila

```bash
dotnet build
```

### Verificar que Funciona

1. Testar endpoint `GET /api/cash-sessions/{id}`
2. Testar endpoint `GET /api/cash-sessions`
3. Testar endpoint `GET /api/cash-sessions/{id}/deposits`
4. Testar endpoint `GET /api/cash-sessions/active`
5. Testar criação de cash session
6. Testar fechamento de cash session
7. Verificar que eventos ainda funcionam

---

## 📊 Resultado Final

### Antes
- **7 pastas** (algumas com 1 arquivo apenas)
- **DTOs separados** (confuso)
- **Errors separados** (desnecessário)
- **EventHandlers** (nome poderia ser melhor)

### Depois
- **5 pastas** (todas com propósito claro)
- **Responses junto com queries** (mais intuitivo)
- **Errors no Domain** (faz mais sentido)
- **Events** (nome mais claro)

### Benefícios
- ✅ Menos pastas para navegar
- ✅ Tudo relacionado fica junto
- ✅ Mais fácil de entender
- ✅ Mantém princípios VSA
- ✅ Mesma quantidade de código, melhor organizado

---

## 💡 Dicas

1. **Faça em etapas:** Mova um arquivo por vez, teste, depois continue
2. **Use refactoring tools:** IDE pode ajudar a atualizar imports automaticamente
3. **Teste após cada mudança:** Não espere terminar tudo para testar
4. **Commit frequente:** Facilita rollback se algo der errado

---

## 🎯 Próximos Passos

Após refatorar CashRegister:

1. ✅ Aplicar mesmo padrão em Products (mover DTOs)
2. ✅ Documentar padrão final
3. ✅ Criar template para novas features
4. ✅ Atualizar documentação do projeto

---

**Boa sorte com a refatoração! 🚀**

