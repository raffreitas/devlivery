import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { orderService } from "../services/order-service";
import type { Order, OrderFormData } from "../types";

export function useOrders() {
  const queryClient = useQueryClient();

  const ordersQuery = useQuery({
    queryKey: ["orders"],
    queryFn: orderService.getAll,
    staleTime: 30_000,
  });

  const createMutation = useMutation({
    mutationFn: orderService.create,
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
    refetch: ordersQuery.refetch,
    createOrder: (data: OrderFormData) => createMutation.mutateAsync(data),
    updateOrderStatus: (id: string, status: Order["status"]) =>
      updateStatusMutation.mutateAsync({ id, status }),
    deleteOrder: (id: string) => deleteMutation.mutateAsync(id),
  };
}
