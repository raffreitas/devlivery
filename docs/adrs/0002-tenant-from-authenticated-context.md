# ADR-002: Tenant derivado do contexto autenticado

- **Status:** Aceita
- **Registrada em:** 2026-09-12

## Contexto

O Devlivery armazena dados de vários estabelecimentos. Aceitar `establishmentId` ou a identidade do operador como autoridade no corpo da requisição permitiria consultar ou atribuir dados a outro tenant caso um cliente manipulasse o contrato.

## Decisão

Derivar o estabelecimento da claim autenticada `establishment_id` e o operador da claim `sub`. Registrar o tenant no contexto da requisição e aplicar filtros globais do EF Core às entidades tenant-aware. Quando uma operação precisa registrar autoria, resolver o usuário no banco dentro do mesmo tenant.

Identificadores enviados pelo cliente não substituem o tenant ou o autor autenticados.

## Consequências

- O isolamento não depende de o frontend enviar o estabelecimento correto.
- Handlers e consultas devem usar o contexto corrente e testes devem cobrir tentativas entre tenants.
- Processamento fora da requisição não pode depender implicitamente de claims; tenant e autor deverão fazer parte do contrato confiável do job ou mensagem.
- SQL direto com Dapper não recebe automaticamente os filtros do EF Core e deve aplicar o tenant explicitamente.
