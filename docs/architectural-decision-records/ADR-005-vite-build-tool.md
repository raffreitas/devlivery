# Vite como Build Tool e Dev Server

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Ferramentas de Build

## Contexto e Problema

O projeto precisa de uma ferramenta de build rápida para desenvolvimento e produção. A decisão fundamental é: devemos usar Create React App, Vite, ou outras alternativas como Next.js ou Turbopack?

A estrutura do repositório revela esta decisão através da organização:

```
vite.config.ts              # ← Configuração do Vite
package.json
├── scripts
│   ├── dev: "vite"         # ← Dev server
│   └── build: "tsc -b && vite build"  # ← Build de produção
└── devDependencies
    └── vite: "^7.3.0"      # ← Vite como dependência
```

**Problema:** Qual ferramenta de build oferece melhor experiência de desenvolvimento e performance?

## Opções Consideradas

* **Create React App (CRA)** - Ferramenta oficial, mas lenta e com configuração limitada
* **Next.js** - Framework completo com SSR, mas adiciona complexidade desnecessária
* **Vite** - Build tool moderna com HMR rápido e configuração flexível
* **Turbopack** - Nova ferramenta do Next.js, ainda em desenvolvimento

## Decisão

**Escolhida:** "Vite", porque:

1. **Performance:** HMR (Hot Module Replacement) extremamente rápido, mesmo em projetos grandes
2. **Developer Experience:** Startup instantâneo do dev server
3. **Configuração Flexível:** Suporta plugins (React, TailwindCSS) e path aliases facilmente
4. **Build Otimizado:** Produz bundles otimizados para produção com code splitting automático
5. **Ecosystem:** Amplamente adotado e estável
6. **TypeScript Nativo:** Suporte nativo sem configuração adicional

### Implementação Técnica

A decisão se materializa em:

1. **Configuração:** `vite.config.ts` define plugins e aliases
2. **Scripts:** `package.json` usa comandos do Vite
3. **Plugins:** React Compiler e TailwindCSS integrados via plugins

```typescript
// vite.config.ts
import path from "node:path";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [
    react({
      babel: {
        plugins: [["babel-plugin-react-compiler"]], // React Compiler
      },
    }),
    tailwindcss(), // TailwindCSS 4
  ],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"), // Path aliases
    },
  },
});
```

**Comandos:**
- `pnpm dev` → Inicia dev server com HMR
- `pnpm build` → Compila TypeScript e gera build de produção
- `pnpm preview` → Preview do build de produção localmente

**Output:**
- `dist/` contém assets otimizados (JS, CSS, imagens)
- Code splitting automático por rotas/imports dinâmicos
- Tree shaking remove código não utilizado

### Consequências

* ✅ **Bom:** HMR extremamente rápido melhora produtividade
* ✅ **Bom:** Startup instantâneo do dev server
* ✅ **Bom:** Build de produção otimizado com code splitting
* ✅ **Bom:** Configuração simples e flexível
* ✅ **Bom:** Suporte nativo a TypeScript e ES modules
* ⚠️ **Neutro:** Requer Node.js 18+ (não é problema na prática)
* ⚠️ **Ruim:** Alguns plugins podem não ter suporte completo (não é o caso aqui)
* ⚠️ **Ruim:** Migração de CRA requer ajustes (não aplicável - projeto novo)

