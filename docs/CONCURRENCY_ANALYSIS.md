# Análise de Concorrência - Devlivery System

## 🚨 Problema Identificado: Race Conditions em Múltiplos Agregados

### **Contexto**
Sistema de delivery com múltiplas operações concorrentes:
- Pedidos sendo confirmados simultaneamente
- Pagamentos sendo registrados no caixa
- Movimentações financeiras paralelas

---

## ✅ **Correções Implementadas**

### **1. Order Aggregate - Idempotência de Pagamento**

**Problema:** Múltiplas chamadas a `UpdateStatus(Delivered)` confirmavam o mesmo pagamento várias vezes.

**Solução:**
```csharp
// ANTES (linha 89-91)
_payments.Where(p => p.PaymentStatus != PaymentStatus.Cancelled)
    .ToList()
    .ForEach(ConfirmPayment);

// DEPOIS
_payments.Where(p => p.PaymentStatus == PaymentStatus.Pending)
    .ToList()
    .ForEach(ConfirmPayment);
```

**Justificativa DDD:**
- **Invariante do Agregado**: Um pagamento só pode ser confirmado uma vez
- **Idempotência**: Múltiplas chamadas com mesmo input = mesmo resultado
- **Exception em OrderPayment.Confirm()**: Segunda linha de defesa contra duplicidade

---

### **2. Optimistic Concurrency Control (OCC)**

**Adicionado RowVersion como Shadow Property:**

#### Order
```csharp
// OrderConfiguration.cs
builder.Property<byte[]>("RowVersion")
    .IsRowVersion()
    .HasColumnName("row_version");
```

#### OrderPayment
```csharp
// OrderPaymentConfiguration.cs
builder.Property<byte[]>("RowVersion")
    .IsRowVersion()
    .HasColumnName("row_version");
```

#### CashSession
```csharp
// CashSessionConfiguration.cs
builder.Property<byte[]>("RowVersion")
    .IsRowVersion()
    .HasColumnName("row_version");
```

#### CashSessionMovement
```csharp
// CashSessionMovementConfiguration.cs
builder.Property<byte[]>("RowVersion")
    .IsRowVersion()
    .HasColumnName("row_version");

// Unique constraint para prevenir pagamentos duplicados
builder.HasIndex(x => new { x.OrderPaymentId, x.CashSessionId, x.EntryType })
    .HasDatabaseName("IX_CashSessionMovements_UniquePayment")
    .IsUnique()
    .HasFilter("[order_payment_id] IS NOT NULL AND [entry_type] = 'Payment'");
```

**Como Funciona:**
1. EF Core verifica `RowVersion` antes de salvar
2. Se outro processo modificou o registro, lança `DbUpdateConcurrencyException`
3. Sistema pode retry automaticamente (Polly) ou informar usuário

---

## 🔍 **Outras Áreas de Risco Identificadas**

### **3. CashSession - Race Conditions Remanescentes**

**Cenários de Risco:**

#### A. Múltiplos Handlers Processando Mesmo Evento
```csharp
// OrderPaymentConfirmedEventHandler.cs (linha 32-48)
var activeSession = await cashSessionRepository.GetActiveSessionAsync(cancellationToken);

// ⚠️ PROBLEMA: 3 threads podem obter o mesmo CashSession simultaneamente
// ⚠️ Proteção em memória (linha 56) não funciona entre processos

activeSession.AddPayment(
    orderPaymentId: notification.PaymentId,  // ← Mesmo ID
    amount: notification.Amount,
    paymentMethod: notification.PaymentMethod,
    relatedOrderId: notification.OrderId
);
```

**Mitigações Aplicadas:**
1. ✅ **Idempotência no Domain**: `if (_movements.Exists(m => m.OrderPaymentId == orderPaymentId)) return;`
2. ✅ **Unique Constraint**: Database garante unicidade de Payment por sessão
3. ✅ **RowVersion**: EF Core detecta conflitos e lança exception
4. ✅ **OrderPayment.Confirm()**: Previne confirmação duplicada na origem

**Fluxo de Proteção:**
```
Request 1 ──┐
Request 2 ──┼──> GetActiveSession() ──> AddPayment() ──> SaveChanges()
Request 3 ──┘                                                  │
                                                               ↓
                                                    RowVersion Check
                                                               │
                                                      Request 1: ✅ Success
                                                      Request 2: ❌ DbUpdateConcurrencyException
                                                      Request 3: ❌ DbUpdateConcurrencyException
```

#### B. AddChange - Mesma Proteção
```csharp
// CashSession.cs (linha 102-103)
if (HasChangeFor(relatedOrderId))
    return;
```
- ✅ Proteção em memória
- ✅ RowVersion detecta conflitos

---

### **4. Estratégias de Retry com Polly (Recomendado)**

**Para Event Handlers com Concorrência:**

```csharp
// Adicionar em Program.cs ou Startup
services.AddMediatR(cfg =>
{
    cfg.AddOpenBehavior(typeof(RetryBehavior<,>));
});

// RetryBehavior.cs
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(50 * Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    // Log retry attempt
                });

        return await retryPolicy.ExecuteAsync(() => next());
    }
}
```

