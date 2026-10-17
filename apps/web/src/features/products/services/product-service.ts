import { type ApiResponse, api } from "@/shared/services/api";
import type { Product, ProductFormData } from "../types";

interface ProductDto {
  id: string;
  name: string;
  description: string;
  price: number;
  category: string;
  available: boolean;
  createdAt: string;
  updatedAt: string;
}

function mapProductDto(dto: ProductDto): Product {
  return {
    id: dto.id,
    name: dto.name,
    description: dto.description,
    price: dto.price,
    category: dto.category,
    available: dto.available,
    createdAt: new Date(dto.createdAt),
    updatedAt: new Date(dto.updatedAt),
  };
}

export const productService = {
  getAll: async (signal?: AbortSignal): Promise<Product[]> => {
    const res = await api.get<ApiResponse<ProductDto[] | null>>(
      "/products",
      signal,
    );
    const list = res.data ?? [];
    return list.map(mapProductDto);
  },

  getById: async (
    id: string,
    signal?: AbortSignal,
  ): Promise<Product | null> => {
    const res = await api.get<ApiResponse<ProductDto | null>>(
      `/products/${id}`,
      signal,
    );
    return res.data ? mapProductDto(res.data) : null;
  },

  create: async (data: ProductFormData): Promise<void> => {
    await api.post<void>("/products", data);
  },

  update: async (id: string, data: ProductFormData): Promise<void> => {
    await api.put<void>(`/products/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete<void>(`/products/${id}`);
  },
};
