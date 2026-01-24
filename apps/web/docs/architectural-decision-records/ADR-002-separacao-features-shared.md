# Separação entre Features e Shared Resources

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** C4 Model - Nível 2 (Container) / Organização de Código

## Contexto e Problema

Dentro da organização por features, é necessário estabelecer critérios claros para decidir o que pertence a uma feature específica versus o que deve ser compartilhado entre múltiplas features. Sem critérios bem definidos, há risco de criar acoplamento desnecessário ou duplicação excessiva.

A estrutura do repositório revela esta decisão através da organização:

```
src/
├── features/              # Domínios de negócio (isolados)
│   ├── auth/            # Autenticação
│   ├── products/        # Gestão de produtos
│   ├── orders/          # Gestão de pedidos
│   ├── dashboard/       # Dashboard e relatórios
│   ├── cash/            # Gestão de caixa
│   └── expenses/        # Gestão de despesas
└── shared/              # Recursos compartilhados
    ├── components/      # UI components reutilizáveis
    ├── services/        # Serviços de infraestrutura
    ├── contexts/        # Contextos React globais
    ├── hooks/           # Hooks compartilhados
    └── utils/           # Utilitários genéricos
```

**Problema:** Como determinar quando um recurso deve ficar em uma feature específica versus ser movido para `shared/`?

## Opções Consideradas

* **Tudo em features** - Cada feature é completamente autossuficiente, mesmo com duplicação
* **Tudo em shared** - Maximizar reutilização, colocando tudo compartilhável em `shared/`
* **Separação criteriosa** - Features contêm lógica de domínio, `shared/` contém infraestrutura e UI genérica

## Decisão

**Escolhida:** "Separação criteriosa", porque:

1. **Features são Domínios:** Features contêm lógica específica de negócio que não deve ser compartilhada
2. **Shared é Infraestrutura:** `shared/` contém recursos técnicos e UI genérica que não pertencem a um domínio específico
3. **Baixo Acoplamento:** Features não dependem de outras features, apenas de `shared/`
4. **Reutilização Controlada:** Componentes e serviços genéricos são compartilhados, mas lógica de negócio permanece isolada

### Implementação Técnica

A decisão se materializa em:

**Critério de Separação:**

1. **Features (`src/features/{feature}/`):**
   - Componentes específicos do domínio (ex: `ProductCard`, `OrderForm`)
   - Serviços de API específicos (ex: `product-service.ts`, `order-service.ts`)
   - Hooks que encapsulam lógica de domínio (ex: `use-products.ts`, `use-orders.ts`)
   - Types/interfaces do domínio (ex: `Product`, `Order`)
   - Páginas/rotas da feature

2. **Shared (`src/shared/`):**
   - Componentes UI genéricos (ex: `Button`, `Modal`, `Input`, `Card`)
   - Serviços de infraestrutura (ex: `api.ts` - cliente HTTP, `auth-events.ts` - eventos globais)
   - Contextos globais (ex: `AuthContext`)
   - Hooks utilitários genéricos (ex: `use-date-range-filter.ts`)
   - Utilitários (ex: `formatters.ts` - formatação de datas/moeda)

```
src/features/products/
├── components/
│   └── product-card.tsx        # ← Específico de produtos
├── services/
│   └── product-service.ts      # ← API de produtos
└── hooks/
    └── use-products.ts         # ← Lógica de produtos

src/shared/
├── components/
│   ├── layout.tsx              # ← Layout genérico
│   ├── modal.tsx               # ← Modal genérico
│   └── ui/
│       └── button.tsx          # ← Componente base
├── services/
│   └── api.ts                  # ← Cliente HTTP genérico
└── contexts/
    └── auth-context.tsx        # ← Contexto global
```

**Regra de Ouro:**
- Se o recurso é usado por **2+ features** e é **genérico/tecnológico** → `shared/`
- Se o recurso é usado por **2+ features** mas contém **lógica de negócio** → considerar extrair para um módulo de domínio compartilhado (não aplicável no momento)
- Se o recurso é usado por **1 feature** → fica na feature

### Consequências

* ✅ **Bom:** Features permanecem independentes e desacopladas
* ✅ **Bom:** Recursos técnicos são reutilizados sem duplicação
* ✅ **Bom:** Fácil identificar onde fazer mudanças (feature vs shared)
* ✅ **Bom:** `shared/` permanece estável, features evoluem independentemente
* ⚠️ **Neutro:** Pode ser necessário mover código de `shared/` para feature quando uso se torna específico
* ⚠️ **Ruim:** Requer disciplina para não colocar lógica de negócio em `shared/`
* ⚠️ **Ruim:** Pode haver tentação de criar "shared features" (evitar - usar composição)

