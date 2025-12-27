# Zod para Validação e Schemas de Runtime

**Data:** 2025-12-26
**Status:** Aceito
**Contexto:** Stack Tecnológica / Validação de Dados

## Contexto e Problema

O projeto precisa validar dados em runtime (formulários, variáveis de ambiente, respostas da API). A decisão fundamental é: devemos usar TypeScript apenas (compile-time), uma biblioteca de validação (Zod, Yup, Joi), ou validação manual?

A estrutura do repositório revela esta decisão através da organização:

```
src/
├── env.ts                    # ← Validação de env com Zod
└── features/
    └── */types/
        └── index.ts          # ← Schemas Zod para formulários
```

**Problema:** Como validar dados em runtime e manter type safety?

## Opções Consideradas

* **TypeScript apenas** - Type safety em compile-time, mas sem validação runtime
* **Yup** - Biblioteca popular, mas sem inferência de tipos TypeScript
* **Joi** - Biblioteca robusta, mas sem TypeScript nativo
* **Zod** - Biblioteca TypeScript-first com inferência de tipos

## Decisão

**Escolhida:** "Zod", porque:

1. **Type Safety:** Schemas Zod geram tipos TypeScript automaticamente
2. **Runtime Validation:** Valida dados em runtime (formulários, API, env vars)
3. **Developer Experience:** Uma única fonte de verdade (schema = tipo)
4. **Integration:** Integra com React Hook Form via `@hookform/resolvers`
5. **Error Messages:** Mensagens de erro customizáveis e i18n-friendly

### Implementação Técnica

A decisão se materializa em:

1. **Environment Validation:** `env.ts` valida variáveis de ambiente
2. **Form Schemas:** Cada feature tem schemas Zod para formulários
3. **Type Inference:** Tipos TypeScript inferidos dos schemas

```typescript
// src/env.ts
import { z } from "zod";

export const envSchema = z.object({
  MODE: z.enum(["production", "development", "test"]),
  VITE_API_URL: z.string(),
});

export const env = envSchema.parse(import.meta.env); // Valida em runtime

// src/features/products/types/index.ts
import z from "zod";

export const productFormSchema = z.object({
  name: z
    .string({ error: "Deve ser informado um nome válido." })
    .min(1, "O nome é obrigatório"),
  price: z
    .number({ error: "Deve ser informado um preço válido" })
    .min(0.01, "O preço deve ser maior que zero"),
  category: z.string().min(1, "A categoria é obrigatória"),
  available: z.boolean(),
});

export type ProductFormData = z.infer<typeof productFormSchema>; // ← Tipo inferido

// Uso com React Hook Form
import { zodResolver } from "@hookform/resolvers/zod";

const form = useForm<ProductFormData>({
  resolver: zodResolver(productFormSchema), // ← Validação automática
});
```

**Padrão de Uso:**
- Schemas Zod definem estrutura e validação
- Tipos TypeScript inferidos via `z.infer<>`
- React Hook Form usa `zodResolver` para validação automática
- Mensagens de erro customizadas em português

### Consequências

* ✅ **Bom:** Type safety + runtime validation em uma única fonte
* ✅ **Bom:** Inferência automática de tipos TypeScript
* ✅ **Bom:** Integração perfeita com React Hook Form
* ✅ **Bom:** Mensagens de erro customizáveis
* ✅ **Bom:** Validação de env vars em startup (fail fast)
* ⚠️ **Neutro:** Bundle size maior que validação manual (aceitável)
* ⚠️ **Ruim:** Curva de aprendizado inicial (mas é intuitivo)
* ⚠️ **Ruim:** Schemas complexos podem ser verbosos (mas type-safe)

