# Decisões arquiteturais

Architecture Decision Records (ADRs) registram escolhas estruturais, seu contexto e suas consequências. Eles complementam a [visão de arquitetura](../architecture/overview.md): a arquitetura descreve o estado atual; os ADRs explicam por que ele existe.

| ADR | Status | Decisão |
| --- | --- | --- |
| [ADR-001](0001-monorepo-and-vertical-slices.md) | Aceita | Monorepo com API organizada por vertical slices |
| [ADR-002](0002-tenant-from-authenticated-context.md) | Aceita | Tenant derivado do contexto autenticado |
| [ADR-003](0003-synchronous-domain-events.md) | Aceita | Eventos de domínio síncronos na requisição |
| [ADR-004](0004-separate-application-and-identity-contexts.md) | Aceita | Contextos separados para negócio e identidade |

## Convenção

Cada decisão deve conter contexto, decisão e consequências. Decisões substituídas permanecem no histórico com status `Substituída` e um link para o novo ADR. Ajustes locais, facilmente reversíveis, não precisam de ADR.
