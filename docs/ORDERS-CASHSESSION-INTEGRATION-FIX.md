# Correções: Integração Orders ↔ CashSessions via Domain Events

## 📋 Resumo Executivo

Este documento detalha as correções realizadas para fechar os gaps e bugs na integração entre **Orders** e **CashSessions** usando **Eventos de Domínio**.

---

## 🐛 Bugs Corrigidos

### 1. **OrderPaymentMethodChangedEvent - Parâmetros Invertidos** ✅
**Problema**: O evento tinha os parâmetros `NewPaymentMethod` antes de `OldPaymentMethod`, mas ao criar o evento passávamos `OldPaymentMethod` primeiro, causando inversão de valores.

**Solução**: Padronizamos a ordem dos parâmetros para `OldPaymentMethod`, `NewPaymentMethod`.

**Arquivos alterados**:
- `Features/Orders/Domain/Events/OrderPaymentMethodChangedEvent.cs`

---

### 2. **DeleteOrder - Sem Evento de Domínio** ✅
**Problema**: Quando um pedido era deletado, o CashSession não era notificado, deixando os totais incorretos.

**Solução**: 
- Criado novo evento `OrderDeletedEvent`
- Criado handler `OrderDeletedEventHandler` no CashRegister
- Adicionado método `Delete()` na entidade `Order`
- Atualizado `DeleteOrderHandler` para disparar o evento antes da exclusão

**Arquivos criados**:
- `Features/Orders/Domain/Events/OrderDeletedEvent.cs`
- `Features/CashRegister/Events/OrderDeletedEventHandler.cs`

**Arquivos alterados**:
- `Features/Orders/Domain/Order.cs`
- `Features/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs`

---

### 3. **OrderUpdatedEvent - Sem Handler no CashRegister** ✅
**Problema**: Quando um pedido tinha itens alterados (mudando o total), o CashSession não era atualizado.

**Solução**:
- Criado handler `OrderUpdatedEventHandler` no CashRegister
- Atualizado evento `OrderUpdatedEvent` para incluir `OldTotal` e `PaymentMethod`
- Modificado método `UpdateDetails()` em `Order` para capturar o total antigo e só disparar evento se o total mudar
- Criado método `AdjustRevenue()` em `CashSession` para ajustes incrementais

**Arquivos criados**:
- `Features/CashRegister/Events/OrderUpdatedEventHandler.cs`

**Arquivos alterados**:
- `Features/Orders/Domain/Events/OrderUpdatedEvent.cs`
- `Features/Orders/Domain/Order.cs`
- `Features/CashRegister/Domain/CashSession.cs`

---

## ⚠️ Melhorias de Qualidade

### 4. **Validação de Status antes de Atualizar** ✅
**Problema**: Pedidos podiam ter status alterado mesmo se já estavam cancelados ou entregues.

**Solução**: Adicionadas validações no método `UpdateStatus()`:
- Não permite alterar status de pedidos cancelados
- Não permite alterar status de pedidos já entregues (exceto para Delivered)

**Arquivos alterados**:
- `Features/Orders/Domain/Order.cs`

---

### 5. **Métodos Duplicados: RemoveOrder vs CancelOrder** ✅
**Problema**: `CashSession` tinha dois métodos idênticos (`RemoveOrder` e `CancelOrder`), causando confusão e dificultando manutenção.

**Solução**: 
- Removido método `CancelOrder()`
- Consolidado toda lógica em `RemoveOrder()`
- Atualizado `OrderStatusChangedEventHandler` para usar `RemoveOrder()`

**Arquivos alterados**:
- `Features/CashRegister/Domain/CashSession.cs`
- `Features/CashRegister/Events/OrderStatusChangedEventHandler.cs`

---

## 📊 Fluxo Completo de Integração

### Criação de Pedido
```
Order.Constructor → OrderCreatedEvent → OrderCreatedEventHandler → CashSession.RecordOrder()
```

