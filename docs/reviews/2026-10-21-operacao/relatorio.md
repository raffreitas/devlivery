# Validação operacional do Devlivery

Data: 21/10/2026. Ambiente local: frontend `http://localhost:5173`, API `http://localhost:5052`. Perfil utilizado: Atendente, do seeder. A senha não foi incluída neste relatório.

## Parecer

O app permite executar o ciclo básico de atendimento: lançar pedido, corrigir dados, preparar, marcar como pronto, entregar, registrar recebimentos no caixa aberto e fechar o caixa. Catálogo e despesas também funcionaram nos cenários exercitados.

**Eu corrigiria os achados B01, B02 e B03 antes de confiar no app para uma operação diária sem conferência paralela.** Eles podem causar preparo incorreto, venda sem registro no caixa e interpretação errada do resultado financeiro. Também priorizaria B09 antes de disponibilizar exclusão de vendas como forma de correção financeira.

Foram identificados **10 achados**, sendo oito comportamentos observados pela interface/conteúdo de impressão e dois riscos derivados do código. Alguns achados representam decisões de produto que precisam ficar explícitas, e não uma exceção técnica. As melhorias operacionais estão separadas dos bugs.

## Como a validação foi feita

- Navegação real pela interface, com registros fictícios identificados como QA.
- Conferência do estado persistido após criar/editar, navegar entre telas e recarregar a página.
- Comparação de totais do pedido, pagamentos, troco, caixa e dashboard.
- Leitura dos formulários, serviços, regras de domínio, consultas e handlers relacionados aos cenários.
- Execução da suíte existente do frontend: **20 arquivos, 85 testes passaram**.
- Tentativa de execução da suíte da API: compilou API e testes, mas o runner abortou por não conseguir conectar ao testhost em 90 segundos. A tentativa fora do sandbox não foi autorizada. **Resultado da suíte da API: inconclusivo, não aprovado nem reprovado por testes de comportamento.**

Não houve alteração no código de aplicação, instalação de dependências, migração ou exclusão de registros. O trabalho gerou este relatório e seis capturas de tela. As operações na interface deixaram dados QA na base, detalhados ao final.

## Cobertura dos fluxos

| Área | Cenários exercitados | Resultado |
| --- | --- | --- |
| Autenticação | Login com seeder, senha inválida, sair, acesso direto a `/orders` após sair, novo login | Funcionou; senha inválida recebeu mensagem compreensível; rota protegida voltou ao login |
| Criação de pedidos | Cliente/endereço, observação geral, seleção de produto, observação do item, taxa de entrega | Funcionou, com perda de observação em itens repetidos (B01) |
| Validações do pedido | Pedido vazio e pagamentos insuficientes | Bloqueados com mensagens de validação |
| Pagamentos | Dinheiro com troco, dinheiro + Pix, excesso em Pix | Valores normais conferiram; excesso eletrônico aceito (B06) |
| Edição | Correção de endereço após alteração de preço e indisponibilidade do produto | Preservou o preço original de R$ 12,34 e aceitou corrigir o pedido existente |
| Produção | Pendente → Em preparo → Pronto → Entregue | Sequência da interface funcionou; estados terminais retiram ações de edição/avanço |
| Cancelamento | Pedido pendente com confirmação | Cancelou, preservou visualização e não lançou venda no caixa |
| Filtros de pedidos | Status Cancelado, forma Pix e combinação dos dois | Retornaram o pedido esperado; calendário abriu, mas a troca de intervalo não foi exercitada |
| Impressão | Acionamento e inspeção do conteúdo gerado para pedido misto | Conteúdo gerado; valor do Pix omitido (B05); impressão física não validada |
| Catálogo | Campos obrigatórios, criar categoria textual/produto, editar preço, indisponibilizar, buscar pelo nome | Funcionou; produto indisponível saiu das opções de novos itens |
| Caixa | Abrir com R$ 100, aportar R$ 20, vendas, troco, pagamento misto | Dinheiro esperado correto; contagem de pedidos incorreta (B04) |
| Fechamento | Simular falta, informar valor exato e fechar | Alerta de falta correto e fechamento concluído; falta histórico acessível (M03) |
| Despesas | Criar categoria, lançar pendente com vencimento hoje, marcar paga, corrigir descrição/data | Funcionou; status Vence Hoje e passagem para Pago corretos |
| Subcategorias | Criar subcategoria durante edição e salvar na despesa | Persistiu categoria QA Operacao / subcategoria QA Embalagens |
| Filtro de despesas | Selecionar categoria pai com despesa na subcategoria | Incluiu a despesa e resumo de R$ 10 corretamente |
| Dashboard | Vendas, Despesas, Análises, tabelas dos gráficos, efeito do cancelamento | Carregaram; divergência financeira entre componentes (B03/B10) |
| Interrupção da digitação | Preencher nome/endereço, pressionar Esc e reabrir | Dados perdidos sem aviso (B07) |

