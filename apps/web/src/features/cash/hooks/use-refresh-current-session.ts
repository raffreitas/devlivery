import { useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "@/shared/services/query-keys";

/**
 * Hook to manually refresh the current cash session totals
 * Useful when you know orders were created/updated and need to refresh
 * the session totals without invalidating the entire cache
 */
export function useRefreshCurrentSession() {
  const queryClient = useQueryClient();

  return () => {
    queryClient.invalidateQueries({
      queryKey: queryKeys.currentCash,
      exact: true,
    });
  };
}
