# Devlivery V2 — Backlog de melhorias

15 de outubro de 2026 · Rafael Freitas

## Contexto

A V2 deve tirar trabalho do atendente, que hoje redigita cada pedido do WhatsApp e é o gargalo da operação. O Devlivery atende um delivery pequeno: 1 atendente, 1 cozinheiro e 1 entregador, com a cozinha trabalhando só com a comanda impressa.

O fluxo de um pedido hoje:

1. O cliente pede pelo WhatsApp.
2. O atendente lança o pedido no Devlivery, digitando nome, telefone, endereço, itens, taxa e pagamento.
3. O atendente imprime a comanda e entrega ao cozinheiro.
4. O cozinheiro prepara a partir do papel; ninguém atualiza o status no sistema durante o preparo.
5. O entregador sai com um ou mais pedidos e volta com o dinheiro recebido.
6. O atendente marca o pedido como entregue, o pagamento entra no caixa e, no fim do turno, o caixa é conferido e fechado.

Com essa equipe, o que pesa é o tempo do atendente por pedido e a confiabilidade do dinheiro. Recursos pensados para equipes maiores, como permissões por papel, tela de cozinha, atualização em tempo real e cadastro de vários entregadores, ficam para quando a operação crescer.

## Como ler este backlog

Os itens estão em quatro fases, ordenadas pelo ganho para o atendente e pelo risco de erro com dinheiro. Dentro de cada fase, a ordem da tabela é a ordem sugerida de execução.



As fases 1 e 3 podem andar em paralelo; a fase 4 só começa com a fundação da fase 2 pronta.

Cada item traz o problema que resolve, a proposta, o que muda na API e a tela de referência no canvas de design. Esforço é relativo: P cabe em um dia, M em uma semana, G em mais de uma semana.

O que ficou de fora nesta escala, e o sinal para trazer de volta:

| Recurso | Por que não agora | Quando reavaliar |
| --- | --- | --- |
| Permissões por papel | Uma pessoa opera tudo; o registro de quem fez cada ação já basta | Entrou um segundo atendente ou um gerente |
| Tela de cozinha (KDS) | Um cozinheiro trabalha bem com a comanda impressa | Dois ou mais cozinheiros, ou pedidos do cardápio sem atendente |
| Atualização em tempo real | Um único computador opera o sistema | Dois dispositivos usados ao mesmo tempo |
| Cadastro de entregadores e acerto por entregador | Com um entregador, o acerto é o próprio caixa | Contratou um segundo entregador |

## Fase 1 — Ganhos rápidos

Oito itens de front ou de backend mínimo que tiram cliques do atendente e fecham dois furos no dinheiro: entrega com caixa fechado e fechamento induzido.

| # | Item | Problema hoje | Proposta | Mudança na API | Esforço | Tela | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1.1 | Imprimir move para Em preparo | Ninguém atualiza o status durante o preparo, então os tempos registrados não valem nada | Ao imprimir a comanda, o pedido Pendente passa para Em preparo sozinho | Nenhuma: chama o endpoint de status que já existe | P | Pedidos v2 | A fazer |
| 1.2 | Tempo decorrido e atraso no card | O card mostra a hora de criação; no pico ninguém vê qual pedido está atrasado | "Há 23 min" no card e destaque a partir de um limite configurável, por exemplo 40 min | Opcional: limite nas configurações do estabelecimento | P | Pedidos v2 | A fazer |
| 1.3 | Novo pedido em painel lateral | O modal central cobre o WhatsApp Web aberto ao lado | Painel à direita que cabe com o WhatsApp em meia tela; Criar e imprimir em Ctrl Enter | Nenhuma | M | Novo pedido v2 | A fazer |
| 1.4 | Bloquear entrega com caixa fechado | Sem caixa aberto, o pagamento confirmado é descartado e só vai para o log | Aviso fixo quando o caixa está fechado; Marcar entregue fica desabilitado com o motivo | 422 ao entregar sem sessão de caixa aberta | P | Pedidos v2 | A fazer |
| 1.5 | Contagem cega no fechamento | Ver o esperado antes de contar induz a "acertar" o valor | O operador informa o valor contado primeiro; esperado e diferença aparecem depois | Nenhuma | P | Caixa v2 | A fazer |
| 1.6 | Motivo do cancelamento | Cancelamentos não dizem por quê, então não dá para reduzi-los | Lista curta: cliente desistiu, fora da área, produto em falta, pedido duplicado, outro | Campo de motivo no pedido cancelado | P | Pedidos v2 | A fazer |
| 1.7 | Sem Excluir em pedido entregue | Excluir um entregue de caixa fechado desalinha caixa e dashboard | Esconder Excluir nos entregues; cancelados podem ser arquivados | 422 para excluir entregue de sessão fechada | P | Pedidos v2 | A fazer |
| 1.8 | Botões de status à prova de duplo clique | Duplo clique em entregar gera eventos repetidos | Desabilitar o botão enquanto a requisição roda | Transição para o mesmo estado vira no-op | P | Todas | A fazer |

