# Padrões de tela

Como montar os tipos de tela que se repetem no Devlivery. Cada padrão diz o componente do shadcn (base-vega) a usar; a tabela completa está em **shadcn e tema**.

## Navegação no celular

- O primeiro bloco implementado usa quatro posições: Pedidos, Caixa, Resumo e Mais. A ação central de novo pedido entra junto da integração do fluxo de criação global; não exibir um botão sem ação.
- Mais (`/more`) reúne Despesas (Financeiro), Produtos (Cadastros), tema e saída. Não exibir cadastros futuros, contadores ilustrativos ou personalização por toque longo.
- Abaixo de 768px, a sidebar vira barra inferior com **5 posições fixas**: Pedidos, Caixa, **+** (novo pedido, em `primary`), Resumo e Mais.
- Funcionalidade nova nunca ganha posição na barra: entra no menu Mais, agrupada (Financeiro, Cadastros, Cardápio do cliente, Configurações).
- Sidebar, barra e Mais saem da mesma lista de navegação (`nav-items.ts`, com `{ id, label, icon, href, group, mobile: "bar" | "more" }`).
- Alvos de toque de 44px no mínimo; a ação principal da tela fica fixa no rodapé.
- A barra é componente próprio; detalhes no celular abrem em `Drawer`.

## Estrutura de navegação implementada

- A composição segue o bloco oficial `sidebar-08` do shadcn (Base UI), com `variant="inset"`: navegação lateral e conteúdo com borda e margem externa no desktop, conforme `telas/Caixa.png`.
- A sidebar aparece a partir de 768px, abre expandida em 240px e permite recolher para ícones com nomes acessíveis e tooltips. A preferência é persistida em `devlivery@sidebar-expanded`; falha no armazenamento não impede navegar.
- Operação reúne Pedidos, Caixa e Resumo; Financeiro reúne Despesas; Cadastros reúne Produtos. `nav-items.ts` é a fonte comum dos destinos e do destaque de rota, incluindo descendentes.
- O topo usa a marca Devlivery: nome e troca de estabelecimento dependem de contratos futuros. O rodapé mostra a sessão de caixa atual (horário de abertura em São Paulo), seguida de avatar com iniciais e nome do operador, com alternância de tema na mesma linha. Sair é um botão ao lado da alternância de tema, separado por uma linha vertical; o nome do usuário não abre menu. Carregamento e falha da consulta são distintos de caixa fechado; o indicador não bloqueia pedidos. Busca global e criação global ficam fora deste bloco.
- As telas atuais continuam dentro do layout, consumindo a API v1; a entrada padrão permanece o dashboard. Login, página pública e 404 mantêm suas telas atuais.

## Cabeçalho das páginas

- `PageHeader` compõe o `Breadcrumb` do shadcn: título, separador `/` e contexto à esquerda; ações disponíveis à direita. O cabeçalho fica no topo da superfície de conteúdo, com uma borda inferior contínua, sem uma segunda faixa global acima.
- A rota de Caixa declara `handle.fullBleed: true` para ocupar a largura da superfície, sem a margem ou o limite de largura das telas ainda não migradas. O corpo da página controla seu próprio padding. As próximas telas podem adotar o mesmo contrato durante a migração.
- O controle de recolher/expandir fica no topo da sidebar. No Caixa, o contexto é Fechado ou Aberto às HH:mm por operador; a data completa de abertura permanece no título descritivo do contexto. Histórico e sangria serão adicionados quando seus fluxos estiverem integrados.

## Diálogo de formulário

Novo pedido, nova despesa, editar despesa, criar categoria.

- `Dialog` sobre a tela de origem, sem trocar de página. Largura de 560px (formulário) a 760px (novo pedido); `radius-xl`.
- Cabeçalho com título `title-lg` e, ao editar, o status do registro logo abaixo.
- Campos com rótulo acima (`Field` + `FieldLabel` + controle), dois por linha quando curtos (valor e vencimento). Densidade compacta: `<FieldGroup className="gap-5 *:data-[slot=field]:gap-2">` (20px entre campos, 8px entre rótulo e controle), como em `docs/development/web-ui.md`.
- Use o `Modal` de `shared/components/` (que compõe o `Dialog`) e as regras de envio do `web-ui.md`: fecha só após sucesso e bloqueia edição durante o envio.
- Rodapé: ação destrutiva à esquerda em `ghost` com texto `destructive` (confirma com `AlertDialog`); à direita, Cancelar e o primário com atalho (`Ctrl Enter`).
- Campo que não pode ser editado no estado atual fica desabilitado com uma frase dizendo como mudar ("Para registrar o pagamento, use Marcar como paga").

## Busca com criação

Categoria, subcategoria, fornecedor, produto.

- `Combobox` do registro (ou o `option-combobox` do projeto) com busca.
- Sem resultado: "Nenhuma subcategoria com “Manutenção”" e, logo abaixo, a opção **Criar subcategoria “Manutenção”** em `primary-text`. Criar abre um diálogo pequeno já preenchido; ao confirmar, volta com o valor escolhido e mostra um toast (`toast.add`).

## Escolha entre poucas opções

Recorrência (avulsa, semanal, mensal), forma de pagamento, Lista/Quadro, Categoria/Subcategoria.

- `ToggleGroup` de seleção única, com uma frase abaixo explicando a consequência ("Recorrente: a próxima é criada quando esta for paga").

## Painel de detalhe

Produto, turno do caixa, reserva.

- No desktop, coluna fixa de 380px à direita, que encolhe a lista em vez de cobri-la; no celular, `Drawer` de baixo para cima.
- Cabeçalho com título e fechar; corpo em seções com rótulo `meta` e pares rótulo/valor alinhados à direita com dígitos tabulares; rodapé com as ações.

## Avisos

- `Alert` em bloco: ícone e borda na cor do significado (`warning`, `destructive`), texto em `foreground`. Ex.: "Sem caixa aberto, os pagamentos de pedidos entregues não são registrados."
- Erro de formulário fica no campo; erro de envio (login inválido, muitas tentativas) fica em `Alert` acima do formulário.

## Tabelas e listas

- `Table` do shadcn para dados tabulares; linhas de 44–48px, valores à direita, colunas de identificação e hora em `muted-foreground`.
- Listas agrupadas (pedidos por status, despesas por natureza): cabeçalho de grupo em `muted`, fixo no topo ao rolar, com contagem e soma.

## Comanda impressa (80 mm)

- Preto no branco, sem cinzas claros; número do pedido grande; itens em negrito com as opções logo abaixo de cada um.
- Duas vias: cozinha (só o que preparar, observações em faixa preta) e entrega (cliente, endereço, pagamento e o troco que o entregador deve levar).
- Pedido alterado depois de impresso sai com a marca "Alterado" e só o que mudou.

## Página pública do produto

- Mesmo tema e mesmas cores do app, escuro por padrão. Títulos maiores (60px no desktop, 38px no celular), texto de 15–18px, largura máxima de 1160px.
- A imagem do produto é a tela real com dados de exemplo, nunca uma ilustração.
- Sem depoimentos, números de clientes ou preço que não existam; perguntas frequentes respondem com o que o produto faz hoje.
