import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { cashService } from "../services/cash-service";
import type {
  CloseCashSessionFormData,
  CreateCashDepositFormData,
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

  // Query for deposits of current session
  const depositsQuery = useQuery({
    queryKey: [
      ...CASH_SESSIONS_QUERY_KEY,
      "deposits",
      currentSessionQuery.data?.id,
    ],
    queryFn: () => {
      if (!currentSessionQuery.data?.id) return Promise.resolve([]);
      return cashService.getDeposits(currentSessionQuery.data.id);
    },
    enabled: !!currentSessionQuery.data?.id,
    staleTime: 20_000,
    placeholderData: (previousData) => previousData,
  });

  // Create deposit for current session
  const depositMutation = useMutation({
    mutationFn: ({
      sessionId,
      dto,
    }: {
      sessionId: string;
      dto: CreateCashDepositFormData;
    }) => cashService.createDeposit(sessionId, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CASH_SESSIONS_QUERY_KEY });
    },
  });

  return {
    // Queries
    sessions: sessionsQuery.data ?? [],
    currentSession: currentSessionQuery.data ?? null,
    deposits: depositsQuery.data ?? [],
    isLoading:
      sessionsQuery.isLoading ||
      currentSessionQuery.isLoading ||
      depositsQuery.isLoading,
    isFetching:
      sessionsQuery.isFetching ||
      currentSessionQuery.isFetching ||
      depositsQuery.isFetching,

    // Mutations
    openCashSession: createMutation.mutateAsync,
    closeCashSession: closeMutation.mutateAsync,
    deleteCashSession: deleteMutation.mutateAsync,
    createDeposit: depositMutation.mutateAsync,

    // Mutation states
    isOpening: createMutation.isPending,
    isClosing: closeMutation.isPending,
    isDeleting: deleteMutation.isPending,
    isCreatingDeposit: depositMutation.isPending,

    // Errors
    openError: createMutation.error,
    closeError: closeMutation.error,
    deleteError: deleteMutation.error,
    depositError: depositMutation.error,
  };
}
