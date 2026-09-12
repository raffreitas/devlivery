# ADR-003: Eventos de domínio síncronos na requisição

- **Status:** Aceita
- **Registrada em:** 2026-09-12

## Contexto

Confirmar, cancelar ou excluir pedidos produz efeitos no caixa. Colocar todo o comportamento dentro do agregado de pedido misturaria o ciclo do pedido com o ledger financeiro; introduzir broker e consistência eventual aumentaria a complexidade operacional do produto atual.

## Decisão

Publicar eventos de domínio com o mediator durante `SaveChanges` e processá-los na requisição corrente. Handlers de eventos traduzem fatos do pedido em pagamentos, trocos ou estornos no caixa ativo, usando o tenant e o operador autenticado da requisição.

Os eventos evitam acoplamento direto entre agregados, mas não constituem mensageria durável.

## Consequências

- O chamador recebe sucesso somente depois do processamento síncrono associado.
- Falhas dos handlers podem falhar a operação corrente e aumentam sua latência.
- Não há fila, retry durável ou replay automático.
- Quando não existe caixa ativo, certas movimentações são apenas registradas em log e não são recuperadas posteriormente.
- Migrar handlers para background exigirá transportar tenant e autor explicitamente e adotar idempotência, persistência de eventos e política de retry.
