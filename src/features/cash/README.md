# Recurso de Controle de Caixa (Cash Register)

## Descrição

POC (Proof of Concept) de um sistema de controle de caixa que permite ao atendente:
- **Abrir caixa** informando valor inicial em dinheiro
- **Acompanhar vendas** do período em tempo real, agrupadas por forma de pagamento
- **Fechar caixa** informando o valor real contado e validando se há sobra/falta

O sistema calcula automaticamente:
- **Saldo esperado**: Valor de abertura + Vendas do período
- **Diferença**: Valor real - Saldo esperado
- **Resumo por pagamento**: Total de vendas em dinheiro, crédito, débito, Pix, etc.

### Características

✅ **Suporte a turnos noturnos**: Permite abrir às 18:00 e fechar às 01:00 do dia seguinte  
✅ **Persistência local**: Dados salvos em `localStorage` (POC)  
✅ **Cálculo automático**: Vendas filtradas pelo intervalo da sessão (não por dia civil)  
✅ **Validação**: Garante apenas 1 caixa aberto por vez  
✅ **UI responsiva**: Funciona em mobile e desktop  

---

## Estrutura de Arquivos

```
src/features/cash/
├── components/
│   ├── cash-payment-breakdown.tsx      # Card com resumo por forma de pagamento
│   ├── cash-summary-card.tsx           # Card principal com abertura, vendas e fechamento
│   ├── close-cash-form.tsx             # Formulário para fechar caixa
│   └── open-cash-form.tsx              # Formulário para abrir caixa
├── hooks/
│   └── use-cash-sessions.ts            # React Query hooks (queries + mutations)
├── services/
│   └── local-cash-store.ts             # CRUD em localStorage (POC)
├── types/
│   └── index.ts                        # Tipos TypeScript
└── README.md                           # Este arquivo
```

---

## Tipos de Dados

### `CashSession`

```typescript
interface CashSession {
  id: string;                           // UUID
  attendant: string;                    // Nome do atendente
  openingAmount: number;                // R$ inicial
  startAt: string;                      // ISO date
  endAt?: string;                       // ISO date (opcional, ao fechar)
  closingAmount?: number;               // R$ real (opcional)
  notes?: string;                       // Observações
  status: "open" | "closed";
  salesTotals: {
    totalRevenue: number;               // Soma das vendas
    totalOrders: number;                // Qtd de pedidos
  };
  paymentBreakdown: PaymentMethodTotal[]; // Vendas por método
}
```

### `PaymentMethodTotal`

```typescript
interface PaymentMethodTotal {
  method: string;                       // "cash", "credit_card", etc.
  amount: number;                       // Total em R$
  count: number;                        // Qtd de pedidos
}
```

---

## Uso

### Abrir Caixa

```tsx
const { openCashSession, isOpening } = useCashSessions();

await openCashSession({
  attendant: "João Silva",
  openingAmount: 100.00,
  notes: "Abertura turno noite"
});
```

### Fechar Caixa

```tsx
const { closeCashSession, isClosing, currentSession } = useCashSessions();

await closeCashSession({
  id: currentSession.id,
  dto: {
    closingAmount: 1350.00,
    notes: "Fechamento normal"
  }
});
```

### Acompanhar Sessão Ativa

```tsx
const { currentSession } = useCashSessions();

if (currentSession) {
  console.log(`Caixa aberto: ${currentSession.attendant}`);
  console.log(`Abertura: R$ ${currentSession.openingAmount}`);
  console.log(`Vendas: R$ ${currentSession.salesTotals.totalRevenue}`);
}
```

---

## Integração no Dashboard

O caixa está integrado na página principal do dashboard (`src/features/dashboard/pages/dashboard-page.tsx`):

1. **Se nenhum caixa estiver aberto**: Exibe banner vazio com botão "Abrir Caixa"
2. **Se caixa estiver aberto**: Exibe
   - Card com resumo (abertura, vendas, saldo esperado)
   - Card com vendas por forma de pagamento
   - Botão "Fechar" no card principal