## Achados e prioridades

P1 = corrigir antes de depender desse comportamento na operação; P2 = corrigir na próxima rodada de qualidade. “Código” significa que a consequência não foi reproduzida por uma operação destrutiva na base.

| ID | Prioridade | Achado | Evidência |
| --- | --- | --- | --- |
| B01 | P1 | Produto repetido sobrescreve observações de preparo | Interface + código |
| B02 | P1 | Entrega com caixa fechado perde o lançamento financeiro | Interface + código |
| B03 | P1 | Cartões e gráficos financeiros usam datas diferentes | Interface + código |
| B04 | P2 | Caixa conta pagamentos como se fossem pedidos | Interface + código |
| B05 | P2 | Comanda omite valor do Pix e não informa troco explicitamente | Conteúdo de impressão + código |
| B06 | P2 | Excesso em pagamento eletrônico vira troco sem validação adequada | Interface + código |
| B07 | P2 | Esc descarta atendimento em digitação sem proteção | Interface + código |
| B08 | P2 | Número impresso do pedido pode se repetir em pedidos próximos | Código |
| B09 | P1 | Exclusão de venda com troco pode descontar o troco duas vezes | Código; precisa teste integrado de reprodução |
| B10 | P2 | “Receita Total” inclui pedidos ainda não recebidos | Interface + código; definição de indicador |

### B01 — Observações diferentes são fundidas no mesmo item

**Reprodução:** adicionar uma Pizza Margherita com “Sem cebola”; adicionar outra Pizza Margherita com “Com cebola”.

**Resultado:** o app produziu uma única linha `2 x Pizza Margherita (Com cebola)`. A primeira instrução foi perdida. O pedido foi salvo assim e continuou assim após navegação.

**Impacto:** o atendente pode copiar corretamente a conversa, mas o sistema encaminha um preparo diferente à cozinha. Pode gerar retrabalho, desperdício e reclamação.

**Causa:** `handleAddItem` procura item existente apenas pelo ID do produto, soma quantidades e substitui `notes` pela última observação não vazia.

**Referência:** [order-form.tsx](../../../apps/web/src/features/orders/components/order-form.tsx), linhas 112–138. [Evidência do pedido persistido](evidencias/04-pedidos-testados.png).

**Correção sugerida:** permitir linhas distintas para o mesmo produto. Consolidar automaticamente somente itens com produto, preço e observações equivalentes. Permitir editar quantidade e observação diretamente na linha.

**Critério de aceite:** duas pizzas com preparos distintos devem continuar distintas na edição, listagem e comanda.

### B02 — Venda entregue com caixa fechado fica fora do caixa

**Reprodução:** com “Caixa Fechado”, criar pedido de R$ 75,00, informar R$ 100,00 em dinheiro e percorrer as etapas até Entregue. Depois abrir o caixa.

**Resultado:** a entrega foi aceita sem alerta financeiro. Ao abrir a sessão QA, o caixa mostrou zero pedidos e zero vendas. A venda anterior não foi recuperada. O pedido permaneceu Entregue.

**Impacto:** receita aparece em pedidos/dashboard, mas não entra no controle de caixa. A abertura posterior não reconcilia o recebimento. O atendente vê sucesso sem saber que perdeu o lançamento.

**Causa:** o handler de pagamento confirmado retorna após escrever um warning quando não encontra sessão ativa. A confirmação do pedido continua válida. O tratamento do troco tem comportamento equivalente.

