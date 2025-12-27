# React Compiler para Otimizações Automáticas

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Performance

## Contexto e Problema

O projeto precisa de otimizações de performance (memoização, evitar re-renders desnecessários) sem adicionar complexidade manual (useMemo, useCallback). A decisão fundamental é: devemos otimizar manualmente ou usar uma ferramenta automática?

A estrutura do repositório revela esta decisão através da organização:

```
vite.config.ts
└── plugins
    └── react({
        babel: {
          plugins: [["babel-plugin-react-compiler"]]  # ← React Compiler
        }
      })
package.json
└── devDependencies
    └── "babel-plugin-react-compiler": "19.1.0-rc.3"
```

**Problema:** Como otimizar performance sem adicionar hooks manuais (useMemo, useCallback) em todo o código?

## Opções Consideradas

* **Otimização manual** - useMemo, useCallback, React.memo onde necessário (verboso)
* **React Compiler** - Otimizações automáticas via análise estática
* **Sem otimização** - Confiar apenas em React padrão (pode ter performance issues)

## Decisão

**Escolhida:** "React Compiler", porque:

1. **Automação:** Otimizações aplicadas automaticamente, sem código manual
2. **Developer Experience:** Código mais limpo, sem useMemo/useCallback espalhados
3. **Performance:** Memoização automática de valores e callbacks
4. **Future-Proof:** Ferramenta oficial do Meta, será padrão no futuro
5. **Type Safety:** Funciona com TypeScript sem problemas

### Implementação Técnica

A decisão se materializa em:

1. **Babel Plugin:** `babel-plugin-react-compiler` configurado no Vite
2. **Build Time:** Compilador analisa código em build time
3. **Runtime:** Otimizações aplicadas automaticamente

```typescript
// vite.config.ts
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [
    react({
      babel: {
        plugins: [["babel-plugin-react-compiler"]], // ← Compiler ativado
      },
    }),
    // ...
  ],
});
```

**O que o Compiler faz:**
- Memoiza automaticamente valores computados
- Memoiza callbacks automaticamente
- Evita re-renders desnecessários
- Otimiza componentes automaticamente

**Exemplo (sem Compiler):**
```tsx
// Código manual
const memoizedValue = useMemo(() => computeExpensiveValue(a, b), [a, b]);
const memoizedCallback = useCallback(() => doSomething(a, b), [a, b]);
```

**Exemplo (com Compiler):**
```tsx
// Código limpo - Compiler otimiza automaticamente
const value = computeExpensiveValue(a, b);
const callback = () => doSomething(a, b);
```

### Consequências

* ✅ **Bom:** Código mais limpo sem hooks de otimização manuais
* ✅ **Bom:** Otimizações automáticas aplicadas consistentemente
* ✅ **Bom:** Performance melhor sem esforço manual
* ✅ **Bom:** Future-proof (ferramenta oficial do Meta)
* ⚠️ **Neutro:** Versão RC (Release Candidate) - mas estável
* ⚠️ **Ruim:** Pode ter edge cases não cobertos (mas raro)
* ⚠️ **Ruim:** Debug pode ser mais complexo (otimizações são "mágicas")

