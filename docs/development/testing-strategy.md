# Estratégia de testes

## Objetivo

Os testes devem proteger invariantes de domínio, comportamento dos casos de uso, isolamento por tenant e integrações que dependem do PostgreSQL. A intensidade da verificação deve acompanhar o risco da mudança.

## Níveis atuais

| Nível | Responsabilidade | Implementação atual |
| --- | --- | --- |
| Domínio | Estados, cálculos, invariantes e idempotência | xUnit e Shouldly, sem infraestrutura |
| Handler e validação | Orquestração, resultados, dependências e entradas inválidas | xUnit, NSubstitute e FluentValidation |
| Integração de persistência | SQL, filtros e consultas que dependem do PostgreSQL | Testcontainers PostgreSQL em cenários específicos |
| Contrato HTTP | Pipeline, autenticação, serialização e status HTTP | Sem suíte dedicada no checkout atual |
| Frontend | Componentes e fluxos da interface | Sem suíte automatizada no checkout atual |

O pacote `coverlet.collector` gera cobertura no CI e o SonarQube recebe o relatório. Não existe limite numérico de cobertura configurado; cobertura não substitui cenários de comportamento.

## Onde adicionar testes

- Mudança em agregado ou Value Object: teste de domínio para sucesso, rejeição e fronteiras relevantes.
- Mudança em comando, consulta ou validator: teste da classe correspondente, incluindo falhas esperadas.
- Mudança em SQL, Dapper, mapeamento EF ou filtro de tenant: teste com PostgreSQL real via Testcontainers.
- Mudança em autenticação, middleware ou representação HTTP: preferir teste de contrato HTTP; essa camada ainda é uma lacuna da suíte.
- Mudança no frontend: executar lint e build e verificar manualmente o fluxo afetado enquanto não houver testes automatizados.

Os testes backend ficam em `apps/api/test/Devlivery.Tests` e espelham as features. Use nomes que expressem comportamento e seções Arrange, Act e Assert.

## Verificação local

Com dependências restauradas e Docker disponível, a partir da raiz:

```powershell
dotnet test apps/api/Devlivery.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
pnpm --dir apps/web lint
pnpm --dir apps/web build
git diff --check
```

Testcontainers exige acesso ao daemon Docker. Uma falha ao iniciar o container é ambiental e não comprova falha da regra testada; registre a limitação se não for possível repetir a execução em ambiente válido.

## Critério de conclusão

Uma mudança está verificada quando:

1. possui regressão automatizada no nível adequado, quando essa infraestrutura existe;
2. as suítes relacionadas passam;
3. lint, tipos e build das aplicações afetadas passam;
4. fluxos sem automação são verificados manualmente e informados no pull request;
5. a documentação de produto, domínio ou operação foi atualizada quando o comportamento mudou.
