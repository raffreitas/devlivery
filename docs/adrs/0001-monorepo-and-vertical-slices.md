# ADR-001: Monorepo com API organizada por vertical slices

- **Status:** Aceita
- **Registrada em:** 2026-09-12

## Contexto

O produto possui uma SPA React, uma API .NET e um job de backup. Pedidos, caixa, produtos, despesas e dashboard evoluem como capacidades relacionadas e compartilham contratos, persistência e fluxo de entrega.

Na API, organizar todo o código primeiro por camada técnica espalharia uma mudança de negócio entre muitos diretórios e dificultaria reconhecer o caso de uso completo.

## Decisão

Manter as aplicações no mesmo repositório. Na API, organizar endpoints, comandos, consultas, validações e handlers por feature em `Features`. Manter invariantes e conceitos independentes de transporte em `Domain`, adaptadores em `Infrastructure` e plumbing transversal em `Common`.

O frontend também é organizado por feature, com elementos reutilizáveis em `shared`.

## Consequências

- Mudanças de uma capacidade ficam mais próximas e descobríveis.
- API, web e documentação podem ser validadas juntas, mas continuam deployables distintos.
- Regras reutilizadas por vários casos de uso devem permanecer no domínio, não ser copiadas entre slices.
- Limites entre features exigem disciplina porque o processo e o banco são compartilhados.
- Uma futura extração de serviço deve ser motivada por necessidade operacional ou de autonomia, não apenas pelo tamanho de uma pasta.
