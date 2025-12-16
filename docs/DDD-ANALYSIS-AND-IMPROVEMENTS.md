# Análise DDD: Sistema de Delivery - Problemas Identificados e Soluções

## 🔴 Problemas Críticos de DDD Identificados

### 1. **OrderItem como Entidade Filha - CORRETO** ✅
**Análise**: `OrderItem` está corretamente implementado como **Entidade Filha** do agregado `Order`.

**Por quê OrderItem É uma Entidade (e não Value Object)?**
- OrderItem TEM identidade significativa dentro do contexto de um Order (precisa ser rastreado individualmente)
- OrderItem pode ser atualizado/removido especificamente (ex: "remover o item 3")
- A ordem e identidade dos itens importa para o negócio
- Necessário para auditoria e rastreamento de mudanças

**Por quê está CORRETO em DDD?**
- ✅ OrderItem NÃO é acessado diretamente por repositórios (só via Order)
- ✅ OrderItem é parte do **limite transacional** de Order
- ✅ OrderItem tem ID para rastreamento interno no agregado
- ✅ OrderItem só existe dentro do ciclo de vida do Order

**Padrão DDD**: Entidade Filha dentro de Agregado

---

### 2. **CashSession Manipulando Lógica de Agregado Externamente** ❌

**Problema**: Em `OrderUpdatedEventHandler`, estamos manipulando diretamente:
```csharp
activeSession.PaymentBreakdown.Remove(existingItem);
activeSession.PaymentBreakdown.Add(updatedItem);
```

**Por quê é problema?**
- Violação do **Encapsulamento** do agregado
- Lógica de negócio do CashSession vazando para fora
- PaymentBreakdown é uma coleção privada, mas estamos manipulando-a via métodos públicos incorretos

**Solução**: Toda lógica deve estar dentro do agregado CashSession através de métodos com nomes de negócio

---

### 3. **Falta de Value Objects** ❌

**Problema**: Conceitos importantes estão sendo representados por tipos primitivos:
- `CustomerName` → deveria ser `CustomerInfo` (VO)
- `DeliveryAddress` → deveria ser `Address` (VO)
- `PaymentMethod` → já é enum, mas poderia ser VO com regras
- `Money` → decimal está sendo usado, mas Money é conceito de domínio

---

### 4. **Agregados e Limites de Consistência Não Claros** ⚠️

**Agregados Identificados**:
```
┌─────────────────────────────────────────┐
│ AGREGADO: Order                         │
│ Raiz: Order                             │
│ Entidades Filhas: OrderItem             │
│ Value Objects: (faltando)               │
│ Limite: Consistência do pedido          │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ AGREGADO: CashSession                   │
│ Raiz: CashSession                       │
│ Entidades Filhas: CashDeposit           │
│ Value Objects: PaymentBreakdownItem (✓) │
│ Limite: Consistência do caixa           │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ AGREGADO: Product                       │
│ Raiz: Product                           │
│ Entidades Filhas: nenhuma               │
│ Limite: Consistência do produto         │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ AGREGADO: Establishment                 │
│ Raiz: Establishment                     │
│ Entidades Filhas: (deveria ter!)        │
│ Limite: Multi-tenancy                   │
└─────────────────────────────────────────┘
```

**Problema**: Product e Establishment deveriam estar relacionados, mas estão em agregados separados.

---

## ✅ O Que Está CORRETO

1. ✅ **Domain Events**: Bem implementados para comunicação entre agregados
2. ✅ **Repository Pattern**: Um repositório por agregado raiz
3. ✅ **Encapsulamento**: Setters privados nas entidades
4. ✅ **PaymentBreakdownItem como Record**: Correto como Value Object
5. ✅ **Vertical Slice Architecture**: Organização por feature é boa
6. ✅ **Unit of Work**: Gerenciamento de transações correto

---

## 🎯 Proposta de Reestruturação DDD

### Estrutura de Agregados para Delivery

