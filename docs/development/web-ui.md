# Componentes de interface do web

O frontend (`apps/web`) usa os componentes oficiais do shadcn/ui no estilo `base-vega`, construídos sobre o [Base UI](https://base-ui.com). Os arquivos de `src/shared/components/ui/` são cópias fiéis do registro; assim, `shadcn add <componente>` continua aplicável sem conflitos.

A configuração fica em `apps/web/components.json`: `style: "base-vega"`, base `base` (Base UI), ícones `lucide`. Confira com `pnpm dlx shadcn@latest info`.

## Adicionar ou atualizar componentes

```powershell
cd apps/web
pnpm dlx shadcn@latest add <componente> --dry-run
pnpm dlx shadcn@latest add <componente> --overwrite --yes
```

Depois de **todo** `add`, corrija o bug do CLI com o alias `@/shared/lib/utils`. O CLI reescreve esse import como o pacote npm `cn` e o instala como dependência:

```bash
sed -i 's#from "cn"#from "@/shared/lib/utils"#' src/shared/components/ui/*.tsx
pnpm remove cn
```

Também remova o `"use client"` que alguns arquivos trazem (o projeto tem `rsc: false`) e rode `pnpm format`.

## Regras

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
- Toasts: `toast.add({ type, title })` de `@/shared/components/ui/toast`. Erros de mutation já são exibidos pelo `MutationCache` global.
- Rotas: cada página entra em `src/app-routes.tsx` com `lazy`, para virar um chunk próprio. Redirecionamentos durante o render usam `<Navigate>`; chamar `navigate()` no render entra em loop enquanto o chunk da rota carrega.
- Empilhamento: no registro, o viewport do toast e os overlays (Dialog, Drawer) usam `z-50`, e o portal aberto por último fica por cima. `src/index.css` eleva `[data-slot="toast-viewport"]` para `z-index: 100`, mantendo `ui/toast.tsx` intacto.

Referências: [estilos do shadcn](https://ui.shadcn.com/docs/changelog/2025-12-shadcn-create), [componentes Base UI](https://ui.shadcn.com/docs/components/base) e [CLI](https://ui.shadcn.com/docs/cli).

## Verificação de regressões

- Crie uma subcategoria em A, selecione B e confira que a opção de A desapareceu. Ao voltar para A, ela deve aparecer após a atualização das categorias. Confira também o `categoryId` enviado ao salvar a despesa.
- Atrase a resposta de criação e troque a categoria ou feche o formulário antes da conclusão. A resposta antiga não deve mudar a seleção atual.
- Abra o filtro de período com teclado. Confira foco inicial, setas em dias e meses consecutivos, seleção do intervalo e retorno do foco ao fechar, em desktop e mobile.
- Provoque erros nos formulários de login, produtos, despesas, categorias, caixa e pedidos. Cada controle inválido deve referenciar seu erro renderizado; descrições devem permanecer associadas mesmo quando houver erro.
- Abra múltiplas instâncias de um formulário e adicione/remova pagamentos em pedidos: não pode haver IDs duplicados ou associações com outra linha. Ao revisitar um campo inválido com leitor de tela, sua mensagem deve estar disponível.
- Execute `pnpm lint` e `pnpm build` em `apps/web`.