## Fase 2 — Fundação de domínio

Nove mudanças de modelo que acabam com a redigitação e dão ao pedido o vocabulário da operação real; o maior ganho do backlog está em 2.2 e 2.3.

| # | Item | Problema hoje | Proposta | Mudança na API | Esforço | Tela | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 2.1 | Número do pedido | O pedido só tem GUID; cozinha, entregador e cliente não têm como se referir a ele | Número sequencial por estabelecimento que reinicia todo dia (#1, #2…) | Campo de número gerado na criação, único por estabelecimento e dia | P | Todas | A fazer |
| 2.2 | Clientes pelo telefone | Nome, telefone e endereço são redigitados a cada pedido | Digitar o telefone encontra o cliente, preenche o último endereço e oferece repetir o último pedido | Entidade Cliente com telefone único, endereços e histórico; busca por telefone | M | Novo pedido v2 | A fazer |
| 2.3 | Variações e acréscimos | "Meia calabresa grd" e "+ palmito" são produtos soltos; a comanda fica confusa e o preço do meio a meio é conta de cabeça | Produto com tamanhos, opção de meio a meio e grupos de acréscimos com preço; o item do pedido guarda as escolhas | Grupos de opções no produto e opções escolhidas no item; regra de preço do meio a meio a decidir | G | Produtos v2, Novo pedido v2 | A fazer |
| 2.4 | Zonas de entrega | A taxa é digitada a cada pedido e o endereço é texto livre | Bairro como campo; cada bairro tem sua taxa, preenchida automaticamente | Cadastro de zonas (bairro, taxa) e bairro no endereço | M | Novo pedido v2 | A fazer |
| 2.5 | Saída para entrega | Quando o cliente pergunta "saiu?", o atendente não sabe | Novo status Saiu para entrega, marcado para vários pedidos de uma vez quando o entregador sai | Status entre Pronto e Entregue, com horário de saída | P | Pedidos v2 | A fazer |
| 2.6 | Confirmação do pagamento na entrega | O cliente troca Pix por dinheiro na porta e o pedido fica com a forma errada | Marcar entregue abre uma confirmação com forma, valor recebido e troco já preenchidos | Entregar aceita o ajuste de pagamentos antes de confirmar | M | Pedidos v2 | A fazer |
| 2.7 | Sangria | Só existe aporte; retirar dinheiro da gaveta durante o turno não tem registro | Registrar sangria com valor, motivo e quem fez | Novo tipo de movimentação no caixa, subtraído do esperado em dinheiro | P | Caixa v2 | A fazer |
| 2.8 | Reimpressão de alteração | Editar um pedido já impresso não avisa a cozinha | Ao salvar a edição, imprimir só o que mudou, marcado como alterado | Histórico dos itens do pedido para calcular a diferença | M | Comanda v2 | A fazer |
| 2.9 | Preço congelado no item | Editar um pedido recalcula os itens pelo preço atual do catálogo | O item guarda o preço unitário do momento da venda; listagem e impressão usam esse preço | Preço unitário no DTO do item; edição mantém o preço dos itens existentes | M | Todas | A fazer |

Decisão pendente para 2.3: como cobrar o meio a meio. O cardápio atual já vende metades com preço próprio (meia crocante grande a R$ 42,00), ou seja, cobra a soma das metades; a proposta mantém essa regra, e a alternativa comum no mercado é cobrar o sabor mais caro.

## Fase 3 — Financeiro e indicadores

Seis itens para que lucro e despesas reflitam a operação; hoje o pró-labore entra como despesa e a receita mistura dinheiro recebido com dinheiro a receber.

