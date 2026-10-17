# OrderCard

Card de pedido do quadro: número e hora, status e cliente, até três itens, pagamento e total, e a ação do próximo passo.

**Consumidor fornece:** `number` (sequencial do dia), `status`, `customer`, `items` (`[{qty, name, price, note?}]`) e opcionalmente `time`, `address`, `notes`, `deliveryFee`, `payments` (`[{method: "cash"|"credit"|"debit"|"pix", amount}]`), `total`, `maxItems` (padrão 3), `active` e a ação como `children` ou `actions`.

- A ação é sempre o próximo status (mapa de próximo status): "Iniciar preparo", "Marcar pronto", "Marcar entregue", com atalho.
- `elapsed` ("12 min") troca a hora pelo tempo desde a criação; `late` pinta tempo e borda de `destructive` quando passa do limite.
- `items[].options` lista as escolhas do item ("½ Calabresa, ½ Crocante", "+ Palmito") abaixo dele.
- `notes` aparece em `warning`: é o que o entregador ou a cozinha não pode perder.
- `active` = pedido selecionado (fundo `primary-soft`, borda `primary`).
- Largura 288px; no quadro ocupa a largura da coluna.
