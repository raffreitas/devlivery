# Correções prioritárias da operação — 21/10/2026

Implementadas na branch `codex/fix-operational-integrity`, criada a partir da `main` (`20e8bde`). A branch `release/2.0.0` não foi alterada. Este documento complementa o [relatório da auditoria](relatorio.md) e o [plano da release](plano-release-2.0.md).

## Escopo entregue

| Achado | Correção agora | Limite / próxima release |
| --- | --- | --- |
| B01 — observações sobrescritas | O mesmo produto mantém linhas distintas quando as observações diferem; observações equivalentes, após remover espaços externos, somam quantidade. | Nova composição visual do pedido fica para a 2.0. |
| B02 — operação sem caixa | Por decisão de negócio, criar, editar, preparar, cancelar e entregar pedidos independe de caixa aberto. Pagamentos confirmados e troco ficam no pedido; com sessão aberta, também são registrados nela. Repetir uma entrega já concluída não gera eventos novamente. | A recomendação inicial de bloquear entrega foi substituída. Abrir caixa não importa vendas anteriores automaticamente. |
| B03 — despesas divergentes | Resumo, categorias e status usam data de pagamento para despesas pagas e vencimento para pendentes, excluindo canceladas. | Separar painéis operacionais e financeiros permanece na 2.0. |
| B04 — pagamentos contados como pedidos | Caixa ativo, detalhe e listagem contam pedidos distintos com pagamento não estornado, inclusive no detalhamento por método. | Um pedido misto pode aparecer em mais de um método; a soma dessas contagens não representa o total de pedidos distintos. |
| B05 — comprovante incompleto | Todos os métodos exibem valores; pagamento misto válido apresenta troco. | Não foi alterada a largura de papel de 48 mm nem criado o novo layout de impressão. |
| B06 — excesso eletrônico virando troco | Frontend e API rejeitam soma de Pix/cartões superior ao total. O formulário pede revisão e o comprovante não orienta troco para registros legados inválidos. | Troco permanece permitido quando o excedente pode ser devolvido em dinheiro. |
| B07 — perda acidental do formulário | Fechar, cancelar ou pressionar Esc pede confirmação quando há alterações ou item em preenchimento; continuar preserva os dados. | Não há rascunho persistente, recuperação após recarregar a página ou proteção de navegação entre rotas. |
| B08 — referência repetida | Comprovante exibe os últimos oito caracteres do GUID em maiúsculas, substituindo o prefixo compartilhado pelos UUIDs v7. | É uma referência visual abreviada; identificador sequencial próprio fica para uma evolução futura. |
| B09 — exclusão com estorno incorreto | Exclusão de pedido entregue é rejeitada pela API e removida da UI, com caixa aberto ou fechado, preservando o histórico financeiro. | Contenção: estorno/devolução auditável será um fluxo próprio na 2.0. |
| B10 — projeção confundida com recebimento | Cards passam a informar “Valor dos Pedidos” e “Saldo Projetado”, com explicação das datas e dos valores ainda não recebidos; gráfico usa “Saldo projetado”. | Não houve mudança do contrato de receitas. Separar vendas, recebimentos e resultado financeiro fica para a 2.0. |

Nenhuma migração de banco, dependência, tema ou nova navegação foi introduzida. Documentação de domínio, UI e backlog foi atualizada.

Decisão de negócio confirmada: o Devlivery deve operar normalmente mesmo sem caixa aberto, inclusive na entrega. O caixa é um controle opcional de sessão; seu saldo não representa todas as vendas do estabelecimento. Recebimentos fora de sessão continuam persistidos no pedido e aparecem no resumo de pagamentos confirmados do Dashboard. Não há abertura automática nem lançamento retroativo ao abrir uma sessão posteriormente. Essa decisão substitui a recomendação de bloqueio de B02 no relatório e no plano iniciais.

## Regressão automatizada

- API: **422 testes aprovados, nenhum falhou ou foi ignorado**, incluindo testes HTTP com PostgreSQL/Testcontainers.
- Frontend: **100 testes aprovados em 22 arquivos**.
- `pnpm lint`, `pnpm build` e `git diff HEAD --check`: aprovados.

Comandos executados:

```powershell
dotnet test apps/api/Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal -p:OutputPath=bin/OperationalPatch
# Em apps/web:
pnpm test
pnpm lint
pnpm build
```

O diretório de saída separado evitou conflito com o executável da API local em execução. Os testes cobrem observações distintas, excesso eletrônico em criação/edição/entrega, entrega sem caixa, repetição de entrega, exclusão de entregue, contagem com pagamentos mistos/estornados, datas das despesas, recibo e descarte do formulário.

Após a decisão de operar sem caixa, a suíte completa foi executada novamente: 422 testes da API e 100 do frontend aprovados. O teste HTTP percorre criação, preparo e entrega com pagamento misto e troco, tanto sem nenhuma sessão quanto com uma sessão fechada. Verifica os pagamentos confirmados no banco e o valor líquido no Dashboard; abrir uma sessão depois e repetir a entrega não gera movimentos retroativos. O teste do card verifica que a entrega continua habilitada sem consultar caixa.

## Conferência manual local

Verificada em `localhost:5173`, com a API em `localhost:5052`:

1. Esc em pedido alterado abre confirmação; “Continuar preenchendo” preserva os dados.
2. Duas pizzas com “Sem cebola” e “Com cebola” persistem separadamente. Excesso eletrônico bloqueia o envio.
3. Pedido de R$ 70 com Pix de R$ 60 e dinheiro de R$ 20 apresenta troco de R$ 10; o comprovante contém valores e observações. Na conferência inicial, o UUID era completo; posteriormente a referência visual foi ajustada para os últimos oito caracteres, com teste de regressão. Impressão física não foi realizada.
4. Na versão inicial do patch, pedido pronto ficava bloqueado com caixa fechado. Esse bloqueio foi removido após a decisão de negócio de permitir operação completa sem caixa. Pedidos entregues continuam sem exclusão para preservar o histórico.
5. Despesas pagas do card (R$ 420.031,23) correspondem à soma da tabela do gráfico, incluindo pagamento cuja data difere do vencimento. Valores são do banco local de desenvolvimento.
6. O pedido criado nesta conferência, `01A1600A-DF33-7E5F-A24A-225D3B12A994` (“QA Patch - observacoes”), foi cancelado ao final, antes da entrega, sem confirmar pagamentos. Os registros QA da auditoria anterior foram preservados.

## Cuidados para disponibilização

Publicar API e frontend deste patch em conjunto para alinhar validações, mensagens e controles. Os testes não corrigem dados anteriores: eventuais entregas sem movimento e estornos incorretos já existentes precisam de conciliação individual. A nova UI e as evoluções de fluxo continuam reservadas para `release/2.0.0`.
