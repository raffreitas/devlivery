# Componentes: shadcn e Devlivery

O app usa o shadcn/ui no estilo `base-vega` (Base UI). As regras de manutenção dos arquivos de `ui/` estão em `docs/development/web-ui.md`; este arquivo diz qual componente usar em cada parte do design.

## Do registro do shadcn

| No design | Componente | Observação |
| --- | --- | --- |
| Botões | `Button` (`default`, `secondary`, `outline`, `ghost`) | 36px; `size="sm"` (32px) em barras de ferramentas e cards. Destrutivo: `ghost` com `text-destructive`, confirmado por `AlertDialog`. |
| Atalho no botão | `<kbd>` (ou `Kbd` do registro, quando instalado) | Única fonte mono. |
| Campos | `Field`, `FieldLabel`, `Input`, `Textarea`, `InputGroup` | Valor em reais: `input-money` do projeto. |
| Seleção simples | `Select` | |
| Busca com criação | `Combobox` / `option-combobox` | Opção "Criar ..." em `text-primary-text` sem resultado. |
| Datas e períodos | `AccessibleCalendar`, `DateRangePicker` do projeto | Formato "qui, 15/10/2026". |
| Formulários sobre a tela | `Modal` do projeto (sobre `Dialog`) | |
| Confirmação | `AlertDialog` | |
| Detalhe no celular | `Drawer` | No desktop o detalhe é uma coluna fixa. |
| Em aberto / Finalizados | `Tabs` | Contador em `text-muted-foreground`. |
| Lista/Quadro, recorrência, pagamento | `ToggleGroup` | Seleção única. |
| Tabelas | `Table` | |
| Menu da linha, filtros | `DropdownMenu`, `Popover` | |
| Avisos | `Alert` | Cor só no ícone e na borda. |
| Painéis | `Card` | |
| Confirmações rápidas | `toast.add` (`ui/toast`) | Erros de mutation já saem pelo `MutationCache`. |
| Gráficos | `ChartContainer` (`ui/chart`) | Regras em `web-ui.md`, seção Gráficos. |
| Carregando | `LoadingState`, `Skeleton`, `Spinner` | Sem cor. |

Ainda não instalados, adicionar com `pnpm dlx shadcn@latest add <nome>` quando a tela precisar: `kbd`, `command` (busca `Ctrl K`), `switch` (disponível para pedidos), `tooltip`, `empty` (estados vazios).

## Do Devlivery (em `src/shared/components/`)

Componentes que o shadcn não tem, feitos com Tailwind sobre os tokens. As especificações estão em [componentes/](componentes/):

- `StatusIcon` / `StatusBadge`: ícone de status que enche conforme o pedido avança, sempre com texto.
- `OrderCard` e `OrderRow`: card do quadro e linha da lista de pedidos.
- `StatTile`: indicador com rótulo, valor e comparação (substitui o `stat-card`).
- Ícone da forma de pagamento: texto `muted-foreground` com ícone, sem cor própria.
- Barra inferior do celular (5 posições) e comanda de 80 mm.

As pílulas coloridas de status e pagamento em `shared/constants/ui-styles.ts` saem conforme cada tela for refeita.
