# Documentação do Devlivery

Este diretório registra o produto, suas regras, as decisões técnicas e os procedimentos necessários para desenvolver e operar o Devlivery. O README da raiz continua sendo a porta de entrada rápida; aqui ficam as fontes de verdade que precisam evoluir com o código.

## Entender o projeto

| Documento | Responde a |
| --- | --- |
| [Visão do produto](product/overview.md) | Qual problema o Devlivery resolve, para quem e com quais capacidades atuais? |
| [Modelo de domínio](product/domain-model.md) | Quais são os conceitos, estados, regras e integrações entre áreas? |
| [Arquitetura](architecture/overview.md) | Como o sistema é dividido e quais são seus limites técnicos? |
| [ADRs](adrs/README.md) | Por que escolhas estruturais importantes foram adotadas? |

## Desenvolver

| Documento | Conteúdo |
| --- | --- |
| [Desenvolvimento local](development/local-development.md) | Preparação do ambiente, migrações, execução e verificações |
| [Estratégia de testes](development/testing-strategy.md) | Níveis de teste, responsabilidades, lacunas e comandos |
| [Configuração](development/configuration.md) | Variáveis da API, frontend, telemetria e backup |

## Operar

| Documento | Conteúdo |
| --- | --- |
| [Publicação da API](operations/deployment.md) | Checklist para Railway, migrações e exposição pública segura |
| [Backup e recuperação](operations/backup-and-restore.md) | Execução, retenção, artefatos e teste de restauração |
| [Segurança do login e do caixa](operations/login-and-cash-security.md) | Lockout, rate limit, IP real e autoria das operações |

## Referência executável

Em desenvolvimento, o contrato HTTP atual pode ser consultado no Scalar em `https://localhost:7141/scalar` ou diretamente em `https://localhost:7141/openapi/v1.json`. Os READMEs de [API](../apps/api/README.md) e [web](../apps/web/README.md) resumem as responsabilidades e os comandos de cada aplicação.

## Manutenção

- Atualize [visão do produto](product/overview.md) quando capacidades ou limites mudarem.
- Atualize [modelo de domínio](product/domain-model.md) junto com estados, invariantes e efeitos entre features.
- Crie um ADR para decisões estruturais difíceis de reverter; não use ADR para detalhes locais de implementação.
- Atualize os guias operacionais junto com variáveis, comandos, migrações, publicação ou garantias de segurança.
- Descreva o comportamento existente como implementado e identifique explicitamente lacunas ou trabalho futuro.
