# Separação Física entre Aplicação e Testes

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Organização de Código e Dependências

## Contexto e Problema

Em projetos .NET, testes podem ser organizados de várias formas: dentro do mesmo projeto da aplicação, em um projeto separado na mesma pasta, ou em uma estrutura de diretórios completamente separada. Cada abordagem tem trade-offs em termos de dependências, build times e clareza de separação de responsabilidades.

A estrutura atual do repositório mostra:

```
webapi/
├── src/
│   └── Devlivery/
│       └── Devlivery.csproj      # Projeto principal
└── test/
    └── Devlivery.Tests/
        └── Devlivery.Tests.csproj # Projeto de testes
```

**Problema:** Como organizar código de produção e código de testes para garantir isolamento de dependências e facilitar builds de CI/CD?

## Opções Consideradas

* **Testes no mesmo projeto** - Uma única `.csproj` com testes e código de produção
* **Testes em projeto paralelo (src/)** - `src/Devlivery` e `src/Devlivery.Tests` lado a lado
* **Testes em hierarquia separada (test/)** - Pastas raiz `src/` e `test/` distintas

## Decisão

**Escolhida:** "Testes em hierarquia separada (test/)", porque:

1. **Isolamento de Dependências:** Pacotes de teste (xUnit, NSubstitute, Testcontainers) não poluem o projeto de produção
2. **Build Otimizado:** CI/CD pode buildar `src/` e `test/` separadamente, paralelizando o pipeline
3. **Clareza Semântica:** Separação física reforça a separação conceitual entre código de produção e testes
4. **Artefatos Limpos:** O package de produção não contém código de teste
5. **Convenção da Comunidade:** Padrão adotado pela maioria de projetos .NET open-source

### Implementação Técnica

**Estrutura de Diretórios:**
```
webapi/
├── src/                         # Código de produção
│   └── Devlivery/
│       ├── Features/
│       ├── Shared/
│       └── Devlivery.csproj
│
└── test/                        # Código de testes
    └── Devlivery.Tests/
        ├── Features/            # Testes organizados por feature
        │   ├── Products/
        │   ├── Orders/
        │   └── CashRegister/
        ├── Common/              # Fixtures e builders reutilizáveis
        │   ├── BaseWebApplicationFactory.cs
        │   ├── WebApiBaseFixture.cs
        │   └── Builders/
        └── Devlivery.Tests.csproj
```

**Referência de Projeto:**
```xml
<!-- Devlivery.Tests.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\src\Devlivery\Devlivery.csproj"/>
</ItemGroup>
```

**Dependências Exclusivas de Teste:**
```xml
<!-- Apenas em Devlivery.Tests.csproj -->
<PackageReference Include="xunit" Version="2.9.3"/>
<PackageReference Include="NSubstitute" Version="5.3.0"/>
<PackageReference Include="Shouldly" Version="4.3.0"/>
<PackageReference Include="Testcontainers.PostgreSql" Version="4.9.0"/>
<PackageReference Include="Bogus" Version="35.6.5"/>
```

**Makefile Targets:**
```makefile
TEST_PROJECT = test/Devlivery.Tests/Devlivery.Tests.csproj

test:
	$(DOTNET_CMD) test $(TEST_PROJECT) --logger "console;verbosity=normal"

test-coverage:
	$(DOTNET_CMD) test $(TEST_PROJECT) --collect:"XPlat Code Coverage"
```

### Consequências

* ✅ **Bom:** Dependências de teste não são incluídas em builds de produção
* ✅ **Bom:** CI/CD pode cachear builds de `src/` e `test/` independentemente
* ✅ **Bom:** Package final (`dotnet publish`) não contém assemblies de teste
* ✅ **Bom:** Desenvolvedores podem executar apenas testes relevantes sem carregar código de produção
* ✅ **Bom:** Estrutura escalável para múltiplos projetos de teste (integration, e2e, performance)
* ⚠️ **Neutro:** Requer caminho relativo para referência de projeto (`..\..\src\...`)
* ⚠️ **Ruim:** Mudanças que afetam ambos projetos requerem commits em múltiplos diretórios

### Convenção de Nomenclatura

- Projeto de produção: `Devlivery`
- Projeto de testes: `Devlivery.Tests` (sufixo `.Tests`)
- Namespaces de teste espelham a estrutura de produção: `Devlivery.Features.Products.Tests`

**Princípio:** "Production code and test code have different lifecycles and dependencies—honor this with physical separation."
