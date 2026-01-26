import {
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { toast } from "sonner";
import { App } from "./app.tsx";
import { router } from "./app-routes.tsx";
import { Toaster } from "./shared/components/ui/sonner.tsx";
import { UnauthorizedError } from "./shared/services/api";
import { authEvents } from "./shared/services/auth-events";

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (error instanceof UnauthorizedError) {
        authEvents.emit();
        void router.navigate("/login", { replace: true });
        return error;
      }
      toast.error(error.message);
      return error;
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      if (error instanceof UnauthorizedError) {
        authEvents.emit();
        void router.navigate("/login", { replace: true });
        return error;
      }
      toast.error(error.message);
      return error;
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

const container = document.getElementById("root");
if (!container) throw new Error("Root element #root not found");
createRoot(container).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      <ReactQueryDevtools initialIsOpen={false} buttonPosition="bottom-right" />
      <Toaster richColors />
    </QueryClientProvider>
  </StrictMode>,
);
