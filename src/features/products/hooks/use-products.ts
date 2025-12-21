import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { productService } from "../services/product-service";
import type { Product, ProductFormData } from "../types";

export function useProducts() {
  const queryClient = useQueryClient();

  const productsQuery = useQuery({
    queryKey: ["products"],
    queryFn: productService.getAll,
    staleTime: 60_000,
    placeholderData: (previousData) => previousData,
  });

  const createMutation = useMutation({
    mutationFn: productService.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["products"] }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: ProductFormData }) =>
      productService.update(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["products"] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => productService.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["products"] }),
    onError: (error) => {
      const message =
        error instanceof Error ? error.message : "Falha ao deletar o produto.";
      toast.error(message);
    },
  });

  return {
    products: (productsQuery.data ?? []) as Product[],
    loading: productsQuery.isLoading,
    isFetching: productsQuery.isFetching,
    refetch: productsQuery.refetch,
    createProduct: (data: ProductFormData) => createMutation.mutateAsync(data),
    updateProduct: (id: string, data: ProductFormData) =>
      updateMutation.mutateAsync({ id, data }),
    deleteProduct: (id: string) => deleteMutation.mutateAsync(id),
  };
}