**Referências:** [OrderPaymentConfirmedEventHandler.cs](../../../apps/api/src/Devlivery/Features/CashRegister/Events/OrderPaymentConfirmedEventHandler.cs), linha 32; [OrderChangeCalculatedEventHandler.cs](../../../apps/api/src/Devlivery/Features/CashRegister/Events/OrderChangeCalculatedEventHandler.cs). O comportamento também é documentado em [domain-model.md](../../product/domain-model.md).

**Correção sugerida:** exigir caixa ativo antes de concluir recebimento/entrega e efetuar tudo atomicamente. Se vender sem caixa for uma capacidade desejada, registrar recebimentos pendentes de conciliação e exibir uma ação clara para reconciliar. Um log isolado não é suficiente para o atendente.

**Critério de aceite:** nenhuma entrega bem-sucedida pode produzir pagamento confirmado sem destino financeiro rastreável.

### B03 — Despesas pagas e lucro divergem entre cartão e gráfico

**Reprodução:** abrir dashboard de 14 a 21/10/2026 e expandir “Ver dados em tabela” de Receitas vs Despesas.

**Resultado observado:** cartão Despesas Pagas de R$ 10,00; gráfico/tabela com R$ 420.021,23 em 17/10 e R$ 10,00 em 20/10. O valor alto já existia na base, com vencimento em 30/10 e pagamento em 17/10. Ele não foi criado nesta validação.

**Causa:** `GetExpenseSummary` filtra por `due_date`; `GetExpensesOverTime` filtra por `payment_date`. O cartão de lucro usa o primeiro total, enquanto o gráfico subtrai o segundo.

**Impacto:** na mesma tela e no mesmo período, o resultado exibido pode variar drasticamente. Isso impede usar os indicadores como base confiável de decisão.

**Referências:** [GetExpenseSummaryHandler.cs](../../../apps/api/src/Devlivery/Features/Dashboard/Queries/GetExpenseSummary/GetExpenseSummaryHandler.cs), linha 34; [GetExpensesOverTimeHandler.cs](../../../apps/api/src/Devlivery/Features/Dashboard/Queries/GetExpensesOverTime/GetExpensesOverTimeHandler.cs), linha 31; [dashboard-page.tsx](../../../apps/web/src/features/dashboard/pages/dashboard-page.tsx). [Captura](evidencias/03-dashboard-divergente.png).

**Correção sugerida:** usar a mesma base temporal para todos os componentes financeiros comparáveis. Manter “contas por vencimento” em indicador próprio, com rótulo explícito. Definir se o painel representa vendas lançadas, vendas concluídas ou dinheiro recebido.

**Critério de aceite:** o total de despesas pagas do cartão deve coincidir com a soma da série do gráfico para o mesmo período.

### B04 — Um pedido com duas formas vira dois pedidos no caixa

**Reprodução:** entregar um pedido de R$ 10,00 com R$ 4,00 em dinheiro + R$ 6,00 em Pix, em sessão inicialmente sem vendas.

**Resultado:** resumo `2 pedidos • R$ 10,00`. Após outra venda, a sessão mostrou três pedidos, embora apenas dois pedidos tivessem gerado as vendas da sessão.

**Causa:** a resposta do caixa ativo calcula `totalPayments - totalRefunds`. O agregado também usa IDs de pagamento para sua contagem, com outra semântica para estornos.

**Referências:** [GetActiveCashSessionResponse.cs](../../../apps/api/src/Devlivery/Features/CashRegister/Queries/GetActiveCashSession/GetActiveCashSessionResponse.cs); [CashSession.cs](../../../apps/api/src/Devlivery/Domain/Aggregates/CashRegister/CashSession.cs), linha 30. [Captura](evidencias/01-caixa-pagamento-misto.png).

**Correção sugerida:** contar pedidos distintos por `RelatedOrderId`, definir como pedidos estornados entram no indicador e compartilhar essa definição entre sessão ativa, detalhe e histórico. Se desejar contar transações, usar esse rótulo.

### B05 — Comanda de pagamento misto não mostra o valor do Pix

