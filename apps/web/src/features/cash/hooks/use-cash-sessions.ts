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

  const createMutation = useMutation({
    mutationFn: (dto: CreateCashSessionFormData) => cashService.create(dto),
    onSuccess: (data) => {
      if (data) {
        queryClient.setQueryData([...CASH_SESSIONS_QUERY_KEY, "current"], data);
      }
      queryClient.invalidateQueries({ queryKey: CASH_SESSIONS_QUERY_KEY });
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
      });
    },
  });

  const closeMutation = useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: CloseCashSessionFormData }) =>
      cashService.close(id, dto),
    onSuccess: () => {
      queryClient.setQueryData([...CASH_SESSIONS_QUERY_KEY, "current"], null);
      queryClient.invalidateQueries({ queryKey: CASH_SESSIONS_QUERY_KEY });
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "deposits"],
      });
    },
  });

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
    staleTime: 0,
    gcTime: 5 * 60 * 1000,
    placeholderData: (previousData) => previousData,
  });

  const depositMutation = useMutation({
    mutationFn: ({
      sessionId,
      dto,
    }: {
      sessionId: string;
      dto: CreateCashDepositFormData;
    }) => cashService.createDeposit(sessionId, dto),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "deposits", variables.sessionId],
      });
      queryClient.invalidateQueries({
        queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
      });
    },
  });

  return {
    sessions: sessionsQuery.data ?? [],
    currentSession: currentSessionQuery.data ?? null,
    deposits: depositsQuery.data ?? [],
    refetchSessions: sessionsQuery.refetch,
    isLoading:
      sessionsQuery.isLoading ||
      currentSessionQuery.isLoading ||
      depositsQuery.isLoading,
    isFetching:
      sessionsQuery.isFetching ||
      currentSessionQuery.isFetching ||
      depositsQuery.isFetching,

    openCashSession: createMutation.mutateAsync,
    closeCashSession: closeMutation.mutateAsync,
    createDeposit: depositMutation.mutateAsync,

    isOpening: createMutation.isPending,
    isClosing: closeMutation.isPending,
    isCreatingDeposit: depositMutation.isPending,

    openError: createMutation.error,
    closeError: closeMutation.error,
    depositError: depositMutation.error,
  };
}
