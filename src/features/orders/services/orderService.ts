import type { Order, OrderFormData } from "../types";

const STORAGE_KEY = "orders";

export const orderService = {
  getAll: (): Order[] => {
    const data = localStorage.getItem(STORAGE_KEY);
    return data ? JSON.parse(data) : [];
  },

  getById: (id: string): Order | null => {
    const orders = orderService.getAll();
    return orders.find((o) => o.id === id) || null;
  },

  getTodayOrders: (): Order[] => {
    const orders = orderService.getAll();
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return orders.filter((order) => {
      const orderDate = new Date(order.createdAt);
      orderDate.setHours(0, 0, 0, 0);
      return orderDate.getTime() === today.getTime();
    });
  },

  create: (data: OrderFormData): Order => {
    const orders = orderService.getAll();
    const total = data.items.reduce(
      (sum, item) => sum + item.product.price * item.quantity,
      0,
    );

    const newOrder: Order = {
      ...data,
      id: crypto.randomUUID(),
      status: "pending",
      total,
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    orders.push(newOrder);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(orders));
    return newOrder;
  },

  updateStatus: (id: string, status: Order["status"]): Order => {
    const orders = orderService.getAll();
    const index = orders.findIndex((o) => o.id === id);
    if (index === -1) throw new Error("Order not found");

    orders[index] = {
      ...orders[index],
      status,
      updatedAt: new Date(),
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(orders));
    return orders[index];
  },

  delete: (id: string): void => {
    const orders = orderService.getAll();
    const filtered = orders.filter((o) => o.id !== id);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
  },
};