**Reprodução:** acionar Imprimir no pedido QA Misto de R$ 10,00.

**Resultado no conteúdo gerado:** “Pix” sem valor e “Dinheiro R$ 4,00”. Os R$ 6,00 do Pix não aparecem. A implementação também não imprime uma linha explícita de troco.

**Impacto:** quem entrega precisa deduzir o valor faltante. Quando existe troco, precisa fazer a conta manualmente. A comanda também não distingue claramente pagamento previsto de pagamento já recebido.

**Referência:** [order-print.tsx](../../../apps/web/src/features/orders/components/order-print.tsx), linha 30. O código condiciona a exibição do valor a `method !== "Pix"`.

**Correção sugerida:** mostrar todas as parcelas, status de recebimento, valor entregue em dinheiro e troco. A informação precisa ser útil antes da entrega, quando o entregador prepara o troco.

**Limite da validação:** o conteúdo de impressão foi inspecionado; impressão física, tamanho do papel, corte e legibilidade em impressora térmica não foram testados.

### B06 — Excesso em Pix é tratado como troco em dinheiro

**Reprodução:** pedido de R$ 10,00; selecionar dinheiro e Pix; informar R$ 1,00 em dinheiro e R$ 20,00 em Pix.

**Resultado:** o formulário exibiu “Troco: R$ 11.00” e aceitou salvar o pedido. Esse pedido foi cancelado antes da entrega, portanto a consequência no saldo não foi reproduzida na sessão QA.

**Impacto:** um erro de digitação em Pix pode orientar devolução em espécie maior que o próprio dinheiro recebido. Não existe fluxo explícito de devolução eletrônica para explicar a intenção.

**Causa:** o schema só garante que a soma cubra o pedido. O domínio calcula troco pela soma geral menos o total; o handler de troco usa dinheiro se houver qualquer parcela positiva em espécie.

**Referências:** [types/index.ts](../../../apps/web/src/features/orders/types/index.ts); [Order.cs](../../../apps/api/src/Devlivery/Domain/Aggregates/Orders/Order.cs), linha 88; [OrderChangeCalculatedEventHandler.cs](../../../apps/api/src/Devlivery/Features/CashRegister/Events/OrderChangeCalculatedEventHandler.cs). [Captura](evidencias/02-troco-excesso-pix.png).

**Correção sugerida:** validar que a parcela eletrônica não exceda o valor a pagar; calcular troco apenas sobre o dinheiro recebido após abater as parcelas eletrônicas. Uma devolução excepcional deve ser uma operação explícita e rastreável.

### B07 — Esc apaga um atendimento em andamento

**Reprodução:** abrir Novo Pedido, preencher cliente/endereço, pressionar Esc e abrir Novo Pedido novamente.

**Resultado:** o modal fechou imediatamente e os campos voltaram vazios, sem aviso de descarte.

**Impacto:** durante alternância entre WhatsApp e app, uma tecla acidental pode obrigar o atendente a reconstruir o pedido. Em horários de pico, aumenta a chance de esquecer um item ou detalhe.

**Referências:** [orders-page.tsx](../../../apps/web/src/features/orders/pages/orders-page.tsx), linha 193; [modal.tsx](../../../apps/web/src/shared/components/modal.tsx).

**Correção sugerida:** confirmar descarte quando houver alterações e preservar rascunho automaticamente por operador. A proteção deve cobrir Esc, botão de fechar e navegação para outra tela.

### B08 — Identificador curto da comanda pode se repetir

**Evidência de código:** `Entity.Id` usa `Guid.CreateVersion7()`. A impressão usa apenas `order.id.slice(0, 8)`.

**Consequência:** os primeiros oito caracteres de um UUID v7 representam parte do timestamp. Pedidos no mesmo bloco de aproximadamente 65,5 segundos compartilham o prefixo. Portanto, esse trecho não é um número de pedido único. Não gerei dois pedidos adicionais só para reproduzir a colisão na base.

**Impacto:** duas comandas podem ter o mesmo “Pedido #” em um período de pico, causando troca de pedidos ou dificuldade para localizar a venda.

