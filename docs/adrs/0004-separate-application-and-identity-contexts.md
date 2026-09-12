# ADR-004: Contextos separados para negócio e identidade

- **Status:** Aceita
- **Registrada em:** 2026-09-12

## Contexto

Dados operacionais e estruturas do ASP.NET Core Identity possuem ciclos de modelagem distintos. Colocar todas as entidades em um único `DbContext` misturaria configuração de autenticação com o modelo da aplicação, embora ambos precisem participar da mesma implantação e usem o mesmo PostgreSQL.

## Decisão

Usar `ApplicationDbContext` para dados de negócio e `ApplicationIdentityDbContext` para ASP.NET Core Identity. Os contextos compartilham a connection string, mas mantêm modelos e históricos de migração separados; as tabelas do Identity ficam no schema `identity`.

Gerar e aplicar migrações para cada contexto de forma explícita. Em produção, os dois executáveis de migração rodam no pre-deploy antes de liberar a nova versão da API.

## Consequências

- Alterações de identidade não poluem o modelo e as migrações de negócio.
- Toda publicação deve considerar dois históricos de migração; aplicar apenas um pode deixar a versão inconsistente.
- O health check de prontidão verifica ambos os contextos.
- Compartilhar o PostgreSQL simplifica a operação atual, mas mantém identidade e negócio acoplados ao mesmo ciclo de disponibilidade do banco.
