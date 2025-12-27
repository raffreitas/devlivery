# Estratégia de Autenticação com Context API e Event-Driven Error Handling

**Data:** 2025-12-26
**Status:** Aceito
**Contexto:** Padrão de Design / Autenticação e Autorização

## Contexto e Problema

O projeto precisa gerenciar estado de autenticação (usuário logado, token) e responder a erros 401 (Unauthorized) de forma global. A decisão fundamental é: devemos usar Context API, Redux, Zustand, ou uma solução híbrida?

A estrutura do repositório revela esta decisão através da organização:

```
src/
├── shared/
│   ├── contexts/
│   │   └── auth-context.tsx      # ← Context API
│   └── services/
│       └── auth-events.ts        # ← Event emitter
├── features/auth/
│   └── services/
│       └── auth-service.ts       # ← Lógica de login/logout
└── main.tsx                      # ← QueryClient com error handling
```

**Problema:** Como gerenciar estado de autenticação e responder a erros 401 de forma centralizada?

## Opções Consideradas

* **Redux/Zustand** - Store global, mas overkill para estado simples
* **Context API** - Solução nativa, adequada para estado global pequeno
* **Local State + Props** - Não escala para múltiplos componentes
* **Context API + Event Emitter** - Context para estado, eventos para comunicação cross-cutting

## Decisão

**Escolhida:** "Context API + Event-Driven Error Handling", porque:

1. **Simplicidade:** Context API é suficiente para estado de autenticação (user, token)
2. **Persistência:** Token salvo no localStorage, estado restaurado na inicialização
3. **Event-Driven:** Eventos globais permitem comunicação entre camadas (API → Context)
4. **Separation of Concerns:** Context gerencia estado, eventos comunicam erros
5. **Type Safety:** TypeScript forte com tipos explícitos

### Implementação Técnica

A decisão se materializa em:

1. **AuthContext:** Gerencia estado de autenticação (user, token, loading)
2. **AuthEvents:** Event emitter para comunicação de erros 401
3. **QueryClient Integration:** Tratamento global de 401 em queries/mutations
4. **Persistência:** Token salvo em localStorage com chave `devlivery@auth`

```typescript
// src/shared/services/auth-events.ts
type Listener = () => void;

export const authEvents = {
  listeners: new Set<Listener>(),
  subscribe: (listener: Listener) => {
    authEvents.listeners.add(listener);
    return () => authEvents.listeners.delete(listener);
  },
  emit: () => {
    authEvents.listeners.forEach((listener) => listener());
  },
};

// src/shared/contexts/auth-context.tsx
export function AuthProvider({ children }: { children: ReactNode }) {
  const auth = useAuthQuery();

  // Escuta eventos de erro de autenticação (ex: 401 Unauthorized)
  useEffect(() => {
    const unsubscribe = authEvents.subscribe(() => {
      void auth.logout(); // Logout automático em 401
    });
    return unsubscribe;
  }, [auth]);

  return <AuthContext.Provider value={auth}>{children}</AuthContext.Provider>;
}

// src/main.tsx
const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (error instanceof UnauthorizedError) {
        authEvents.emit(); // ← Emite evento global
        void router.navigate("/login", { replace: true });
      }
    },
  }),
  // ...
});
```

**Fluxo de Autenticação:**
1. Login → `auth-service.ts` faz POST → salva token no localStorage
2. `AuthContext` lê token do localStorage na inicialização
3. `api.ts` injeta token no header `Authorization` automaticamente
4. Erro 401 → `QueryClient` detecta → emite evento → `AuthContext` faz logout → redireciona para `/login`

### Consequências

* ✅ **Bom:** Simplicidade - Context API é suficiente para estado pequeno
* ✅ **Bom:** Persistência automática via localStorage
* ✅ **Bom:** Tratamento global de 401 sem acoplamento
* ✅ **Bom:** Event-driven permite comunicação cross-cutting
* ✅ **Bom:** TypeScript forte com tipos explícitos
* ⚠️ **Neutro:** Context API pode ter performance issues com muitos consumers (não é problema aqui)
* ⚠️ **Ruim:** Event emitter customizado requer manutenção (mas é simples)
* ⚠️ **Ruim:** Migração para Redux/Zustand seria necessária se estado crescer muito (improvável)

