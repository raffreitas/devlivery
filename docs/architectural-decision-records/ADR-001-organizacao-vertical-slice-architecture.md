# Organização por Vertical Slice Architecture (Features)

**Data:** 2025-01-27  
**Status:** Aceito  
**Contexto:** C4 Model - Nível 2 (Containers) / Padrão Arquitetural

## Contexto e Problema

Em projetos monólitos tradicionais, a organização por camadas (Controllers → Services → Repositories → Domain) cria acoplamento horizontal entre features. Quando uma feature precisa ser modificada, mudanças se espalham por múltiplas camadas, dificultando manutenção e evolução independente de funcionalidades.

A estrutura do repositório revela esta decisão através da organização:

```
src/Devlivery/
├── Features/
│   ├── Auth/
│   ├── Products/
│   │   ├── ProductFeature.cs
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Domain/
│   │   └── Infrastructure/
│   ├── Orders/
│   ├── CashRegister/
│   └── ...
└── Shared/
```

**Problema:** Como organizar o código de um monólito modular para permitir que features sejam desenvolvidas, testadas e evoluídas de forma independente, minimizando acoplamento entre funcionalidades?

## Opções Consideradas

* **Organização por Camadas (Layered Architecture)** - Separar por responsabilidade técnica (Controllers, Services, Repositories, Domain)
* **Organização por Domínios (Domain-Driven Design - Bounded Contexts)** - Agrupar por domínio de negócio com contextos delimitados
* **Vertical Slice Architecture (Features)** - Organizar por feature completa, com cada feature contendo sua própria pilha vertical (endpoint → handler → domain → repository)

## Decisão

**Escolhida:** "Vertical Slice Architecture (Features)", porque:

1. Cada feature é autocontida e possui sua própria pilha vertical completa, reduzindo acoplamento entre features
2. Facilita evolução independente: mudanças em uma feature não afetam outras
3. Melhora a navegabilidade do código: toda a lógica relacionada a uma funcionalidade está em um único local
4. Permite escalar o time: diferentes desenvolvedores podem trabalhar em features diferentes sem conflitos frequentes
5. Alinha com princípios de Domain-Driven Design, mantendo agregações e lógica de negócio próximas

### Implementação Técnica

A decisão se materializa em:

**Estrutura de uma Feature:**
```
Features/Products/
├── ProductFeature.cs              # Bootstrap: DI registration + endpoint mapping
├── Commands/                       # Operações de escrita (CUD)
│   └── CreateProduct/
│       ├── CreateProductCommand.cs
│       ├── CreateProductHandler.cs
│       ├── CreateProductValidator.cs
│       ├── CreateProductEndpoint.cs
│       └── CreateProductResponse.cs
├── Queries/                        # Operações de leitura (R)
│   └── GetAllProducts/
│       ├── GetAllProductsQuery.cs
│       ├── GetAllProductsHandler.cs
│       └── GetAllProductsEndpoint.cs
├── Domain/                         # Lógica de negócio
│   ├── Product.cs                  # Entity
│   └── IProductRepository.cs
└── Infrastructure/
    └── ProductRepository.cs        # Implementação EF Core
```

**Registro no Startup:**
```csharp
// Startup.cs
services.AddProductFeature();        // Registra dependências
app.MapProductEndpoints();           // Mapeia endpoints
```

**Princípio Fundamental:** Cada feature possui sua própria pilha completa — do endpoint HTTP até o repositório de dados. Features não dependem diretamente de outras features, apenas compartilham infraestrutura através de `Shared/`.

### Consequências

* ✅ **Bom:** Reduz acoplamento horizontal entre features, facilitando manutenção
* ✅ **Bom:** Melhora a navegabilidade: toda lógica de uma feature está em um único local
* ✅ **Bom:** Permite evolução independente de features sem impactar outras funcionalidades
* ✅ **Bom:** Facilita onboarding: desenvolvedores podem focar em uma feature por vez
* ⚠️ **Neutro:** Pode haver alguma duplicação de código entre features (trade-off aceitável)
* ⚠️ **Ruim:** Requer disciplina para não criar dependências cruzadas entre features
* ⚠️ **Ruim:** Pode ser mais difícil encontrar código compartilhado se não estiver bem documentado em `Shared/`

