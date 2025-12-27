# TailwindCSS 4 com Plugin Vite

**Data:** 2025-12-26
**Status:** Aceito
**Contexto:** Stack Tecnológica / Estilização

## Contexto e Problema

O projeto precisa de uma solução de estilização eficiente e produtiva. A decisão fundamental é: devemos usar CSS modules, styled-components, ou TailwindCSS (qual versão)?

A estrutura do repositório revela esta decisão através da organização:

```
vite.config.ts
└── plugins
    └── tailwindcss()           # ← Plugin Vite do TailwindCSS 4
package.json
└── dependencies
    ├── tailwindcss: "^4.1.18"
    └── "@tailwindcss/vite": "^4.1.18"
src/index.css
└── @import "tailwindcss"        # ← Import direto (v4)
```

**Problema:** Qual solução de estilização oferece melhor DX e performance?

## Opções Consideradas

* **CSS Modules** - Escopo automático, mas sem utilities
* **styled-components** - CSS-in-JS, mas runtime overhead
* **TailwindCSS v3** - Utilities CSS, mas requer config file
* **TailwindCSS v4** - Utilities CSS + configuração via CSS, plugin Vite nativo

## Decisão

**Escolhida:** "TailwindCSS 4 com Plugin Vite", porque:

1. **Performance:** Plugin Vite nativo, sem PostCSS separado
2. **Developer Experience:** Utilities CSS permitem desenvolvimento rápido
3. **Configuração Simplificada:** Config via CSS (não precisa de `tailwind.config.js`)
4. **Tree Shaking:** Apenas classes usadas são incluídas no bundle
5. **Consistency:** Design system consistente via utilities
6. **Modern:** Versão mais recente com melhorias de performance

### Implementação Técnica

A decisão se materializa em:

1. **Plugin Vite:** `@tailwindcss/vite` integrado no Vite
2. **CSS Import:** `@import "tailwindcss"` no `index.css`
3. **Utilities:** Classes Tailwind usadas diretamente nos componentes

```typescript
// vite.config.ts
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [
    tailwindcss(), // ← Plugin nativo do Vite
    // ...
  ],
});

// src/index.css
@import "tailwindcss"; /* ← Import direto, sem config file */

/* Customizações podem ser feitas via CSS variables */
@theme {
  --color-primary: #3b82f6;
}
```

**Uso em Componentes:**
```tsx
// Classes Tailwind diretamente no JSX
<div className="flex items-center gap-4 p-6 bg-white rounded-lg shadow-md">
  <h1 className="text-2xl font-bold text-gray-900">Título</h1>
</div>
```

**Vantagens do v4:**
- Não requer `tailwind.config.js` (config via CSS)
- Plugin Vite nativo (mais rápido)
- Melhor tree shaking
- Suporte a CSS variables nativo

### Consequências

* ✅ **Bom:** Desenvolvimento rápido com utilities CSS
* ✅ **Bom:** Performance otimizada (plugin Vite nativo)
* ✅ **Bom:** Bundle size otimizado (tree shaking)
* ✅ **Bom:** Design system consistente
* ✅ **Bom:** Configuração simplificada (via CSS)
* ⚠️ **Neutro:** Curva de aprendizado inicial (classes utilitárias)
* ⚠️ **Ruim:** HTML pode ficar verboso com muitas classes (mitigado por componentes)
* ⚠️ **Ruim:** v4 é relativamente nova (mas estável e melhor que v3)

