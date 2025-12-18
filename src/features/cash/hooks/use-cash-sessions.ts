import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { cashService } from "../services/cash-service";
import type {
  CloseCashSessionFormData,
  CreateCashDepositFormData,
  CreateCashSessionFormData,
} from "../types";

const CASH_SESSIONS_QUERY_KEY = ["cash-sessions"];

export function useCashSessions() {
  const queryClient = useQueryClient();

  const currentSessionQuery = useQuery({
    queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
    queryFn: () => cashService.getActive(),
    staleTime: 0,
    gcTime: 5 * 60 * 1000,
    placeholderData: (previousData) => previousData,
  });

  const sessionsQuery = useQuery({
    queryKey: CASH_SESSIONS_QUERY_KEY,
    queryFn: () => cashService.getAll(),
    staleTime: 60_000,
    placeholderData: (previousData) => previousData,
    enabled: false,
  });

  // Create new session
  const createMutation = useMutation({
    mutationFn: (dto: CreateCashSessionFormData) => cashService.create(dto),
    onSuccess: () => {
      // Invalidate both current and all sessions since a new one was opened
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
        exact: true,
      });
      queryClient.invalidateQueries({
        queryKey: CASH_SESSIONS_QUERY_KEY,
        exact: true,
      });
    },
  });

  // Close session - backend now calculates sales totals from orders
  const closeMutation = useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: CloseCashSessionFormData }) =>
      cashService.close(id, dto),
    onSuccess: () => {
      // Invalidate both current and all sessions since session was closed
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
        exact: true,
      });
      queryClient.invalidateQueries({
        queryKey: CASH_SESSIONS_QUERY_KEY,
        exact: true,
      });
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

  // Query for deposits of current session - always refetch but keep temporary cache
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
    staleTime: 0, // Always refetch to ensure fresh data
    gcTime: 5 * 60 * 1000, // Keep in cache for 5min for smooth navigation
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
    onSuccess: (_data, variables) => {
      // Invalidate deposits list for this session
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "deposits", variables.sessionId],
        exact: true,
      });
      // Invalidate current session to update totals
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
        exact: true,
      });
    },
  });

  return {
    // Queries
    sessions: sessionsQuery.data ?? [],
    currentSession: currentSessionQuery.data ?? null,
    deposits: depositsQuery.data ?? [],
    refetchSessions: sessionsQuery.refetch, // Manual refetch for all sessions list
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