```
📦 CONTEXTO: DELIVERY

┌────────────────────────────────────────────────────────────┐
│ AGREGADO: Order (Raiz de Agregado)                         │
├────────────────────────────────────────────────────────────┤
│ Responsabilidades:                                         │
│ - Manter consistência do pedido                            │
│ - Calcular total                                           │
│ - Gerenciar ciclo de vida (status)                         │
│ - Validar transições de estado                             │
│                                                             │
│ Entidades Filhas:                                          │
│ - OrderItem (SEM ID EXTERNO - apenas posição na lista)     │
│                                                             │
│ Value Objects:                                             │
│ - CustomerInfo (name, phone)                               │
│ - DeliveryAddress (street, number, city, etc)              │
│ - Money (amount, currency)                                 │
│ - OrderTotal (subtotal, deliveryFee, total)                │
│                                                             │
│ Invariantes:                                               │
│ - Total = Σ(items) + deliveryFee                           │
│ - Status: Pending → Preparing → Ready → Delivered          │
│ - Não pode alterar pedido cancelado/entregue               │
│ - Deve ter pelo menos 1 item                               │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ AGREGADO: CashSession (Raiz de Agregado)                   │
├────────────────────────────────────────────────────────────┤
│ Responsabilidades:                                         │
│ - Rastrear vendas do dia                                   │
│ - Calcular totais por forma de pagamento                   │
│ - Gerenciar depósitos                                      │
│ - Fechar caixa com diferença                               │
│                                                             │
│ Entidades Filhas:                                          │
│ - CashDeposit                                              │
│                                                             │
│ Value Objects:                                             │
│ - PaymentBreakdownItem ✓                                   │
│ - Money                                                    │
│ - CashBalance (expected, actual, difference)               │
│                                                             │
│ Invariantes:                                               │
│ - Apenas 1 sessão aberta por establishment                 │
│ - ExpectedCash = Opening + CashSales + Deposits            │
│ - Não pode fechar com sessão já fechada                    │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ AGREGADO: Product (Raiz de Agregado)                       │
├────────────────────────────────────────────────────────────┤
│ Responsabilidades:                                         │
│ - Manter informações do produto                            │
│ - Disponibilidade                                          │
│                                                             │
│ Value Objects:                                             │
│ - Money (price)                                            │
│ - ProductCategory                                          │
│                                                             │
│ Invariantes:                                               │
│ - Price > 0                                                │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ AGREGADO: Establishment (Raiz de Agregado)                 │
├────────────────────────────────────────────────────────────┤
│ Responsabilidades:                                         │
│ - Multi-tenancy                                            │
│ - Configurações do estabelecimento                         │
│                                                             │
│ Value Objects:                                             │
│ - BusinessHours                                            │
│ - Address                                                  │
│                                                             │
│ Invariantes:                                               │
│ - Deve ter nome                                            │
└────────────────────────────────────────────────────────────┘
```

---

## 🛠️ Refatorações Necessárias

### ~~Refatoração 1: OrderItem como Value Object~~ CORRIGIDO ✅

**CORREÇÃO**: OrderItem É uma **Entidade Filha**, não um Value Object!

**Por quê?**
- Precisa de ID para rastreamento individual dentro do Order
- Pode ser modificado/removido especificamente
- Tem ciclo de vida próprio dentro do agregado

**Implementação Correta** ✅:
```csharp
public sealed class OrderItem : Entity  // ← Entidade Filha
{
    public Guid Id { get; } // ← NECESSÁRIO para identidade
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    
    // Métodos de comportamento
    public void UpdateQuantity(int newQuantity) { ... }
    public void UpdateNotes(string? newNotes) { ... }
}
```

**Importante**: Não tem repositório próprio - só acessível via Order

---

### Refatoração 2: Value Objects Essenciais

**CustomerInfo**:
```csharp
public sealed record CustomerInfo
{
    public string Name { get; init; }
    public PhoneNumber? Phone { get; init; }
    
    public CustomerInfo(string name, PhoneNumber? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do cliente é obrigatório");
        
        Name = name;
        Phone = phone;
    }
}
```

**DeliveryAddress**:
```csharp
public sealed record DeliveryAddress
{
    public string Street { get; init; }
    public string Number { get; init; }
    public string? Complement { get; init; }
    public string Neighborhood { get; init; }
    public string City { get; init; }
    public string ZipCode { get; init; }
    
    public string FullAddress => 
        $"{Street}, {Number}{(Complement != null ? $" - {Complement}" : "")}, {Neighborhood}, {City} - {ZipCode}";
}
```

