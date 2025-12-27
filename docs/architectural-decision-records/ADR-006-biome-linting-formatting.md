# Biome como Ferramenta de Linting e Formatting

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Ferramentas de Qualidade de Código

## Contexto e Problema

O projeto precisa de ferramentas para garantir qualidade de código, formatação consistente e detecção de problemas. A decisão fundamental é: devemos usar ESLint + Prettier (stack tradicional) ou Biome (ferramenta unificada)?

A estrutura do repositório revela esta decisão através da organização:

```
biome.json                  # ← Configuração do Biome
package.json
├── scripts
│   ├── lint: "biome check"
│   └── format: "biome format --write"
└── devDependencies
    └── "@biomejs/biome": "2.3.1"  # ← Biome como única ferramenta
```

**Problema:** Qual ferramenta oferece melhor DX e performance para linting e formatação?

## Opções Consideradas

* **ESLint + Prettier** - Stack tradicional, mas requer configuração complexa e pode ter conflitos
* **Biome** - Ferramenta unificada (linting + formatting) escrita em Rust, extremamente rápida
* **ESLint standalone** - Apenas linting, sem formatação automática

## Decisão

**Escolhida:** "Biome", porque:

1. **Performance:** Escrito em Rust, é 10-100x mais rápido que ESLint + Prettier
2. **Unificação:** Uma única ferramenta para linting e formatação (menos configuração)
3. **Zero Config:** Funciona bem com configuração mínima
4. **Compatibilidade:** Suporta regras similares ao ESLint
5. **Organize Imports:** Pode organizar imports automaticamente
6. **Developer Experience:** Feedback instantâneo mesmo em projetos grandes

### Implementação Técnica

A decisão se materializa em:

1. **Configuração:** `biome.json` define regras e formatação
2. **Scripts:** `package.json` expõe comandos `lint` e `format`
3. **Editor Integration:** Biome pode ser usado como formatter no editor

```json
// biome.json
{
  "$schema": "https://biomejs.dev/schemas/2.3.1/schema.json",
  "formatter": {
    "enabled": true,
    "indentStyle": "space",
    "useEditorconfig": true
  },
  "linter": {
    "enabled": true,
    "rules": {
      "recommended": true
    }
  },
  "javascript": {
    "formatter": {
      "quoteStyle": "double"
    }
  },
  "assist": {
    "enabled": true,
    "actions": {
      "source": {
        "organizeImports": "on"  // Organiza imports automaticamente
      }
    }
  }
}
```

**Comandos:**
- `pnpm lint` → Verifica problemas de linting
- `pnpm format` → Formata código e organiza imports

**Regras:**
- `recommended` ativa regras recomendadas (similar ao ESLint)
- Formatação com double quotes e 2 espaços
- Organização automática de imports

### Consequências

* ✅ **Bom:** Performance extremamente rápida (segundos vs minutos)
* ✅ **Bom:** Configuração simples e unificada
* ✅ **Bom:** Organização automática de imports
* ✅ **Bom:** Menos dependências (uma ferramenta vs duas)
* ✅ **Bom:** Compatível com regras do ESLint (migração fácil)
* ⚠️ **Neutro:** Ecossistema menor que ESLint (mas suficiente para maioria dos casos)
* ⚠️ **Ruim:** Alguns plugins do ESLint podem não estar disponíveis (não é problema aqui)
* ⚠️ **Ruim:** Migração de ESLint+Prettier requer ajustes (não aplicável - projeto novo)

