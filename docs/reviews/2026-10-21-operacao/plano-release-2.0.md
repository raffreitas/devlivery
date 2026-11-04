# Versão atual e release 2.0: comparação e divisão de escopo

Análise de 21/10/2026, cruzando o diff Git com a [validação operacional](relatorio.md).

## Base da análise

| Referência | Commit |
| --- | --- |
| `main` atual | `20e8bde1f9286d0408298390d8ba38b7f47a1479` |
| `release/2.0.0` local | `c00b5f93d943103374aec30cd39f70397d5d988e` |
| `origin/release/2.0.0` disponível localmente | Mesmo commit da branch local |
| Ancestral comum | `b5726792c4cbdfcf41167eabd5e4417f75d75224` |

Foram usados `git log main...release/2.0.0`, `git diff main...release/2.0.0`, comparação das pontas e leitura dos documentos/protótipos com `git show`. Não foi feito fetch; esta análise reflete as referências locais disponíveis. A branch de trabalho continuou em `main`; não houve checkout, merge ou alteração no código da aplicação.

## O que a branch já entrega

A release tem dois commits exclusivos: aplicação do tema e documentação de design. O diff desde o ancestral comum contém **69 arquivos**, com 5.361 inserções e 112 remoções. Grande parte são documentos e referências HTML/PNG.

As mudanças executáveis estão concentradas em:

- `apps/web/src/index.css`: tema claro/escuro, cores, tipografia Geist/Geist Mono, escalas de texto e tokens semânticos.
- `apps/web/src/app.tsx`: tema escuro como padrão.
- `apps/web/src/shared/constants/ui-styles.ts`: cores dos gráficos de status passam a referenciar os novos tokens.
- `apps/web/src/features/dashboard/components/expense-status-chart.tsx`: ajuste da cor de despesa paga.
- Dependências de fontes e lockfile.

**Não há implementação nova dos fluxos de pedidos, caixa, produtos ou despesas, nem mudanças funcionais da API nos dois commits da release.** Os HTMLs são referências; suas ações e valores de exemplo não provam funcionamento integrado. Nenhum dos dez achados B01–B10 foi corrigido por esses commits.

A `main` tem um commit exclusivo: extração do backup para `apps/backup`, com testes. Essa mudança precisa ser incorporada à release para que a versão 2.0 preserve a organização atual. A diferença nas pastas de backup entre as pontas decorre dessa divergência, não de uma intenção da V2 de remover os testes.

## O produto pretendido na 2.0

O contexto de `release/2.0.0:docs/design/backlog-v2.md` é específico: **um atendente, um cozinheiro que trabalha com papel e um entregador**. O objetivo principal é reduzir digitação/cliques e manter o dinheiro confiável.

| Frente | Mudança planejada | Estado no diff |
| --- | --- | --- |
| Identidade visual | Geist, laranja de ação, superfícies discretas, dois temas, densidade e teclado | Tema aplicado; migração das telas ainda pendente |
| Navegação | Sidebar, barra inferior no celular, busca/atalhos e navegação declarada como dados | Documentada/prototipada |
| Operação | Pedidos em lista/quadro, abertos separados de finalizados, atraso e tempo decorrido | Documentada/prototipada |
| Lançamento | Painel ao lado do WhatsApp, criar e imprimir, telefone, repetir pedido e endereços | Documentada/prototipada |
| Cardápio | Tamanhos, meio a meio, acréscimos, custo/margem e zonas de entrega | Backlog; exige modelo e contratos novos |
| Expedição | Saída em lote, status Saiu para entrega, conferir/ajustar pagamentos ao entregar | Backlog e protótipos |
| Comanda | 80 mm, vias cozinha/entrega, troco visível e alteração após impressão | Referência de design |
| Caixa | Contagem cega, sangria, movimentos e histórico de turnos | Referência de design; parte das consultas já existe |
| Financeiro | Receita realizada/prevista, categorias por natureza, retiradas e recorrência | Backlog e protótipos |
| Evolução posterior | Cardápio público, carrinho por WhatsApp, pedido direto, pagamento online, impressão silenciosa | Fase futura explícita |

Com essa escala, retiro da prioridade imediata as sugestões de KDS, múltiplos entregadores, permissões por função e atualização em tempo real. Os próprios documentos definem quando retomá-las. Elas não são pré-requisitos da nova UI.

