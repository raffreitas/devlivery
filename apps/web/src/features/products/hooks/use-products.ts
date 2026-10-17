import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  invalidateAfterMutation,
  queryKeys,
} from "@/shared/services/query-keys";
import { productService } from "../services/product-service";
import type { Product, ProductFormData } from "../types";

export function useProducts() {
  const queryClient = useQueryClient();

  const productsQuery = useQuery({
    queryKey: queryKeys.products,
    queryFn: ({ signal }) => productService.getAll(signal),
    staleTime: 60_000,
    placeholderData: (previousData) => previousData,
  });

  const createMutation = useMutation({
    mutationFn: productService.create,
    meta: { skipGlobalErrorToast: true },
    onSuccess: () => invalidateAfterMutation(queryClient, "products"),
  });

  const updateMutation = useMutation({
    meta: { skipGlobalErrorToast: true },
    mutationFn: ({ id, data }: { id: string; data: ProductFormData }) =>
      productService.update(id, data),
    onSuccess: () => invalidateAfterMutation(queryClient, "products"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => productService.delete(id),
    onSuccess: () => invalidateAfterMutation(queryClient, "products"),
  });

  return {
    products: (productsQuery.data ?? []) as Product[],
    loading: productsQuery.isLoading,
    error: productsQuery.error,
    hasData: productsQuery.data !== undefined,
    isFetching: productsQuery.isFetching,
    refetch: productsQuery.refetch,
    createProduct: (data: ProductFormData) => createMutation.mutateAsync(data),
    updateProduct: (id: string, data: ProductFormData) =>
      updateMutation.mutateAsync({ id, data }),
    deleteProduct: (id: string) => deleteMutation.mutateAsync(id),
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