---

## 🎯 **Checklist de Validação**

### Testes de Concorrência Necessários

#### 1. Order - Confirmação Simultânea
```csharp
[Fact]
public async Task UpdateStatus_ConcurrentDeliveryConfirmations_ShouldConfirmPaymentOnlyOnce()
{
    // Setup: Order com 1 pagamento
    var order = CreateOrderWithOnePayment();
    
    // Act: 3 threads confirmam entrega simultaneamente
    var tasks = Enumerable.Range(0, 3)
        .Select(_ => Task.Run(() => order.UpdateStatus(OrderStatus.Delivered)))
        .ToArray();
        
    // Apenas primeira deve suceder, demais devem lançar exception
    await Should.ThrowAsync<InvalidOperationException>(() => Task.WhenAll(tasks));
    
    // Assert: Pagamento confirmado apenas 1 vez
    order.Payments.Count(p => p.PaymentStatus == PaymentStatus.Confirmed).ShouldBe(1);
}
```

#### 2. CashSession - Lançamentos Duplicados
```csharp
[Fact]
public async Task AddPayment_ConcurrentRequests_ShouldPreventDuplicates()
{
    // Setup: CashSession ativa + mesmo OrderPaymentId
    var session = CreateActiveCashSession();
    var paymentId = Guid.NewGuid();
    
    // Act: 3 threads tentam adicionar mesmo pagamento
    var tasks = Enumerable.Range(0, 3)
        .Select(_ => Task.Run(() => 
            session.AddPayment(paymentId, 100m, PaymentMethod.Pix, orderId)))
        .ToArray();
        
    await Task.WhenAll(tasks);
    
    // Assert: Apenas 1 movimento criado
    session.Movements.Count(m => m.OrderPaymentId == paymentId).ShouldBe(1);
}
```

#### 3. Database Constraint - Última Linha de Defesa
```sql
-- Validar que constraint funciona
INSERT INTO cash_session_movements (order_payment_id, cash_session_id, entry_type, ...)
VALUES ('same-id', 'same-session', 'Payment', ...);

-- Segunda inserção deve falhar
INSERT INTO cash_session_movements (order_payment_id, cash_session_id, entry_type, ...)
VALUES ('same-id', 'same-session', 'Payment', ...);
-- ❌ Violation of UNIQUE KEY constraint 'IX_CashSessionMovements_UniquePayment'
```

---

## 📊 **Níveis de Proteção (Defense in Depth)**

| Nível | Mecanismo | Localização | Tipo |
|-------|-----------|-------------|------|
| 1️⃣ | Idempotência | `OrderPayment.Confirm()` | Exception |
| 2️⃣ | Invariante de Agregado | `Order.UpdateStatus()` | Filter (Pending only) |
| 3️⃣ | Check em Memória | `CashSession.AddPayment()` | Early return |
| 4️⃣ | Optimistic Concurrency | EF Core RowVersion | DbUpdateConcurrencyException |
| 5️⃣ | Unique Constraint | Database Index | SQL Exception |

**Princípio:** Cada camada protege independentemente. Falha em uma, próxima captura.

---

## 🔧 **Próximos Passos Recomendados**

### 1. **Adicionar Retry Policy (Alta Prioridade)**
- Instalar `Polly` package
- Configurar retry automático para `DbUpdateConcurrencyException`
- Logs de tentativas de retry

### 2. **Testes de Carga (Alta Prioridade)**
- Simular 50+ requisições simultâneas confirmando mesmo pedido
- Validar que não há duplicidade no CashSession
- Monitorar exceptions de concorrência

### 3. **Monitoring & Alerting (Média Prioridade)**
```csharp
// Adicionar métricas
_metrics.Increment("order.payment.confirmation.duplicate_attempt");
_metrics.Increment("cashsession.concurrency_exception");
```

### 4. **Audit Trail (Baixa Prioridade)**
- Log de todas as tentativas de confirmação
- Rastreamento de quem/quando/por que houve retry

---

## 📚 **Referências DDD & Concurrency**

- **Aggregate Pattern**: Garantir consistência transacional dentro do boundary
- **Optimistic Locking**: Assume conflitos são raros, detecta ao salvar
- **Idempotência**: Operações podem ser repetidas com mesmo resultado
- **Invariantes**: Regras de negócio que nunca podem ser violadas
- **Event Sourcing** (futuro): Garantia absoluta contra perda de eventos

---

## ✅ **Conclusão**

**Estado Atual:** Sistema **SIGNIFICATIVAMENTE MAIS SEGURO** após correções.

**Camadas de Proteção:**
1. ✅ Domain validation (exceptions)
2. ✅ Idempotent operations
3. ✅ Optimistic concurrency (RowVersion)
4. ✅ Database constraints (unique indexes)
5. ⚠️ Retry policies (TODO)

**Risco Residual:** Baixo, mas requer testes de carga para validar comportamento sob stress.

---

_Última atualização: 2026-01-15_
_Autor: AI Assistant (DDD Specialist)_
