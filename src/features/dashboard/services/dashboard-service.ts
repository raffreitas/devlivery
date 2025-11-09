import type { Order } from "@/features/orders/types";
import { type ApiResponse, api } from "@/shared/services/api";
import type { DashboardStats } from "../types";

export const dashboardService = {
  calculateStats: (orders: Order[]): DashboardStats => {
    const totalOrders = orders.length;
    const totalRevenue = orders
      .filter((o) => o.status !== "cancelled")
      .reduce((sum, order) => sum + order.total, 0);

    const pendingOrders = orders.filter(
      (o) =>
        o.status === "pending" ||
        o.status === "preparing" ||
        o.status === "ready",
    ).length;

    const deliveredOrders = orders.filter(
      (o) => o.status === "delivered",
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
      pending: orders.filter((o) => o.status === "pending").length,
      preparing: orders.filter((o) => o.status === "preparing").length,
      ready: orders.filter((o) => o.status === "ready").length,
      delivered: orders.filter((o) => o.status === "delivered").length,
      cancelled: orders.filter((o) => o.status === "cancelled").length,
    };
  },

  getPaymentBreakdown: (orders: Order[]) => {
    const validOrders = orders.filter((o) => o.status !== "cancelled");

    const breakdown = {
      Cash: validOrders
        .filter((o) => o.paymentMethod === "Cash")
        .reduce((sum, o) => sum + o.total, 0),
      CreditCard: validOrders
        .filter((o) => o.paymentMethod === "CreditCard")
        .reduce((sum, o) => sum + o.total, 0),
      DebitCard: validOrders
        .filter((o) => o.paymentMethod === "DebitCard")
        .reduce((sum, o) => sum + o.total, 0),
      Pix: validOrders
        .filter((o) => o.paymentMethod === "Pix")
        .reduce((sum, o) => sum + o.total, 0),
    };

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
