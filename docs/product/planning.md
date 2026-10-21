# Planejamento e acompanhamento do produto

O trabalho pendente é acompanhado no GitHub. Este documento define como organizar esse trabalho; não mantém uma segunda lista de tarefas.

## Onde acompanhar

- [Devlivery Kanban](https://github.com/users/raffreitas/projects/1): fila, prioridade e estado de execução.
- [Épico da V2](https://github.com/raffreitas/devlivery/issues/52): escopo essencial, dependências e critérios de conclusão.
- [Milestone v2.0.0](https://github.com/raffreitas/devlivery/milestone/1): entregas que compõem a nova versão.
- [Milestone Pós-v2.0.0](https://github.com/raffreitas/devlivery/milestone/2): evoluções posteriores, sem compromisso de versão específica ou data.
- [Issues abertas](https://github.com/raffreitas/devlivery/issues): fonte de verdade para problemas e melhorias ainda não entregues.

## O que fica no repositório

READMEs apresentam o produto e os comandos necessários para trabalhar nele. O modelo de domínio registra regras implementadas; ADRs explicam decisões técnicas duradouras. Guias documentam configuração, testes, publicação e operação. Referências de design descrevem aparência e interação pretendidas, sem representar funcionalidade pronta.

Pendências, propostas, decisões ainda abertas e critérios de aceite ficam em issues. Roadmap, prioridade e status não devem ser copiados para tabelas Markdown. Relatórios de revisão podem permanecer como registros históricos, datados e vinculados às issues; uma observação antiga não substitui uma decisão de negócio posterior.

O antigo `docs/product/backlog.md` foi substituído por issues: o estorno auditável está em [#69](https://github.com/raffreitas/devlivery/issues/69), a separação dos indicadores em [#66](https://github.com/raffreitas/devlivery/issues/66) e a padronização dos contratos em [#67](https://github.com/raffreitas/devlivery/issues/67). Preços históricos, contagem de pedidos, datas das despesas e entrega idempotente já corrigidos devem ser preservados por regressões; não são novas tarefas a implementar.

## Classificação das issues

Use o tipo existente (`bug`, `enhancement` ou `documentation`), a camada afetada (`frontend` e/ou `backend`) e pelo menos uma área: `area:pedidos`, `area:caixa`, `area:financeiro`, `area:catalogo`, `area:clientes`, `area:impressao` ou `area:plataforma`.

Cada issue priorizada tem uma única label de prioridade:

| Prioridade | Critério |
| --- | --- |
| `priority:P0` | Incidente crítico de produção ou perda de dados; interrompe a fila. |
| `priority:P1` | Essencial para a V2 ou para a integridade operacional. |
| `priority:P2` | Evolução importante após o núcleo operacional. |
| `priority:P3` | Expansão ou exploração, sem compromisso de release. |

`epic` agrupa entregas e aponta para issues de execução; não representa uma tarefa pequena. `decision` indica uma regra de produto que precisa ser definida antes da implementação. A milestone define o destino da entrega; não criar labels de versão redundantes. As labels de prioridade são a referência para alinhar o campo Priority do Project.

Prioridade não substitui dependência: uma issue P1 pode estar bloqueada por outra P1. Use referências explícitas no corpo e mantenha as decisões registradas na issue. As prioridades são revisáveis conforme o uso real; não equivalem a prazo ou estimativa de esforço.

## Fluxo do Kanban

- **Backlog:** pendência conhecida, ainda não selecionada ou com dependências/decisões em aberto.
- **Ready:** critérios claros, dependências resolvidas e decisões registradas; pode ser iniciada.
- **In progress:** trabalho efetivamente em execução, com responsável definido; respeitar os limites já configurados no quadro.
- **In review:** implementação submetida à revisão e ligada à PR correspondente.
- **Done:** critérios atendidos e mudança integrada ao destino previsto. Tema/protótipo e commit isolado não comprovam conclusão de uma funcionalidade.

Não mover todo o roadmap para Ready ou In progress. Novas issues entram em Backlog; manter trabalho que já estava em andamento até verificar sua situação. Corrigir duplicidades por vínculo e encerramento justificado, preservando histórico. Rascunhos do Project devem ser convertidos em issues do repositório quando o escopo estiver definido.

## Conteúdo e conclusão

Cada issue descreve o problema, o resultado esperado, critérios verificáveis, dependências e origem da proposta. Mudanças financeiras explicitam a base temporal e o comportamento com e sem sessão de caixa. Operar pedidos sem caixa aberto é uma decisão vigente: criação, preparo, cancelamento e entrega não podem ser bloqueados pela nova UI.

Use os templates para novos bugs e melhorias. Antes de criar uma issue, procure duplicatas. Ao entregar, vincule a PR à issue, registre a validação, atualize a documentação de comportamento implementado e encerre a pendência somente quando seus critérios forem atendidos. PRs para `main` continuam exigindo exatamente um marcador `[Patch]`, `[Minor]` ou `[Major]`; isso não determina a prioridade da issue.
