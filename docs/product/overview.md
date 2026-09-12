# Visão do produto

## Propósito

Devlivery centraliza a rotina operacional de estabelecimentos que trabalham com entrega, como pizzarias, hamburguerias e lanchonetes. O produto conecta o registro do pedido ao recebimento, ao controle do caixa, às despesas e aos indicadores do negócio.

O objetivo atual é dar clareza ao expediente em uma única aplicação, reduzindo controles paralelos para catálogo, pedidos e conferência financeira.

## Usuários e necessidades

O sistema atende pessoas que operam ou acompanham um estabelecimento:

- **atendente ou operador:** registra e acompanha pedidos, pagamentos e movimentações do caixa;
- **responsável pela operação:** mantém produtos e despesas e confere o fechamento;
- **gestor:** consulta indicadores consolidados de vendas, pedidos, produtos e despesas.

Os papéis descrevem necessidades de uso. O checkout atual autentica usuários por estabelecimento, mas não implementa autorização funcional diferente para cada um desses papéis.

## Jornada operacional

1. O operador autentica-se no estabelecimento e abre o caixa com um valor inicial.
2. Produtos disponíveis compõem os itens dos pedidos.
3. O pedido avança pelo preparo e recebe um ou mais pagamentos.
4. Na entrega, os pagamentos pendentes são confirmados e eventual troco é calculado.
5. Pagamentos, trocos, aportes e estornos alimentam o caixa ativo.
6. Despesas são classificadas e acompanhadas por vencimento e pagamento.
7. O dashboard consolida informações da operação e o caixa é conferido e fechado.

Consulte o [modelo de domínio](domain-model.md) para as regras que condicionam cada etapa.

## Capacidades implementadas

| Área | Capacidades atuais |
| --- | --- |
| Autenticação | Login JWT, bloqueio por falhas e limitação por IP |
| Produtos | Cadastro, edição, disponibilidade, categoria e preço |
| Pedidos | Itens, cliente, endereço, taxa de entrega, status, múltiplos pagamentos e troco |
| Caixa | Abertura, aportes, pagamentos, trocos, estornos, consulta e fechamento |
| Despesas | Categorias e subcategorias, vencimento, pagamento, situação e filtros |
| Dashboard | Indicadores de vendas, pedidos, meios de pagamento, produtos e despesas |
| Continuidade | Backup agendável do PostgreSQL para bucket privado no Cloudflare R2 |

## Limites atuais

O repositório atual não contém capacidades de pedido feito pelo consumidor, roteirização ou rastreamento de entregadores, controle de estoque, mensageria assíncrona ou cache distribuído. A limitação de login por IP é local à instância da API. O frontend ainda não possui suíte automatizada de testes.

Esses itens são limites do estado atual, não compromissos de roadmap. Uma capacidade futura só deve ser apresentada como planejada quando houver uma decisão de produto registrada.

## Critério de fonte de verdade

- Este documento define propósito, público, capacidades e limites do produto.
- O [modelo de domínio](domain-model.md) define vocabulário e regras de negócio.
- O [OpenAPI](../README.md#referência-executável) define o contrato HTTP executável.
- Os guias de operação não devem redefinir regras do produto; devem apontar para sua fonte.
