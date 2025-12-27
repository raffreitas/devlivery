# React Query como Gerenciador de Estado do Servidor

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** Stack Tecnológica / Gerenciamento de Estado

## Contexto e Problema

O frontend precisa gerenciar estado que vem do servidor (produtos, pedidos, dashboard) de forma eficiente, com cache, sincronização e tratamento de erros. A decisão fundamental é: devemos usar Redux, Zustand, Context API, ou uma solução especializada em estado do servidor?

A estrutura do repositório revela esta decisão através da organização:

```
src/
├── main.tsx                    # ← QueryClientProvider configurado
├── features/
│   ├── products/
│   │   └── hooks/
│   │       └── use-products.ts # ← useQuery + useMutation
│   └── orders/
│       └── hooks/
│           └── use-orders.ts   # ← useQuery + useMutation
└── shared/
    └── services/
        └── api.ts              # ← Cliente HTTP base
```

**Problema:** Como gerenciar estado assíncrono do servidor com cache, invalidação automática e sincronização?

## Opções Consideradas

* **Redux Toolkit + RTK Query** - Solução completa com store global e cache de queries
* **Zustand + SWR** - Store leve + biblioteca de fetching
* **Context API + fetch manual** - Solução nativa, mas sem cache automático
* **React Query (TanStack Query)** - Biblioteca especializada em estado do servidor

## Decisão

**Escolhida:** "React Query (TanStack Query)", porque:

1. **Especialização:** Focado exclusivamente em estado do servidor (não tenta resolver estado local)
2. **Cache Inteligente:** Cache automático com `staleTime` e `cacheTime`, reduzindo chamadas desnecessárias
3. **Invalidação Automática:** Mutations invalidam queries relacionadas automaticamente
4. **Developer Experience:** Hooks simples (`useQuery`, `useMutation`) com TypeScript forte
5. **Background Refetching:** Atualiza dados em background quando necessário
6. **Tratamento de Erros:** Integração nativa com tratamento de erros global

### Implementação Técnica

A decisão se materializa em:

1. **Configuração Global:** `QueryClient` configurado em `src/main.tsx`
2. **Hooks Customizados:** Cada feature expõe um hook que encapsula `useQuery` e `useMutation`
3. **Cache Strategy:** `staleTime` configurado por query (30s-60s)
4. **Error Handling:** Tratamento global de 401 via `QueryCache` e `MutationCache`

```typescript
// src/main.tsx
const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (error instanceof UnauthorizedError) {
        authEvents.emit();
        void router.navigate("/login", { replace: true });
      }
    },
  }),
  defaultOptions: {
    queries: {
      retry: (failureCount, error) => {
        if (error instanceof UnauthorizedError) return false;
        return failureCount < 3;
      },
    },
  },
});

// src/features/products/hooks/use-products.ts
export function useProducts() {
  const queryClient = useQueryClient();

  const productsQuery = useQuery({
    queryKey: ["products"],
    queryFn: productService.getAll,
    staleTime: 60_000,                    // Cache por 60s
    placeholderData: (previousData) => previousData, // Previne flicker
  });

  const createMutation = useMutation({
    mutationFn: productService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] }); // Refetch automático
    },
  });

  return {
    products: productsQuery.data ?? [],
    loading: productsQuery.isLoading,
    createProduct: createMutation.mutateAsync,
    // ...
  };
}
```

**Padrão de Uso:**
- Cada feature tem um hook customizado (ex: `useProducts`, `useOrders`)
- Hooks encapsulam `useQuery` para leitura e `useMutation` para escrita
- Mutations invalidam queries relacionadas no `onSuccess`
- `staleTime` evita refetch desnecessário durante navegação

### Consequências

* ✅ **Bom:** Cache automático reduz chamadas à API e melhora performance
* ✅ **Bom:** Invalidação automática mantém UI sincronizada após mutations
* ✅ **Bom:** Background refetching mantém dados atualizados sem impacto na UX
* ✅ **Bom:** TypeScript forte com inferência de tipos
* ✅ **Bom:** DevTools integrado para debug de queries
* ⚠️ **Neutro:** Apenas para estado do servidor (estado local ainda usa `useState`/`useReducer`)
* ⚠️ **Ruim:** Curva de aprendizado inicial (conceitos de `staleTime`, `cacheTime`, `queryKey`)
* ⚠️ **Ruim:** Pode ser overkill para aplicações muito simples (não é o caso aqui)

