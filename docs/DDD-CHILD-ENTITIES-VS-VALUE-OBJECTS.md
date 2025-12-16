# DDD: Entidades Filhas vs Value Objects - Guia Prático

## 📚 Conceitos Fundamentais

### Entidade (Entity)
- **TEM identidade** única e rastreável
- Identidade persiste mesmo que propriedades mudem
- Comparação por ID, não por valor
- Pode ser mutável

### Value Object (VO)
- **NÃO tem identidade** - apenas valor importa
- Dois VOs com mesmos valores são idênticos
- Comparação por valor, não por ID
- DEVE ser imutável

### Entidade Filha (Child Entity)
- **TEM identidade** dentro do contexto do agregado pai
- **NÃO existe independentemente** fora do agregado
- **NÃO tem repositório próprio**
- Acessada apenas através da raiz do agregado
- Parte do mesmo limite transacional

---

## 🎯 Regra de Ouro: Como Decidir?

### Use **ENTIDADE FILHA** quando:

1. ✅ **Necessita rastreamento individual**
   - "Remover o item 3 do pedido"
   - "Atualizar a quantidade do item X"
   - "Mostrar histórico de mudanças do item Y"

2. ✅ **Pode ser modificado individualmente**
   - Item pode ter quantidade alterada
   - Item pode ter observações atualizadas
   - Item pode ser removido/adicionado à coleção

3. ✅ **Ordem/Posição importa**
   - "Primeiro item da lista"
   - "Mover item para cima/baixo"
   - Sequência tem significado de negócio

4. ✅ **Auditoria/Rastreamento necessário**
   - "Quem alterou este item?"
   - "Quando este item foi adicionado?"
   - Logs de mudanças específicas

### Use **VALUE OBJECT** quando:

1. ✅ **Descritivo, sem identidade própria**
   - Endereço: "Rua X, 123" é só um endereço
   - Money: R$ 50,00 é apenas um valor
   - CustomerInfo: Nome e telefone descrevem o cliente

2. ✅ **Completamente substituível**
   - Trocar endereço antigo por novo completo
   - Substituir valor monetário
   - Sem necessidade de "atualizar parcialmente"

3. ✅ **Imutável por natureza**
   - Não faz sentido "mudar parte do valor"
   - Cria-se um novo VO com novo valor

4. ✅ **Comparação por valor faz sentido**
   - Dois endereços iguais SÃO o mesmo endereço
   - R$ 10,00 = R$ 10,00 sempre

---

## 📊 Exemplos do Sistema de Delivery

### ✅ ENTIDADES FILHAS (Correto)

#### 1. OrderItem
```csharp
public sealed class OrderItem : Entity
{
    public Guid Id { get; } // ✅ NECESSÁRIO
    
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    
    public void UpdateQuantity(int newQuantity) 
    { 
        Quantity = newQuantity; 
    }
}
```

**Por quê?**
- ✅ Precisa ser rastreado: "alterar quantidade do item #123"
- ✅ Pode ser modificado individualmente
- ✅ Ordem dos itens no pedido importa
- ✅ Auditoria: "item foi adicionado às 14:30"

#### 2. CashDeposit
```csharp
public sealed class CashDeposit : Entity
{
    public Guid Id { get; } // ✅ NECESSÁRIO
    
    public decimal Amount { get; private set; }
    public DateTime DepositedAt { get; private set; }
    public string? Notes { get; private set; }
}
```

**Por quê?**
- ✅ Cada depósito é um evento rastreável
- ✅ Necessário para auditoria: "quem fez este depósito?"
- ✅ Histórico temporal: "depósito das 10h da manhã"

---

### ✅ VALUE OBJECTS (Correto)

#### 1. Money
```csharp
public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    
    // ❌ SEM ID
    // ✅ Imutável (record)
}
```

**Por quê?**
- ✅ R$ 50,00 não tem identidade - é apenas um valor
- ✅ Dois "R$ 50,00" são exatamente iguais
- ✅ Não faz sentido "atualizar parte do dinheiro"

#### 2. DeliveryAddress
```csharp
public sealed record DeliveryAddress
{
    public string Street { get; init; }
    public string Number { get; init; }
    
    // ❌ SEM ID
    // ✅ Imutável (record)
}
```

**Por quê?**
- ✅ Endereço é apenas descritivo
- ✅ "Rua X, 123" não precisa de identidade
- ✅ Troca-se o endereço inteiro, não partes dele

#### 3. CustomerInfo
```csharp
public sealed record CustomerInfo
{
    public string Name { get; init; }
    public PhoneNumber? Phone { get; init; }
    
    // ❌ SEM ID
    // ✅ Imutável (record)
}
```

**Por quê?**
- ✅ Nome e telefone apenas descrevem o cliente
- ✅ Cliente (entidade) existe, mas seu "nome" é apenas VO
- ✅ Substituível: muda nome completo, não partes

#### 4. PaymentBreakdownItem
```csharp
public sealed record PaymentBreakdownItem(
    string Method, 
    decimal Amount, 
    int Count
);
```

**Por quê?**
- ✅ Apenas estatística/resumo
- ✅ "Pix: R$ 150,00 (3 vendas)" é só um dado agregado
- ✅ Não precisa ser rastreado individualmente

