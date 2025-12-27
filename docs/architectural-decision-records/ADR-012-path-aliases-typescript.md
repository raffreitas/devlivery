# Path Aliases com TypeScript

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Organização de Código

## Contexto e Problema

O projeto precisa de imports relativos limpos e manuteníveis. A decisão fundamental é: devemos usar imports relativos (`../../shared/`), imports absolutos (`/src/shared/`), ou path aliases (`@/shared/`)?

A estrutura do repositório revela esta decisão através da organização:

```
vite.config.ts
└── resolve.alias
    └── "@": "./src"              # ← Alias configurado no Vite
tsconfig.json
└── compilerOptions.paths
    └── "@/*": ["./src/*"]        # ← Alias configurado no TypeScript
```

**Problema:** Como tornar imports mais legíveis e evitar caminhos relativos complexos?

## Opções Consideradas

* **Imports relativos** - `../../shared/services/api` (verboso e frágil)
* **Imports absolutos** - `/src/shared/services/api` (melhor, mas ainda verboso)
* **Path aliases** - `@/shared/services/api` (limpo e semântico)

## Decisão

**Escolhida:** "Path aliases", porque:

1. **Legibilidade:** `@/shared/` é mais claro que `../../shared/`
2. **Manutenibilidade:** Refatorações não quebram imports (alias é estável)
3. **Consistência:** Padrão comum em projetos React/TypeScript
4. **Developer Experience:** Autocomplete do IDE funciona melhor
5. **Type Safety:** TypeScript resolve aliases corretamente

### Implementação Técnica

A decisão se materializa em:

1. **Vite Config:** Alias `@` mapeado para `./src`
2. **TypeScript Config:** Paths configurados em `tsconfig.json`
3. **Uso:** Imports usam `@/` ao invés de caminhos relativos

```typescript
// vite.config.ts
import path from "node:path";

export default defineConfig({
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"), // ← Alias para src/
    },
  },
});

// tsconfig.json
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"] // ← TypeScript resolve aliases
    }
  }
}

// Uso em componentes
import { api } from "@/shared/services/api";           // ← Ao invés de ../../shared/services/api
import { useAuth } from "@/shared/contexts/auth-context";
import { Product } from "@/features/products/types";
```

**Padrão de Uso:**
- `@/` sempre aponta para `src/`
- Imports de `shared/` → `@/shared/...`
- Imports de `features/` → `@/features/...`
- Evitar imports relativos (`../`, `../../`)

### Consequências

* ✅ **Bom:** Imports mais legíveis e semânticos
* ✅ **Bom:** Refatorações não quebram imports
* ✅ **Bom:** Autocomplete do IDE funciona melhor
* ✅ **Bom:** Padrão comum e familiar
* ⚠️ **Neutro:** Requer configuração em Vite e TypeScript (feito uma vez)
* ⚠️ **Ruim:** Pode confundir desenvolvedores novos (mas é padrão comum)
* ⚠️ **Ruim:** Alguns tools podem não resolver aliases (não é problema aqui)

