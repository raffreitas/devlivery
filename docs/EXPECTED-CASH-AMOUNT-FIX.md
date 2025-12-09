# Fix: ExpectedCashAmount Calculation

## Issue
O cálculo de `ExpectedCashAmount` estava incompleto, causando diferença incorreta no fechamento do caixa.

**Situação antes do fix:**
```
Opening = R$ 100
Deposits = R$ 50
CashSales = R$ 200
Closing Contado = R$ 350

ExpectedCashAmount = Opening + Deposits = R$ 150 ❌
Difference = Closing - Expected = 350 - 150 = +200 (parecia superávit enorme!)
```

## Root Cause
A arquitetura atual calcula `PaymentBreakdown` (vendas por método de pagamento) apenas quando o caixa é **fechado**. 

Durante a sessão aberta:
- Sabemos o `OpeningAmount` (abertura)
- Sabemos os `Deposits` (aportes adicionados)
- ❌ Não sabemos as `CashSales` (não foram calculadas ainda)

## Solution

### 1. CreateCashDepositHandler (sem mudança)
Continua calculando `ExpectedCashAmount = Opening + Deposits` porque é tudo que temos disponível naquele momento.

```csharp
// Comentário explicativo adicionado
// Note: PaymentBreakdown is only calculated when closing the session,
// so at this point we only add the deposit to the opening amount
var totalDeposits = await dbContext.CashDeposits
    .Where(cd => cd.CashSessionId == command.CashSessionId)
    .SumAsync(cd => cd.Amount, cancellationToken);

cashSession.UpdateExpectedCashAmount(cashSession.OpeningAmount + totalDeposits);
```

### 2. CloseCashSessionHandler (ATUALIZADO)
Agora recalcula `ExpectedCashAmount` ANTES de fechar, incluindo as vendas em dinheiro:

```csharp
// ✅ IMPORTANTE: Recalcular ExpectedCashAmount ANTES de fechar
// Formula: Opening + Deposits + CashSales
var totalDeposits = await dbContext.CashDeposits
    .Where(cd => cd.CashSessionId == cashSession.Id)
    .SumAsync(cd => cd.Amount, cancellationToken);

var cashSales = paymentBreakdown
    .Where(pb => pb.Method.Equals("cash", StringComparison.OrdinalIgnoreCase))
    .Sum(pb => pb.Amount);

var expectedCashAmount = cashSession.OpeningAmount + totalDeposits + cashSales;
cashSession.UpdateExpectedCashAmount(expectedCashAmount);

// Agora fecha com o valor correto
cashSession.UpdateTotals(totalRevenue, totalOrders, paymentBreakdown);
cashSession.Close(command.ClosingAmount, command.Notes);
```

## Resultado

**Agora com o fix:**
```
Opening = R$ 100
Deposits = R$ 50
CashSales = R$ 200
Closing Contado = R$ 350

ExpectedCashAmount = Opening + Deposits + CashSales = R$ 350 ✅
Difference = Closing - Expected = 350 - 350 = 0 (correto!)
```

## Files Modified
- `Features/CashRegister/Commands/CloseCashSession/CloseCashSessionHandler.cs` - Added ExpectedCashAmount recalculation
- `Features/CashRegister/Commands/CreateCashDeposit/CreateCashDepositHandler.cs` - Added clarifying comment (no behavior change)
- `docs/CLOSE-CASH-SESSION-REFACTOR.md` - Added detailed explanation of calculation stages

## Testing
Para verificar que o fix está funcionando:

1. Abrir caixa com R$ 100
2. Adicionar aporte de R$ 50 → Expected = 150
3. Vender R$ 200 em dinheiro
4. Fechar caixa contando R$ 350
5. Verificar que Expected = 350 e Difference = 0

## Nota sobre Arquitetura Futura
Este é um "patch" que funciona com a arquitetura atual. A solução ideal seria implementar cálculo em **tempo real** do `PaymentBreakdown` usando um serviço dedicado (ver `CASH-REGISTER-BREAKDOWN-REFACTOR.md`). Assim `ExpectedCashAmount` seria sempre preciso sem precisar de recálculo no fechamento.
