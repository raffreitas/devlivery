import type { Expense } from "@/features/expenses/types";
import type { Order, PaymentMethod } from "@/features/orders/types";
import { type ApiResponse, api } from "@/shared/services/api";
import type {
  DashboardStats,
  ExpensesByCategory,
  ExpensesByStatus,
  ExpenseTimeSeries,
} from "../types";

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

  getSalesOverTime: (orders: Order[]) => {
    const salesByDate = orders
      .filter((o) => o.status !== "Canceled")
      .reduce(
        (acc, order) => {
          const date = new Date(order.createdAt).toLocaleDateString("pt-BR", {
            day: "2-digit",
            month: "2-digit",
          });
          acc[date] = (acc[date] || 0) + order.total;
          return acc;
        },
        {} as Record<string, number>,
      );

    return Object.entries(salesByDate)
      .map(([date, total]) => ({ date, total }))
      .sort((a, b) => {
        const [dayA, monthA] = a.date.split("/").map(Number);
        const [dayB, monthB] = b.date.split("/").map(Number);
        return monthA - monthB || dayA - dayB;
      });
  },

  getTopProducts: (orders: Order[]) => {
    const productSales = orders
      .filter((o) => o.status !== "Canceled")
      .flatMap((o) => o.items)
      .reduce(
        (acc, item) => {
          acc[item.product.name] =
            (acc[item.product.name] || 0) + item.quantity;
          return acc;
        },
        {} as Record<string, number>,
      );

    return Object.entries(productSales)
      .map(([name, quantity]) => ({ name, quantity }))
      .sort((a, b) => b.quantity - a.quantity)
      .slice(0, 5);
  },

  getExpensesOverTime: (expenses: Expense[]): ExpenseTimeSeries[] => {
    const expensesByDate = expenses
      .filter((e) => e.status === "Paid" && e.paymentDate)
      .reduce(
        (acc, expense) => {
          if (expense.paymentDate) {
            const date = expense.paymentDate.toLocaleDateString("pt-BR", {
              day: "2-digit",
              month: "2-digit",
            });
            acc[date] = (acc[date] || 0) + expense.amount;
          }
          return acc;
        },
        {} as Record<string, number>,
      );

    return Object.entries(expensesByDate)
      .map(([date, total]) => ({ date, total }))
      .sort((a, b) => {
        const [dayA, monthA] = a.date.split("/").map(Number);
        const [dayB, monthB] = b.date.split("/").map(Number);
        return monthA - monthB || dayA - dayB;
      });
  },

  getExpensesByCategory: (expenses: Expense[]): ExpensesByCategory[] => {
    const categoryTotals = expenses.reduce(
      (acc, expense) => {
        const categoryName = expense.category.name;
        acc[categoryName] = (acc[categoryName] || 0) + expense.amount;
        return acc;
      },
      {} as Record<string, number>,
    );

    const total = Object.values(categoryTotals).reduce(
      (sum, value) => sum + value,
      0,
    );

    return Object.entries(categoryTotals)
      .map(([category, totalAmount]) => ({
        category,
        total: totalAmount,
        percentage: total > 0 ? (totalAmount / total) * 100 : 0,
      }))
      .sort((a, b) => b.total - a.total);
  },

  getExpensesByStatus: (expenses: Expense[]): ExpensesByStatus[] => {
    const statusMap = expenses.reduce(
      (acc, expense) => {
        if (!acc[expense.status]) {
          acc[expense.status] = { count: 0, total: 0 };
        }
        acc[expense.status].count += 1;
        acc[expense.status].total += expense.amount;
        return acc;
      },
      {} as Record<string, { count: number; total: number }>,
    );

    return Object.entries(statusMap).map(([status, data]) => ({
      status,
      count: data.count,
      total: data.total,
    }));
  },

  calculateNetProfit: (revenue: number, expenses: number): number => {
    return revenue - expenses;
  },

  getUpcomingExpenses: (expenses: Expense[], days: number): Expense[] => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const futureDate = new Date(today);
    futureDate.setDate(futureDate.getDate() + days);

    return expenses
      .filter((expense) => {
        if (expense.status === "Paid" || expense.status === "Cancelled") {
          return false;
        }
        const dueDate = new Date(expense.dueDate);
        dueDate.setHours(0, 0, 0, 0);
        return dueDate >= today && dueDate <= futureDate;
      })
      .sort((a, b) => {
        const dateA = new Date(a.dueDate).getTime();
        const dateB = new Date(b.dueDate).getTime();
        return dateA - dateB;
      });
  },
};
