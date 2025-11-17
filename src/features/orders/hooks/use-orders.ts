import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { orderService } from "../services/order-service";
import type { Order, OrderFormData, PaymentMethod } from "../types";

export function useOrders(
  startDate?: string,
  endDate?: string,
  paymentMethod?: PaymentMethod,
) {
  const queryClient = useQueryClient();

  const ordersQuery = useQuery({
    queryKey: ["orders", { startDate, endDate, paymentMethod }],
    queryFn: () => orderService.getAll({ startDate, endDate, paymentMethod }),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const createMutation = useMutation({
    mutationFn: orderService.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["orders"] }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: OrderFormData }) =>
      orderService.update(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["orders"] }),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: Order["status"] }) =>
      orderService.updateStatus(id, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["orders"] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => orderService.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["orders"] }),
  });

  return {
    orders: (ordersQuery.data ?? []) as Order[],
    loading: ordersQuery.isLoading,
    isFetching: ordersQuery.isFetching,
    refetch: ordersQuery.refetch,
    createOrder: (data: OrderFormData) => createMutation.mutateAsync(data),
    updateOrder: (id: string, data: OrderFormData) =>
      updateMutation.mutateAsync({ id, data }),
    updateOrderStatus: (id: string, status: Order["status"]) =>
      updateStatusMutation.mutateAsync({ id, status }),
    deleteOrder: (id: string) => deleteMutation.mutateAsync(id),
  };
}
