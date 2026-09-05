# Publicação da API

Este guia descreve a exposição da API em uma instância Railway. Ajuste-o se outro proxy, CDN ou múltiplas instâncias forem introduzidos.

## Antes da publicação

1. Execute a suíte backend e o build do frontend.
2. Crie um PostgreSQL dedicado ao ambiente.
3. Configure connection string, JWT, CORS e ambiente conforme [configuração](configuration.md).
4. Gere uma chave JWT aleatória e mantenha-a apenas no secret store da plataforma.
5. Defina `ALLOWED_ORIGINS` com a origem HTTPS exata do frontend.
6. Mantenha `LoginProtection__RailwayIngress=false` até validar o ingresso.
7. Configure o health check da plataforma para `/alive`; use `/health` para diagnosticar dependências.

O Dockerfile da API está em `apps/api/src/Devlivery/Dockerfile` e espera `apps/api` como contexto de build.

## Migrações

Aplique os dois contextos antes de liberar tráfego para a nova versão:

```powershell
cd apps/api
dotnet tool restore
dotnet ef database update --project src/Devlivery --context ApplicationDbContext
dotnet ef database update --project src/Devlivery --context ApplicationIdentityDbContext
```

Execute migrações uma vez por publicação. A API só as aplica automaticamente em `Development`.

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

## Ordem de entrega

Publique a API e aplique migrações antes do frontend. O backend aceita clientes antigos que ainda enviam campos de autoria, mas ignora esses campos. Em seguida, publique o frontend com `VITE_API_URL` apontando para a API pública.

## Operação

Configure o backup como serviço cron separado. Monitore falhas de health check, respostas 5xx, respostas 429, bloqueios de conta e falhas do backup. Uma segunda instância da API exige um limitador por IP compartilhado; o bloqueio por conta já permanece no PostgreSQL.
