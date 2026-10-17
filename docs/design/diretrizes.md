# Diretrizes visuais

Devlivery é a ferramenta de trabalho de estabelecimentos de delivery: pedidos, produtos, caixa e despesas. A interface é precisa e rápida de operar pelo teclado, escura por padrão e com tema claro completo. Superfícies em cinza, bordas finas e uma única cor forte: o laranja, que marca ação e o que está em andamento.

O app é feito com **shadcn/ui no estilo base-vega** (Base UI por baixo). As cores usam os nomes de variável do shadcn (`background`, `primary`, `muted`...), e o que o shadcn não tem entra como extensão no mesmo padrão (`primary-soft`, `success`, `status-*`). O tema já está aplicado em `apps/web/src/index.css`. O mapa de componentes está em [shadcn.md](shadcn.md) e os padrões de cada tipo de tela em [padroes.md](padroes.md).

## Princípios

- **Uma cor forte.** `primary` aparece no botão primário, no status Em preparo, no item selecionado e na série principal dos gráficos. Todo o resto é escala de cinza. `success`, `info`, `warning` e `destructive` existem só como significado (status, variação, erro), nunca para enfeitar.
- **shadcn primeiro.** Botão, campo, select, diálogo, tabela, menu, abas: sempre o componente do shadcn, com o tamanho dele. Só é próprio o que é do domínio do delivery: card e linha de pedido, ícone de status, indicador, barra do celular e comanda.
- **Densidade com respiro.** Telas próprias usam texto de 13px (`ui`) e linhas de 44px; componentes do shadcn ficam em 14px (`text-sm`). O espaço vem de margens consistentes (`space-6` na página, `space-5` entre painéis).
- **Profundidade por superfície.** `background` → `card` → `muted` → `accent`: quanto mais clara (no escuro), mais à frente. Bordas `border` de 1px; sombra só no que flutua (`shadow-pop`).
- **Teclado primeiro.** Toda ação frequente tem atalho visível: `C` novo pedido, `P` marcar pronto, `E` marcar entregue, `Ctrl K` busca, `Ctrl Enter` criar e imprimir.
- **Status pela forma.** O ícone de status enche conforme o pedido avança (`StatusBadge`), então a cor nunca é a única pista.

## Conteúdo e tom

- Português do Brasil, direto. Botões são verbos: "Novo pedido", "Marcar pronto", "Fechar caixa".
- Caixa de frase em tudo: títulos, botões, rótulos, cabeçalhos de tabela. Sem caixa alta.
- Dinheiro `R$ 1.234,50` (`formatCurrency` / `shared/utils/money.ts`), sempre com dígitos tabulares; hora `19:42`; pedido `#46` (sequencial do dia); data `qui, 15/10/2026`.
- Erros dizem o que fazer ("Informe o valor recebido"). Estados vazios convidam à ação ("Nenhum pedido pendente. Novo pedido").
- Sem emoji, sem "Bem-vindo", sem frases de marketing dentro do produto.

## Cor e temas

- **Dois temas completos.** Escuro é o padrão do produto (operação à noite, tela ligada o turno todo); claro tem os mesmos papéis e o mesmo contraste. O usuário escolhe na sidebar; o app guarda a escolha. No código: claro em `:root`, escuro em `.dark`, como o shadcn.
- Texto `foreground` em qualquer superfície; secundário `muted-foreground` (≥4.7:1 em todas, nos dois temas).
- Botão primário `primary` com texto `primary-foreground` (escuro nos dois temas). Branco sobre o laranja não passa contraste. Laranja como texto ou link: `primary-text`. Seleção: `primary-soft`.
- Bordas: `border` decorativa; `input` em campos e controles (≥3:1). Foco: `ring`.
- Status do pedido: Pendente `status-pending`, Em preparo `status-preparing` (o próprio `primary`), Pronto e Saiu para entrega `status-ready`/`status-out` (`info`), Entregue `status-delivered` (`success`), Cancelado `status-canceled`.
- Formas de pagamento são texto `muted-foreground` com ícone (ícone da forma de pagamento), sem cor própria.
- Gráficos: `chart-1` (laranja) na série principal, `chart-2` (cinza) na comparação, grade `border`. Receita prevista em `chart-1` tracejado.

## Tipografia

- Geist em toda a interface, com dígitos tabulares ligados (`font-feature-settings: "tnum"`).
- `title-xl` (24px, 600) para o título da página; `title-lg` (17px) para painéis e diálogos; `metric` (28px) para valores de indicador.
- `ui` (13px, 500) nas telas próprias; `body` (14px) é o `text-sm` dos componentes do shadcn; `meta` (12px) para metadados.
- Geist Mono só nas teclas de atalho (`Kbd`).

## Forma e layout

- Raios do shadcn: `radius` = 0.625rem; `radius-md` (8px) em controles, `radius-lg` (10px) em painéis e cards, `radius-xl` (14px) em diálogos. Nada acima disso.
- Controles do shadcn base-vega: botão e campo com 36px, `sm` com 32px. Barras de ferramentas e cards de pedido usam `sm`.
- App: sidebar de 240px em `sidebar` (a cor do fundo, separada por `border`), conteúdo em `background` com painéis em `card`. Item ativo da navegação em `sidebar-accent`.
- Topo de cada tela: título à esquerda; à direita, busca (`Ctrl K`) e a ação primária. Abaixo, uma barra de visualização: abas, filtros em botões `ghost`, alternância Lista/Quadro.
- No celular (abaixo de 768px), a sidebar vira barra inferior de 5 posições (detalhes em Padrões de tela).

## Estados e interação

- Hover: superfície um degrau acima (`muted`, depois `accent`). Selecionado: `accent` (navegação) ou `primary-soft` (pedido, linha).
- Foco: anel 2px `ring`, offset 2px.
- Transições de 100ms só em cor e fundo. Sem animação de entrada, sem skeleton colorido.

## Iconografia

- Lucide, traço 1.5px, 16px na interface e 14px em linhas e cards, na cor do texto ao lado (`muted-foreground` por padrão).
- Ícones de status e pagamento são os do sistema (ícone de status e ícone da forma de pagamento).
- Logotipo: "Devlivery" em Geist 700 com o ponto final em `primary`.

## O que evitar

Pílulas coloridas de status ou pagamento, ícones dentro de círculos coloridos, gráficos de área em verde e vermelho, gradientes e brilhos, glassmorphism, cantos acima de 14px, sombras difusas em cards, título "Bem-vindo", texto em caixa alta, botão "Excluir" vermelho em destaque, cores fixas no código em vez das variáveis.
