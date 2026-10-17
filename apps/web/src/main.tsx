import {
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import axios from "axios";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { toast } from "@/shared/components/ui/toast";
import { App } from "./app.tsx";
import { router } from "./app-routes.tsx";
import { shouldRetryQuery, UnauthorizedError } from "./shared/services/api";
import { authEvents } from "./shared/services/auth-events";

declare module "@tanstack/react-query" {
  interface Register {
    mutationMeta: { skipGlobalErrorToast?: boolean };
  }
}

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (axios.isCancel(error)) return;
      if (error instanceof UnauthorizedError) {
        authEvents.emit();
        void router.navigate("/login", { replace: true });
        return error;
      }
      toast.add({ type: "error", title: error.message });
      return error;
    },
  }),
  mutationCache: new MutationCache({
    onError: (error, _variables, _onMutateResult, mutation) => {
      if (axios.isCancel(error)) return;
      if (error instanceof UnauthorizedError) {
        authEvents.emit();
        void router.navigate("/login", { replace: true });
        return error;
      }
      // Telas que exibem a própria mensagem de erro optam por sair do aviso global
      if (mutation.meta?.skipGlobalErrorToast) return error;
      toast.add({ type: "error", title: error.message });
      return error;
    },
  }),
  defaultOptions: {
    mutations: { retry: false },
    queries: {
      retry: shouldRetryQuery,
    },
  },
});

const container = document.getElementById("root");
if (!container) throw new Error("Root element #root not found");
createRoot(container).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      <ReactQueryDevtools initialIsOpen={false} buttonPosition="bottom-right" />
    </QueryClientProvider>
  </StrictMode>,
);
