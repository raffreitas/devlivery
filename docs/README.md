# Documentação do Devlivery

Este diretório reúne informações que precisam acompanhar o código durante desenvolvimento e operação.

## Guias

| Documento | Conteúdo |
| --- | --- |
| [Arquitetura](architecture.md) | Componentes, limites, tenancy, persistência e contratos HTTP |
| [Desenvolvimento local](local-development.md) | Preparação do ambiente, migrações, execução e testes |
| [Configuração](configuration.md) | Variáveis da API, frontend, telemetria e backup |
| [Publicação da API](deployment.md) | Checklist para Railway e exposição pública segura |
| [Backup e recuperação](backup-and-restore.md) | Execução, retenção, artefatos e teste de restauração |
| [Segurança do login e do caixa](login-and-cash-security.md) | Lockout, rate limit, IP real e autoria das operações |

## Manutenção

Atualize estes documentos junto com mudanças de arquitetura, variáveis, comandos, processos de publicação ou garantias de segurança. Decisões futuras que alterem um limite importante devem ser registradas em um documento próprio ou em uma seção de decisões arquiteturais.
