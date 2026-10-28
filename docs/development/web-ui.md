# Componentes de interface do web

O frontend (`apps/web`) usa os componentes oficiais do shadcn/ui no estilo `base-vega`, construídos sobre o [Base UI](https://base-ui.com). Os arquivos de `src/shared/components/ui/` são cópias fiéis do registro; assim, `shadcn add <componente>` continua aplicável sem conflitos.

A configuração fica em `apps/web/components.json`: `style: "base-vega"`, base `base` (Base UI), ícones `lucide`. Confira com `pnpm dlx shadcn@latest info`.

## Adicionar ou atualizar componentes

```powershell
cd apps/web
pnpm dlx shadcn@latest add <componente> --dry-run
pnpm dlx shadcn@latest add <componente> --overwrite --yes
```

Componentes oficiais e composições do projeto importam `cn` diretamente do pacote `cn`, conforme a [recomendação atual do shadcn](https://ui.shadcn.com/docs/changelog/2026-09-cn). Não reescreva esses imports para o helper local nem reinstale `clsx` ou `tailwind-merge`. O arquivo `src/shared/lib/utils.ts` apenas reexporta `cn` para compatibilidade com o alias de utilitários do CLI.

Também remova o `"use client"` que alguns arquivos trazem (o projeto tem `rsc: false`) e rode `pnpm format`.

O preset atual é `b3kbO1glU` (Vega, zinc, orange, Inter). A fonte `Inter Variable` vem do pacote local `@fontsource-variable/inter`; o corpo, os títulos e a landing page usam a mesma família, sem carregar Google Fonts. Após aplicar um preset, normalize também imports e exports com `pnpm exec biome check --write src/shared/components/ui`.

Para textos laranja sobre superfícies do tema, use `text-primary-text`. O token mantém a cor primária no modo claro e usa o laranja legível de `sidebar-primary` no escuro. Preserve `primary`/`primary-foreground` nos botões e demais controles preenchidos. Não use o token de texto sobre superfícies que permanecem brancas no modo escuro.

## Regras

### Espaçamentos

- Preserve os controles oficiais do Vega (36 px de altura), mas use densidade compacta nos formulários: `<FieldGroup className="gap-5 *:data-[slot=field]:gap-2">`, ou seja, 20 px entre campos e 8 px entre label e controle (os valores do estilo Nova). O ajuste fica no consumidor; não altere `ui/field.tsx`.
- O `Modal` usa `gap-4` entre cabeçalho, conteúdo e rodapé e título em `text-base`.
- Use 8 px (`gap-2`) entre ícones/textos e ações relacionadas, 12 px (`gap-3`) em listas simples e 16 px (`gap-4`) entre blocos relacionados.
- Seções de página e grades de cards usam 16 px no mobile e 24 px a partir de `sm` (`gap-4 sm:gap-6`). Grades de indicadores podem usar 12/16 px para agrupar métricas relacionadas.
- Cards seguem o padding oficial: 24 px no tamanho padrão e 16 px com `size="sm"`. Preserve a adaptação 16/24 px nas composições responsivas existentes.
- Defina o espaçamento entre filhos com uma única fonte. `CardContent` já usa `flex` e `gap-3`; use apenas `gap-*` para ajustar essa distância, sem somar `space-y-*` ou margens nos mesmos filhos.
- Modais reservam a linha de rodapé somente quando ele é informado. O padding de 24 px do diálogo e a área de rolagem dos formulários permanecem preservados.

### Composição e manutenção

- Não personalize arquivos de `src/shared/components/ui/`. Adapte os consumidores ou crie um componente composto em `src/shared/components/`, como `option-combobox.tsx` e `modal.tsx`.
- O `biome.json` desliga, apenas para `apps/web/src/shared/components/ui/**`, regras que disparam no código oficial:
  - `a11y/noLabelWithoutControl`
  - `a11y/useKeyWithClickEvents`
  - `a11y/useSemanticElements`
  - `suspicious/noArrayIndexKey`
  - `suspicious/noDoubleEquals`
- O SonarQube exclui `src/shared/components/ui/**` da análise (`sonar.exclusions` em `.github/workflows/web-app-ci.yaml`), pois o código vem do registro. Por isso, `ui/` contém apenas arquivos oficiais; componentes do projeto, como `input-money.tsx`, ficam em `src/shared/components/`.
- Formulários: `Controller` + `Field`, com `aria-invalid` no controle e `data-invalid` no `Field`.
- Gere IDs por instância com `useId()`. Vincule `FieldLabel.htmlFor` ao controle e use `aria-describedby` para as descrições e erros renderizados. Nas linhas dinâmicas, derive os IDs da chave estável do `useFieldArray`.
- Calendários da aplicação usam `AccessibleCalendar`, fora de `ui/`, que encaminha o ref ao `CalendarDayButton` oficial para recuperar o foco por teclado. O `DateRangePicker` também fica fora de `ui/` por ser uma composição do projeto.
- Comboboxes de subcategoria reiniciam por `parentCategoryId`; resultados assíncronos de instâncias desmontadas não podem atualizar a seleção do formulário.
- Ícones dentro de componentes: sem classes de tamanho; use `data-icon="inline-start"` ou `"inline-end"`.
- Carregamento (`src/shared/components/loading/`): `LoadingState` com o skeleton adequado na primeira carga; `LoadingOverlay position="top-bar"` para refetch de página, ou `inline`/`badge` para feedback numa seção; `LoadingButton` em mutations. Refetch nunca bloqueia a UI.
- Toasts: `toast.add({ type, title })` de `@/shared/components/ui/toast`. Erros de mutation já são exibidos pelo `MutationCache` global.
- Rotas: cada página entra em `src/app-routes.tsx` com `lazy`, para virar um chunk próprio. Redirecionamentos durante o render usam `<Navigate>`; chamar `navigate()` no render entra em loop enquanto o chunk da rota carrega.
- Empilhamento: no registro, o viewport do toast e os overlays (Dialog, Drawer) usam `z-50`, e o portal aberto por último fica por cima. `src/index.css` eleva `[data-slot="toast-viewport"]` para `z-index: 100`, mantendo `ui/toast.tsx` intacto.

Referências: [estilos do shadcn](https://ui.shadcn.com/docs/changelog/2025-12-shadcn-create), [componentes Base UI](https://ui.shadcn.com/docs/components/base) e [CLI](https://ui.shadcn.com/docs/cli).

## Verificação de regressões

### Gráficos do dashboard

- Use o `ChartContainer` oficial com altura explícita e componha Recharts com `ChartTooltipContent` e `ChartLegendContent`. A configuração (`ChartConfig`) define os rótulos e referencia variáveis CSS; não personalize `ui/chart.tsx`.
- `src/index.css` mantém os tokens semânticos `chart-sales`, `chart-revenue`, `chart-expense`, `chart-info`, `chart-warning`, `chart-ready` e `chart-neutral`, com cores adaptadas ao tema. Os tokens `chart-1` a `chart-5` continuam pertencendo ao preset. As categorias usam a paleta `chart-category-1` a `chart-category-8`.
- A cor de categoria depende do nome fornecido pela API, preservando-a ao filtrar ou reordenar. A paleta é finita e cores podem se repetir; mantenha todas as categorias na legenda e na tabela de valores. Quando a API fornecer ID de categoria, use-o como identidade estável entre renomeações.
- Receitas e despesas também se distinguem pelo traçado. Preserve `accessibilityLayer` e a opção de abrir os dados em tabela, incluindo valores zero. Os IDs de gradientes vêm de `useId()` por instância.
- Transformações e formatadores compartilhados ficam em `features/dashboard/utils/chart-data.ts`. Dados derivados não precisam de estado ou efeito; o React Compiler já está habilitado.
- `ui/chart.tsx` injeta CSS com a implementação oficial do shadcn. Somente esse arquivo tem a exceção `security/noDangerouslySetInnerHtml` no Biome. Nomes de categorias são codificados antes de entrar nas chaves de `ChartConfig`; rótulos são renderizados como texto React e as cores vêm de tokens internos.
- Ao alterar gráficos, verifique os temas claro/escuro, largura mobile, legendas com mais de seis categorias, tabelas pelo teclado e duas instâncias do gráfico financeiro na mesma tela.

### Fluxos gerais

- Crie uma subcategoria em A, selecione B e confira que a opção de A desapareceu. Ao voltar para A, ela deve aparecer após a atualização das categorias. Confira também o `categoryId` enviado ao salvar a despesa.
- Atrase a resposta de criação e troque a categoria ou feche o formulário antes da conclusão. A resposta antiga não deve mudar a seleção atual.
- Abra o filtro de período com teclado. Confira foco inicial, setas em dias e meses consecutivos, seleção do intervalo e retorno do foco ao fechar, em desktop e mobile.
- Provoque erros nos formulários de login, produtos, despesas, categorias, caixa e pedidos. Cada controle inválido deve referenciar seu erro renderizado; descrições devem permanecer associadas mesmo quando houver erro.
- Abra múltiplas instâncias de um formulário e adicione/remova pagamentos em pedidos: não pode haver IDs duplicados ou associações com outra linha. Ao revisitar um campo inválido com leitor de tela, sua mensagem deve estar disponível.
- Execute `pnpm lint` e `pnpm build` em `apps/web`.
- Execute também `pnpm test`. Testes ficam ao lado do código, usando `.test.ts` sem JSX e `.test.tsx` com JSX. A infraestrutura compartilhada fica em `apps/web/test/`, fora de `src`, com alias `@test/*`. Futuros testes e2e ficam em `test/e2e/`, com configuração própria.

## Dados, salvamento e valores financeiros

- Formulários recebem `onSubmit` assíncrono: resolução indica sucesso, rejeição indica falha. Feche o modal e limpe os campos somente após sucesso. Durante o envio, bloqueie edição, novo envio e fechamento do modal.
- Erros de formulário usam uma lista explícita de controles editáveis ao chamar `applyFormError`, independentemente do valor atual do campo. Mensagens desses controles ficam somente inline; erros gerais e de campos estruturados/ocultos ficam no alerta do formulário. Essas mutations usam `skipGlobalErrorToast`.
- Consultas distinguem carregamento, falha, vazio confirmado e sucesso. `QueryState` e `QueryRegion` são responsáveis pelo feedback, sem toast global duplicado. Falha ao atualizar mantém dados anteriores apenas da mesma chave de consulta; ao trocar filtros, aguarde os dados correspondentes. Somente 404 da sessão ativa significa que nenhum caixa está aberto.
- Use as chaves e invalidações de `shared/services/query-keys.ts`. Cada operação invalida também as consultas de dashboard e caixa que dependem dela. A atualização ocorre em segundo plano e não mantém o formulário bloqueado após uma gravação confirmada. Encaminhe o `AbortSignal` das queries aos serviços HTTP.
- O timeout de 30 segundos se aplica a leituras. Gravações não têm timeout automático do cliente nem retry automático. Uma falha de transporte após envio pode ter resultado desconhecido: informe que é necessário conferir os registros antes de reenviar, sem anunciar que a gravação falhou. Impedir duplicações em qualquer perda de resposta exige idempotência no servidor.
- Some e compare dinheiro em centavos usando `shared/utils/money.ts`; os payloads HTTP permanecem em reais.
- Itens de pedidos usam `unitPrice`, o preço registrado na venda, para apresentação, edição e impressão. `product.price` continua sendo o preço atual do catálogo. Produtos presentes no pedido original conservam o preço ao aumentar quantidade ou remover e readicionar a linha durante a mesma edição; produtos novos recebem o preço atual validado pelo servidor. O seletor também apresenta o preço aplicável ao pedido.
- Consolide linhas de pedido somente quando produto e observações de preparo forem equivalentes. Preserve observações diferentes em linhas separadas, inclusive ao imprimir.
- Pagamentos eletrônicos não podem exceder o total do pedido. A comanda informa valores de todas as formas e o troco previsto. A referência visual usa os últimos oito caracteres do GUID em maiúsculas; o identificador completo continua nas operações da API. Um identificador sequencial próprio fica para uma evolução futura.
- Ao fechar o formulário de pedido com alterações, solicite confirmação de descarte; durante o envio, mantenha o bloqueio de fechamento. Esse mecanismo protege Esc e os controles do modal; não é persistência de rascunho nem bloqueio de navegação entre rotas.
- O estado do caixa é informativo para pedidos. Criação, edição, preparo, cancelamento e entrega não dependem de sessão aberta; desabilite o avanço somente enquanto a requisição estiver em andamento.
