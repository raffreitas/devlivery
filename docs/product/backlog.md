# Backlog de decisões de produto

Comportamentos encontrados durante a revisão dos testes do backend (outubro de 2026) que funcionam como implementados, mas podem não ser o comportamento desejado. Cada item precisa de uma decisão de produto antes de virar correção.

## 1. Edição de pedido recalcula o preço dos itens

**Hoje:** `PUT /api/orders/{id}` (`UpdateOrderHandler`) recria todos os itens com o preço **atual** do catálogo. Se o preço de um produto mudou depois da criação do pedido, qualquer edição — mesmo só do endereço — altera o total.

**Conflito:** o [modelo de domínio](domain-model.md) diz que o item mantém o preço unitário próprio e que o total não depende do preço atual do catálogo.

**Opções:**

- **(a) Congelar o preço:** itens existentes mantêm o preço original; só itens novos (ou com quantidade alterada, a definir) usam o preço vigente.
- **(b) Manter o recálculo**, assumindo que a edição representa um novo acordo com o cliente, e corrigir o modelo de domínio.

**Impacto no front-end:** com (a), o formulário de edição precisa exibir e somar o preço salvo no item, não o do catálogo — confirmar qual preço `order-form.tsx` usa hoje. Com (b), nenhum.

## 2. Exclusão de pedido entregue

**Hoje:** o front-end oferece "Excluir" para pedidos entregues e cancelados. A exclusão é física e dispara `OrderDeletedEvent`:

- entregue na sessão de caixa **aberta**: os pagamentos são estornados no caixa;
- entregue em sessão **já fechada**: o pedido desaparece, mas a sessão fechada continua contando a receita dele. O Dashboard (que lê a tabela de pedidos) passa a divergir dos totais do caixa.

**Opções:**

- **(a) Bloquear** a exclusão de pedido entregue cuja sessão de caixa já foi fechada (retornar 422).
- **(b) Arquivar** em vez de excluir (exclusão lógica), mantendo o histórico para relatórios.
- **(c) Manter** como está e documentar a divergência.

**Impacto no front-end:** (a) exige apenas exibir o erro 422, que o tratamento global de mutations já faz; talvez esconder o botão quando não puder excluir. (b) muda listagens e filtros de pedidos e do Dashboard. (c) nenhum.

## 3. Entregar um pedido já entregue

**Hoje:** `Order.UpdateStatus(Delivered)` sobre um pedido já entregue não gera erro. Os pagamentos não são confirmados de novo, mas `OrderChangeCalculatedEvent` e `OrderStatusChangedEvent` são emitidos outra vez. Não há dano atual porque o registro de troco no caixa é idempotente, mas um duplo clique em "Marcar como Entregue" gera eventos e atualizações de `UpdatedAt` desnecessários.

**Opções:**

- **(a) No-op silencioso:** ignorar a transição para o mesmo estado.
- **(b) Rejeitar** com 422, alinhado à regra de que pedido entregue não muda de estado.

**Impacto no front-end:** (a) nenhum. (b) um duplo clique passaria a mostrar o toast de erro global; considerar desabilitar o botão enquanto `isUpdatingStatus`.

## 4. Receita do Dashboard inclui pedidos ainda não entregues

**Hoje:** `GET /api/dashboard/stats` e `GET /api/dashboard/sales-over-time` somam o total de todo pedido não cancelado — inclusive pendentes, em preparo e prontos, cujo pagamento ainda não foi confirmado. Já `GET /api/dashboard/payment-breakdown` considera apenas pagamentos **confirmados** (pedidos entregues), descontando o troco.

**Efeito:** na mesma tela, o card "Receita Total" e o gráfico por forma de pagamento mostram valores diferentes; o **lucro líquido** (`receita - despesas pagas`) inclui dinheiro ainda não recebido.

**Opções:**

- **(a) Receita realizada:** considerar apenas pedidos entregues em `stats` e `sales-over-time`.
- **(b) Separar** "receita realizada" e "receita prevista" no contrato.
- **(c) Manter** e documentar que "Receita Total" é receita prevista.

**Impacto no front-end:** (a) nenhum no código, muda os números exibidos. (b) novos campos e um card a mais em `dashboard-page.tsx`.

## 5. Preço exibido nos pedidos vem do catálogo, não do item

**Hoje:** `GET /api/orders` e `GET /api/orders/{id}` devolvem cada item com o `Product` atual (nome, **preço atual**), sem o `UnitPrice` gravado no item. Se o preço do catálogo mudou, a listagem mostra um valor diferente do que compõe o total do pedido. Relacionado ao item 1.

**Opção:** incluir `unitPrice` (e o subtotal) no `OrderItemDto` e usá-lo no front-end.

**Impacto no front-end:** cards, impressão e formulário de edição de pedido passariam a usar o preço do item.

## 6. Contratos inconsistentes entre consultas equivalentes

Comportamentos sem impacto visível hoje, mas que confundem consumidores da API:

- **Pagamentos cancelados:** `GET /api/orders` omite pagamentos cancelados; `GET /api/orders/{id}` os inclui.
- **Total de pedidos do caixa:** a listagem de sessões usa pagamentos distintos (inclui os estornados); a sessão ativa e o detalhe usam `pagamentos - estornos`.
- **Resumo de despesas:** em `GET /api/dashboard/expense-summary`, `total` e `count` incluem despesas canceladas, então `total ≠ paid + pending + overdue`. O front-end não usa esses dois campos.
- **`GET /api/expenses/{id}`:** devolve o status do domínio (`Pending`) em vez do status de exibição (`Overdue`/`DueToday`) e a categoria sem a hierarquia pai/subcategoria usada pela listagem. Não é chamado por nenhuma tela (`useExpense` não tem consumidores).
- **Nome dos filtros de data:** o Dashboard usa `startDate`/`endDate`; pedidos, caixa e despesas usam `start`/`end`.

**Sugestão:** alinhar cada par ao contrato que o front-end já consome, removendo o que não é usado.

## Corrigido durante a revisão

- `GET /api/cash-register/sessions` não carregava as movimentações e devolvia receita, caixa esperado e total de pedidos zerados. Corrigido com o `Include` das movimentações; a listagem ainda não é exibida pelo front-end.
