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
