# Recurso de Controle de Caixa (Cash Register)

## Descrição

Sistema de controle de caixa integrado ao backend, permitindo ao atendente:
- **Abrir caixa** informando valor inicial em dinheiro
- **Adicionar aportes** quando o caixa precisa de dinheiro (para troco, etc.)
- **Acompanhar vendas** do período em tempo real, agrupadas por forma de pagamento
- **Fechar caixa** informando o valor real contado e validando se há sobra/falta

O sistema calcula automaticamente via Backend:
- **Saldo esperado em Dinheiro**: Valor de abertura + Aportes de Dinheiro + Vendas em Dinheiro
- **Diferença**: Valor real informado - Saldo esperado
- **Resumo por pagamento**: Total de vendas em dinheiro, crédito, débito, Pix, etc.

## Estrutura de Arquivos

```
src/features/cash/
├── components/
│   ├── cash-payment-breakdown.tsx      # Card com resumo por forma de pagamento
│   ├── cash-summary-card.tsx           # Card principal com abertura, vendas e fechamento
│   ├── cash-deposit-form.tsx           # Formulário para adicionar aporte
│   ├── cash-deposits-list.tsx          # Lista/histórico de aportes da sessão
│   ├── close-cash-form.tsx             # Formulário para fechar caixa
│   └── open-cash-form.tsx              # Formulário para abrir caixa
├── hooks/
│   └── use-cash-sessions.ts            # React Query hooks (API integration)
├── pages/
│   └── cash-page.tsx                   # Página principal (/cash)
├── services/
│   └── cash-service.ts                 # Cliente API REST
├── types/
│   └── index.ts                        # Tipos TypeScript e Zod schemas
└── README.md                           # Este arquivo
```

---

## Uso

A interação principal é feita através do hook `useCashSessions`, que abstrai as chamadas à API e gerencia o estado de cache via React Query.

### Abrir Caixa

```tsx
const { openCashSession, isOpening } = useCashSessions();

await openCashSession({
  openingAmount: 100.00,
  notes: "Abertura turno noite"
});
```

### Adicionar Aporte

```tsx
const { createDeposit, isCreatingDeposit, currentSession } = useCashSessions();

await createDeposit({
  sessionId: currentSession.id,
  dto: {
    amount: 50.00,
    notes: "Suprimento de troco"
  }
});
```

### Fechar Caixa

```tsx
const { closeCashSession, isClosing, currentSession } = useCashSessions();

await closeCashSession({
  id: currentSession.id,
  dto: {
    closingAmount: 1250.00, // Valor contado fisicamente
    notes: "Fechamento sem divergências"
  }
});
```

---

## Integração Backend

O frontend se comunica com os endpoints `/api/cash-register/sessions`.
Para detalhes sobre a implementação do backend, consulte `docs/cash-register-backend.md` (se disponível) ou o código da API.

### Regra de Conferência

Importante notar que o aplicativo foca na conferência do **Dinheiro**.
O valor "Esperado" exibido na tela de fechamento é a soma de:
1. `Opening Amount`
2. `Deposits` (Aportes)
3. `Cash Sales` (Vendas marcadas como 'money')

Vendas em cartão/PIX são exibidas apenas para conferência de totais, mas não exigem contagem física na gaveta.

---

## Validações

- **Unicidade**: O backend impede a abertura de múltiplos caixas para o mesmo usuário simultaneamente.
- **Valores**: Entradas de valores negativos são bloqueadas pelo frontend e backend.
- **Sequência**: Não é possível fechar um caixa já fechado ou adicionar aportes em sessão encerrada.

---

**Status:** Produção (Integrado com API)