### Atualização de Pedido (Itens/Total)
```
Order.UpdateDetails() → OrderUpdatedEvent → OrderUpdatedEventHandler → CashSession.AdjustRevenue()
```

### Mudança de Método de Pagamento
```
Order.UpdatePaymentMethod() → OrderPaymentMethodChangedEvent → OrderPaymentMethodChangedEventHandler 
→ CashSession.RemoveOrder() + CashSession.RecordOrder()
```

### Cancelamento de Pedido
```
Order.UpdateStatus(Canceled) → OrderStatusChangedEvent → OrderStatusChangedEventHandler 
→ CashSession.RemoveOrder()
```

### Exclusão de Pedido
```
Order.Delete() → OrderDeletedEvent → OrderDeletedEventHandler → CashSession.RemoveOrder()
(Apenas se pedido não estava cancelado)
```

---

## 🔧 Registros de DI Atualizados

Adicionados os novos handlers em `CashRegisterFeature.cs`:
```csharp
services.AddScoped<OrderCreatedEventHandler>();
services.AddScoped<OrderStatusChangedEventHandler>();
services.AddScoped<OrderPaymentMethodChangedEventHandler>();
services.AddScoped<OrderUpdatedEventHandler>();  // ✨ NOVO
services.AddScoped<OrderDeletedEventHandler>();  // ✨ NOVO
```

---

## ✅ Checklist de Validação

- [x] Criação de pedido atualiza CashSession
- [x] Atualização de total do pedido ajusta CashSession
- [x] Mudança de método de pagamento reequilibra breakdown
- [x] Cancelamento de pedido remove valores do CashSession
- [x] Exclusão de pedido remove valores do CashSession (se não cancelado)
- [x] Pedidos cancelados não podem ter status alterado
- [x] Pedidos entregues não podem ter status alterado
- [x] Código sem duplicação (RemoveOrder unificado)
- [x] Todos os eventos de domínio registrados
- [x] Sem erros de compilação

---

## 🧪 Testes Recomendados

1. **Criar pedido** → Verificar se CashSession.TotalRevenue aumenta
2. **Atualizar itens do pedido** → Verificar se CashSession ajusta o total
3. **Trocar forma de pagamento** → Verificar se PaymentBreakdown está correto
4. **Cancelar pedido** → Verificar se CashSession desconta o valor
5. **Deletar pedido não cancelado** → Verificar se CashSession desconta
6. **Deletar pedido cancelado** → Verificar se CashSession não é afetado
7. **Tentar cancelar pedido já cancelado** → Deve lançar exceção
8. **Tentar alterar pedido entregue** → Deve lançar exceção

---

## 📁 Arquivos Criados (3)

1. `Features/Orders/Domain/Events/OrderDeletedEvent.cs`
2. `Features/CashRegister/Events/OrderUpdatedEventHandler.cs`
3. `Features/CashRegister/Events/OrderDeletedEventHandler.cs`

---

## 📝 Arquivos Modificados (7)

1. `Features/Orders/Domain/Order.cs`
2. `Features/Orders/Domain/Events/OrderPaymentMethodChangedEvent.cs`
3. `Features/Orders/Domain/Events/OrderUpdatedEvent.cs`
4. `Features/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs`
5. `Features/CashRegister/Domain/CashSession.cs`
6. `Features/CashRegister/Events/OrderStatusChangedEventHandler.cs`
7. `Features/CashRegister/CashRegisterFeature.cs`

---

## 🎯 Próximos Passos Sugeridos

1. **Testes de Integração**: Criar testes end-to-end para cada fluxo
2. **Logs de Auditoria**: Considerar adicionar eventos de auditoria para rastreabilidade
3. **Reconciliação**: Criar comando para recalcular totais do CashSession baseado em pedidos
4. **Dashboard**: Exibir discrepâncias entre CashSession e Orders na UI

---

**Data**: 16 de dezembro de 2025  
**Status**: ✅ Concluído  
**Impacto**: Alto - Corrige inconsistências críticas na contabilidade do caixa
