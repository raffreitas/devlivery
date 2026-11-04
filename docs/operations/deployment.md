# Publicação

Este guia descreve a publicação do frontend e a exposição da API no Railway. Ajuste-o se outro proxy, CDN ou múltiplas instâncias forem introduzidos.

## Frontend no Railway

O frontend usa o workspace pnpm e o `pnpm-lock.yaml` da raiz do repositório. No serviço `devlivery-webapp`, configure:

| Configuração | Valor |
| --- | --- |
| Root Directory | `/` (raiz do repositório) |
| Railway Config File | `/apps/web/railway.toml` |
| `RAILPACK_SPA_OUTPUT_DIR` | `apps/web/dist` |
| `VITE_API_URL` | Origem HTTPS pública da API, sem `/api/v1` e sem barra final |
| Custom Start Command | Deixe vazio para usar o Caddy do Railpack |

O arquivo `apps/web/railway.toml` define o build do pacote web e os caminhos que disparam deploy, incluindo o lockfile compartilhado. O Railpack detecta pnpm pelo lockfile da raiz, instala com `--frozen-lockfile` e usa Node.js 24 e pnpm 10 conforme `engines` no `package.json` da raiz. `RAILPACK_SPA_OUTPUT_DIR` ativa a publicação estática com fallback para `index.html`, necessário às rotas do React Router.

Não use `/apps/web` como Root Directory: essa configuração exclui o lockfile e o manifesto do workspace do contexto de build. Adicionar apenas `packageManager` ao pacote web não torna esses arquivos acessíveis. Mantenha `vitest` e `@vitest/coverage-v8` na mesma versão exata ao atualizá-los e atualize o lockfile junto.

Depois de publicar essas alterações e aplicar as configurações do serviço, faça um novo deploy. Confira nos logs a instalação via pnpm com lockfile, o comando `pnpm --filter @devlivery/web run build` e a geração de `apps/web/dist`. Valide a página inicial e o acesso direto a uma rota da aplicação. `VITE_API_URL` é incorporada durante o build; sua alteração exige novo build.

Referências: [monorepos no Railway](https://docs.railway.com/deployments/monorepo) e [Node.js e sites estáticos no Railpack](https://railpack.com/languages/node/).

## Antes da publicação

1. Execute a suíte backend e o build do frontend.
2. Crie um PostgreSQL dedicado ao ambiente.
3. Configure connection string, JWT, CORS e ambiente conforme [configuração](../development/configuration.md).
4. Gere uma chave JWT aleatória e mantenha-a apenas no secret store da plataforma.
5. Defina `ALLOWED_ORIGINS` com a origem HTTPS exata do frontend.
6. Mantenha `LoginProtection__RailwayIngress=false` até validar o ingresso.
7. Configure o health check da plataforma para `/alive`; use `/health` para diagnosticar dependências.

O Dockerfile da API está em `apps/api/src/Devlivery/Dockerfile` e espera `apps/api` como contexto de build.

## Migrações

O deploy da API gera executáveis de migração para os dois contextos durante o build da imagem. No serviço da API no Railway, configure em **Settings > Deploy > Pre-deploy Command** a execução sequencial:

```sh
/app/migrations/migrate-application && /app/migrations/migrate-identity
```

Mantenha `ConnectionStrings__DefaultConnection` disponível no ambiente de deploy. Se qualquer bundle falhar, o comando retorna um status diferente de zero e impede a publicação da nova versão. Em **Settings > Deploy**, configure também um timeout de pre-deploy compatível com o volume do banco; 300 segundos é um ponto de partida razoável.

As migrações são aplicadas uma vez por publicação, antes da liberação de tráfego. A API só as aplica automaticamente em `Development`. Em produção, prefira migrações compatíveis com a versão anterior da aplicação, pois o banco é atualizado antes da validação de saúde da nova versão.

## Validação pública

- `/alive` responde sem consultar o banco;
- `/health` confirma os bancos de negócio e identidade;
- HTTP é redirecionado para HTTPS e HSTS está ativo fora de desenvolvimento;
- uma origem não permitida não recebe cabeçalhos CORS;
- login inválido retorna mensagem genérica;
- a 31ª tentativa na mesma janela retorna 429 e `Retry-After`;
- endpoints privados retornam 401 sem token e funcionam com um token válido;
- logs e traces não contêm senha, token, connection string ou email do login.

## IP real no Railway

Em homologação, envie logins a partir de duas redes e tente falsificar `X-Real-IP` e `X-Forwarded-For`. Confirme que o ingresso sobrescreve `X-Real-IP`, que valores falsificados não criam novos orçamentos e que não existe rota alternativa até a aplicação.

Somente depois desse teste configure `LoginProtection__RailwayIngress=true`. Com o modo desativado, a API usa o IP da conexão. Consulte [segurança do login e do caixa](login-and-cash-security.md).

## Operação

Configure o backup como serviço cron separado. Monitore falhas de health check, respostas 5xx, respostas 429, bloqueios de conta e falhas do backup. Uma segunda instância da API exige um limitador por IP compartilhado; o bloqueio por conta já permanece no PostgreSQL.
