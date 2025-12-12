# Refatoração: Cálculo em Tempo Real do Payment Breakdown

## Contexto

Atualmente, o `PaymentBreakdown` de uma `CashSession` é calculado apenas quando o caixa é **fechado**. Isso impede que o operador acompanhe as vendas por forma de pagamento em tempo real durante a sessão aberta.

## Objetivo

Implementar cálculo do `PaymentBreakdown` em tempo real, atualizando a sessão de caixa toda vez que um pedido é completado.

## Arquitetura Proposta: Solução Híbrida (Serviço + Events)

### 1. Criar Serviço de Cálculo de Breakdown

**Arquivo:** `Features/CashRegister/Services/CashSessionBreakdownService.cs`

```csharp
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Shared.Database.Context;
using Devlivery.Shared.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Services;

public interface ICashSessionBreakdownService
{
    /// <summary>
    /// Calcula o breakdown de vendas para uma sessão de caixa aberta.
    /// Busca todos os pedidos completados desde a abertura da sessão.
    /// </summary>
    Task<(List<PaymentBreakdownItem> Breakdown, decimal TotalRevenue, int TotalOrders)> 
        CalculateBreakdownAsync(Guid cashSessionId, CancellationToken cancellationToken = default);
}

public sealed class CashSessionBreakdownService(ApplicationDbContext dbContext) : ICashSessionBreakdownService
{
    public async Task<(List<PaymentBreakdownItem>, decimal, int)> 
        CalculateBreakdownAsync(Guid cashSessionId, CancellationToken cancellationToken = default)
    {
        // Busca a sessão para obter startAt
        var cashSession = await dbContext.CashSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Id == cashSessionId, cancellationToken);

        if (cashSession is null)
            return (new List<PaymentBreakdownItem>(), 0, 0);

        // Busca pedidos completados desde a abertura da sessão
        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CashSessionId == cashSessionId && 
                        o.Status == OrderStatus.Completed)
            .Include(o => o.OrderItems)
            .ToListAsync(cancellationToken);

        // Agrupa por payment method
        var breakdown = orders
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new PaymentBreakdownItem(
                Method: g.Key,
                Amount: g.Sum(o => o.Total),
                Count: g.Count()))
            .ToList();

        var totalRevenue = orders.Sum(o => o.Total);
        var totalOrders = orders.Count;

        return (breakdown, totalRevenue, totalOrders);
    }
}
```

### 2. Registrar Serviço na Feature

**Arquivo:** `Features/CashRegister/CashRegisterFeature.cs`

```csharp
public static IServiceCollection AddCashRegisterFeature(this IServiceCollection services)
{
    // ... handlers existentes ...
    
    // Novo serviço
    services.AddScoped<ICashSessionBreakdownService, CashSessionBreakdownService>();
    
    return services;
}
```

### 3. Criar Event Handler para Order Completed

**Arquivo:** `Features/CashRegister/EventHandlers/UpdateCashSessionBreakdownOnOrderCompletedHandler.cs`

```csharp
using MediatR;
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Services;
using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.EventHandlers;

public sealed class UpdateCashSessionBreakdownOnOrderCompletedHandler(
    ICashSessionBreakdownService breakdownService,
    ApplicationDbContext dbContext) 
    : INotificationHandler<OrderCompletedEvent>
{
    public async Task Handle(OrderCompletedEvent @event, CancellationToken cancellationToken)
    {
        // Busca a sessão de caixa aberta para este pedido
        var cashSession = await dbContext.CashSessions
            .FirstOrDefaultAsync(
                cs => cs.Id == @event.CashSessionId && 
                      cs.Status == CashSessionStatus.Open,
                cancellationToken);

        if (cashSession is null)
            return; // Pedido sem sessão de caixa ou sessão já fechada

        // Recalcula o breakdown
        var (breakdown, totalRevenue, totalOrders) = 
            await breakdownService.CalculateBreakdownAsync(@event.CashSessionId, cancellationToken);

        // Atualiza a sessão com os novos totais
        cashSession.UpdateTotals(totalRevenue, totalOrders, breakdown);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### 4. Emit Event quando Pedido é Completado

**Arquivo:** `Features/Orders/Domain/Order.cs` (modificar)

```csharp
public sealed class Order : Entity
{
    // ... propriedades existentes ...

