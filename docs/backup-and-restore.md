# Backup e recuperação

O backup roda em `Devlivery.BackupJob`, separado da API. O processo cria um dump PostgreSQL em formato custom, calcula SHA-256, envia o dump e um manifesto JSON ao Cloudflare R2 e aplica a retenção configurada.

## Execução

Use o Dockerfile `apps/api/src/Devlivery.BackupJob/Dockerfile`, com `apps/api` como contexto. A imagem instala o PostgreSQL client 17. Configure as variáveis `Backup__*` descritas em [configuração](configuration.md).

No Railway, execute o job como serviço cron dedicado. Um exemplo de agenda diária às 03:00 UTC é:

```text
0 3 * * *
```

Os objetos seguem a estrutura:

```text
{prefix}/{environment}/yyyy/MM/dd/devlivery-{environment}-yyyyMMddTHHmmssZ.dump
{prefix}/{environment}/yyyy/MM/dd/devlivery-{environment}-yyyyMMddTHHmmssZ.manifest.json
```

## Bucket R2

- use um bucket privado por ambiente;
- não habilite URL pública, domínio customizado ou CORS;
- crie um Account API Token com `Object Read & Write`, restrito ao bucket;
- mantenha as credenciais apenas no secret store;
- considere Bucket Lock com retenção mínima de sete dias;
- use uma lifecycle rule com prazo maior que o job como proteção contra falha na limpeza.

O endpoint padrão é `https://<ACCOUNT_ID>.r2.cloudflarestorage.com`. Contas com jurisdição EU usam o endpoint correspondente da região.

## Teste de restauração

Um backup só é considerado operacional depois de restaurado em um banco isolado. Faça o teste periodicamente e após mudanças importantes no schema ou no job:

1. Baixe o `.dump` e o manifesto correspondente.
2. Compare o SHA-256 do arquivo com `sha256Checksum` do manifesto.
3. Crie um PostgreSQL vazio, sem acesso da aplicação pública.
4. Restaure com `pg_restore --no-owner --no-privileges --clean --if-exists`.
5. Confirme a existência dos schemas e aplique consultas de sanidade sem expor dados pessoais.
6. Inicie uma API temporária contra o banco restaurado e verifique `/health` e fluxos essenciais.
7. Destrua com segurança o banco temporário ao finalizar.

Exemplo, com valores fictícios:

```powershell
pg_restore --dbname "postgresql://user:password@host:5432/devlivery_restore" --no-owner --no-privileges --clean --if-exists backup.dump
```

Registre data, artefato, duração e resultado de cada exercício. O processo atual automatiza criação e retenção; o download e a restauração continuam operacionais e devem ser exercitados deliberadamente.
