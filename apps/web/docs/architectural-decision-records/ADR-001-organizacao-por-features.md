# Organização por Features (Feature-Based Architecture)

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** C4 Model - Nível 2 (Container) / Organização de Código

## Contexto e Problema

O frontend do Devlivery precisa de uma estrutura que facilite a manutenção, permita escalabilidade do time e mantenha baixo acoplamento entre diferentes áreas de funcionalidade. A decisão fundamental é: devemos organizar o código por tipo técnico (components/, services/, hooks/) ou por domínio de negócio (features/)?

A estrutura do repositório revela esta decisão através da organização:

```
src/
├── features/              # ← Organização por domínio de negócio
│   ├── auth/
│   ├── products/
│   ├── orders/
│   ├── dashboard/
│   ├── cash/
│   └── expenses/
└── shared/                # ← Recursos compartilhados
    ├── components/
    ├── services/
    ├── contexts/
    ├── hooks/
    └── utils/
```

**Problema:** Como organizar o código frontend para maximizar coesão dentro de features e minimizar acoplamento entre elas?

## Opções Consideradas

* **Organização por tipo técnico** - Separar por camadas (components/, services/, hooks/) na raiz
* **Organização por features** - Agrupar por domínio de negócio, com cada feature contendo seus próprios components/, services/, hooks/, types/
* **Híbrido** - Mistura de ambos os padrões

## Decisão

**Escolhida:** "Organização por features", porque:

1. **Coesão Alta:** Tudo relacionado a um domínio (ex: Products) fica junto, facilitando navegação e compreensão
2. **Baixo Acoplamento:** Features são independentes entre si, reduzindo risco de mudanças em cascata
3. **Escalabilidade de Time:** Diferentes desenvolvedores podem trabalhar em features diferentes sem conflitos frequentes
4. **Facilidade de Refatoração:** Features podem ser extraídas, movidas ou removidas com menor impacto
5. **Developer Experience:** Fácil localizar código relacionado a uma funcionalidade específica

### Implementação Técnica

A decisão se materializa em:

1. **Estrutura de Features:** Cada pasta em `src/features/` representa um domínio de negócio
2. **Estrutura Interna Consistente:** Cada feature segue o mesmo padrão de subpastas
3. **Shared Resources:** Componentes, serviços e utilitários compartilhados ficam em `src/shared/`

```
src/features/products/
├── components/            # Componentes específicos de produtos
│   ├── product-card.tsx
│   ├── product-form.tsx
│   └── products-filters.tsx
├── pages/                # Páginas/rotas da feature
│   └── products-page.tsx
├── services/              # Lógica de comunicação com API
│   └── product-service.ts
├── hooks/                 # React Query hooks customizados
│   └── use-products.ts
└── types/                 # TypeScript types/interfaces
    └── index.ts

src/shared/                # Recursos compartilhados
├── components/            # Componentes UI reutilizáveis
│   ├── layout.tsx
│   ├── modal.tsx
│   └── ui/                # Componentes base (shadcn/ui)
├── services/              # Serviços compartilhados
│   ├── api.ts            # Cliente HTTP base
│   └── auth-events.ts
├── contexts/              # Contextos React globais
│   └── auth-context.tsx
├── hooks/                 # Hooks compartilhados
│   └── use-date-range-filter.ts
└── utils/                 # Funções utilitárias
    └── formatters.ts
```

**Critério de Separação:**
- Se é usado por **apenas uma feature** → fica dentro da feature
- Se é usado por **múltiplas features** → vai para `shared/`

### Consequências

* ✅ **Bom:** Código relacionado fica co-localizado, facilitando manutenção
* ✅ **Bom:** Novos desenvolvedores encontram rapidamente onde fazer mudanças
* ✅ **Bom:** Features podem ser desenvolvidas/refatoradas independentemente
* ✅ **Bom:** Reduz risco de acoplamento acidental entre features
* ⚠️ **Neutro:** Alguma duplicação pode ocorrer (aceitável se mantém features independentes)
* ⚠️ **Ruim:** Pode ser tentador colocar tudo em `shared/` (requer disciplina)
* ⚠️ **Ruim:** Migração de código de `shared/` para `features/` pode ser necessária quando uso se torna específico