3. **Modal para abrir**: Solicita nome do atendente e valor inicial
4. **Modal para fechar**: Solicita valor real contado, calcula diferença

### Cálculo Automático de Vendas

As vendas são recalculadas toda vez que um pedido é criado/atualizado enquanto um caixa está aberto.

```tsx
useEffect(() => {
  if (currentSession && orders.length > 0) {
    const sessionStart = new Date(currentSession.startAt);
    const sessionEnd = currentSession.endAt
      ? new Date(currentSession.endAt)
      : new Date();

    // Filtra pedidos dentro do intervalo da sessão
    const sessionOrders = orders.filter((order) => {
      if (order.status === "canceled") return false;
      const orderDate = new Date(order.createdAt);
      return orderDate >= sessionStart && orderDate <= sessionEnd;
    });

    // Calcula totais e atualiza sessão
    // ...
  }
}, [currentSession, orders, updateSessionTotals]);
```

---

## LocalStorage (POC)

### Chave
```
devlivery@cash-sessions
```

### Estrutura
```javascript
localStorage.getItem("devlivery@cash-sessions")
// Retorna:
// [
//   {
//     id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//     attendant: "João Silva",
//     openingAmount: 100.00,
//     startAt: "2025-12-08T18:00:00.000Z",
//     endAt: null,
//     closingAmount: null,
//     notes: "Abertura turno noite",
//     status: "open",
//     salesTotals: { totalRevenue: 1234.50, totalOrders: 42 },
//     paymentBreakdown: [...]
//   }
// ]
```

### Limpeza (apenas para desenvolvimento)

```tsx
import { localCashStore } from "@/features/cash/services/local-cash-store";

localCashStore.clearAll(); // Remove tudo
```

---

## Próximos Passos: Implementação Backend

Quando implementar a API, consulte `docs/cash-register-backend.md` para:
- Modelo de dados no banco
- Endpoints necessários
- Migrations
- Testes de integração

### Passos para Migrar para API

1. Criar endpoints no backend (ver guia)
2. Atualizar `use-cash-sessions.ts` para chamar `api.ts` em vez de `localCashStore`
3. Remover `local-cash-store.ts`
4. Testar integração completa

---

## Validações

### Abertura
- ✅ Nome do atendente obrigatório (até 200 caracteres)
- ✅ Valor inicial deve ser >= 0
- ✅ Validação: não permite abrir se já existe caixa aberto

### Fechamento
- ✅ Valor real deve ser >= 0
- ✅ Calcula e exibe diferença (positiva = sobra, negativa = falta)
- ✅ Validação: não permite fechar um caixa que já está fechado

### Período
- ✅ Suporta sessões que atravessam meia-noite
- ✅ Aviso se sessão estiver aberta há mais de 24h
- ✅ Filtra vendas por `createdAt` dentro do intervalo, não por dia civil

---

## Troubleshooting

### Caixa não aparece ao atualizar página
- Dados são salvos em `localStorage` apenas durante a sessão ativa
- Feche o caixa para salvar os dados finais
- Em produção (com API), será persistido no banco

### Vendas não estão atualizando
- Verifique se há pedidos `completed` no período
- Pedidos `canceled` são ignorados
- Cálculo é automático quando `useOrders` atualiza

### LocalStorage cheio
- `localStorage` tem limite de ~5-10MB por domínio
- POC não é escalável para muitos dados históricos
- Backend resolve este problema

---

## Recursos Relacionados

- `src/features/dashboard/` — Integração no dashboard
- `src/features/orders/` — Dados de pedidos usados no cálculo
- `docs/cash-register-backend.md` — Guia de implementação backend
- `.github/copilot-instructions.md` — Padrões e convenções do projeto

---

**Última atualização:** 08/12/2025  
**Status:** POC com localStorage (pronto para migração backend)
