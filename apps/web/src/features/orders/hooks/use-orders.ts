import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  invalidateAfterMutation,
  queryKeys,
} from "@/shared/services/query-keys";
import { formatDate } from "@/shared/utils/formatters";
import { orderService } from "../services/order-service";
import type {
  Order,
  OrderFormData,
  OrderStatus,
  PaymentMethod,
} from "../types";

export function useOrders(
  startDate?: Date,
  endDate?: Date,
  paymentMethod?: PaymentMethod,
) {
  const queryClient = useQueryClient();

  const ordersQuery = useQuery({
    queryKey: [
      ...queryKeys.orders,
      { ...queryKeys.period(startDate, endDate), paymentMethod },
    ],
    queryFn: ({ signal }) =>
      orderService.getAll(
        {
          startDate: startDate ? formatDate(startDate) : undefined,
          endDate: endDate ? formatDate(endDate) : undefined,
          paymentMethod,
        },
        signal,
      ),
    staleTime: 30_000,
  });

  const createMutation = useMutation({
    mutationFn: orderService.create,
    meta: { skipGlobalErrorToast: true },
    onSuccess: () => invalidateAfterMutation(queryClient, "orders"),
  });

  const updateMutation = useMutation({
    meta: { skipGlobalErrorToast: true },
    mutationFn: ({ id, data }: { id: string; data: OrderFormData }) =>
      orderService.update(id, data),
    onSuccess: () => invalidateAfterMutation(queryClient, "orders"),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) =>
      orderService.updateStatus(id, status),
    onSuccess: () => invalidateAfterMutation(queryClient, "orders"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => orderService.delete(id),
    onSuccess: () => invalidateAfterMutation(queryClient, "orders"),
  });

  return {
    orders: (ordersQuery.data ?? []) as Order[],
    loading: ordersQuery.isLoading,
    error: ordersQuery.error,
    hasData: ordersQuery.data !== undefined,
    isFetching: ordersQuery.isFetching,
    refetch: ordersQuery.refetch,
    createOrder: (data: OrderFormData) => createMutation.mutateAsync(data),
    updateOrder: (id: string, data: OrderFormData) =>
      updateMutation.mutateAsync({ id, data }),
    updateOrderStatus: (id: string, status: OrderStatus) =>
      updateStatusMutation.mutateAsync({ id, status }),
    deleteOrder: (id: string) => deleteMutation.mutateAsync(id),
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isUpdatingStatus: updateStatusMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
