# Design do Devlivery

Referência visual da V2 do `apps/web`. O tema (cores, raio, fontes) já está aplicado em `apps/web/src/index.css`; esta pasta guarda as regras e as telas que guiam a migração de cada página.

A fonte da verdade é o design system **Devlivery** no Claude (tokens, componentes e textos). Quando ele mudar, atualize esta pasta e o `index.css` no mesmo PR (`docs(design): ...`).

| Arquivo | Para que serve |
| --- | --- |
| [diretrizes.md](diretrizes.md) | Princípios, cor e temas, tipografia, layout, estados, o que evitar. |
| [padroes.md](padroes.md) | Como montar cada tipo de tela: navegação no celular, diálogos, busca com criação, painel de detalhe, avisos, tabelas, comanda e página pública. |
| [shadcn.md](shadcn.md) | Qual componente do shadcn usar em cada parte do design e o que é próprio do Devlivery. |
| [componentes/](componentes/) | Especificação dos componentes de domínio (status, card e linha de pedido, indicador). |
| [telas/](telas/) | Cada tela em PNG (aparência) e HTML estático (estrutura e textos), com o índice de dependências do backlog. |
| [backlog-v2.md](backlog-v2.md) | Melhorias de produto por fase, com mudanças de API e a tela de cada item. |

## Ordem de migração

Uma tela por PR, cada um com print ao lado do PNG de referência:

1. Shell: sidebar, barra do celular e navegação declarada como dados.
2. Componentes de domínio em `shared/components/`.
3. Pedidos, Novo pedido e Comanda.
4. Caixa, Caixa fechado e Histórico de turnos.
5. Produtos e Despesas (lista e formulários).
6. Dashboard, Login, 404 e página do produto.

O que depende de item do backlog ainda não feito na API fica de fora da tela até o item chegar (ex.: número do pedido antes do 2.1).
