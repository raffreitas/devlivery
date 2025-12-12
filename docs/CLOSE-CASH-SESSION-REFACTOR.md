# Refatoração: CloseCashSession com Breakdown em Tempo Real

## ⚡ Status: Parcialmente Implementado

- ✅ **ExpectedCashAmount Fix** - IMPLEMENTADO em `CloseCashSessionHandler`
  - Leia `EXPECTED-CASH-AMOUNT-FIX.md` para detalhes da solução atual
- ⏳ **Breakdown Service Refactoring** - Planejado para fase 2 (para arquitetura em tempo real)

---

## Contexto

Com a implementação da refatoração do cálculo de `PaymentBreakdown` em tempo real (ver `CASH-REGISTER-BREAKDOWN-REFACTOR.md`), a feature de **fechar caixa** deve ser refatorada para aproveitar dados já atualizados.

## Situação Atual

**Arquivo:** `Features/CashRegister/Commands/CloseCashSession/CloseCashSessionHandler.cs`

```csharp
public sealed class CloseCashSessionHandler(ApplicationDbContext dbContext, ...)
{
    public async Task<Result<CashSessionResponse>> HandleAsync(
        CloseCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var cashSession = await dbContext.CashSessions
            .ForTenant(tenantId)
            .FirstOrDefaultAsync(cs => cs.Id == command.CashSessionId, cancellationToken);

        // ❌ PROBLEMA: Recalcula do zero ao fechar
        var orders = await dbContext.Orders
            .Where(o => o.CashSessionId == command.CashSessionId &&
                        o.CreatedAt >= cashSession.StartAt &&
                        (!cashSession.EndAt.HasValue || o.CreatedAt <= cashSession.EndAt.Value) &&
                        o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        // Agrupa por payment method (lógica duplicada)
        var breakdown = orders
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new PaymentBreakdownItem(
                g.Key,
                g.Sum(o => o.Total),
                g.Count()))
            .ToList();

        var totalRevenue = orders.Sum(o => o.Total);
        var totalOrders = orders.Count;

        // Atualiza totais e fecha
        cashSession.UpdateTotals(totalRevenue, totalOrders, breakdown);
        cashSession.Close(command.ClosingAmount, command.Notes);

        await dbContext.SaveChangesAsync(cancellationToken);
        // ...
    }
}
```

### Problemas

1. **Duplicação de Lógica** - Breakdown é calculado tanto aqui quanto no `CashSessionBreakdownService`
2. **Performance** - Query pesada executada apenas no fechamento (não distribuída)
3. **Inconsistência** - Se mudança no cálculo, precisa atualizar 2 lugares
4. **❌ CRÍTICO: ExpectedCashAmount Incompleto** - Não inclui as vendas em dinheiro
   - Atualmente: `ExpectedCashAmount = Opening + Deposits`
   - Deveria ser: `ExpectedCashAmount = Opening + Deposits + CashSales`
   - Isso causa diferença (closing - expected) incorreta na resposta da API

---

## Solução Proposta

### 1. Refatorar Handler para Usar Serviço