**Money**:
```csharp
public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";
    
    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Valor não pode ser negativo");
        Amount = amount;
    }
    
    public Money Add(Money other) => new(Amount + other.Amount);
    public Money Subtract(Money other) => new(Amount - other.Amount);
    public Money Multiply(int quantity) => new(Amount * quantity);
    
    public static Money Zero => new(0);
}
```

---

### Refatoração 3: Encapsular Lógica no CashSession

**Antes** ❌ (em OrderUpdatedEventHandler):
```csharp
activeSession.PaymentBreakdown.Remove(existingItem);
activeSession.PaymentBreakdown.Add(updatedItem);
```

**Depois** ✅:
```csharp
// No CashSession.cs
public void AdjustOrderTotal(decimal oldTotal, decimal newTotal, string paymentMethod)
{
    var difference = newTotal - oldTotal;
    TotalRevenue += difference;
    
    var item = PaymentBreakdown.FirstOrDefault(p => p.Method == paymentMethod);
    if (item is not null)
    {
        PaymentBreakdown.Remove(item);
        PaymentBreakdown.Add(item with { Amount = item.Amount + difference });
    }
    
    if (paymentMethod == "Cash")
    {
        UpdateExpectedCashAmount(ExpectedCashAmount + difference);
    }
    
    UpdatedAt = DateTime.UtcNow;
}

// No Handler
activeSession.AdjustOrderTotal(
    notification.OldTotal, 
    notification.NewTotal, 
    notification.PaymentMethod.ToString()
);
```

---

## 📋 Checklist de Conformidade DDD

###x] OrderItem corretamente implementado como Entidade Filha ✓
- [ ] CashDeposit avaliar se deve ter ID (provavelmente sim - Entidade Filha)
- [x] Limites de agregados bem definidos ✓ ou ser VO
- [ ] Limites de agregados bem definidos
- [ ] Regras de invariante documentadas

### Value Objects
- [ ] Criar Money
- [ ] Criar CustomerInfo
- [ ] Criar DeliveryAddress
- [ ] Criar PhoneNumber (com validação)

### Encapsulamento
- [x] Setters privados ✓
- [ ] Métodos de negócio em vez de manipulação direta de coleções
- [ ] Validações no construtor dos VOs

### Domain Events
- [x] Eventos para comunicação entre agregados ✓
- [x] Handlers não manipulam diretamente estado de outros agregados ⚠️ (precisa ajuste)

### Repositories
- [x] Um repositório por agregado raiz ✓
- [ ] Repositórios retornam agregados completos
- [ ] Não há repositórios para entidades filhas

---

## 🎯 Prioridades de Refatoração

**Alta Prioridade** (Impacto imediato):
1. ✅ Criar Value Objects: Money, CustomerInfo, DeliveryAddress
2. ✅ ~~Refatorar OrderItem~~ **CORRIGIDO**: OrderItem É Entidade Filha (correto)
3. ✅ Encapsular lógica do CashSession (remover manipulação externa)

**Média Prioridade** (Qualidade de código):
4. Adicionar validações de invariantes nos agregados
5. Documentar limites de agregados explicitamente
6. Criar testes de unidade para regras de negócio

**Baixa Prioridade** (Otimizações):
7. Avaliar se CashDeposit precisa de ID
8. Considerar Specification Pattern para queries complexas
9. Avaliar Domain Services se houver lógica entre múltiplos agregados

---

## 📚 Referências DDD para Delivery

**Ubiquitous Language Sugerido**:
- **Pedido** (Order) - não "Requisição"
- **Cliente** (Customer) - pessoa que faz o pedido
- **Estabelecimento** (Establishment) - restaurante/loja
- **Caixa** (CashSession) - sessão de vendas do dia
- **Entrega** (Delivery) - ato de entregar o pedido
- **Item do Pedido** (OrderItem) - produto + quantidade
- **Forma de Pagamento** (PaymentMethod) - como será pago

---

**Data**: 16 de dezembro de 2025  
**Status**: 🔴 Necessita Refatoração  
**Impacto**: Alto - Melhora significativa na manutenibilidade e correção conceitual
