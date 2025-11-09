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
