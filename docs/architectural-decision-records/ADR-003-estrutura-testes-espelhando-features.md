# Estrutura de Testes Espelhando Features

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** C4 Model - Nível 2 (Containers) / Padrão de Testes

## Contexto e Problema

Em projetos grandes, encontrar testes relacionados a uma feature específica pode ser desafiador se a organização de testes não espelhar a estrutura de produção. Testes espalhados ou organizados por tipo (unitários, integração) dificultam a manutenção e evolução de features.

A estrutura do repositório revela esta decisão através da organização:

```
test/Devlivery.Tests/
├── Common/                          # Infraestrutura de testes compartilhada
│   ├── BaseWebApplicationFactory.cs
│   └── Builders/
└── Features/                        # Espelha src/Devlivery/Features/
    ├── Products/
    │   ├── ProductsWebApplicationFactory.cs
    │   ├── ProductsUnitTestFixture.cs
    │   ├── Commands/
    │   └── Queries/
    ├── Orders/
    │   ├── OrdersWebApplicationFactory.cs
    │   └── Commands/
    └── ...
```

**Problema:** Como organizar testes para facilitar localização, manutenção e evolução de testes relacionados a cada feature?

## Opções Consideradas

* **Organização por Tipo de Teste** - Separar por `UnitTests/`, `IntegrationTests/`, `E2ETests/`
* **Organização por Camada** - Separar por `Controllers/`, `Services/`, `Repositories/`
* **Espelhar Estrutura de Features** - Organizar testes na mesma estrutura de `Features/` em produção
* **Um Projeto de Teste por Feature** - Criar projetos separados (complexidade desnecessária)

## Decisão

**Escolhida:** "Espelhar Estrutura de Features", porque:

1. Facilita localização: testes de uma feature estão no mesmo caminho relativo que o código de produção
2. Melhora manutenção: ao modificar uma feature, desenvolvedores encontram testes relacionados imediatamente
3. Alinha com princípio de Vertical Slice: testes fazem parte da "fatia vertical" da feature
4. Permite evolução independente: cada feature pode ter sua própria estratégia de testes
5. Facilita refatoração: mover código de uma feature move também seus testes

### Implementação Técnica

A decisão se materializa em:

**Estrutura de Testes:**
```
test/Devlivery.Tests/
├── Common/                          # Infraestrutura compartilhada
│   ├── BaseWebApplicationFactory.cs # Factory base com Testcontainers
│   ├── WebApiBaseFixture.cs         # Fixture base para testes HTTP
│   └── Builders/                    # Builders para entidades de teste
│       ├── ProductBuilder.cs
│       ├── OrderBuilder.cs
│       └── ...
└── Features/
    ├── Products/
    │   ├── ProductsWebApplicationFactory.cs  # Factory específica da feature
    │   ├── ProductsUnitTestFixture.cs        # Fixture para testes unitários
    │   ├── Commands/
    │   │   └── CreateProductTests.cs
    │   ├── Queries/
    │   │   └── GetAllProductsTests.cs
    │   └── Domain/
    │       └── ProductTests.cs
    ├── Orders/
    │   ├── OrdersWebApplicationFactory.cs
    │   └── Commands/
    └── ...
```

**Padrão de Teste: Cada feature pode ter:**
- `XxxWebApplicationFactory.cs` - Factory para testes de integração HTTP
- `XxxUnitTestFixture.cs` - Fixture para testes unitários de domínio
- `Commands/` - Testes de comandos (writes)
- `Queries/` - Testes de queries (reads)
- `Domain/` - Testes de entidades e lógica de negócio

**Exemplo de Uso:**
```csharp
// test/Devlivery.Tests/Features/Products/Commands/CreateProductTests.cs
public class CreateProductTests(ProductsWebApplicationFactory factory)
    : WebApiBaseFixture(factory)
{
    [Fact]
    public async Task CreateProduct_Should_Return_201()
    {
        await ResetDatabaseAsync();
        // ...
    }
}
```

### Consequências

* ✅ **Bom:** Facilita localização de testes relacionados a uma feature específica
* ✅ **Bom:** Melhora manutenção: mudanças em features encontram testes facilmente
* ✅ **Bom:** Alinha com princípio de Vertical Slice: testes são parte da feature
* ✅ **Bom:** Permite evolução independente de estratégias de teste por feature
* ⚠️ **Neutro:** Pode haver alguma duplicação de infraestrutura de teste entre features (aceitável)
* ⚠️ **Ruim:** Requer disciplina para manter estrutura espelhada quando features são reorganizadas
* ⚠️ **Ruim:** Testes de integração entre features podem não ter local óbvio (podem ficar em `Common/` ou feature principal)

