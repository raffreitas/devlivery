import { MutationCache, QueryCache, QueryClient } from "@tanstack/react-query";
import axios from "axios";
import { toast } from "@/shared/components/ui/toast";
import { shouldRetryQuery, UnauthorizedError } from "./api";

declare module "@tanstack/react-query" {
  interface Register {
    mutationMeta: { skipGlobalErrorToast?: boolean };
  }
}

export function createAppQueryClient(onUnauthorized: () => void) {
  return new QueryClient({
    queryCache: new QueryCache({
      onError: (error) => {
        // QueryState/QueryRegion own query feedback. Authentication remains global.
        if (error instanceof UnauthorizedError) onUnauthorized();
      },
    }),
    mutationCache: new MutationCache({
      onError: (error, _variables, _onMutateResult, mutation) => {
        if (axios.isCancel(error)) return;
        if (error instanceof UnauthorizedError) {
          onUnauthorized();
          return;
        }
        if (!mutation.meta?.skipGlobalErrorToast) {
          toast.add({ type: "error", title: error.message });
        }
      },
    }),
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: shouldRetryQuery },
    },
  });
}
