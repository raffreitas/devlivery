import type { Product } from "@/features/products/types";
import { type ApiResponse, api } from "@/shared/services/api";
import type { Order, OrderFormData, PaymentMethod } from "../types";

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

interface OrderItemDto {
  product: ProductDto;
  quantity: number;
  notes?: string | null;
}

interface OrderDto {
  id: string;
  items: OrderItemDto[];
  customerName: string;
  customerPhone?: string;
  deliveryAddress: string;
  status: Order["status"] | string;
  paymentMethod: string;
  total: number;
  deliveryFee: number;
  createdAt: string;
  updatedAt: string;
}

function mapProduct(dto: ProductDto): Product {
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

function mapOrder(dto: OrderDto): Order {
  return {
    id: dto.id,
    items: dto.items.map((i) => ({
      product: mapProduct(i.product),
      quantity: i.quantity,
      notes: i.notes ?? undefined,
    })),
    customerName: dto.customerName,
    customerPhone: dto.customerPhone,
    deliveryAddress: dto.deliveryAddress,
    status: dto.status as Order["status"],
    paymentMethod: dto.paymentMethod as PaymentMethod,
    total: dto.total,
    deliveryFee: dto.deliveryFee,
    createdAt: new Date(dto.createdAt),
    updatedAt: new Date(dto.updatedAt),
  };
}

export const orderService = {
  getAll: async (): Promise<Order[]> => {
    const res = await api.get<ApiResponse<OrderDto[] | null>>("/api/orders");
    const list = res.data ?? [];
    return list.map(mapOrder);
  },

  getById: async (id: string): Promise<Order | null> => {
    const res = await api.get<ApiResponse<OrderDto | null>>(
      `/api/orders/${id}`,
    );
    return res.data ? mapOrder(res.data) : null;
  },

  getTodayOrders: async (): Promise<Order[]> => {
    const all = await orderService.getAll();
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return all.filter((order) => {
      const orderDate = new Date(order.createdAt);
      orderDate.setHours(0, 0, 0, 0);
      return orderDate.getTime() === today.getTime();
    });
  },

  create: async (data: OrderFormData): Promise<Order> => {
    const payload = {
      items: data.items.map((i) => ({
        productId: i.product.id,
        quantity: i.quantity,
        notes: i.notes ?? null,
      })),
      customerName: data.customerName,
      customerPhone: data.customerPhone,
      deliveryAddress: data.deliveryAddress,
      deliveryFee: data.deliveryFee,
      paymentMethod: data.paymentMethod,
    };
    const res = await api.post<ApiResponse<OrderDto | null>>(
      "/api/orders",
      payload,
    );
    if (!res.success || !res.data)
      throw new Error(res.message || "Erro ao criar pedido");
    return mapOrder(res.data);
  },

  updateStatus: async (id: string, status: Order["status"]): Promise<Order> => {
    const res = await api.patch<ApiResponse<OrderDto | null>>(
      `/api/orders/${id}/status`,
      { status },
    );
    if (!res.success || !res.data)
      throw new Error(res.message || "Erro ao atualizar status do pedido");
    return mapOrder(res.data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete<void>(`/api/orders/${id}`);
  },
};
