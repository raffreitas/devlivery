import { apiV2 } from "@/shared/services/api";
import type { PaymentMethodTotal } from "../types";

export interface CashSessionSummary {
  id: string;
  attendant: string;
  openingAmount: number;
  expectedCashAmount: number;
  totalRevenue: number;
  totalOrders: number;
  depositsTotal: number;
  cashPayments: number;
  cashRefunds: number;
  changeAmount: number;
  netCashReceived: number;
  closingAmount: number | null;
  difference: number | null;
  startAt: string;
  endAt: string | null;
  notes: string | null;
  paymentBreakdown: PaymentMethodTotal[];
}
export interface CashOverview {
  activeSession: CashSessionSummary | null;
  lastClosedSession: CashSessionSummary | null;
  receivableCash: { amount: number; orderCount: number } | null;
}
export interface CashMovement {
  id: string;
  createdAt: string;
  type: string;
  signedAmount: number;
  method: string | null;
  relatedOrderId: string | null;
  orderPaymentId: string | null;
  createdBy: string;
  authorName: string;
  reason: string | null;
}
export interface CashMovementsPage {
  data: CashMovement[];
  meta: {
    currentPage: number;
    totalCount: number;
    pageSize: number;
    totalPages: number;
  };
}

export const cashOverviewService = {
  async getOverview(signal?: AbortSignal): Promise<CashOverview> {
    const response = await apiV2.get<{ data: CashOverview }>(
      "/cash-register/overview",
      signal,
    );
    return response.data;
  },
  getMovements(id: string, page: number, signal?: AbortSignal) {
    return apiV2.get<CashMovementsPage>(
      `/cash-register/sessions/${id}/movements?page=${page}&pageSize=50`,
      signal,
    );
  },
};
