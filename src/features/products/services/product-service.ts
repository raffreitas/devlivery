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
  getAll: async (): Promise<Product[]> => {
    const res =
      await api.get<ApiResponse<ProductDto[] | null>>("/api/products");
    const list = res.data ?? [];
    return list.map(mapProductDto);
  },

  getById: async (id: string): Promise<Product | null> => {
    const res = await api.get<ApiResponse<ProductDto | null>>(
      `/api/products/${id}`,
    );
    return res.data ? mapProductDto(res.data) : null;
  },

  create: async (data: ProductFormData): Promise<Product> => {
    const res = await api.post<ApiResponse<ProductDto | null>>(
      "/api/products",
      data,
    );
    if (!res.success || !res.data)
      throw new Error(res.message || "Erro ao criar produto");
    return mapProductDto(res.data);
  },

  update: async (id: string, data: ProductFormData): Promise<Product> => {
    const res = await api.put<ApiResponse<ProductDto | null>>(
      `/api/products/${id}`,
      data,
    );
    if (!res.success || !res.data)
      throw new Error(res.message || "Erro ao atualizar produto");
    return mapProductDto(res.data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete<void>(`/api/products/${id}`);
  },
};
