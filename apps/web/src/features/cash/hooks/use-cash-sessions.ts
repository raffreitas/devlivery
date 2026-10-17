import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  invalidateAfterMutation,
  queryKeys,
} from "@/shared/services/query-keys";
import { cashService } from "../services/cash-service";
import type {
  CloseCashSessionFormData,
  CreateCashDepositFormData,
  CreateCashSessionFormData,
} from "../types";

export function useCashSessions() {
  const queryClient = useQueryClient();

  const currentSessionQuery = useQuery({
    queryKey: queryKeys.currentCash,
    queryFn: ({ signal }) => cashService.getActive(signal),
    staleTime: 0,
    gcTime: 5 * 60 * 1000,
  });

  const sessionsQuery = useQuery({
    queryKey: queryKeys.cash,
    queryFn: ({ signal }) => cashService.getAll(signal),
    staleTime: 60_000,
    enabled: false,
  });

  const createMutation = useMutation({
    mutationFn: (dto: CreateCashSessionFormData) => cashService.create(dto),
    meta: { skipGlobalErrorToast: true },
    onSuccess: (data) => {
      if (data) {
        queryClient.setQueryData(queryKeys.currentCash, data);
      }
      invalidateAfterMutation(queryClient, "cash");
    },
  });

  const closeMutation = useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: CloseCashSessionFormData }) =>
      cashService.close(id, dto),
    meta: { skipGlobalErrorToast: true },
    onSuccess: () => {
      queryClient.setQueryData(queryKeys.currentCash, null);
      invalidateAfterMutation(queryClient, "cash");
    },
  });

  const depositsQuery = useQuery({
    queryKey: queryKeys.deposits(currentSessionQuery.data?.id),
    queryFn: ({ signal }) => {
      if (!currentSessionQuery.data?.id) return Promise.resolve([]);
      return cashService.getDeposits(currentSessionQuery.data.id, signal);
    },
    enabled: !!currentSessionQuery.data?.id,
    staleTime: 0,
    gcTime: 5 * 60 * 1000,
  });

  const depositMutation = useMutation({
    mutationFn: ({
      sessionId,
      dto,
    }: {
      sessionId: string;
      dto: CreateCashDepositFormData;
    }) => cashService.createDeposit(sessionId, dto),
    meta: { skipGlobalErrorToast: true },
    onSuccess: () => invalidateAfterMutation(queryClient, "cash"),
  });

  return {
    sessions: sessionsQuery.data ?? [],
    currentSession: currentSessionQuery.data ?? null,
    error: currentSessionQuery.error ?? depositsQuery.error,
    hasData:
      currentSessionQuery.data !== undefined &&
      (!currentSessionQuery.data || depositsQuery.data !== undefined),
    refetch: async () => {
      await Promise.allSettled([
        currentSessionQuery.refetch(),
        depositsQuery.refetch(),
      ]);
    },
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
