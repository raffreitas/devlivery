# Configuração

Use User Secrets no desenvolvimento e variáveis de ambiente no ambiente hospedado. A notação `__` representa seções da configuração .NET.

## API

| Variável | Obrigatória | Descrição |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | Sim | Conexão PostgreSQL usada pelos contextos de negócio e Identity |
| `JwtTokenSettings__Issuer` | Sim | Emissor aceito nos tokens |
| `JwtTokenSettings__Audience` | Sim | Audiência aceita nos tokens |
| `JwtTokenSettings__SecretKey` | Sim | Chave de assinatura longa, aleatória e secreta |
| `JwtTokenSettings__ExpirationInMinutes` | Sim | Validade do token em minutos |
| `ALLOWED_ORIGINS` | Sim em produção | Origens CORS separadas por vírgula e sem barra final |
| `ASPNETCORE_ENVIRONMENT` | Sim | Use `Production` no ambiente público |
| `ASPNETCORE_URLS` ou `PORT` | Conforme a plataforma | Binding HTTP do container |

OpenTelemetry é ativado quando `OTEL_EXPORTER_OTLP_ENDPOINT` está configurado. O exporter pode exigir variáveis adicionais fornecidas pelo backend de observabilidade.

## Proteção do login

| Variável | Padrão | Descrição |
| --- | --- | --- |
| `LoginProtection__MaxFailedAccessAttempts` | `5` | Falhas consecutivas antes do bloqueio |
| `LoginProtection__LockoutMinutes` | `5` | Duração do bloqueio |
| `LoginProtection__PermitLimit` | `30` | Requisições por janela e IP |
| `LoginProtection__WindowSeconds` | `60` | Duração da janela fixa |
| `LoginProtection__RailwayIngress` | `false` | Usa um único `X-Real-IP` válido como origem |

Não ative o modo Railway sem executar a validação descrita no [guia de segurança](login-and-cash-security.md).

## Frontend

| Variável | Obrigatória | Descrição |
| --- | --- | --- |
| `VITE_API_URL` | Sim | URL pública da API, sem barra final |
| `MODE` | Não | Identificador usado pelo ambiente local |

Variáveis `VITE_*` são incorporadas ao bundle e ficam visíveis no navegador. Nunca coloque segredos nelas.

## Backup job

| Variável | Padrão | Descrição |
| --- | --- | --- |
| `Backup__ApplicationName` | `devlivery` | Nome nos artefatos |
| `Backup__EnvironmentName` | — | Ambiente, por exemplo `production` |
| `Backup__DatabaseConnectionString` | — | Conexão PostgreSQL de origem |
| `Backup__BucketName` | — | Bucket privado no R2 |
| `Backup__BucketPrefix` | `postgres` | Prefixo dos objetos |
| `Backup__R2Endpoint` | — | Endpoint S3 da conta Cloudflare |
| `Backup__AccessKeyId` | — | Chave restrita ao bucket |
| `Backup__SecretAccessKey` | — | Segredo da chave |
| `Backup__PgDumpPath` | `pg_dump` | Executável do PostgreSQL client |
| `Backup__RetentionDays` | `7` | Retenção aplicada pelo job |
| `Backup__DumpCompressionLevel` | `9` | Compressão do formato custom |
| `Backup__DumpTimeoutMinutes` | `30` | Tempo máximo do dump |
| `Backup__ForcePathStyle` | `true` | Compatibilidade do cliente S3 |
