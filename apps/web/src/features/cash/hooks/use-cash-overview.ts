import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "@/shared/services/api";
import {
  invalidateAfterMutation,
  queryKeys,
} from "@/shared/services/query-keys";
import { cashOverviewService } from "../services/cash-overview-service";
import { cashService } from "../services/cash-service";
import type {
  CashWithdrawalRequest,
  CloseCashSessionFormData,
  CreateCashDepositFormData,
  CreateCashSessionFormData,
} from "../types";

export const cashOverviewKey = [...queryKeys.cash, "overview-v2"] as const;

export function useCashOverview() {
  const client = useQueryClient();
  const overview = useQuery({
    queryKey: cashOverviewKey,
    queryFn: ({ signal }) => cashOverviewService.getOverview(signal),
  });
  const refresh = () => invalidateAfterMutation(client, "cash");
  const open = useMutation({
    mutationFn: (dto: CreateCashSessionFormData) => cashService.create(dto),
    meta: { skipGlobalErrorToast: true },
    onSuccess: (session) => {
      client.setQueryData(queryKeys.currentCash, session);
      refresh();
    },
  });
  const close = useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: CloseCashSessionFormData }) =>
      cashService.close(id, dto),
    meta: { skipGlobalErrorToast: true },
    onSuccess: () => {
      client.setQueryData(queryKeys.currentCash, null);
      refresh();
    },
  });
  const deposit = useMutation({
    mutationFn: ({
      sessionId,
      dto,
    }: {
      sessionId: string;
      dto: CreateCashDepositFormData;
    }) => cashService.createDeposit(sessionId, dto),
    meta: { skipGlobalErrorToast: true },
    onSuccess: refresh,
  });
  const withdrawal = useMutation({
    mutationFn: ({
      sessionId,
      request,
    }: {
      sessionId: string;
      request: CashWithdrawalRequest;
    }) => cashService.createWithdrawal(sessionId, request),
    retry: false,
    meta: { skipGlobalErrorToast: true },
    onSuccess: refresh,
    onError: (error) => {
      if (error instanceof ApiError && error.status === 409) refresh();
    },
  });
  return {
    overview,
    openCashSession: open.mutateAsync,
    closeCashSession: close.mutateAsync,
    createDeposit: deposit.mutateAsync,
    createWithdrawal: withdrawal.mutateAsync,
    isOpening: open.isPending,
    isClosing:
      close.isPending ||
      (close.isSuccess &&
        overview.data?.activeSession?.id === close.variables?.id),
    isCreatingDeposit: deposit.isPending,
    isCreatingWithdrawal: withdrawal.isPending,
  };
}
