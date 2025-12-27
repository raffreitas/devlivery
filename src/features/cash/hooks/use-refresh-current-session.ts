import { useQueryClient } from "@tanstack/react-query";

/**
 * Hook to manually refresh the current cash session totals
 * Useful when you know orders were created/updated and need to refresh
 * the session totals without invalidating the entire cache
 */
export function useRefreshCurrentSession() {
  const queryClient = useQueryClient();

  return () => {
    queryClient.invalidateQueries({
      queryKey: ["cash-sessions", "current"],
      exact: true,
    });
  };
}
