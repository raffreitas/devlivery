# Guia de Organização de Slices - Melhorias Práticas

Este documento mostra como melhorar a organização de cada slice (feature) na arquitetura VSA, focando em simplicidade e clareza.

---

## 🎯 Problema Identificado: CashRegister

### Estrutura Atual (Confusa)

```
CashRegister/
├── Commands/              ✅ OK
├── Queries/              ✅ OK
├── Domain/                ✅ OK
├── DTOs/                  ⚠️ Redundante/Confuso
├── Errors/                ⚠️ Pode ser simplificado
├── EventHandlers/         ⚠️ Pode ser reorganizado
└── Infrastructure/        ✅ OK
```

**Problemas:**
1. **DTOs/** separado cria confusão - Responses já estão nos Commands/Queries
2. **Errors/** como pasta separada para 1 arquivo é over-engineering
3. **EventHandlers/** poderia estar mais próximo do Domain

---

## ✅ Estrutura Recomendada (Simplificada)

### Opção 1: Organização Limpa (Recomendada)

```
CashRegister/
├── Commands/
│   ├── CreateCashSession/
│   │   ├── CreateCashSessionCommand.cs
│   │   ├── CreateCashSessionHandler.cs
│   │   ├── CreateCashSessionEndpoint.cs
│   │   └── CreateCashSessionResponse.cs
│   ├── CloseCashSession/
│   └── CreateCashDeposit/
│
├── Queries/
│   ├── GetActiveCashSession/
│   │   ├── GetActiveCashSessionQuery.cs
│   │   ├── GetActiveCashSessionHandler.cs
│   │   ├── GetActiveCashSessionEndpoint.cs
│   │   └── GetActiveCashSessionResponse.cs    ← Mover DTOs para cá
│   ├── GetCashSessionById/
│   ├── GetCashSessionDeposits/
│   └── GetCashSessions/
│
├── Domain/
│   ├── CashSession.cs
│   ├── CashDeposit.cs
│   ├── CashSessionStatus.cs
│   └── Errors.cs                              ← Mover Errors para cá
│
├── Infrastructure/
│   └── CashSessionRepository.cs
│
└── EventHandlers/                             ← Manter separado (cross-feature)
    ├── OrderCreatedEventHandler.cs
    └── OrderStatusChangedEventHandler.cs
```

**Benefícios:**
- ✅ Menos pastas (de 7 para 5)
- ✅ DTOs/Responses ficam onde são usados
- ✅ Errors ficam no Domain (onde fazem sentido)
- ✅ EventHandlers separados (são cross-feature)

---

### Opção 2: Mais Consolidada (Para Features Simples)

```
CashRegister/
├── Commands/
│   ├── CreateCashSession/
│   ├── CloseCashSession/
│   └── CreateCashDeposit/
│
├── Queries/
│   ├── GetActiveCashSession/
│   ├── GetCashSessionById/
│   ├── GetCashSessionDeposits/
│   └── GetCashSessions/
│
├── Domain/
│   ├── CashSession.cs
│   ├── CashDeposit.cs
│   ├── CashSessionStatus.cs
│   └── CashRegisterErrors.cs                  ← Errors no Domain
│
├── Infrastructure/
│   └── CashSessionRepository.cs
│
└── Events/                                    ← Renomear EventHandlers para Events
    ├── OrderCreatedEventHandler.cs
    └── OrderStatusChangedEventHandler.cs
```

**Benefícios:**
- ✅ Ainda mais simples
- ✅ Nome "Events" é mais claro que "EventHandlers"

---

## 📋 Plano de Refatoração: CashRegister

### Passo 1: Mover DTOs para Queries/Commands

**Antes:**
```
DTOs/
├── CashSessionResponse.cs          ← Usado em queries
├── CashDepositResponse.cs          ← Usado em queries
└── PaymentBreakdownDto.cs          ← Usado em CashSessionResponse
```

**Depois:**
```
Queries/GetCashSessionById/
└── GetCashSessionByIdResponse.cs   ← Renomear e mover CashSessionResponse

Queries/GetCashSessions/
└── GetCashSessionsResponse.cs      ← Criar se necessário

Queries/GetCashSessionDeposits/
└── GetCashSessionDepositsResponse.cs ← Renomear e mover CashDepositResponse
```

**Ação:**
1. Mover `CashSessionResponse` para dentro das queries que o usam
2. Mover `CashDepositResponse` para `GetCashSessionDeposits`
3. `PaymentBreakdownDto` pode ficar em `Domain/` ou junto com a Response que o usa

---

### Passo 2: Mover Errors para Domain

**Antes:**
```
Errors/
└── CashRegisterErrors.cs
```

**Depois:**
```
Domain/
└── CashRegisterErrors.cs
```

**Razão:**
- Errors são parte do domínio
- Não precisa de pasta separada para 1 arquivo
- Mantém Domain completo e autocontido

---

### Passo 3: Renomear EventHandlers (Opcional)

**Antes:**
```
EventHandlers/
├── OrderCreatedEventHandler.cs
└── OrderStatusChangedEventHandler.cs
```

**Depois:**
```
Events/                              ← Nome mais claro
├── OrderCreatedEventHandler.cs
└── OrderStatusChangedEventHandler.cs
```

**Razão:**
- "Events" é mais curto e claro
- Indica que são handlers de eventos (não eventos em si)

---

## 🎯 Padrão Recomendado para Todos os Slices

### Estrutura Padrão (Simplificada)

```
FeatureName/
├── Commands/
│   └── [CommandName]/
│       ├── [CommandName]Command.cs
│       ├── [CommandName]Handler.cs
│       ├── [CommandName]Endpoint.cs
│       └── [CommandName]Response.cs      ← Response junto com comando
│
├── Queries/
│   └── [QueryName]/
│       ├── [QueryName]Query.cs
│       ├── [QueryName]Handler.cs
│       ├── [QueryName]Endpoint.cs
│       └── [QueryName]Response.cs       ← Response junto com query
│
├── Domain/
│   ├── [Entity].cs
│   ├── [ValueObject].cs
│   ├── [Enum].cs
│   └── [Feature]Errors.cs               ← Errors no Domain
│
├── Infrastructure/                      ← Apenas se necessário
│   └── [Feature]Repository.cs
│
├── Events/                              ← Apenas se necessário (cross-feature)
│   └── [Event]Handler.cs
│
└── [Feature]Feature.cs
```

---

## 📊 Comparação: Antes vs Depois

### CashRegister - Antes

```
CashRegister/
├── Commands/          (3 comandos)
├── Queries/           (4 queries)
├── Domain/            (3 arquivos)
├── DTOs/              (3 arquivos) ❌ Redundante
├── Errors/            (1 arquivo)  ❌ Desnecessário
├── EventHandlers/     (2 arquivos)
└── Infrastructure/    (1 arquivo)
```

**Total:** 7 pastas, ~20 arquivos

---

### CashRegister - Depois

```
CashRegister/
├── Commands/          (3 comandos)
├── Queries/           (4 queries)
├── Domain/            (4 arquivos - inclui Errors)
├── Infrastructure/   (1 arquivo)
└── Events/            (2 arquivos)
```

**Total:** 5 pastas, ~20 arquivos (mesma quantidade, melhor organizados)

---

## 🔍 Análise por Feature

### Products (Já está bem organizado!)

```
Products/
├── Commands/
├── Queries/
├── Domain/
├── DTOs/              ⚠️ Poderia ser movido para Queries
└── Infrastructure/
```

**Melhoria Sugerida:**
- Mover `ProductDto` para dentro das queries que o usam
- Ou criar `Queries/Shared/ProductDto.cs` se usado em múltiplas queries

---

### Orders (Bem organizado!)

```
Orders/
├── Commands/
├── Queries/
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Events/         ✅ Events no Domain faz sentido!
└── Infrastructure/
```

**Status:** ✅ **Excelente organização!**

**Observação:** Orders tem subpastas no Domain (Entities, Enums, Events), o que faz sentido para features complexas.

---

## 💡 Regras de Ouro para Organização

### 1. **Responses/DTOs ficam onde são usados**
- ✅ Response do comando → dentro da pasta do comando
- ✅ Response da query → dentro da pasta da query
- ✅ DTO compartilhado → `Queries/Shared/` ou `Domain/`

### 2. **Errors ficam no Domain**
- ✅ Errors são parte do domínio
- ✅ Não precisa de pasta separada para 1 arquivo

### 3. **EventHandlers ficam separados**
- ✅ São cross-feature (reagem a eventos de outras features)
- ✅ Podem ser renomeados para `Events/` para clareza

### 4. **Domain pode ter subpastas se necessário**
- ✅ Features simples: arquivos diretos em `Domain/`
- ✅ Features complexas: `Domain/Entities/`, `Domain/Events/`, etc.

### 5. **Infrastructure apenas se necessário**
- ✅ Se tem Repository → `Infrastructure/`
- ✅ Se não tem → não criar pasta vazia

---

## 🛠️ Checklist de Refatoração

### Para CashRegister:

- [ ] Mover `DTOs/CashSessionResponse` para queries que o usam
- [ ] Mover `DTOs/CashDepositResponse` para `GetCashSessionDeposits`
- [ ] Decidir onde colocar `PaymentBreakdownDto` (Domain ou junto com Response)
- [ ] Mover `Errors/CashRegisterErrors` para `Domain/`
- [ ] Renomear `EventHandlers/` para `Events/` (opcional)
- [ ] Deletar pasta `DTOs/` vazia
- [ ] Deletar pasta `Errors/` vazia
- [ ] Atualizar imports em todos os arquivos
- [ ] Testar que tudo ainda funciona

### Para Products:

- [ ] Avaliar se `DTOs/ProductDto` pode ser movido
- [ ] Se usado em múltiplas queries, criar `Queries/Shared/ProductDto.cs`
- [ ] Se usado apenas em 1 query, mover para dentro dela

---

## 📝 Exemplo de Código Após Refatoração

### Antes (DTOs separados)

```csharp
// DTOs/CashSessionResponse.cs
namespace Devlivery.Features.CashRegister.DTOs;
public sealed record CashSessionResponse(...)

// Queries/GetCashSessionById/GetCashSessionByIdHandler.cs
using Devlivery.Features.CashRegister.DTOs;
var response = CashSessionResponse.FromDomain(session);
```

### Depois (Response junto com Query)

```csharp
// Queries/GetCashSessionById/GetCashSessionByIdResponse.cs
namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;
public sealed record GetCashSessionByIdResponse(...)
{
    public static GetCashSessionByIdResponse FromDomain(...)
}

// Queries/GetCashSessionById/GetCashSessionByIdHandler.cs
namespace Devlivery.Features.CashRegister.Queries.GetCashSessionById;
var response = GetCashSessionByIdResponse.FromDomain(session);
```

**Benefícios:**
- ✅ Tudo relacionado fica junto
- ✅ Menos navegação entre pastas
- ✅ Mais fácil de entender

---

## 🎯 Resultado Final Esperado

### CashRegister (Simplificado)

```
CashRegister/
├── Commands/              ← 3 comandos, cada um autocontido
├── Queries/               ← 4 queries, cada uma autocontida
├── Domain/                 ← Tudo do domínio junto (entities + errors)
├── Infrastructure/        ← Repository
└── Events/                 ← Event handlers (cross-feature)
```

**Características:**
- ✅ Menos pastas (5 vs 7)
- ✅ Mais intuitivo (tudo relacionado junto)
- ✅ Mais fácil de navegar
- ✅ Mantém princípios VSA

---

## 💬 Resumo

**Problema:** Muitas pastas criam confusão

**Solução:** Consolidar e organizar melhor
- DTOs/Responses → dentro de Commands/Queries
- Errors → dentro de Domain
- EventHandlers → renomear para Events (mais claro)

**Resultado:** Estrutura mais limpa, menos confusa, mais fácil de manter

---

**Próximos Passos:**
1. Aplicar refatoração no CashRegister
2. Avaliar outras features (Products, Orders)
3. Documentar padrão final
4. Criar template para novas features