| # | Item | Problema hoje | Proposta | Mudança na API | Esforço | Tela | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 3.1 | Retiradas fora do lucro | Pró-labore conta como despesa e reduz o lucro líquido | Categoria marcada como retirada; o dashboard mostra lucro operacional e, abaixo, as retiradas | Marca de retirada na categoria; o cálculo do lucro ignora essas despesas | P | Despesas v2, Dashboard v2 | A fazer |
| 3.2 | Categorias por natureza | "Fornecedor" e "Despesas da empresa" misturam quem recebe com o tipo de gasto | Categorias por natureza (insumos, ocupação, pessoal, retiradas) e fornecedor como campo próprio | Fornecedor como cadastro ligado à despesa; migração das categorias atuais | M | Despesas v2 | A fazer |
| 3.3 | Despesas recorrentes | Aluguel, água, luz e salários são relançados todo mês | Marcar a despesa como mensal ou semanal; a próxima nasce pendente quando a anterior é paga | Regra de recorrência na despesa e geração da próxima ocorrência | M | Despesas v2 | A fazer |
| 3.4 | Custo e margem do produto | Não há como saber quais produtos dão lucro | Custo estimado por produto e margem na lista e no painel de edição | Campo de custo no produto | M | Produtos v2 | A fazer |
| 3.5 | Receita realizada e prevista | O card Receita Total soma pedidos ainda não pagos e não bate com o gráfico de pagamentos | Dois números: realizada (entregues) e prevista (em aberto) | Separar os dois valores no contrato do dashboard | P | Dashboard v2 | A fazer |
| 3.6 | Dashboard operacional | Faltam os números que mudam decisões no dia a dia | Pedidos por hora, cancelamentos por motivo e comparação com o mesmo dia da semana anterior | Novos agregados por hora e por motivo; depende de 1.6 | M | Dashboard v2 | A fazer |

Clientes recorrentes e ticket por bairro entram no dashboard quando 2.2 e 2.4 estiverem no ar.

## Fase 4 — Futuro

O cardápio do cliente chega em quatro etapas, cada uma útil sozinha, para não depender de pagamento online nem de conta de cliente logo de início.

1. **Cardápio público só de leitura.** Um link com produtos, fotos, preços e disponibilidade, enviado no WhatsApp. Exige foto no produto e horário de funcionamento.
2. **Carrinho que envia pelo WhatsApp.** O cliente monta o pedido e o envia como mensagem com um código curto. O atendente cola o código no Devlivery e o pedido entra pronto, sem redigitar. Exige 2.2, 2.3 e 2.4.
3. **Pedido direto.** O pedido entra como Pendente com origem Cardápio; Iniciar preparo vira o aceite e já imprime a comanda. Exige origem do pedido e um aviso sonoro de pedido novo.
4. **Pagamento online.** Pix com confirmação automática, para pedidos que chegam já pagos.

Impressão silenciosa também fica aqui: um agente local que manda a comanda direto para a impressora térmica (ESC/POS), sem a janela de impressão do navegador. Para um atendente sozinho é um ganho real por pedido; pode subir para a fase 2 se a janela de impressão incomodar no pico.

## Relação com o backlog de decisões

Os seis itens de `docs/product/backlog.md` entram neste backlog com uma decisão sugerida cada.

| Item do backlog atual | Decisão sugerida | Onde entra |
| --- | --- | --- |
| 1. Edição recalcula o preço dos itens | (a) Congelar o preço dos itens existentes | 2.9 |
| 2. Exclusão de pedido entregue | (a) Bloquear com 422 e esconder o botão | 1.7 |
| 3. Entregar um pedido já entregue | (a) No-op silencioso e botão desabilitado durante a requisição | 1.8 |
| 4. Receita inclui pedidos não entregues | (b) Separar realizada e prevista | 3.5 |
| 5. Preço exibido vem do catálogo | Incluir o preço unitário no item | 2.9 |
| 6. Contratos inconsistentes | Alinhar cada contrato quando a tela correspondente for migrada para a V2 | Contínuo |

## Telas de referência

As propostas estão no canvas de design, que tem uma versão por tela (o design final), com dados de exemplo do cardápio real.

| Tela | Itens que demonstra |
| --- | --- |
| Pedidos — tempo, atraso e saída | 1.1, 1.2, 1.4 (ligue "caixaFechado" nos ajustes da tela), 2.1, 2.3, 2.5 |
| Novo pedido — ao lado do WhatsApp | 1.3, 2.2, 2.3, 2.4 |
| Entregar e cancelar | 1.6, 2.6 |
| Comanda — opções, alteração e troco | 2.3, 2.8 e o troco a levar na via da entrega |
| Caixa — contagem cega e sangria | 1.5, 2.7 |
| Produtos — tamanhos, opções e margem | 2.3, 3.4 |
| Despesas — natureza, recorrência e retiradas | 3.1, 3.2, 3.3 |
| Dashboard — lucro operacional e pico | 3.1, 3.5, 3.6 |
