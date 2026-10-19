# Devlivery Backup

Job .NET 10 que exporta o PostgreSQL do Devlivery e guarda os artefatos no Cloudflare R2. Roda como processo de execução única, normalmente agendado como cron, e encerra com código `0` em caso de sucesso e `1` em caso de falha.

O job não compartilha código com a API: depende apenas do PostgreSQL e do bucket. Se a hospedagem passar a oferecer backup gerenciado, esta pasta e o workflow `backup-ci.yaml` podem ser removidos sem afetar as outras aplicações.

## Fluxo

1. `pg_dump` em formato custom para um diretório temporário;
2. SHA-256 do arquivo;
3. upload do dump ao R2;
4. retenção: remove objetos do ambiente mais antigos que `Backup__RetentionDays`;
5. upload do manifesto JSON com chave, tamanho, checksum, horários e itens removidos.

Uma falha em qualquer etapa interrompe o fluxo; a retenção só roda depois que o novo dump foi enviado.

## Estrutura

```text
apps/backup/
├── src/Devlivery.BackupJob/
│   ├── Configuration/           # opções Backup__* e validação
│   ├── Observability/           # métricas e traces OpenTelemetry
│   ├── Services/                # pg_dump, checksum, R2, retenção e orquestração
│   └── Dockerfile               # imagem com PostgreSQL client 17
├── test/Devlivery.BackupJob.Tests/
└── Devlivery.BackupJob.slnx
```

## Testes

```powershell
dotnet test Devlivery.BackupJob.slnx --disable-build-servers -m:1 --verbosity minimal
```

Os testes do `PostgresDumpExporter` executam o `pg_dump` real contra um PostgreSQL 17 via Testcontainers. Eles são ignorados quando o PostgreSQL client 17 não está no PATH; a CI instala o client e os executa.

## Imagem

```powershell
docker build --file src/Devlivery.BackupJob/Dockerfile --tag devlivery-backup .
```

O contexto de build é `apps/backup`.

## Configuração e operação

- [Configuração](../../docs/development/configuration.md#backup-job)
- [Backup e recuperação](../../docs/operations/backup-and-restore.md)
