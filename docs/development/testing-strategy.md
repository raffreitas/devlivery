# Estratégia de testes

## Objetivo

Os testes devem proteger invariantes de domínio, comportamento dos casos de uso, isolamento por tenant e integrações que dependem do PostgreSQL. A intensidade da verificação deve acompanhar o risco da mudança.

## Níveis atuais

| Nível | Responsabilidade | Implementação atual |
| --- | --- | --- |
| Domínio | Estados, cálculos, invariantes e idempotência | xUnit e Shouldly, sem infraestrutura |
| Handler e validação | Orquestração, resultados, dependências e entradas inválidas | xUnit, NSubstitute e FluentValidation |
| Integração de persistência | SQL, filtros e consultas que dependem do PostgreSQL | Testcontainers PostgreSQL em cenários específicos |
| Contrato HTTP | Pipeline, autenticação, tenant, serialização, status HTTP e persistência ponta a ponta | `ApiFactory` (`WebApplicationFactory` + Testcontainers PostgreSQL com as migrations reais); cada teste cria seu próprio estabelecimento via `ApiTenant` |
| Frontend | Cálculos, hooks, componentes e fluxos da interface | Vitest, Testing Library e MSW em jsdom |
| Job de backup | Configuração, chaves dos objetos, retenção, orquestração, cliente R2 e `pg_dump` real | xUnit, NSubstitute e Testcontainers PostgreSQL 17 em `apps/backup/test/Devlivery.BackupJob.Tests` |

O pacote `coverlet.collector` gera cobertura no CI e o SonarQube recebe o relatório. Não existe limite numérico de cobertura configurado; cobertura não substitui cenários de comportamento.

## Onde adicionar testes

- Mudança em agregado ou Value Object: teste de domínio para sucesso, rejeição e fronteiras relevantes.
- Mudança em comando, consulta ou validator: teste da classe correspondente, incluindo falhas esperadas.
- Mudança em SQL, Dapper, mapeamento EF ou filtro de tenant: teste com PostgreSQL real via Testcontainers.
- Mudança em autenticação, middleware, representação HTTP ou endpoint: teste de contrato HTTP em `Features/<Feature>/Api`.
- Mudança no frontend: adicionar regressões de comportamento, executar testes, lint e build e verificar manualmente os fluxos afetados no navegador.

A suíte segue a pirâmide de testes: regras e ramos de erro ficam nos testes de domínio e de handler; os testes HTTP de comandos são finos (caminho feliz e contratos que só existem nessa camada, como 401, formato do 422 e isolamento de tenant), enquanto consultas recebem mais cenários porque sua lógica está no SQL ou LINQ executado pelo PostgreSQL.

Os testes backend ficam em `apps/api/test/Devlivery.Tests` e espelham as features. Convenções:

- nomes no padrão `GivenContexto_WhenAcao_ThenResultado`, por exemplo `GivenClosedSession_WhenAddingPayment_ThenThrowsDomainException`;
- seções `// Arrange`, `// Act` e `// Assert` separadas; exceções são capturadas no Act com `Record.Exception`/`Record.ExceptionAsync` e verificadas no Assert;
- dados criados pelos builders de `Common/Builders` e mocks de tenant e operador por `Common/Mocks/TestMocks`;
- validators testados com um caso válido e um `[Theory]` de casos inválidos derivados do comando válido com `with`;
- não testar chamadas de mock que não representam efeito observável, mensagens de log ou código sem uso na aplicação.

## Verificação local

Com dependências restauradas e Docker disponível, a partir da raiz:

```powershell
dotnet test apps/api/Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
dotnet test apps/backup/Devlivery.BackupJob.slnx --disable-build-servers -m:1 --verbosity minimal
pnpm --dir apps/web test
pnpm --dir apps/web lint
pnpm --dir apps/web build
git diff --check
```

Os testes do exportador de backup executam o `pg_dump` real e são ignorados quando o PostgreSQL client 17 não está no PATH; a CI do backup instala o client e os executa.

Testcontainers exige acesso ao daemon Docker. Uma falha ao iniciar o container é ambiental e não comprova falha da regra testada; registre a limitação se não for possível repetir a execução em ambiente válido.

## Critério de conclusão

Os testes do frontend ficam ao lado do arquivo correspondente, com o mesmo nome-base: `money.test.ts`, `use-orders.test.tsx` e `products-page.test.tsx`. Use `.test.ts` sem JSX e `.test.tsx` com JSX; não use `.spec.*`. A infraestrutura compartilhada (setup, renderização com providers e servidor MSW) fica em `apps/web/test/`, fora de `src`, e usa o alias `@test/*`. Futuros testes ponta a ponta ficam em `apps/web/test/e2e/`, com configuração própria. Fixtures e handlers exclusivos de uma feature permanecem próximos dela.

Cada teste usa um QueryClient isolado, sem retries, e handlers HTTP resetados entre cenários. Teste resultados observáveis, incluindo respostas atrasadas, falhas, preservação de preenchimento, mudanças de período e valores monetários. A CI executa a suíte com `pnpm test`.

Uma mudança está verificada quando:

1. possui regressão automatizada no nível adequado, quando essa infraestrutura existe;
2. as suítes relacionadas passam;
3. lint, tipos e build das aplicações afetadas passam;
4. fluxos sem automação são verificados manualmente e informados no pull request;
5. a documentação de produto, domínio ou operação foi atualizada quando o comportamento mudou.
