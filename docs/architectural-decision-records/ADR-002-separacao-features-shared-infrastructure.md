# Separação entre Features e Shared Infrastructure

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** C4 Model - Nível 2 (Containers) / Padrão Arquitetural

## Contexto e Problema

Em uma arquitetura Vertical Slice, features devem ser autocontidas, mas ainda precisam compartilhar infraestrutura comum (persistência, autenticação, observabilidade, etc.). Sem uma separação clara, código compartilhado pode se espalhar pelas features, criando acoplamento indesejado e dificultando manutenção.

A estrutura do repositório revela esta decisão através da organização:

```
src/Devlivery/
├── Features/                       # Domínio de negócio (features específicas)
│   ├── Products/
│   ├── Orders/
│   └── ...
└── Shared/                         # Infraestrutura compartilhada
    ├── Application/               # Abstrações e comportamentos
    │   ├── Abstractions/
    │   └── Behaviors/
    └── Infrastructure/             # Implementações técnicas
        ├── Persistence/
        ├── Identity/
        ├── Tenancy/
        ├── Observability/
        └── ...
```

**Problema:** Como separar código específico de features do código compartilhado (cross-cutting concerns) sem criar dependências circulares ou acoplamento excessivo?

## Opções Consideradas

* **Tudo em Features** - Cada feature implementa sua própria infraestrutura (duplicação)
* **Tudo em Shared** - Toda infraestrutura fica compartilhada (acoplamento)
* **Separação Features/Shared** - Features contêm lógica de negócio; Shared contém infraestrutura comum
* **Módulos Separados (NuGet)** - Extrair Shared para pacotes NuGet (complexidade desnecessária para monólito)

## Decisão

**Escolhida:** "Separação Features/Shared", porque:

1. Mantém features focadas em lógica de negócio, sem preocupação com detalhes de infraestrutura
2. Centraliza código compartilhado em `Shared/`, facilitando manutenção e evolução
3. Permite que features dependam de `Shared/`, mas `Shared/` não depende de features específicas
4. Facilita testes: infraestrutura pode ser mockada através de abstrações em `Shared/Application/Abstractions`
5. Alinha com princípios de Clean Architecture: dependências apontam para dentro (features → shared)

### Implementação Técnica

A decisão se materializa em:

**Estrutura de Shared:**
```
Shared/
├── Application/                    # Camada de aplicação (abstrações)
│   ├── Abstractions/
│   │   └── IUnitOfWork.cs
│   └── Behaviors/
│       └── ValidationPipelineBehavior.cs
└── Infrastructure/                 # Camada de infraestrutura (implementações)
    ├── Persistence/
    │   ├── Context/
    │   ├── Configurations/
    │   ├── Factory/
    │   └── UnitOfWork.cs
    ├── Identity/
    │   ├── Context/
    │   ├── Tokens/
    │   └── Users/
    ├── Tenancy/
    │   ├── TenantAccessor.cs
    │   └── Middleware/
    ├── Observability/
    └── WebServer/
```

**Regra de Dependência:**
- `Features/*` → pode depender de `Shared/*`
- `Shared/*` → **não** pode depender de `Features/*`
- `Shared/Application` → pode depender de `Shared/Infrastructure`

**Exemplo de Uso em Feature:**
```csharp
// Features/Products/Commands/CreateProduct/CreateProductHandler.cs
public sealed class CreateProductHandler(
    IProductRepository repo,           // Feature-specific
    IUnitOfWork unitOfWork,             // Shared abstraction
    ITenantAccessor tenantAccessor)     // Shared infrastructure
    : ICommandHandler<...>
{
    // ...
}
```

### Consequências

* ✅ **Bom:** Separação clara de responsabilidades: features = negócio, shared = infraestrutura
* ✅ **Bom:** Facilita manutenção de código compartilhado (persistência, autenticação, etc.)
* ✅ **Bom:** Permite evolução independente: mudanças em shared não quebram features se interfaces forem mantidas
* ✅ **Bom:** Facilita testes: abstrações em `Shared/Application` podem ser mockadas
* ⚠️ **Neutro:** Requer disciplina para não criar dependências de Shared → Features
* ⚠️ **Ruim:** Pode ser tentador colocar lógica de negócio em Shared (deve ser evitado)
* ⚠️ **Ruim:** Se Shared crescer muito, pode ser necessário considerar extração para módulos separados

