# Devlivery

API Web para gestão de features relacionadas a entregas — monólito modular organizado por features (vertical slice).

**Visão geral**

- Projeto ASP.NET Core Web API (solução: Devlivery.slnx).
- Organização por features em `src/Devlivery/Features` com testes em `test/Devlivery.Tests`.

**Principais tecnologias**

- .NET 10 / ASP.NET Core
- Entity Framework Core, Dapper (persistência)
- Minimal APIs / Features por responsabilidade
- Docker / docker-compose para ambiente local

**Pré-requisitos**

- .NET SDK 10.0
- Docker (opcional para execução em containers)

**Como rodar localmente**

1. Restaurar dependências e compilar:

```bash
dotnet restore
dotnet build
```

2. Executar a API (diretamente):

```bash
dotnet run --project src/Devlivery
```

3. Ou via Docker (docker-compose):

```bash
docker compose up --build -d
```

**Testes**

- Executar testes unitários/integrados:

```bash
dotnet test test/Devlivery.Tests
```

**Backup job**

- O backup de produção deve rodar como um serviço cron dedicado, separado da API.
- Projeto do job: `src/Devlivery.BackupJob`.
- Imagem dedicada: `src/Devlivery.BackupJob/Dockerfile`.
- Execução local:

```bash
dotnet run --project src/Devlivery.BackupJob
```

- Variáveis esperadas no ambiente usam o prefixo `Backup__`, por exemplo:
	`Backup__DatabaseConnectionString`, `Backup__BucketName`, `Backup__R2Endpoint`, `Backup__AccessKeyId`, `Backup__SecretAccessKey`.
- O container do job precisa ter `pg_dump` disponível. A imagem dedicada já instala `postgresql-client`.
- Exemplo de cron no Railway para backup diário às 03:00 UTC: `0 3 * * *`.

**Provisionamento seguro do bucket R2**

- Use um bucket dedicado por ambiente. Exemplo recomendado para produção: `devlivery-prod-backups`.
- O nome do bucket deve usar apenas letras minúsculas, números e hífens, com 3 a 63 caracteres.
- Na criação do bucket, prefira `Automatic` como localização. Só use jurisdição `EU` se você realmente precisar de residência de dados garantida nesse escopo.
- Mantenha o bucket privado. Não habilite `Public Development URL (r2.dev)`.
- Não configure `Custom Domain` para esse bucket de backup.
- Não configure `CORS` para esse bucket. CORS só faz sentido para acesso via navegador, o que não é o caso do job de backup.
- Crie um token exclusivo para o job com permissão `Object Read & Write` e escopo restrito apenas ao bucket de backup.
- Prefira `Account API token` em vez de `User API token`, para não atrelar a automação a um usuário específico.
- O endpoint do job deve usar o formato `https://<ACCOUNT_ID>.r2.cloudflarestorage.com`. Se o bucket for criado com jurisdição `EU`, use `https://<ACCOUNT_ID>.eu.r2.cloudflarestorage.com`.
- O R2 já cifra objetos em repouso automaticamente com AES-256 e usa TLS em trânsito. Você não precisa habilitar isso manualmente.
- Para proteção adicional contra exclusão prematura, considere adicionar `Bucket Lock` no prefixo `postgres/production/` com retenção mínima de 7 dias.
- Se quiser um trilho extra de retenção no próprio bucket, adicione uma `Object Lifecycle Rule` no prefixo `postgres/production/` para expirar objetos com uma janela ligeiramente maior que a do job, por exemplo 8 ou 14 dias. Isso evita crescimento indefinido se a limpeza do job falhar.

**Passo a passo no Cloudflare Dashboard**

1. Entre em `R2 Object Storage` no painel da Cloudflare.
2. Clique em `Create bucket`.
3. Defina o nome do bucket, por exemplo `devlivery-prod-backups`.
4. Em `Location`, deixe `Automatic` ou escolha `EU` apenas se houver requisito formal de residência.
5. Crie o bucket.
6. Abra o bucket e vá em `Settings`.
7. Confirme que `Public Development URL` está desabilitado.
8. Não adicione `Custom Domains`.
9. Não adicione política de `CORS`.
10. Em `Object Lifecycle Rules`, opcionalmente crie uma regra para o prefixo `postgres/production/` com expiração acima da retenção do job.
11. Em `Bucket lock rules`, opcionalmente crie uma regra para o prefixo `postgres/production/` com retenção mínima de 7 dias.
12. Volte para `R2 Overview`, abra `Manage API Tokens` e crie um token `Object Read & Write` escopado só para esse bucket.
13. Copie o `Access Key ID`, o `Secret Access Key` e o `Account ID`.

**Mapeamento para o job**

```bash
Backup__BucketName=devlivery-prod-backups
Backup__BucketPrefix=postgres
Backup__EnvironmentName=production
Backup__R2Endpoint=https://<ACCOUNT_ID>.r2.cloudflarestorage.com
Backup__AccessKeyId=<ACCESS_KEY_ID>
Backup__SecretAccessKey=<SECRET_ACCESS_KEY>
```

Se você habilitar jurisdição `EU`, ajuste apenas o endpoint:

```bash
Backup__R2Endpoint=https://<ACCOUNT_ID>.eu.r2.cloudflarestorage.com
```

**Estrutura importante**

- `src/Devlivery` — código da aplicação (Features, Shared, Infrastructure).
- `src/Devlivery.BackupJob` — executável de backup PostgreSQL para R2.
- `test/Devlivery.Tests` — suíte de testes.
