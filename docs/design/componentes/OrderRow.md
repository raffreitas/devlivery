# OrderRow

Linha de pedido da visão em lista: status, número, cliente e endereço, itens, pagamento, hora e total.

**Consumidor fornece:** as mesmas props do `OrderCard` (sem ação). Coloque as linhas dentro de um contêiner com borda `border` e `radius-lg`, agrupadas por status com um cabeçalho de grupo em `muted`.

- 44px de altura; hover `muted`; selecionada `primary-soft`.
- Itens viram um resumo em uma linha ("2× X-Burger da casa, 1× Batata rústica"), cortado com reticências.
