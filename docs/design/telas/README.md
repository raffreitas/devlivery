# Telas de referência

Cada tela contém um PNG de aparência e um HTML estático de estrutura/textos. O HTML é referência, não código para copiar: reconstrua com os componentes do app e os tokens de `apps/web/src/index.css`. Os dados são exemplos; `data-shadcn` indica componentes de referência.

A fonte de verdade para escopo, prioridade, dependências e status é o [épico #52](https://github.com/raffreitas/devlivery/issues/52). Esta tabela apenas relaciona arquivos de design às issues; não representa uma fila de tarefas nem comprova implementação.

| Tela / arquivo base | Área atual | Issues relacionadas |
| --- | --- | --- |
| `Sidebar`, `MobileNav`, `MMais` | `shared/components` | [#54](https://github.com/raffreitas/devlivery/issues/54) |
| `Pedidos`, `MPedidos`, `MPedidoDetalhe` | `features/orders` | [#55](https://github.com/raffreitas/devlivery/issues/55), [#57](https://github.com/raffreitas/devlivery/issues/57), [#59](https://github.com/raffreitas/devlivery/issues/59), [#61](https://github.com/raffreitas/devlivery/issues/61) |
| `NovoPedido`, `NovoPedidoWhatsApp`, `MNovoPedido` | `features/orders` | [#56](https://github.com/raffreitas/devlivery/issues/56), [#58](https://github.com/raffreitas/devlivery/issues/58) |
| `MAdicionarProduto` | `features/orders` | [#56](https://github.com/raffreitas/devlivery/issues/56); opções futuras em [#70](https://github.com/raffreitas/devlivery/issues/70) |
| `Entrega` | `features/orders` | [#60](https://github.com/raffreitas/devlivery/issues/60), [#61](https://github.com/raffreitas/devlivery/issues/61) |
| `Comanda` | `features/orders` | [#62](https://github.com/raffreitas/devlivery/issues/62); confirmar largura física antes de adotar 80 mm |
| `Caixa`, `CaixaFechado`, `CaixaHistorico` | `features/cash` | [#63](https://github.com/raffreitas/devlivery/issues/63), [#3](https://github.com/raffreitas/devlivery/issues/3), [#29](https://github.com/raffreitas/devlivery/issues/29) |
| `Produtos` | `features/products` | [#64](https://github.com/raffreitas/devlivery/issues/64); opções e custos futuros em [#70](https://github.com/raffreitas/devlivery/issues/70) e [#73](https://github.com/raffreitas/devlivery/issues/73) |
| `Despesas`, `DespesaNova`, `DespesaEditar` | `features/expenses` | [#65](https://github.com/raffreitas/devlivery/issues/65); recorrência e classificação futuras em [#72](https://github.com/raffreitas/devlivery/issues/72) e [#74](https://github.com/raffreitas/devlivery/issues/74) |
| `Dashboard` | `features/dashboard` | [#66](https://github.com/raffreitas/devlivery/issues/66); análises posteriores em [#75](https://github.com/raffreitas/devlivery/issues/75) |
| `Login`, `NaoEncontrada`, `PaginaProduto` | auth, shared/pages e landing | [#54](https://github.com/raffreitas/devlivery/issues/54) |

## Contratos e regras

Use as rotas reais versionadas em `/api/v1`, os enums da API e testes de contrato. Não use os nomes ilustrativos de endpoints ou métodos de pagamento como especificação.

Os pedidos funcionam com o caixa fechado, incluindo a entrega. A antiga proposta de bloqueio foi substituída por decisão de negócio. Criar, imprimir e reimprimir não pode duplicar pedidos ou recebimentos; abrir um diálogo de impressão não comprova saída física do papel.

As referências visuais permanecem úteis para recursos posteriores, sem obrigar sua inclusão na v2.0.0. Consulte a milestone de cada issue antes de implementar uma parte do desenho.