**Referências:** [Entity.cs](../../../apps/api/src/Devlivery/Domain/SeedWork/Entity.cs), linha 7; [order-print.tsx](../../../apps/web/src/features/orders/components/order-print.tsx), linha 15.

**Correção sugerida:** gerar número operacional único por estabelecimento, por exemplo sequência diária acompanhada da data. Exibir o mesmo número na listagem, comanda e busca. Manter o UUID completo como identificador técnico.

### B09 — Estorno de pedido com troco pode retirar mais que a venda líquida

**Classificação:** achado de código com alta confiança; não executei exclusão irreversível de venda para reproduzir.

**Caminho observado:** `OrderDeletedEventHandler` estorna o valor bruto de cada pagamento da sessão ativa. O movimento de troco permanece. `TotalRevenue` e `TotalCashPayments` continuam subtraindo esse troco.

**Exemplo derivado do código:** venda de R$ 12,34 com recebimento de R$ 20 e troco de R$ 7,66. A venda líquida é R$ 12,34. Excluir gera estorno de R$ 20, mantendo a saída de R$ 7,66; o conjunto passa a contribuir **-R$ 7,66**, em vez de zero. O efeito não é uma reprodução executada na base.

Existe outro limite: o handler procura pagamentos somente na sessão ativa. Se a venda pertence a uma sessão já fechada, não há correspondência na sessão atual para registrar a compensação.

**Referências:** [OrderDeletedEventHandler.cs](../../../apps/api/src/Devlivery/Features/CashRegister/Events/OrderDeletedEventHandler.cs), linhas 27, 37 e 55; [CashSession.cs](../../../apps/api/src/Devlivery/Domain/Aggregates/CashRegister/CashSession.cs), linhas 25–28 e método `TotalCashPayments`.

**Correção sugerida:** substituir exclusão financeira por estorno explícito com motivo, operador, referência à venda original e compensação líquida correta. Tratar também vendas de sessões encerradas.

**Critério de aceite:** venda + troco + estorno devem zerar receita e impacto líquido em dinheiro; histórico do recebimento original deve continuar consultável.

### B10 — “Receita Total” mistura pedidos abertos com recebimentos

**Resultado observado:** antes do cancelamento do pedido QA Excesso, o dashboard mostrou Receita Total de R$ 241,45; o Resumo de Vendas por formas de pagamento somava R$ 193,45. O pedido QA de R$ 10 ainda pendente já compunha o cartão de receita. Após cancelá-lo, a receita caiu R$ 10 e os recebimentos permaneceram iguais.

**Causa:** `GetDashboardStats` soma todos os pedidos não cancelados; o detalhamento por forma de pagamento considera somente pagamentos confirmados.

**Impacto:** chamar os dois de resumo de receita/vendas sem explicar o regime induz a comparar números diferentes. O cálculo chamado “Lucro Líquido” mistura esse total de pedidos com despesas pagas.

**Referências:** [GetDashboardStatsHandler.cs](../../../apps/api/src/Devlivery/Features/Dashboard/Queries/GetDashboardStats/GetDashboardStatsHandler.cs), linhas 27–30; [GetPaymentBreakdownHandler.cs](../../../apps/api/src/Devlivery/Features/Dashboard/Queries/GetPaymentBreakdown/GetPaymentBreakdownHandler.cs).

**Recomendação:** separar “Pedidos lançados”, “Vendas concluídas”, “Recebido” e “A receber”. Se o indicador pretendido for margem/resultado por outro regime, explicitar sua definição. Esse achado é uma inconsistência de significado operacional, não uma soma aritmética errada.

## Melhorias para o atendimento vindo do WhatsApp

