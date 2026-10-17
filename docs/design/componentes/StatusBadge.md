# StatusBadge

Status do pedido como ícone de progresso mais rótulo: o círculo vai enchendo conforme o pedido avança.

**Consumidor fornece:** `status` (`pending`, `preparing`, `ready`, `delivered`, `canceled`, o `OrderStatus` da API; `out` = Saiu para entrega, proposto para a V2) e opcional `plain` (sem borda, para tabelas). `children` substitui o rótulo.

- Pendente: círculo tracejado `status-pending`. Em preparo: meio cheio `primary`. Pronto: três quartos `status-ready`. Saiu para entrega: cheio com seta `status-ready`. Entregue: cheio com check `success`. Cancelado: cheio com x `status-canceled`.
- O formato muda junto com a cor, então o status é legível sem depender de cor.
- `StatusIcon` é só o ícone, para listas e cabeçalhos de grupo.