---

## ⚠️ Casos Que Geram Confusão

### Caso 1: "Mas OrderItem não existe sem Order!"

**Resposta**: Isso NÃO faz dele um Value Object!
- Entidade Filha também não existe fora do agregado
- A diferença é que precisa de **identidade para rastreamento**

```
Entidade Filha: Existe DENTRO do agregado, COM identidade
Value Object: Existe DENTRO do agregado, SEM identidade
```

### Caso 2: "CashDeposit poderia ser VO?"

**Não!** Porque:
- ❌ Cada depósito é um evento único no tempo
- ❌ "Depósito das 10h" ≠ "Depósito das 14h" (mesmo valor)
- ✅ Necessita rastreamento para auditoria
- ✅ Pode ter observações/notas modificadas

### Caso 3: "E se OrderItem fosse imutável?"

Ainda seria Entidade Filha porque:
- A **necessidade de identidade** é o critério principal
- Imutabilidade é implementação, não conceito
- Mesmo imutável, precisa ser rastreado individualmente

---

## 🎯 Checklist de Decisão

```
┌─────────────────────────────────────────────────────────┐
│ Entidade Filha SE:                                      │
├─────────────────────────────────────────────────────────┤
│ [ ] Precisa ser rastreado individualmente?              │
│ [ ] Pode ser modificado/removido especificamente?       │
│ [ ] Ordem/posição tem significado?                      │
│ [ ] Auditoria/histórico necessário?                     │
│ [ ] Tem ciclo de vida próprio dentro do agregado?       │
│                                                          │
│ SE 2+ RESPOSTAS "SIM" → ENTIDADE FILHA                  │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ Value Object SE:                                        │
├─────────────────────────────────────────────────────────┤
│ [ ] Apenas descritivo (sem identidade)?                 │
│ [ ] Completamente substituível?                         │
│ [ ] Comparação por valor faz sentido?                   │
│ [ ] Imutável por natureza do negócio?                   │
│ [ ] Dois com mesmos valores são idênticos?              │
│                                                          │
│ SE 3+ RESPOSTAS "SIM" → VALUE OBJECT                    │
└─────────────────────────────────────────────────────────┘
```

---

## 📐 Padrões de Implementação

### Entidade Filha ✅
```csharp
public sealed class OrderItem : Entity  // ← Herda de Entity
{
    public Guid Id { get; }  // ← TEM ID
    
    public Guid ProductId { get; private set; }  // ← Setters privados
    public int Quantity { get; private set; }
    
    private OrderItem() { }  // ← EF Core constructor
    
    public OrderItem(...)  // ← Construtor público com validações
    {
        // Validações
        Quantity = quantity;
    }
    
    public void UpdateQuantity(int newQuantity)  // ← Métodos de comportamento
    {
        // Validações
        Quantity = newQuantity;
    }
}
```

### Value Object ✅
```csharp
public sealed record Money  // ← Record (imutável)
{
    public decimal Amount { get; init; }  // ← Init-only
    public string Currency { get; init; }
    
    // ❌ SEM ID
    // ❌ SEM setters
    
    public Money(decimal amount, string currency = "BRL")  // ← Construtor com validações
    {
        if (amount < 0)
            throw new ArgumentException("Valor não pode ser negativo");
        
        Amount = amount;
        Currency = currency;
    }
    
    public Money Add(Money other) => new(Amount + other.Amount);  // ← Retorna NOVO VO
}
```

---

## 🚫 Anti-Padrões

### ❌ ERRADO: Value Object com ID
```csharp
public sealed record Money
{
    public Guid Id { get; init; }  // ← NUNCA!
    public decimal Amount { get; init; }
}
```

### ❌ ERRADO: Entidade Filha sem ID
```csharp
public sealed class OrderItem : Entity
{
    // ❌ Sem Id próprio - como rastrear este item?
    public int Quantity { get; private set; }
}
```

### ❌ ERRADO: Entidade Filha com repositório
```csharp
public interface IOrderItemRepository  // ← NUNCA!
{
    Task<OrderItem> GetByIdAsync(Guid id);
}
```

### ❌ ERRADO: Value Object mutável
```csharp
public sealed record Money
{
    public decimal Amount { get; set; }  // ← set público - ERRADO!
}
```

---

## 📚 Referências Rápidas

| Característica | Entidade Raiz | Entidade Filha | Value Object |
|----------------|---------------|----------------|--------------|
| Tem ID próprio | ✅ Sim | ✅ Sim | ❌ Não |
| Tem repositório | ✅ Sim | ❌ Não | ❌ Não |
| Existe independente | ✅ Sim | ❌ Não | ❌ Não |
| É mutável | ✅ Pode | ✅ Pode | ❌ Não |
| Comparação | Por ID | Por ID | Por valor |
| Exemplo Delivery | Order | OrderItem | Money |

---

**Conclusão**: 
- **OrderItem é Entidade Filha** ✅
- Não confundir "não existe fora do agregado" com "não tem identidade"
- Identidade ≠ Existência Independente

**Data**: 16 de dezembro de 2025