| ID | Prioridade sugerida | Melhoria | Ganho para o dia a dia |
| --- | --- | --- | --- |
| M01 | Alta | Campo de telefone/WhatsApp e reaproveitamento de cliente/endereço | O tipo e o backend suportam telefone, mas o formulário não o oferece. Facilita retorno, evita redigitação e ajuda a distinguir homônimos |
| M02 | Alta | Número operacional visível, busca por cliente/telefone/número e destaque dos pedidos ativos | A listagem atual usa cartões por ordem de criação, sem busca. Em pico, pedidos entregues ocupam espaço e dificultam encontrar os pendentes |
| M03 | Alta | Histórico de caixas com resumo, diferença, movimentos e exportação | Depois de fechar a sessão QA, restou apenas “Nenhum caixa aberto”. O atendente/gestor precisa consultar o fechamento e justificar diferenças depois |
| M04 | Alta | Registrar recebimento separadamente da entrega | Pix antecipado e dinheiro recebido precisam poder ser confirmados quando acontecem. Hoje a confirmação automática está vinculada a Entregue |
| M05 | Alta | Visão de expedição: pronto, saiu para entrega, entregador e horário | “Pronto → Entregue” não distingue pedido aguardando retirada do que está com o entregador. Uma confirmação de recebimento na conclusão reduz cliques incorretos |
| M06 | Alta | Correção financeira com estorno e trilha de auditoria | A interface oferece Excluir em vendas entregues. Para a operação, corrigir/estornar preservando histórico é mais rastreável do que apagar |
| M07 | Média | Editar quantidade/observação de um item sem removê-lo | Hoje a tabela permite remover; para corrigir uma unidade ou preparo, o atendente precisa reconstruir a linha |
| M08 | Média | Tornar observações gerais, parcelas de pagamento e troco acessíveis no detalhe do pedido finalizado | O cartão mostra itens e totais, mas não todos os detalhes. Após entregar, a edição desaparece e a impressão vira caminho indireto para consultar informação |
| M09 | Média | Alertas de pedidos antigos e pendências de outro dia/turno | O filtro inicial usa o dia de criação. Um pedido de antes da meia-noite pode ficar fora da visão de hoje ao reabrir a página; consequência inferida do código, sem teste atravessando a meia-noite |
| M10 | Média | Atualização entre estações e prevenção de sobrescrita concorrente | O hook usa `staleTime`, sem atualização periódica declarada. Avaliar polling/eventos e controle de versão do pedido. Concorrência não foi reproduzida nesta rodada |
| M11 | Média | Histórico de status, autor e motivo de cancelamento | Ajuda a responder quem alterou o pedido, quando saiu da cozinha e por que foi cancelado. A tela atual não oferece essa linha do tempo |
| M12 | Média | Baixa de despesa com data e forma de pagamento explícitas; sangria de caixa | “Marcar como Pago” usou a data atual imediatamente. Para pagamentos retroativos precisei editar depois. Despesa paga em espécie também precisa de uma regra clara de saída do caixa |
| M13 | Baixa | Padronizar idioma e moeda em toda a interface | Calendário mistura textos em inglês e formato `out 21, 2026`; totais intermediários usam `R$ 10.00`, enquanto o restante usa `R$ 10,00` |
| M14 | Média | Preenchimento assistido a partir do texto do WhatsApp | Colar mensagem para sugerir cliente, endereço, itens e observações pode reduzir tempo. Sempre apresentar revisão antes de salvar; não automatizar confirmação de pagamento por texto do cliente |

Não tratei fiado, retirada ou fechamento após meia-noite como regras obrigatórias: você informou que não existem regras específicas adicionais. As sugestões acima dependem do modo de trabalho desejado.

## Ordem recomendada de trabalho

1. Corrigir preservação de observações, lançamento com caixa fechado e consistência dos indicadores (B01–B03).
2. Criar testes de regressão de recebimento/caixa, especialmente pedido misto e estorno com troco (B04/B06/B09).
3. Corrigir comanda e identificação de pedido (B05/B08).
4. Proteger rascunhos e disponibilizar consulta dos caixas encerrados (B07/M03).
5. Melhorar atendimento repetido e expedição: telefone, busca, detalhes, prioridade de ativos e recebimento separado da entrega.

## Regressões que recomendo automatizar

