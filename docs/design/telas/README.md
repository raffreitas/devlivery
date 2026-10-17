# Telas de referência

Uma versão por tela: o design final do Devlivery V2. Cada tela tem dois arquivos:

- `.png`: como a tela deve ficar. Use para conferir o resultado.
- `.html`: a mesma tela em HTML estático, para ler estrutura, textos e espaçamentos. Os elementos principais têm `data-shadcn="..."` com o componente a usar (veja [../shadcn.md](../shadcn.md); `Sheet` e `Sonner` no HTML equivalem a `Drawer` e `toast.add`).

O HTML é **referência, não código para copiar**: usa nomes de variável antigos do canvas e componentes de demonstração. Reconstrua com os componentes do app e os tokens de `apps/web/src/index.css`. Os dados são de exemplo.

## Como lidar com o backlog

Algumas telas mostram recursos que ainda não existem na API (coluna "Backlog"). Construa a tela já no layout final e deixe de fora só a parte que depende do item pendente. Exemplos: sem número do pedido até o 2.1, sem a coluna "Saiu para entrega" até o 2.5, sem recorrência até o 3.3. Itens sem mudança de API (1.1, 1.2, 1.3, 1.5, 1.8) podem entrar desde o início.

## Operação

| Tela | Arquivo | Código atual | Backlog |
| --- | --- | --- | --- |
| Pedidos (quadro e lista) | `Pedidos` | `apps/web/src/features/orders` | 1.1, 1.2, 1.4, 2.1, 2.3, 2.5 |
| Novo pedido (diálogo sobre Pedidos) | `NovoPedido` | `features/orders` (`order-form`) | 2.2, 2.3, 2.4 (cliente pelo telefone, opções, zona) |
| Novo pedido ao lado do WhatsApp | `NovoPedidoWhatsApp` | mesmo formulário em painel lateral estreito | 1.3, 2.2, 2.3, 2.4 |
| Entregar e cancelar | `Entrega` | `features/orders` (mudança de status) | 1.6, 2.6 |
| Comanda impressa (80 mm) | `Comanda` | `features/orders` (`order-print`) | 2.3, 2.8 |
| Sidebar | `Sidebar` | `shared/components/sidebar.tsx` | — |

## Dinheiro

| Tela | Arquivo | Código atual | Backlog |
| --- | --- | --- | --- |
| Caixa: contagem cega e sangria | `Caixa` | `features/cash` | 1.5, 2.7 |
| Caixa fechado: abrir turno | `CaixaFechado` | `features/cash` (`open-cash-form`) | — |
| Histórico de turnos | `CaixaHistorico` | `GET /api/cash-register/sessions` (já existe) | 2.7 para a linha de sangrias |
| Dashboard: lucro operacional e pico | `Dashboard` | `features/dashboard` | 3.1, 3.5, 3.6 |

## Cadastros

| Tela | Arquivo | Código atual | Backlog |
| --- | --- | --- | --- |
| Produtos: tamanhos, opções e margem | `Produtos` | `features/products` | 2.3, 3.4 |
| Despesas: natureza, recorrência e retiradas | `Despesas` | `features/expenses` | 3.1, 3.2, 3.3 |
| Nova despesa (interativo: criar subcategoria) | `DespesaNova` | `expense-form`, `create-category-dialog` | 3.2 (fornecedor), 3.3 (recorrência) |
| Editar despesa | `DespesaEditar` | `expense-form` | 3.2, 3.3 |

## Celular

| Tela | Arquivo | Backlog |
| --- | --- | --- |
| Pedidos | `MPedidos` | como `Pedidos` |
| Detalhe do pedido | `MPedidoDetalhe` | 2.5 |
| Novo pedido | `MNovoPedido` | 2.2, 2.3 |
| Adicionar produto (busca, categorias e opções do item) | `MAdicionarProduto` | 2.3 |
| Menu Mais | `MMais` | — |
| Barra inferior (5 posições) | `MobileNav` | — |

A regra da navegação do celular está em `../diretrizes.md` e não depende da API.

## Acesso

| Tela | Arquivo | Código atual | Backlog |
| --- | --- | --- | --- |
| Entrar (normal, erro, muitas tentativas) | `Login` | `features/auth/pages/login-page.tsx` | — |
| Página não encontrada (404) | `NaoEncontrada` | `shared/pages/not-found-page.tsx` | — |

## Público

| Tela | Arquivo | Código atual | Backlog |
| --- | --- | --- | --- |
| Página do produto (substitui a landing `/lp`) | `PaginaProduto` | `features/landing` | — (link de contato pendente) |
