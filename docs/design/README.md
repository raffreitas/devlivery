# Design do Devlivery

Referência visual da V2 do `apps/web`. Esta pasta guarda regras de aparência e interação e as telas que guiam a migração; protótipos não representam funcionalidade já implementada.

Acompanhe o trabalho no [épico da V2 #52](https://github.com/raffreitas/devlivery/issues/52), na [milestone v2.0.0](https://github.com/raffreitas/devlivery/milestone/1) e no [Devlivery Kanban](https://github.com/users/raffreitas/projects/1). O antigo `backlog-v2.md` foi migrado para issues com critérios de aceite e dependências. A fila e o status não são mantidos em Markdown.

| Referência | Para que serve |
| --- | --- |
| [diretrizes.md](diretrizes.md) | Princípios, cores e temas, tipografia, layout e estados. |
| [padroes.md](padroes.md) | Navegação, diálogos, busca, detalhe, avisos, tabelas e comanda. |
| [shadcn.md](shadcn.md) | Uso dos componentes shadcn e dos componentes próprios. |
| [componentes/](componentes/) | Especificação dos componentes de domínio. |
| [telas/](telas/) | PNGs e HTMLs de referência, com vínculos para as issues relacionadas. |
| [Planejamento do produto](../product/planning.md) | Onde ficam prioridades, escopo, decisões e critérios de conclusão. |

## Uso das referências

Implemente com os componentes e contratos reais do aplicativo. O HTML contém exemplos e nomes que podem divergir dos enums e endpoints atuais; não copie essas estruturas diretamente. Vincule cada PR à issue correspondente e inclua a validação visual de desktop/celular e dos dois temas.

A operação de pedidos sem caixa aberto é uma decisão de negócio vigente, inclusive na entrega. Avisos ou controles dos protótipos que bloqueiem essa operação estão superados. Não introduza a antiga regra 1.4.

Preserve pagamentos mistos, troco, observações distintas e preços históricos na migração. Funcionalidades futuras representadas nos desenhos só entram quando sua issue for implementada; não criar APIs ou regras implicitamente para reproduzir um mockup. O recorte da release e a ordem de entrega são mantidos em #52.

Atualize regras e referências de design junto com a mudança correspondente; registre novas pendências em issues.
