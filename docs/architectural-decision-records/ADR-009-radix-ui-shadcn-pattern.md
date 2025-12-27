# Radix UI + shadcn/ui Pattern para Componentes de UI

**Data:** 2025-12-26
**Status:** Aceito
**Contexto:** Stack Tecnológica / Componentes de UI

## Contexto e Problema

O projeto precisa de componentes de UI acessíveis, customizáveis e com boa DX. A decisão fundamental é: devemos usar uma biblioteca completa (Material-UI, Chakra UI), componentes headless (Radix UI), ou construir do zero?

A estrutura do repositório revela esta decisão através da organização:

```
src/shared/components/ui/        # ← 25 componentes shadcn/ui
components.json                   # ← Configuração shadcn/ui
package.json
└── dependencies
    ├── @radix-ui/react-dialog
    ├── @radix-ui/react-select
    └── [outros @radix-ui/*]     # ← Radix UI como base
```

**Problema:** Como obter componentes acessíveis e customizáveis sem perder controle sobre o código?

## Opções Consideradas

* **Material-UI / Chakra UI** - Bibliotecas completas, mas com estilos pré-definidos e bundle grande
* **Radix UI + shadcn/ui** - Componentes headless (Radix) + implementação copy-paste (shadcn)
* **Construir do zero** - Máximo controle, mas muito trabalho e risco de problemas de acessibilidade

## Decisão

**Escolhida:** "Radix UI + shadcn/ui Pattern", porque:

1. **Acessibilidade:** Radix UI fornece primitivos acessíveis (ARIA, keyboard navigation)
2. **Customização:** Código dos componentes fica no projeto (não é dependência)
3. **Controle:** Pode modificar componentes conforme necessário
4. **Bundle Size:** Apenas primitivos do Radix (menor que bibliotecas completas)
5. **TailwindCSS:** Integração nativa com TailwindCSS (estilização via classes)
6. **Developer Experience:** `components.json` facilita adicionar novos componentes

### Implementação Técnica

A decisão se materializa em:

1. **Radix UI Primitives:** Base acessível (dialog, select, dropdown, etc.)
2. **shadcn/ui Components:** Implementação copy-paste em `src/shared/components/ui/`
3. **TailwindCSS Styling:** Componentes estilizados com TailwindCSS
4. **Configuration:** `components.json` define paths e configurações

```
src/shared/components/ui/
├── button.tsx              # ← Baseado em Radix + TailwindCSS
├── dialog.tsx              # ← Baseado em @radix-ui/react-dialog
├── select.tsx              # ← Baseado em @radix-ui/react-select
├── dropdown-menu.tsx       # ← Baseado em @radix-ui/react-dropdown-menu
└── [20+ outros componentes]

components.json
{
  "style": "default",
  "rsc": false,
  "tsx": true,
  "tailwind": {
    "config": "tailwind.config.js",
    "css": "src/index.css"
  },
  "aliases": {
    "components": "@/shared/components",
    "utils": "@/shared/lib/utils"
  }
}
```

**Padrão de Uso:**
- Componentes em `src/shared/components/ui/` são copiados do shadcn/ui
- Podem ser modificados conforme necessário
- Usam primitivos do Radix UI para acessibilidade
- Estilizados com TailwindCSS via `cn()` utility

### Consequências

* ✅ **Bom:** Acessibilidade garantida pelos primitivos do Radix
* ✅ **Bom:** Controle total sobre código dos componentes
* ✅ **Bom:** Customização fácil (modificar código diretamente)
* ✅ **Bom:** Bundle size menor que bibliotecas completas
* ✅ **Bom:** Integração perfeita com TailwindCSS
* ⚠️ **Neutro:** Requer manutenção manual ao atualizar componentes (mas é controle)
* ⚠️ **Ruim:** Mais trabalho inicial comparado a bibliotecas completas (mas vale a pena)
* ⚠️ **Ruim:** Atualizações do shadcn/ui requerem merge manual (aceitável)