```csharp
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.DTOs;
using Devlivery.Features.CashRegister.Errors;
using Devlivery.Features.CashRegister.Services;
using Devlivery.Shared.Database.Context;
using Devlivery.Shared.Database.Extensions;
using Devlivery.Shared.Tenancy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public sealed class CloseCashSessionHandler(
    ApplicationDbContext dbContext,
    ITenantAccessor tenantAccessor,
    ICashSessionBreakdownService breakdownService) // ✅ NOVO
{
    public async Task<Result<CashSessionResponse>> HandleAsync(
        CloseCashSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantAccessor.Tenant.Id;

        var cashSession = await dbContext.CashSessions
            .ForTenant(tenantId)
            .FirstOrDefaultAsync(cs => cs.Id == command.CashSessionId, cancellationToken);

        if (cashSession is null)
            return Result.Fail<CashSessionResponse>(CashRegisterErrors.CashSessionNotFound);

        if (cashSession.Status == CashSessionStatus.Closed)
            return Result.Fail<CashSessionResponse>(CashRegisterErrors.CashSessionAlreadyClosed);

        // ✅ NOVO: Usa serviço para obter breakdown final
        var (breakdown, totalRevenue, totalOrders) = 
            await breakdownService.CalculateBreakdownAsync(command.CashSessionId, cancellationToken);

        // ✅ NOVO: Calcula deposits totais
        var totalDeposits = await dbContext.CashDeposits
            .Where(cd => cd.CashSessionId == command.CashSessionId)
            .SumAsync(cd => cd.Amount, cancellationToken);

        // ✅ NOVO: Extrai vendas em dinheiro do breakdown
        var cashSales = breakdown
            .Where(pb => pb.Method.Equals("cash", StringComparison.OrdinalIgnoreCase))
            .Sum(pb => pb.Amount);

        // ✅ NOVO: Calcula expected correto (opening + deposits + cash sales)
        var expectedCashAmount = cashSession.OpeningAmount + totalDeposits + cashSales;
        
        // ✅ NOVO: Atualiza expected ANTES de fechar
        cashSession.UpdateExpectedCashAmount(expectedCashAmount);

        // ✅ APENAS FECHA (não recalcula totals)
        cashSession.Close(command.ClosingAmount, command.Notes);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = CashSessionResponse.FromDomain(cashSession, expectedCashAmount);

        return Result.Ok(response);
    }
}
```

**Mudanças Críticas:**
1. ✅ Injeta `ICashSessionBreakdownService`
2. ✅ Calcula breakdown final (sem query manual)
3. ✅ **Calcula deposits totais da sessão**
4. ✅ **Extrai cash sales do breakdown**
5. ✅ **Recalcula expectedCashAmount CORRETAMENTE antes de fechar**
6. ✅ Chama `UpdateExpectedCashAmount()` com valor final
7. ✅ Apenas chama `Close()` (não `UpdateTotals()`)


### 2. Análise de Mudanças

| Aspecto | Antes | Depois |
|--------|-------|--------|
| **Linhas de código** | ~40 | ~25 |
| **Query de Orders** | ✅ Executada | ❌ Removida |
| **Cálculo de Breakdown** | Local (duplicado) | ✅ Centralizado em serviço |
| **Atualização de Totais** | Via `UpdateTotals()` | ✅ Já está atualizado |
| **Validações** | Nenhuma | ✅ Pode adicionar se necessário |
| **Tempo de Execução** | Lento (recalcula) | ⚡ Rápido (apenas confirma) |

### 3. Dependências a Adicionar

Em `CloseCashSessionHandler` constructor:
```csharp
ICashSessionBreakdownService breakdownService
```

### 4. Refatorar Endpoint (se necessário)

O endpoint `CloseCashSessionEndpoint` pode ficar igual, pois a assinatura do handler não muda:

```csharp
// Sem alterações necessárias
public static class CloseCashSessionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{id}/close", Handle)
            .Produces<ApiResponse<CashSessionResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ApiResponse<CashSessionResponse>>, ValidationProblem, 
        BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> Handle(
        Guid id,
        CloseCashSessionCommand request,
        IValidator<CloseCashSessionCommand> validator,
        CloseCashSessionHandler handler,
        CancellationToken ct)
    {
        // Lógica existente, sem mudanças
        var command = request with { CashSessionId = id };
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var result = await handler.HandleAsync(command, ct);
        // ...
    }
}
```

---

## Estágios de Cálculo do ExpectedCashAmount

Este é um detalhe **CRÍTICO** para entender quando cada componente é incluído no valor esperado de caixa:

### Estágio 1: Abertura de Caixa
```
Estado: CashSession criado com abertura
Expected = OpeningAmount
Exemplo: Abre caixa com R$ 100
Expected = 100
```

