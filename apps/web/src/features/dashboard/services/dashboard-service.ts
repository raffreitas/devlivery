import { type ApiResponse, api } from "@/shared/services/api";
import { formatDate } from "@/shared/utils/formatters";
import { moneyDifference } from "@/shared/utils/money";
import type {
  DashboardStats,
  ExpensesByCategory,
  ExpensesByStatus,
  ExpenseTimeSeries,
  OrdersByStatus,
  PaymentBreakdown,
} from "../types";

interface DashboardStatsDto {
  totalOrders: number;
  totalRevenue: number;
  pendingOrders: number;
  deliveredOrders: number;
  averageOrderValue: number;
}

interface PaymentBreakdownDto {
  breakdown: {
    Cash: number;
    CreditCard: number;
    DebitCard: number;
    Pix: number;
  };
  total: number;
}

interface OrdersByStatusDto {
  pending: number;
  preparing: number;
  ready: number;
  delivered: number;
  canceled: number;
}

interface SalesOverTimeDto {
  data: Array<{ date: string; total: number }>;
}

interface TopProductsDto {
  products: Array<{ name: string; quantity: number }>;
}

interface ExpensesByCategoryDto {
  categories: Array<{ category: string; total: number; percentage: number }>;
}

interface ExpensesByStatusDto {
  statuses: Array<{ status: string; count: number; total: number }>;
}

interface ExpensesOverTimeDto {
  data: Array<{ date: string; total: number }>;
}

interface ExpenseSummaryDto {
  total: number;
  paid: number;
  pending: number;
  overdue: number;
  count: number;
}

interface UpcomingExpenseDto {
  id: string;
  category: {
    id: string;
    name: string;
    isActive: boolean;
    subcategories: Array<{
      id: string;
      name: string;
      isActive: boolean;
      subcategories: never[];
    }>;
  };
  supplier?: string | null;
  description?: string | null;
  amount: number;
  dueDate: string;
  status: string;
}

interface UpcomingExpensesDto {
  expenses: UpcomingExpenseDto[];
}

export const dashboardService = {
  async getStats(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<DashboardStats> {
    let url = "/dashboard/stats";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<DashboardStatsDto>>(url, signal);
    return res.data;
  },

  async getPaymentBreakdown(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<PaymentBreakdown> {
    let url = "/dashboard/payment-breakdown";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<PaymentBreakdownDto>>(url, signal);

    return res.data;
  },

  async getOrdersByStatus(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<OrdersByStatus> {
    let url = "/dashboard/orders-by-status";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<OrdersByStatusDto>>(url, signal);
    return {
      Pending: res.data.pending,
      Preparing: res.data.preparing,
      Ready: res.data.ready,
      Delivered: res.data.delivered,
      Canceled: res.data.canceled,
    };
  },

  async getSalesOverTime(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<ExpenseTimeSeries[]> {
    let url = "/dashboard/sales-over-time";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<SalesOverTimeDto>>(url, signal);
    return res.data.data;
  },

  async getTopProducts(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<Array<{ name: string; quantity: number }>> {
    let url = "/dashboard/top-products";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<TopProductsDto>>(url, signal);
    return res.data.products;
  },

  async getExpensesByCategory(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<ExpensesByCategory[]> {
    let url = "/dashboard/expenses-by-category";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<ExpensesByCategoryDto>>(url, signal);

    return res.data.categories;
  },

  async getExpensesByStatus(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<ExpensesByStatus[]> {
    let url = "/dashboard/expenses-by-status";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<ExpensesByStatusDto>>(url, signal);

    return res.data.statuses;
  },

  async getExpensesOverTime(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<ExpenseTimeSeries[]> {
    let url = "/dashboard/expenses-over-time";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<ExpensesOverTimeDto>>(url, signal);

    return res.data.data;
  },

  async getExpenseSummary(
    startDate?: Date,
    endDate?: Date,
    signal?: AbortSignal,
  ): Promise<ExpenseSummaryDto> {
    let url = "/dashboard/expense-summary";
    const params = new URLSearchParams();
    if (startDate) params.set("startDate", formatDate(startDate));
    if (endDate) params.set("endDate", formatDate(endDate));
    if (params.toString()) url = `${url}?${params.toString()}`;

    const res = await api.get<ApiResponse<ExpenseSummaryDto>>(url, signal);

    return res.data;
  },

  async getUpcomingExpenses(
    days = 7,
    signal?: AbortSignal,
  ): Promise<UpcomingExpenseDto[]> {
    const url = `/dashboard/upcoming-expenses?days=${days}`;

    const res = await api.get<ApiResponse<UpcomingExpensesDto>>(url, signal);

    return res.data.expenses;
  },

  calculateNetProfit(revenue: number, expenses: number): number {
    return moneyDifference(revenue, expenses);
  },
};