    public void MarkAsCompleted()
    {
        if (Status == OrderStatus.Completed)
            return;

        Status = OrderStatus.Completed;
        
        // Emit event para atualizar breakdown do caixa
        AddDomainEvent(new OrderCompletedEvent(
            OrderId: this.Id,
            CashSessionId: this.CashSessionId,
            PaymentMethod: this.PaymentMethod,
            Amount: this.Total));
        
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Nota:** Certificar que `OrderCompletedEvent` existe em `Features/Orders/Domain/Events/OrderCompletedEvent.cs`

### 5. Refatoração do CloseCashSession

Na feature de **fechar caixa**, remover o cálculo manual do breakdown:

**Antes:**
```csharp
// CloseCashSessionHandler.cs - REMOVER ISSO
var orders = await dbContext.Orders
    .Where(o => o.CashSessionId == command.CashSessionId)
    .ToListAsync(cancellationToken);

var breakdown = orders.GroupBy(...).ToList();
cashSession.UpdateTotals(totalRevenue, totalOrders, breakdown);
```

**Depois:**
```csharp
// CloseCashSessionHandler.cs - USAR SERVIÇO
var (breakdown, totalRevenue, totalOrders) = 
    await breakdownService.CalculateBreakdownAsync(command.CashSessionId, cancellationToken);

// Apenas valida/confirma - não calcula
cashSession.Close(command.ClosingAmount, command.Notes);
```

**Benefício:** O breakdown já está atualizado, só falta confirmar ao fechar.

---

## Implementação Step-by-Step

### Fase 1: Preparação
- [ ] Criar `CashSessionBreakdownService` (interface + implementação)
- [ ] Registrar serviço em `CashRegisterFeature`
- [ ] Criar folder `Features/CashRegister/EventHandlers/`

### Fase 2: Integration com Events
- [ ] Criar `UpdateCashSessionBreakdownOnOrderCompletedHandler`
- [ ] Garantir que `OrderCompletedEvent` está sendo emitido
- [ ] Registrar handler em MediatR (ou usar `INotificationHandler`)

### Fase 3: Refatoração CloseCashSession
- [ ] Remover lógica de cálculo de breakdown de `CloseCashSessionHandler`
- [ ] Usar `ICashSessionBreakdownService` para pegar breakdown final
- [ ] Testar fluxo completo: abrir → vender → fechar

### Fase 4: Testes
- [ ] Testes unitários do `CashSessionBreakdownService`
- [ ] Testes de integração do event handler
- [ ] Testes end-to-end do fluxo completo

---

## Benefícios

✅ **Tempo Real:** Breakdown atualizado a cada venda completada  
✅ **Reutilizável:** Serviço pode ser usado em qualquer contexto (relatórios, etc)  
✅ **Desacoplado:** Order não conhece CashSession (via events)  
✅ **Testável:** Lógica isolada e fácil de mockar  
✅ **Escalável:** Fácil adicionar mais handlers se necessário  

---

## Riscos / Considerações

⚠️ **Performance:** Se muitos pedidos por sessão, recalcular pode ficar lento
  - *Mitigação:* Adicionar índices em `Orders.CashSessionId`, `Orders.Status`

⚠️ **Race Conditions:** Múltiplos pedidos completando ao mesmo tempo
  - *Mitigação:* EF Core usa transaction automática, mas considerar lock pessimista se crítico

⚠️ **Dados Inconsistentes:** Event falhar após Order ser salvo
  - *Mitigação:* Implementar retry policy no handler

---

## Referências

- **Domain Events Pattern:** Usar `INotificationHandler<T>` do MediatR
- **Event Sourcing:** Considerar guardar histórico de cada cálculo se auditoria crítica
- **Performance:** Considerar caching do breakdown com invalidação automática
