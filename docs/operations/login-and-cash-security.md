# Segurança do login e do caixa

## Proteção do login

O ASP.NET Core Identity bloqueia a conta após cinco falhas consecutivas por cinco minutos. O estado é persistido no PostgreSQL e um login bem-sucedido zera o contador. Conta inexistente, senha incorreta e conta bloqueada retornam a mesma resposta 401 para não revelar usuários cadastrados.

Além do bloqueio por conta, `POST /api/auth/login` possui uma janela fixa de 60 segundos com 30 requisições por IP e sem fila. Exceder o limite retorna 429 no formato de erro da API e inclui `Retry-After`. Os valores são configuráveis pelas opções `LoginProtection`.

O limite por IP reside na memória do processo e reinicia com a instância. Uma implantação com múltiplas instâncias exige limitação compartilhada. O bloqueio por conta continua persistente independentemente disso.

## IP do cliente

Por padrão, a API usa o IP da conexão e ignora cabeçalhos enviados pelo cliente. O modo Railway usa somente `X-Real-IP` quando há exatamente um endereço válido. Listas, múltiplos valores, ausência ou valor inválido entram no grupo compartilhado `unknown`. IPv4 e sua representação IPv6 mapeada usam a mesma chave.

Ative `LoginProtection__RailwayIngress=true` apenas depois do teste de ingresso descrito em [publicação da API](deployment.md). Um proxy adicional ou uma rota alternativa até a aplicação invalida essa premissa e exige nova avaliação.

## Autoria das operações de caixa

A API ignora identidade enviada nos corpos de abertura ou aporte. O identificador vem de `sub`, o tenant vem de `establishment_id`, e o usuário é consultado no banco dentro desse tenant. Claim ausente ou inválida, usuário inexistente ou vínculo com outro tenant interrompe a operação com 401.

Abertura armazena ID e nome do usuário resolvido. Aportes, pagamentos, trocos e estornos registram explicitamente o usuário que executou a requisição. Não existe fallback para o responsável que abriu o caixa.

A consulta de aportes busca os autores em lote e dentro do tenant. Um autor histórico que já não existe aparece como “Usuário indisponível”; registros históricos não são reescritos.

Eventos de caixa são processados dentro da requisição atual. Se passarem a ser executados em background, o contrato deverá transportar autor e tenant explicitamente.

## Logs

Eventos de limitação e bloqueio são estruturados e não registram senha, token ou email. Preserve essa regra ao ampliar observabilidade ou integrar serviços externos.
