import { createTestClient, testWrapper } from "@test/render";
import { server } from "@test/server";
import { act, renderHook, waitFor } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import { useDashboardExpenses } from "./use-dashboard-expenses";
import { useDashboardOverview } from "./use-dashboard-overview";
import { useDashboardSales } from "./use-dashboard-sales";

const url = "http://localhost:5000/api/v1/dashboard";

describe("dashboard periods", () => {
  it("never reuses a disabled tab's previous-period series, including empty results", async () => {
    let release: () => void = () => {};
    const pending = new Promise<void>((resolve) => {
      release = resolve;
    });
    const requests: string[] = [];
    server.use(
      http.get(`${url}/:metric`, async ({ request, params }) => {
        const date = new URL(request.url).searchParams.get("startDate");
        const metric = String(params.metric);
        requests.push(`${metric}:${date}`);
        if (date === "2026-10-02") await pending;
        const series =
          date === "2026-10-02" ? [] : [{ date: "2026-10-01", total: 100 }];
        const data = metric.includes("over-time")
          ? { data: series }
          : metric === "expenses-by-status"
            ? { statuses: [] }
            : metric === "expenses-by-category"
              ? { categories: [] }
              : metric === "payment-breakdown"
                ? {
                    breakdown: { Cash: 0, CreditCard: 0, DebitCard: 0, Pix: 0 },
                    total: 0,
                  }
                : metric === "top-products"
                  ? { products: [] }
                  : metric === "stats"
                    ? {
                        totalOrders: 1,
                        totalRevenue: 100,
                        pendingOrders: 1,
                        deliveredOrders: 0,
                        averageOrderValue: 100,
                      }
                    : {
                        total: 100,
                        paid: 100,
                        pending: 0,
                        overdue: 0,
                        count: 1,
                      };
        return HttpResponse.json({ data });
      }),
    );
    const client = createTestClient();
    const { result, rerender } = renderHook(
      ({ day, tab }) => {
        const start = new Date(2026, 9, day);
        return {
          overview: useDashboardOverview(start, start),
          expenses: useDashboardExpenses(start, start, tab === "expenses"),
          sales: useDashboardSales(start, start, tab === "sales"),
        };
      },
      {
        initialProps: { day: 1, tab: "expenses" },
        wrapper: testWrapper(client),
      },
    );
    await waitFor(() =>
      expect(result.current.overview.queries.expensesOverTime.isSuccess).toBe(
        true,
      ),
    );
    expect(result.current.overview.expensesOverTime).toHaveLength(1);
    rerender({ day: 2, tab: "sales" });
    expect(
      result.current.overview.queries.expensesOverTime.data,
    ).toBeUndefined();
    expect(result.current.overview.queries.expensesOverTime.isPending).toBe(
      true,
    );
    await act(async () => release());
    await waitFor(() =>
      expect(result.current.overview.queries.expensesOverTime.isSuccess).toBe(
        true,
      ),
    );
    expect(result.current.overview.expensesOverTime).toEqual([]);
    expect(
      requests.filter((key) => key === "expenses-over-time:2026-10-02"),
    ).toHaveLength(1);
    expect(requests).not.toContain("expenses-by-category:2026-10-02");
  });
});
