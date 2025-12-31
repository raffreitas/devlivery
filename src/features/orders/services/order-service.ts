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

interface OrderPaymentDto {
  id: string;
  paymentMethod: string;
  amount: number;
}

interface OrderDto {
  id: string;
  items: OrderItemDto[];
  customerName: string;
  customerPhone?: string;
  deliveryAddress: string;
  status: Order["status"] | string;
  payments: OrderPaymentDto[];
  total: number;
  deliveryFee: number;
  notes?: string | null;
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
    payments: dto.payments.map((p) => ({
      id: p.id,
      method: p.paymentMethod as PaymentMethod,
      amount: p.amount,
    })),
    total: dto.total,
    deliveryFee: dto.deliveryFee,
    notes: dto.notes ?? undefined,
    createdAt: new Date(dto.createdAt),
    updatedAt: new Date(dto.updatedAt),
  };
}

export const orderService = {
  getAll: async (params?: {
    startDate?: string;
    endDate?: string;
    paymentMethod?: string;
  }): Promise<Order[]> => {
    let url = "/api/orders";
    if (params?.startDate || params?.endDate || params?.paymentMethod) {
      const qp = new URLSearchParams();
      if (params.startDate) qp.set("start", params.startDate);
      if (params.endDate) qp.set("end", params.endDate);
      if (params.paymentMethod) qp.set("paymentMethod", params.paymentMethod);
      url = `${url}?${qp.toString()}`;
    }
    const res = await api.get<ApiResponse<OrderDto[] | null>>(url);
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

  create: async (data: OrderFormData): Promise<void> => {
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
      payments: data.payments.map((p) => ({
        id: p.id,
        method: p.method,
        amount: p.amount,
      })),
      notes: data.notes ?? null,
    };
    await api.post<void>("/api/orders", payload);
  },

  update: async (id: string, data: OrderFormData): Promise<void> => {
    const payload = {
      id,
      items: data.items.map((i) => ({
        productId: i.product.id,
        quantity: i.quantity,
        notes: i.notes ?? null,
      })),
      customerName: data.customerName,
      customerPhone: data.customerPhone,
      deliveryAddress: data.deliveryAddress,
      deliveryFee: data.deliveryFee,
      payments: data.payments.map((p) => ({
        id: p.id,
        method: p.method,
        amount: p.amount,
      })),
      notes: data.notes ?? null,
    };
    await api.put<void>(`/api/orders/${id}`, payload);
  },

  updateStatus: async (id: string, status: Order["status"]): Promise<void> => {
    await api.patch<void>(`/api/orders/${id}/status`, {
      status,
    });
  },

  delete: async (id: string): Promise<void> => {
    await api.delete<void>(`/api/orders/${id}`);
  },
};