## Divisão recomendada dos achados

“Agora” significa um patch da versão atual, com teste de regressão e sem reconstruir a interface. “2.0” significa a evolução completa do fluxo. A proteção necessária hoje deve ser incorporada também na branch da release.

| Achado | Fazer agora | Parte que fica para a 2.0 | Relação com o backlog da release |
| --- | --- | --- | --- |
| B01 — Observações sobrescritas | Separar linhas do mesmo produto quando observações diferirem; preservar no envio e impressão | Editor de item com variações/acréscimos e identidade própria das escolhas | Acrescentar critério explícito ao 2.3; o backlog ainda não cobre a perda de observação diretamente |
| B02 — Entrega sem caixa | Rejeitar na API, sem confirmar pagamentos/status; avisar no front | Aviso fixo e ação orientada na nova tela | Antecipar 1.4 |
| B03 — Datas diferentes no financeiro | Unificar a base de data dos totais comparáveis e testar despesa com vencimento fora do período e pagamento dentro | Reorganizar indicadores e filtros no novo dashboard | Adicionar explicitamente à fase 3; 3.5 trata receita, não corrige esse filtro de despesa |
| B04 — Pagamentos contados como pedidos | Contar pedidos distintos e alinhar sessão ativa/detalhe/listagem | Usar o contrato coerente no histórico e no novo caixa | Antecipar a parte financeira de “contratos inconsistentes”; não esperar cada tela ser migrada |
| B05 — Pix/troco ausentes na comanda | Imprimir todas as parcelas e o troco previsto, sem redesenhar o papel | Duas vias, novo layout e impressão de alterações | Correção mínima agora; desenho completo em 2.8 e referência Comanda |
| B06 — Excesso eletrônico vira troco | Validar no front e API; impedir troco implícito por erro na parcela eletrônica | Ajuste de formas/valores na confirmação da entrega | Base obrigatória para 2.6 |
| B07 — Perda ao pressionar Esc | Proteger descarte de formulário alterado | Rascunho recuperável no painel/modal novo | Adicionar ao 1.3 e aos padrões de formulário; não está explicitado |
| B08 — Número impresso repetível | Parar de apresentar o prefixo de oito caracteres como identificador único; usar o ID completo como solução provisória | Número operacional sequencial por estabelecimento/data, com unicidade no banco | 2.1; pode ser antecipado se a migração couber no patch |
| B09 — Estorno bruto mantém saída de troco | Primeiro reproduzir em teste. Como contenção, bloquear na API exclusão de pedido entregue, além de retirar a ação da UI | Estorno explícito e rastreável, inclusive para vendas de sessão encerrada | 1.7 é uma contenção parcial: seu texto só exige bloqueio da API para sessão fechada; ampliar a proteção evita deixar o caso com troco da sessão aberta acessível |
| B10 — Receita mistura previsto/recebido | Corrigir rótulo/explicação e evitar chamar de lucro recebido um cálculo que inclui pedidos abertos | Separar realizada e prevista no contrato, cartões e séries | 3.5 já contempla a evolução; documentar o regime financeiro adotado |

A recomendação é **não postergar nenhum erro já conhecido de integridade para o redesenho**. O que fica para a 2.0 é a solução de produto mais ampla; o patch entrega a correção ou uma contenção clara.

Para B03/B10, minha proposta é que os componentes apresentados como recebimentos/despesas pagas usem o regime de caixa. O painel por vencimento permanece separado. A referência temporal da venda deve ser definida: criação e confirmação podem cair em dias diferentes. A V2 não deve manter essa ambiguidade ao renomear os cartões.

## Como ficam as melhorias do relatório anterior

