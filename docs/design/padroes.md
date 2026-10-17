# Padrões de tela

Como montar os tipos de tela que se repetem no Devlivery. Cada padrão diz o componente do shadcn (base-vega) a usar; a tabela completa está em **shadcn e tema**.

## Navegação no celular

- Abaixo de 768px, a sidebar vira barra inferior com **5 posições fixas**: Pedidos, Caixa, **+** (novo pedido, em `primary`), Resumo e Mais.
- Funcionalidade nova nunca ganha posição na barra: entra no menu Mais, agrupada (Financeiro, Cadastros, Cardápio do cliente, Configurações).
- Sidebar, barra e Mais saem da mesma lista de navegação (`nav-items.ts`, com `{ id, label, icon, href, group, mobile: "bar" | "more" }`).
- Alvos de toque de 44px no mínimo; a ação principal da tela fica fixa no rodapé.
- A barra é componente próprio; detalhes no celular abrem em `Drawer`.

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
