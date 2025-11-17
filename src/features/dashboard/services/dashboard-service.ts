import type { Order, PaymentMethod } from "@/features/orders/types";
import { type ApiResponse, api } from "@/shared/services/api";
import type { DashboardStats } from "../types";

export const dashboardService = {
  calculateStats: (orders: Order[]): DashboardStats => {
    const totalOrders = orders.length;
    const totalRevenue = orders
      .filter((o) => o.status !== "Canceled")
      .reduce((sum, order) => sum + order.total, 0);

    const pendingOrders = orders.filter(
      (o) =>
        o.status === "Pending" ||
        o.status === "Preparing" ||
        o.status === "Ready",
    ).length;

    const deliveredOrders = orders.filter(
      (o) => o.status === "Delivered",
    ).length;

    const averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

    return {
      totalOrders,
      totalRevenue,
      pendingOrders,
      deliveredOrders,
      averageOrderValue,
    };
  },

  getOrdersByStatus: (orders: Order[]) => {
    return {
      Pending: orders.filter((o) => o.status === "Pending").length,
      Preparing: orders.filter((o) => o.status === "Preparing").length,
      Ready: orders.filter((o) => o.status === "Ready").length,
      Delivered: orders.filter((o) => o.status === "Delivered").length,
      Canceled: orders.filter((o) => o.status === "Canceled").length,
    };
  },

  getPaymentBreakdown: (orders: Order[]) => {
    const validOrders = orders.filter((o) => o.status !== "Canceled");

    const breakdown = validOrders.reduce(
      (acc, order) => {
        acc[order.paymentMethod] =
          (acc[order.paymentMethod] || 0) + order.total;
        return acc;
      },
      { Cash: 0, CreditCard: 0, DebitCard: 0, Pix: 0 } as Record<
        PaymentMethod,
        number
      >,
    );

    const total = Object.values(breakdown).reduce(
      (sum, value) => sum + value,
      0,
    );
    return { breakdown, total };
  },

  getStats: async (): Promise<DashboardStats> => {
    const res = await api.get<
      ApiResponse<{
        totalOrders: number;
        totalRevenue: number;
        pendingOrders: number;
        deliveredOrders: number;
        averageOrderValue: number;
      } | null>
    >("/api/dashboard/stats");
    if (!res.success || !res.data) {
      return {
        totalOrders: 0,
        totalRevenue: 0,
        pendingOrders: 0,
        deliveredOrders: 0,
        averageOrderValue: 0,
      };
    }
    return res.data;
  },
};