| Cenário | Garantia esperada |
| --- | --- |
| Mesmo produto com observações diferentes | Linhas distintas em criação, atualização e impressão |
| Entrega com caixa fechado | Rejeição atômica ou recebimento explicitamente pendente de conciliação |
| Pedido misto de R$ 4 dinheiro + R$ 6 Pix | Uma venda, um pedido, R$ 4 no dinheiro e R$ 6 no Pix |
| Pedido de R$ 12,34 com R$ 20 em espécie | Troco R$ 7,66 e dinheiro líquido R$ 12,34 |
| Estorno da venda anterior | Receita e dinheiro líquidos zerados, com histórico preservado |
| Pagamento eletrônico acima do total | Bloqueio ou fluxo explícito de devolução; nenhum troco implícito indevido |
| Despesa paga no período e vencida fora dele | Cartão e gráfico financeiros coerentes |
| Esc/navegação em formulário alterado | Proteção de descarte e/ou recuperação de rascunho |
| Dois pedidos criados no mesmo segundo | Números operacionais diferentes |
| Alterar preço e pausar produto após lançar pedido | Edição preserva preço histórico e itens existentes — cenário manual passou |

## Estado final dos dados de teste

Todos os valores abaixo são lançamentos fictícios no aplicativo local. Não houve transferência real, contato com clientes ou envio pelo WhatsApp.

| Registro | Estado final |
| --- | --- |
| QA Operacao 21-10 | Entregue; total R$ 75,00, dinheiro previsto R$ 100; criado/entregue antes da abertura do caixa QA; duas pizzas persistiram com a última observação |
| QA Misto 21-10 | Entregue; total R$ 10,00; R$ 4 dinheiro + R$ 6 Pix; registrado na sessão QA |
| QA Edicao 21-10 | Entregue; total R$ 12,34; R$ 20 em dinheiro; troco R$ 7,66; endereço corrigido; registrado na sessão QA |
| QA Excesso Pix 21-10 | Cancelado; total R$ 10,00; formulário havia aceitado R$ 1 dinheiro + R$ 20 Pix; não foi entregue nem gerou venda na sessão |
| QA Produto Operacao | Indisponível; preço atual R$ 20; preço original do pedido permaneceu R$ 12,34 |
| Categoria de produto QA | Criada para o produto fictício |
| Categoria QA Operacao / subcategoria QA Embalagens | Criadas para classificar a despesa fictícia |
| Despesa QA compra de embalagem - descrição corrigida | Paga, R$ 10,00; vencimento 21/10/2026; pagamento corrigido para 20/10/2026; fornecedor QA Fornecedor |
| Sessão de caixa QA | Fechada; abertura R$ 100 + aporte R$ 20 + vendas em dinheiro líquidas R$ 16,34 = R$ 136,34 conferidos; receita da sessão R$ 22,34 incluindo Pix; contagem exibida incorretamente como três pedidos |
| Rascunho QA Rascunho em atendimento | Não persistido; descartado pelo teste de Esc |

Os registros de teste permanecem para reprodução e conferência. Eles alteram as métricas da base local. Nenhum registro preexistente foi editado ou excluído.

## Limites e próximos cenários de validação

Esta é uma validação exploratória ampla dos fluxos operacionais principais, não uma certificação de todas as combinações de entrada.

Não foram validados: impressão térmica física; dispositivos móveis/viewport pequeno; concorrência entre atendentes; perda real de rede durante gravação; expiração de token durante atendimento; virada de dia em execução; grandes volumes; exclusões irreversíveis; recuperação de backup; isolamento entre estabelecimentos; permissões entre papéis diferentes. Também não foi executada uma auditoria de segurança.

Criação/uso de cartão de crédito e débito compartilham o fluxo e foram analisados no código, mas as vendas manuais completas desta rodada usaram dinheiro e Pix. Os filtros de intervalo foram inspecionados, mas não receberam uma bateria de datas de fronteira. A suíte da API precisa ser executada em um ambiente em que o testhost e os containers de teste funcionem.

## Evidências visuais

1. [Pedido misto contado como dois pedidos](evidencias/01-caixa-pagamento-misto.png).
2. [Excesso em Pix calculando troco](evidencias/02-troco-excesso-pix.png).
3. [Divergência dos indicadores financeiros](evidencias/03-dashboard-divergente.png).
4. [Pedidos QA persistidos e observação consolidada](evidencias/04-pedidos-testados.png).
5. [Fechamento conferido da sessão QA](evidencias/05-fechamento-conferido.png).
6. [Despesa com categoria/subcategoria e filtro aplicado](evidencias/06-despesa-subcategoria.png).