### Estágio 2: Adicionar Aporte (Depósito)
```
Estado: CashDeposit criado para aportes
Expected = OpeningAmount + TotalDeposits
Exemplo: Adiciona aporte de R$ 50
Expected = 100 + 50 = 150

❌ IMPORTANTE: Não inclui vendas em dinheiro ainda!
   Motivo: PaymentBreakdown é calculado apenas no fechamento
   (limitação arquitetural atual - ver CASH-REGISTER-BREAKDOWN-REFACTOR.md)
```

### Estágio 3: Vendas Ocorrem
```
Estado: Orders criadas e finalizadas
Expected = 100 + 50 = 150  (continua sem incluir vendas!)

Cenário:
- Vende R$ 200 em dinheiro
- Vende R$ 100 em débito
- Total de vendas: R$ 300

Expected ainda é 150 porque:
- PaymentBreakdown não foi calculado
- Não temos acesso aos totais de vendas por método de pagamento
- Isso será resolvido na refatoração (ver CASH-REGISTER-BREAKDOWN-REFACTOR.md)

❌ PROBLEMA: Se fechar agora, a diferença estará ERRADA!
```

### Estágio 4: Fechar Caixa (✅ SOLUÇÃO IMPLEMENTADA)
```
Estado: CloseCashSessionHandler.HandleAsync() é chamado
Expected = OpeningAmount + TotalDeposits + CashSalesTotal

Processo:
1. Calcula breakdown via ICashSessionBreakdownService
2. Extrai vendas em dinheiro do breakdown
3. Soma: Opening (100) + Deposits (50) + CashSales (200) = 350

Então pode fechar corretamente:
- Closing Amount (contado): R$ 350
- Expected (calculado): R$ 350
- Difference: 0 ✅ (perfeito!)

Ou se houver diferença:
- Closing Amount (contado): R$ 348
- Expected (calculado): R$ 350
- Difference: -2 (déficit de R$ 2)
```

### Código-Chave: Como Obter CashSalesTotal

No novo `CloseCashSessionHandler`:

```csharp
// ✅ 1. Calcula breakdown (retorna lista de PaymentBreakdownItem)
var (breakdown, totalRevenue, totalOrders) = 
    await breakdownService.CalculateBreakdownAsync(sessionId, ct);

// ✅ 2. Filtra apenas pagamentos em "cash"
var cashSales = breakdown
    .Where(pb => pb.Method.Equals("cash", StringComparison.OrdinalIgnoreCase))
    .Sum(pb => pb.Amount);

// ✅ 3. Obtém total de aportes da sessão
var totalDeposits = await dbContext.CashDeposits
    .Where(cd => cd.CashSessionId == sessionId)
    .SumAsync(cd => cd.Amount, ct);

// ✅ 4. Calcula expected CORRETO antes de fechar
var expectedCashAmount = cashSession.OpeningAmount + totalDeposits + cashSales;

// ✅ 5. Atualiza a entidade com o valor correto
cashSession.UpdateExpectedCashAmount(expectedCashAmount);

// ✅ 6. Fecha o caixa
cashSession.Close(command.ClosingAmount, command.Notes);
```

### Por Que Isso É Importante?

A diferença calculada no fechamento é: **Difference = ClosingAmount - ExpectedCashAmount**

Se `ExpectedCashAmount` não incluir vendas em dinheiro:
```
Cenário Real:
- Opening: R$ 100
- Deposits: R$ 50
- Cash Sales: R$ 200
- Closing Amount contado: R$ 350

Com BUG (antes):
Expected = 100 + 50 = 150
Difference = 350 - 150 = +200 ❌ (parece um SUPERÁVIT enorme!)

Com FIX (depois):
Expected = 100 + 50 + 200 = 350
Difference = 350 - 350 = 0 ✅ (correto!)
```

---

## Fluxo Completo Após Implementação

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Abrir Caixa (OpenCashSession)                           │
│    └─ CashSession.PaymentBreakdown = []                    │
│    └─ CashSession.TotalOrders = 0                          │
└────────────────┬────────────────────────────────────────────┘