| Melhoria anterior | Encaminhamento |
| --- | --- |
| M01 — Telefone/reaproveitamento | 2.2; um dos maiores ganhos. Campo de telefone pode existir antes do cadastro completo de clientes |
| M02 — Número/busca/pedidos ativos | 2.1 + nova lista/quadro + busca; validar busca por número, nome e telefone |
| M03 — Histórico de caixa | Já tem referência `CaixaHistorico`; incluir explicitamente na entrega da V2. Consultas existentes permitem começar sem esperar sangria |
| M04 — Recebimento separado da entrega | Ajustar à operação descrita: priorizar conferência e ajuste no retorno do entregador, conforme 2.6. Confirmação independente de Pix antecipado pode evoluir depois; não é requisito presumido |
| M05 — Expedição | 2.5 resolve o status ausente sem precisar cadastrar vários entregadores |
| M06 — Correção com auditoria | Contenção agora + evolução de estorno; motivo de cancelamento em 1.6 não substitui um estorno financeiro |
| M07 — Editar quantidade/observação | Incorporar ao novo editor de item em 2.3 e à migração do formulário |
| M08 — Detalhes depois de finalizar | Incorporar ao detalhe de pedido, mantendo pagamentos, observações e troco consultáveis |
| M09 — Pedidos de outro dia | Nova aba Em aberto deve incluir pedidos ainda ativos de dias anteriores, sem depender do filtro Hoje |
| M10 — Atualização entre estações | Adiar atualização em tempo real até haver dois dispositivos; manter prevenção básica de requisição repetida (1.8) |
| M11 — Histórico/motivo | 1.6 + horários das transições; não presumir múltiplos papéis |
| M12 — Baixa/sangria | Sangria em 2.7; explicitar método/data da baixa de despesas e sua relação com a gaveta |
| M13 — Idioma/moeda | Incorporar como critério transversal da migração visual |
| M14 — Texto do WhatsApp | O backlog escolheu outra estratégia: carrinho com código enviado pelo WhatsApp na fase 4. Priorizar essa estratégia; interpretação livre de mensagens não precisa entrar na 2.0 |

## Ajustes necessários no backlog/design antes de implementar

1. **Marcar 2.9 como implementado, mantendo regressão.** Nas duas branches, `UpdateOrderHandler` preserva os preços gravados, os DTOs têm `unitPrice` e o front o usa. O teste manual anterior confirmou isso. Atualizar também os itens 1 e 5 de `docs/product/backlog.md`, que ainda descrevem comportamento antigo.
2. **Adicionar os casos omitidos:** B01, B03, B04, B06 e proteção de descarte. Uma tela nova não resolve automaticamente essas regras.
3. **Reescrever 1.7:** esconder Excluir não é proteção de API. Bloquear todas as exclusões de entregues é a contenção simples; projetar estorno em item próprio. Arquivar cancelados também exige persistência/filtro, não apenas esconder o cartão.
4. **Manter pagamento misto.** O protótipo Entrega menciona “Pagou com duas formas”, mas Novo pedido enfatiza uma seleção única. A migração precisa preservar dinheiro + Pix/cartão e mostrar valores em ambas as vias apropriadas.
5. **Definir imprimir/preparar e falhas.** Imprimir pelo navegador não confirma que o papel saiu. “Criar e imprimir” deve criar uma venda só; falha/cancelamento da impressão precisa permitir reimprimir sem duplicar pedido. Explicitar quando passa a Em preparo e como registrar reimpressão.
6. **Reavaliar a palavra “cega”.** Ocultar o esperado apenas no modal não produz contagem realmente cega se ele estiver visível ao lado, no resumo ou no histórico da sessão ativa. Para um operador único pode ser só conferência em duas etapas; definir a intenção sem superestimar o controle.
7. **Definir as transições na API.** Acrescentar Saiu para entrega exige validar a máquina de estados, cancelamento e repetição de ação. O front não deve ser o único responsável pela ordem das etapas.
8. **Regra de variantes precisa de snapshot.** Congelar preço também deve valer para tamanho, metades e acréscimos. Renomear/alterar catálogo não pode mudar uma comanda histórica. Decidir a regra de meio a meio antes de construir o modelo.
9. **Recorrência exige idempotência.** Pagar a mesma despesa duas vezes/repetir uma requisição não pode gerar duas próximas ocorrências. Definir se a data seguinte parte do vencimento ou do dia em que foi paga.
10. **Revisar a definição de resultado e retiradas.** O item 3.1 trata pró-labore e retirada dos sócios como equivalentes. Especificar os conceitos do produto e o enquadramento pretendido antes de fazer o dashboard excluir automaticamente uma categoria do resultado. Esta análise de código não determina tratamento contábil/fiscal.
11. **Comanda 80 mm é mudança de compatibilidade.** Hoje o estilo de impressão usa 48 mm. Confirmar o papel/equipamento utilizado e validar impressão física; se necessário, suportar largura configurável.
12. **Padronizar os nomes do contrato.** A referência de `OrderCard` usa métodos `cash/credit/debit/pix`, enquanto a API atual usa `Cash/CreditCard/DebitCard/Pix`; o índice do histórico cita `/api/cash-register/sessions`, enquanto a API atual é versionada em `/api/v1`. Tratar referências como desenho, não copiar esses contratos literalmente.

