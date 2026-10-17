import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "@/shared/services/query-keys";
import { cashService } from "../services/cash-service";

export function useCurrentCashSession() {
  const currentSessionQuery = useQuery({
    queryKey: queryKeys.currentCash,
    queryFn: ({ signal }) => cashService.getActive(signal),
    staleTime: 0,
    gcTime: 5 * 60 * 1000,
  });

  return {
    currentSession: currentSessionQuery.data ?? null,
    isLoading: currentSessionQuery.isLoading,
    isFetching: currentSessionQuery.isFetching,
    error: currentSessionQuery.error,
  };
}
