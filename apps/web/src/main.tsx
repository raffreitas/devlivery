import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { App } from "./app.tsx";
import { router } from "./app-routes.tsx";
import { authEvents } from "./shared/services/auth-events";
import { createAppQueryClient } from "./shared/services/query-client";

const queryClient = createAppQueryClient(() => {
  authEvents.emit();
  void router.navigate("/login", { replace: true });
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
