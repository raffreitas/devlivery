# Dívida técnica do Devlivery

Data: 27/10/2026. Base: `main` em `3bc60af`. Escopo: `apps/api`, `apps/web`, `apps/backup`, workflows de CI e configuração do repositório.

## Parecer

O código está em bom estado para o tamanho do produto: vertical slices consistentes, domínio isolado de HTTP, validação centralizada, quase nenhum marcador `TODO`/`FIXME`/supressão, e os achados B01–B10 da validação operacional de 21/10/2026 já foram corrigidos na `main` (PR #51).

A dívida relevante está concentrada em três pontos que **não aparecem em uso normal, mas falham silenciosamente**:

1. **A concorrência otimista provavelmente não funciona** (D01). As colunas `row_version` existem, mas o PostgreSQL nunca as preenche.
2. **O isolamento de tenant depende de listas e SQL mantidos à mão** (D02). Hoje está correto; falta uma proteção estrutural contra o próximo esquecimento.
3. **O dashboard agrega em memória** (D03). Carrega todos os pedidos do período, ou do histórico inteiro sem filtro de data. O custo cresce com o tempo de uso.

Foram identificados **14 itens**: 3 de prioridade alta, 7 de prioridade média e 4 de prioridade baixa.

## Como a análise foi feita

- Leitura da composição (`Startup`, features de infraestrutura), domínio, persistência, tenancy, handlers de consulta, serviços do web e workflows.
- Varredura por arquivos grandes, marcadores de dívida, dependências declaradas sem uso, duplicações e testes por área.
- Conferência de migrações e snapshot do EF para o comportamento de `row_version`.
- **Não executei as suítes de teste nem alterei código.** D01 foi deduzido da migração e da configuração; a confirmação final exige o teste de concorrência sugerido.

## Resumo

| ID | Prioridade | Área | Item |
| --- | --- | --- | --- |
| D01 | Alta | API / Persistência | `row_version` em `bytea` nunca é gerado: concorrência otimista inoperante |
| D02 | Alta | API / Tenancy | Isolamento de tenant mantido manualmente em filtros e SQL |
| D03 | Alta | API / Dashboard | Agregações feitas em memória sobre todos os pedidos do período |
| D04 | Média | Testes | Consultas quase sem testes de handler; web sem cobertura medida |
| D05 | Média | API / HTTP | Middleware de tenant com rotas públicas fixas e 401 fora do padrão |
| D06 | Média | API / Config | CORS lê variável de ambiente diretamente e tem fallback silencioso |
| D07 | Média | Web / Arquitetura | `shared` depende de `features/auth`; token lido em dois lugares |
| D08 | Média | Web / Contrato | Tipos do contrato HTTP duplicados à mão, sem geração a partir do OpenAPI |
| D09 | Média | Web / Componentes | Componentes de formulário e lista acima de 350 linhas |
| D10 | Média | API / Build | Pacotes sem uso e ausência de política de analyzers/warnings |
| D11 | Baixa | CI | Sonar obrigatório sem segredo, `tsc` duplicado, lógica de SemVer repetida |
| D12 | Baixa | API / Persistência | Migrações `v001`…`v019` sem nome descritivo |
| D13 | Baixa | API / SQL | SQL com literais de enum e parâmetro sem uso |
| D14 | Baixa | Repositório | Skills duplicadas em `.agents` e `.claude`; pequenos resíduos de código |

## Prioridade alta

### D01 — Concorrência otimista inoperante

**Onde:** `Infrastructure/Persistence/Configurations/{Order,OrderPayment,CashSession,CashSessionMovement}Configuration.cs` e a migração `20260115231434_v019.cs`.

**Situação:** as quatro entidades declaram `builder.Property<byte[]>("RowVersion").IsRowVersion()`. A migração cria `row_version bytea NULL` sem default nem trigger. O PostgreSQL não tem `rowversion` nativo como o SQL Server, então a coluna permanece `NULL`. O EF trata o valor como gerado pelo banco e nunca o envia. No `UPDATE`, ele filtra por `row_version IS NULL`, condição que sempre é verdadeira. Não há tratamento de `DbUpdateConcurrencyException` no código e nenhum teste exercita conflito.

**Impacto:** duas operações simultâneas no mesmo pedido ou sessão de caixa terminam em *last write wins*, justamente nos agregados financeiros que motivaram a v019 (que também deduplicou movimentos de pagamento). A falsa sensação de proteção é pior que a ausência declarada.

**Correção sugerida:** usar o `xmin` do PostgreSQL (`uint` sombra com `IsRowVersion()`, que o Npgsql mapeia para a coluna de sistema), remover as colunas `row_version` e mapear `DbUpdateConcurrencyException` para `409 Conflict` no `GlobalExceptionHandler`.

**Critério de aceite:** um teste com Testcontainers que carrega o mesmo `CashSession` em dois contextos, grava os dois e espera conflito na segunda gravação.

### D02 — Isolamento de tenant mantido à mão

**Onde:** `ApplicationDbContext.ApplyQueryFilters`, `TenantAccessor` e os sete handlers Dapper (`Features/Dashboard/Queries/GetExpense*`, `GetUpcomingExpenses`, `Features/Expenses/Queries/GetAllExpenses`).

**Situação:**

- Os filtros globais são uma lista explícita de oito entidades. `OrderPayment` tem `EstablishmentId`, mas não tem filtro, e o `DbSet<OrderPayment>` é público. Hoje só é acessado via `Include` de `Order` filtrado, o que é seguro por acidente, não por construção.
- Dapper ignora os filtros do EF. Cada SQL repete `where e.establishment_id = @EstablishmentId`. Todos os sete estão corretos hoje, mas nada falha se o próximo esquecer.
- `TenantAccessor.Tenant` é `default` (`Guid.Empty`) até `Register`. Se um caminho novo pular o middleware (job, evento assíncrono, endpoint público), as consultas retornam vazio em vez de falhar.

O ADR-002 já aceita o filtro explícito no SQL Dapper como consequência da decisão. A dívida não é a escolha, e sim a ausência de verificação automática.

**Impacto:** o ADR-002 e o AGENTS.md tratam isolamento de tenant como requisito. Uma regressão aqui vaza dados entre estabelecimentos sem nenhum sinal visível.

**Correção sugerida:**

- Criar uma interface `ITenantScoped { Guid EstablishmentId }` e aplicar o filtro por convenção a todas as entidades que a implementam.
- Fazer `TenantAccessor.Tenant` lançar exceção quando nenhum tenant estiver registrado.
- Adicionar um teste de arquitetura com duas verificações: toda entidade com `EstablishmentId` tem filtro, e todo handler que usa `IDbConnectionFactory` referencia `establishment_id`. Isso também pode ser feito por um teste HTTP multi-tenant por endpoint de consulta.

### D03 — Dashboard agrega em memória

**Onde:** `GetDashboardStatsHandler`, `GetPaymentBreakdownHandler`, `GetSalesOverTime`, `GetTopProducts`, `GetOrdersByStatus` e os handlers Dapper de despesas.

**Situação:** os handlers EF fazem `ToListAsync()` de todos os pedidos do intervalo, com `Include(o => o.Payments)` no breakdown, e só então calculam `Count`/`Sum`/`GroupBy` em C#. Os handlers Dapper trazem cada despesa e somam em memória. Quando `StartDate`/`EndDate` são nulos, o volume é o histórico inteiro.

**Impacto:** com poucos meses de operação o custo passa despercebido. Ele cresce linearmente com o histórico, a cada abertura do dashboard e a cada refetch do TanStack Query, e em vários endpoints em paralelo.

**Correção sugerida:** projetar e agregar no banco (`GroupBy(...).Select(g => new { g.Key, Sum = g.Sum(...) })`, traduzível pelo EF, ou `sum`/`count ... filter (where ...)` no SQL). Isso pode entrar no refactor de consultas que já está planejado, sem precisar deduplicar handlers. Avaliar também um limite padrão de intervalo quando nenhuma data é enviada.

## Prioridade média

### D04 — Lacunas de teste e de visibilidade

- **API:** são 21 handlers de consulta e apenas um tem teste próprio (`GetAllExpensesHandlerTests`). O restante é coberto indiretamente por `DashboardApiTests` e pelos testes HTTP das features. As regras de cálculo do dashboard (troco subtraído do dinheiro, status considerados "pendentes", datas de despesa paga/pendente) são as que mais mudaram em B03/B10 e merecem testes diretos.
- `LoginHandlerTests` usa `UseInMemoryDatabase`, a única exceção à política de PostgreSQL/Testcontainers. Por causa dela o projeto de testes carrega `Microsoft.EntityFrameworkCore.InMemory`.
- **Web:** `features/auth` (login, sessão, expiração) e `features/landing` não têm testes. `products` tem um teste de página.
- `web-app-ci.yaml` define `sonar.coverage.exclusions=**/*`, então o Sonar não mede cobertura do frontend. Não há gate numérico (decisão documentada), mas também não há visibilidade da evolução.

### D05 — Middleware de tenant

**Onde:** `Infrastructure/Http/Middlewares/TenantRegisterMiddleware.cs`.

- As rotas públicas (`/scalar`, `/openapi`, `/health`, `/alive`, `/api/v1/auth/login`) estão fixas num array, duplicando o `AllowAnonymous` dos endpoints. Um novo endpoint anônimo exige lembrar de editar o middleware.
- O `try/catch (UnauthorizedAccessException)` envolve também o `await next(context)`. Qualquer `UnauthorizedAccessException` lançada adiante vira um 401 de corpo vazio, sem Problem Details e sem log, contornando o `GlobalExceptionHandler`.

**Correção sugerida:** decidir pela metadata do endpoint (`context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>()`) e restringir o `try` à resolução do tenant.

### D06 — Configuração de CORS fora do `IConfiguration`

**Onde:** `Infrastructure/Http/Configuration/CorsConfiguration.cs`.

- `Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")` ignora user secrets, appsettings e o padrão de `docs/development/configuration.md`.
- O fallback `http://localhost:5173` vale também em produção. Uma variável esquecida não impede a API de subir; ela só bloqueia o frontend.
- `AllowCredentials()` é desnecessário com autenticação por header `Authorization` e amplia a superfície sem benefício.

**Correção sugerida:** criar um `CorsOptions` validado na inicialização (`ValidateOnStart`), como `JwtTokenSettings`, sem fallback fora de Development.

### D07 — Dependência invertida no web e token duplicado

- `src/shared/services/api.ts` importa `AUTH_STORAGE_KEY` de `@/features/auth/services/auth-service`. O código compartilhado passa a depender de uma feature.
- O token é lido de `localStorage` em `api.ts` (`getAuthToken`) e em `auth-service.ts`, com o mesmo parse duplicado.
- O JWT não tem renovação. Ao expirar (`ExpirationInMinutes`), o atendente é levado ao login no meio do turno e um formulário aberto pode ser perdido. É uma decisão de produto/segurança que convém registrar.

**Correção sugerida:** mover o armazenamento da sessão para `shared` (ex.: `shared/services/auth-storage.ts`), consumido pela feature e pelo cliente HTTP.

### D08 — Contrato HTTP duplicado à mão

Os tipos de resposta do web (`features/*/types`, `ApiResponse`, `Metadata`, `PaginatedResponse`) são escritos manualmente, enquanto a API já publica `/openapi/v1.json`. Mudanças de contrato só aparecem em runtime ou nos testes MSW, que também são mantidos à mão.

**Correção sugerida:** gerar os tipos a partir do OpenAPI (ex.: `openapi-typescript`) num script versionado e, no CI, falhar se o arquivo gerado divergir.

### D09 — Componentes grandes no web

| Arquivo | Linhas |
| --- | --- |
| `features/orders/components/order-form.tsx` | 554 |
| `features/expenses/components/expense-form.tsx` | 404 |
| `features/expenses/components/expense-list.tsx` | 396 |
| `features/expenses/components/create-category-dialog.tsx` | 361 |
| `shared/components/autocomplete-select.tsx` | 353 |
| `features/cash/components/cash-summary-card.tsx` | 350 |

`order-form.tsx` mistura seleção/consolidação de itens, pagamentos, troco, proteção contra descarte (B07) e layout. Extrair as regras para hooks (`use-order-items`, `use-order-payments`) facilita testar o que causou B01/B06 sem renderizar o formulário inteiro. Como a 2.0 vai redesenhar essas telas, vale fazer a extração junto com a migração visual, não antes.

### D10 — Higiene de build da API

- `Microsoft.Extensions.Http.Resilience` e `Microsoft.Extensions.ServiceDiscovery` estão referenciados em `Devlivery.csproj` sem nenhum uso (`AddServiceDiscovery`/`AddStandardResilienceHandler` não aparecem no código).
- Não há `Directory.Build.props` com `TreatWarningsAsErrors`, `AnalysisLevel` ou `EnforceCodeStyleInBuild`. A qualidade depende do Sonar, que roda só no CI.
- Os testes usam xUnit v2 (`2.9.3`), e a v3 já é a linha mantida. A migração é baixa prioridade, mas fica mais cara conforme a suíte cresce.

Mantenha o `Directory.Build.props` dentro de `apps/api` e `apps/backup` separadamente, para preservar a independência do backup.

## Prioridade baixa

### D11 — CI

- `api-ci.yaml` e `web-app-ci.yaml` executam o Sonar sem condição. Em PRs do Dependabot ou de forks, sem `SONAR_TOKEN`, o job inteiro falha. O build e os testes deveriam passar independentemente da análise.
- O web roda `tsc` duas vezes: no passo "Check types" e dentro de `pnpm build` (`tsc -b`).
- O cálculo de SemVer está duplicado entre `pull-request-validation.yaml` e `release.yaml`, com pequenas diferenças (`Number` vs `parseInt`, `raw`).
- O CI do backup constrói a imagem Docker; o da API não constrói o `Dockerfile` dela, então uma quebra só aparece no deploy.

### D12 — Migrações

As 19 migrações chamam-se `v001`…`v019`. O histórico não diz o que cada uma faz, o que dificulta revisão e rollback. Para as próximas, use nomes descritivos, como o guia de desenvolvimento local já sugere (`NomeDaMigracao`). A publicação já usa *migration bundles* gerados no Dockerfile e executados no pre-deploy.

### D13 — SQL acoplado a nomes de enum

Os handlers Dapper comparam `e.status != 'Cancelled'` e `= 'Paid'` como literais. Renomear um membro de `ExpenseStatus` quebra a consulta sem erro de compilação. Use `nameof(ExpenseStatus.X)` como parâmetro. `GetExpenseSummaryHandler` também envia `@Today` sem usá-lo no SQL.

### D14 — Resíduos no repositório

- `.agents/skills/shadcn` e `.claude/skills/shadcn` são idênticos (46 arquivos versionados nas duas pastas). Prefira uma fonte única com link ou sincronização.
- `AddHttpContextAccessor()` é chamado em `Startup` e em `TenancyFeature`. O `ApplyConfiguration` lista as configurações uma a uma; `ApplyConfigurationsFromAssembly` eliminaria o passo manual.

## Dívidas aceitas e documentadas

Estes pontos não são dívida oculta e não entram na priorização acima:

- Rate limit de login em memória do processo (`docs/operations/login-and-cash-security.md`): aceitável com uma instância, exige revisão ao escalar horizontalmente.
- Eventos de domínio síncronos no mesmo `SaveChanges` (ADR-003).
- Ausência de gate numérico de cobertura no CI.
- Deduplicação dos handlers de consulta: refactor já planejado à parte.

## Ordem sugerida

1. **D01** com o teste de conflito: risco financeiro real e correção pequena (uma migração e o mapeamento para 409).
2. **D02**: `ITenantScoped`, guarda no `TenantAccessor` e teste de arquitetura.
3. **D05 e D06**: ajustes locais de baixo risco, que podem ir no mesmo PR de hardening.
4. **D03 e D13**: junto com o refactor de consultas, acompanhados dos testes de handler de D04.
5. **D07, D08 e D09**: junto com a migração das telas para a 2.0.
6. **D10, D11, D12 e D14**: em PRs de manutenção conforme houver espaço.
