export interface DashboardStats {
  totalOrders: number;
  totalRevenue: number;
  pendingOrders: number;
  deliveredOrders: number;
  averageOrderValue: number;
}

export interface PaymentBreakdown {
  breakdown: {
    Cash: number;
    CreditCard: number;
    DebitCard: number;
    Pix: number;
  };
  total: number;
}

export interface OrdersByStatus {
  Pending: number;
  Preparing: number;
  Ready: number;
  Delivered: number;
  Canceled: number;
}

export interface ExpensesByCategory {
  category: string;
  total: number;
  percentage: number;
}

export interface ExpensesByStatus {
  status: string;
  count: number;
  total: number;
}

export interface ExpenseTimeSeries {
  date: string;
  total: number;
}