┌────────────────▼────────────────────────────────────────────┐
│ 2. Vender (Order Created & Completed)                      │
│    └─ OrderCompletedEvent emitido                          │
│    └─ Event Handler recalcula breakdown                    │
│    └─ CashSession.UpdateTotals() chamado                   │
│    └─ Breakdown atualizado em TEMPO REAL ✅                │
└────────────────┬────────────────────────────────────────────┘

┌────────────────▼────────────────────────────────────────────┐
│ 3. Fechar Caixa (CloseCashSession)                         │
│    └─ Usa ICashSessionBreakdownService.CalculateAsync()    │
│    └─ Valida/confirma breakdown (já está atualizado)       │
│    └─ Apenas chama cashSession.Close()                     │
│    └─ Retorna CashSessionResponse com dados finais ✅      │
└─────────────────────────────────────────────────────────────┘
```

---

## Checklist de Implementação

### Fase 1: Preparação
- [ ] Implementar `ICashSessionBreakdownService` (ver CASH-REGISTER-BREAKDOWN-REFACTOR.md)
- [ ] Implementar `UpdateCashSessionBreakdownOnOrderCompletedHandler`
- [ ] Garantir que `OrderCompletedEvent` está sendo emitido corretamente

### Fase 2: Refatoração CloseCashSession
- [ ] Adicionar injeção de `ICashSessionBreakdownService` no handler
- [ ] Remover cálculo manual de breakdown
- [ ] Remover query de Orders
- [ ] Remover chamada a `UpdateTotals()`
- [ ] Adicionar validações opcionais (se necessário)

### Fase 3: Testes
- [ ] Teste unitário: Validar que handler usa serviço
- [ ] Teste integração: Abrir → vender → fechar
- [ ] Teste: Breakdown está atualizado antes de fechar
- [ ] Teste: Performance melhorou (sem recalcular)

### Fase 4: Validação
- [ ] Build compila sem erros
- [ ] Testes passam
- [ ] API funciona end-to-end
- [ ] Remover código morto (se houver)

---

## Impactos Secundários

### No Frontend
- ✅ **Nenhum impacto** - API retorna mesmo formato

### Em Outros Handlers
- ⚠️ Verificar se algum outro handler chama `CloseCashSessionHandler`
- ⚠️ Se houver `GetCashSessionById`, garantir que retorna breakdown atualizado

### Em Testes
- ⚠️ Testes de integração podem precisar mockar `ICashSessionBreakdownService`
- ⚠️ Testes de performance devem verificar melhoria

---

## Ganhos de Performance

### Antes
```
1. Abrir caixa: 0 queries
2. Cada venda: 1 event (sem impacto)
3. Fechar caixa: 
   - Query de Orders
   - GroupBy em memória
   - Update CashSession
   ≈ 50-100ms (depende de vendas)
```

### Depois
```
1. Abrir caixa: 0 queries
2. Cada venda:
   - Recalcula breakdown incremental (distribuído)
   ≈ 10-20ms por venda (não concentrado)
3. Fechar caixa:
   - Apenas confirma
   ≈ 5ms
   ✅ 10-20x MAIS RÁPIDO
```

---

## Rollback Plan

Se algo der errado durante implementação:

1. Manter código antigo comentado por 1-2 sprints
2. Feature flag para habilitar/desabilitar novo serviço
3. Testes paralelos (ambas implementações)

```csharp
if (useNewBreakdownService)
    (breakdown, revenue, orders) = await breakdownService.CalculateAsync(...);
else
    (breakdown, revenue, orders) = await CalculateBreakdownLegacy(...); // OLD
```

---

## Referências

- [CASH-REGISTER-BREAKDOWN-REFACTOR.md](./CASH-REGISTER-BREAKDOWN-REFACTOR.md) - Implementação da solução
- [Domain Events Pattern](https://martinfowler.com/eaaDev/DomainEvent.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