## Sequência recomendada de entrega

| Etapa | Escopo | Condição de conclusão |
| --- | --- | --- |
| Patch atual — integridade | B01–B06, proteção de B07, contenção de B08/B09 e clareza mínima de B10 | Regressões de domínio/API e front; impressão de pagamento misto; caixa conferido |
| 2.0 — base visual | Shell, navegação, tokens e componentes de domínio | Telas nos dois temas; teclado sem atalhos disparando durante digitação; mobile utilizável |
| 2.0 — atendimento | Lista/quadro, busca, telefone, repetir pedido, formulário/painel, números e detalhes | Lançar pedido realista do WhatsApp sem perder dados ou observações |
| 2.0 — produção/expedição | Criar/imprimir, atraso, saída em lote, confirmação da entrega e comanda | Reimpressão segura; pagamentos mistos e mudança de forma conferidos; cozinha recebe correções |
| 2.0 — caixa | Histórico e movimentos, sangria, fechamento em duas etapas | Saldo e contagem coerentes antes/depois de fechamento; operações rastreáveis |
| 2.0 — financeiro | Realizada/prevista, datas coerentes e categorias definidas | Cartões, tabelas e gráficos reconciliam para o mesmo conceito e período |
| Incrementos posteriores, se necessário | Opções complexas, recorrência, custo/margem, zonas e análises avançadas | Entram por valor operacional; não devem bloquear a estabilização da nova UI |
| Futuro explícito | Cardápio do cliente, código WhatsApp, pedido direto, pagamento online e impressão silenciosa | Conforme as dependências da fase 4 e demanda real |

Tamanhos/metades/acréscimos podem ser parte essencial da 2.0 se hoje exigem contas manuais frequentes. Nesse caso, devem preceder o editor definitivo de itens. Se o catálogo atual já atende, lançar a UI usando o contrato existente e adicionar opções em uma entrega posterior evita transformar o redesenho numa migração de tudo ao mesmo tempo.

## Estratégia entre branches e validação

- Fazer correções pequenas na linha atual e incorporá-las à `release/2.0.0` regularmente, por merge ou cherry-pick controlado; não reimplementar a mesma regra em cada UI.
- Sincronizar também a extração do backup já presente na `main`.
- Migrar uma tela por PR, como o próprio design propõe. Alterações de domínio/contrato podem vir em PRs independentes, com compatibilidade para a UI atual até sua migração.
- Validar primeiro as correções com regressões de pedido misto, troco/estorno, observações distintas e despesa paga fora do mês de vencimento.
- Antes de liberar a V2, exercitar uma jornada: abrir caixa → lançar pedido de cliente recorrente → imprimir → editar/reimprimir → sair para entrega → mudar pagamento → entregar → sangria → fechar → consultar histórico.

Não executei a UI da branch nem seus testes nesta comparação: ela foi lida diretamente pelo Git. As evidências da aplicação rodando e os 85 testes frontend aprovados pertencem à validação da `main` registrada no relatório original.

## Referências na branch analisada

As referências abaixo podem ser consultadas com `git show release/2.0.0:<caminho>`, mesmo com a `main` aberta:

- `docs/design/backlog-v2.md`: fases, escala da equipe e mudanças previstas na API.
- `docs/design/README.md`: ordem de migração e distinção entre referência e implementação.
- `docs/design/diretrizes.md` e `padroes.md`: regras visuais, teclado, formulários e impressão.
- `docs/design/telas/README.md`: mapa das telas e dependências.
- `docs/design/telas/NovoPedidoWhatsApp.html`, `Entrega.html`, `Comanda.html`, `Caixa.html`, `CaixaHistorico.html` e `Dashboard.html`: fluxos representados.
- `apps/api/src/Devlivery/Features/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs`: preço original já preservado.
- `apps/web/src/features/orders/components/order-print.tsx`: implementação atual ainda contém o prefixo curto e a omissão do valor do Pix.
