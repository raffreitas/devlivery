import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { cashService } from "../services/cash-service";
import type {
  CloseCashSessionFormData,
  CreateCashSessionFormData,
} from "../types";

const CASH_SESSIONS_QUERY_KEY = ["cash-sessions"];

/**
 * Hook for managing cash sessions with React Query
 * Uses API service for backend integration
 */
export function useCashSessions() {
  const queryClient = useQueryClient();

  // Query all sessions
  const sessionsQuery = useQuery({
    queryKey: CASH_SESSIONS_QUERY_KEY,
    queryFn: () => cashService.getAll(),
    staleTime: 30_000, // 30 seconds
    placeholderData: (previousData) => previousData,
  });

  // Query current open session
  const currentSessionQuery = useQuery({
    queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
    queryFn: () => cashService.getActive(),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  // Create new session
  const createMutation = useMutation({
    mutationFn: (dto: CreateCashSessionFormData) => cashService.create(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CASH_SESSIONS_QUERY_KEY });
    },
  });

  // Close session - backend now calculates sales totals from orders
  const closeMutation = useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: CloseCashSessionFormData }) =>
      cashService.close(id, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CASH_SESSIONS_QUERY_KEY });
    },
  });

  // Delete session (for corrections/cleanup if needed)
  const deleteMutation = useMutation({
    mutationFn: (_id: string) => {
      // This would need backend support - not implemented yet
      throw new Error("Delete not implemented on backend yet");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CASH_SESSIONS_QUERY_KEY });
    },
  });

  return {
    // Queries
    sessions: sessionsQuery.data ?? [],
    currentSession: currentSessionQuery.data ?? null,
    isLoading: sessionsQuery.isLoading || currentSessionQuery.isLoading,
    isFetching: sessionsQuery.isFetching || currentSessionQuery.isFetching,

    // Mutations
    openCashSession: createMutation.mutateAsync,
    closeCashSession: closeMutation.mutateAsync,
    deleteCashSession: deleteMutation.mutateAsync,

    // Mutation states
    isOpening: createMutation.isPending,
    isClosing: closeMutation.isPending,
    isDeleting: deleteMutation.isPending,

    // Errors
    openError: createMutation.error,
    closeError: closeMutation.error,
    deleteError: deleteMutation.error,
  };
}
