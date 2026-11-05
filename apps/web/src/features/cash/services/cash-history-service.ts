import { apiV2 } from "@/shared/services/api";
import type {
  CashMovementsPage,
  CashSessionSummary,
} from "./cash-overview-service";

export type CashHistorySession = Pick<
  CashSessionSummary,
  | "id"
  | "attendant"
  | "startAt"
  | "endAt"
  | "status"
  | "totalOrders"
  | "totalRevenue"
  | "closingAmount"
  | "difference"
>;
export interface CashHistoryTotals {
  closedCount: number;
  openCount: number;
  totalRevenue: number;
  totalOrders: number;
  totalDifference: number;
  sessionsWithDifference: number;
}
export interface CashHistoryResponse {
  data: { sessions: CashHistorySession[]; summary: CashHistoryTotals };
  meta: CashMovementsPage["meta"];
}
export const cashHistoryService = {
  list(start: string, end: string, page: number, signal?: AbortSignal) {
    const query = new URLSearchParams({
      start,
      end,
      page: String(page),
      pageSize: "50",
    });
    return apiV2.get<CashHistoryResponse>(
      `/cash-register/sessions?${query}`,
      signal,
    );
  },
  async get(id: string, signal?: AbortSignal) {
    const result = await apiV2.get<{ data: CashSessionSummary }>(
      `/cash-register/sessions/${id}`,
      signal,
    );
    return result.data;
  },
};
