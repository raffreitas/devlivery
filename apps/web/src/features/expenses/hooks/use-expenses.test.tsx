import { testWrapper } from "@test/render";
import { server } from "@test/server";
import { renderHook, waitFor } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import type { ExpenseStatus } from "../types";
import { useExpenses } from "./use-expenses";

describe("expense filter identity", () => {
  it("filters by subcategory with period and status and clears stale totals on failure", async () => {
    const requests: URLSearchParams[] = [];
    server.use(
      http.get("http://localhost:5000/api/v1/expenses", ({ request }) => {
        const params = new URL(request.url).searchParams;
        requests.push(params);
        if (params.get("categoryId") === "broken")
          return HttpResponse.json({ detail: "Falha" }, { status: 500 });
        return HttpResponse.json({
          data: [
            {
              id: "expense",
              category: {
                id: "parent",
                name: "Categoria",
                isActive: true,
                subcategories: [],
              },
              amount: params.get("categoryId") === "child" ? 10 : 30,
              dueDate: "2026-10-17",
              status: "Paid",
              createdAt: "2026-10-17T12:00:00Z",
              updatedAt: "2026-10-17T12:00:00Z",
            },
          ],
        });
      }),
    );
    const { result, rerender } = renderHook(
      ({ subcategoryId }) =>
        useExpenses({
          categoryId: "parent",
          subcategoryId,
          status: "Paid",
          duePeriod: { from: new Date(2026, 9, 1), to: new Date(2026, 9, 31) },
        }),
      {
        initialProps: { subcategoryId: undefined as string | undefined },
        wrapper: testWrapper(),
      },
    );
    await waitFor(() => expect(result.current.summary.total).toBe(30));
    rerender({ subcategoryId: "child" });
    expect(result.current.expenses).toEqual([]);
    await waitFor(() => expect(result.current.summary.total).toBe(10));
    expect(Object.fromEntries(requests.at(-1) ?? [])).toEqual({
      categoryId: "child",
      status: "Paid",
      start: "2026-10-01",
      end: "2026-10-31",
    });
    rerender({ subcategoryId: undefined });
    await waitFor(() => expect(result.current.summary.total).toBe(30));
    rerender({ subcategoryId: "broken" });
    expect(result.current.summary.total).toBe(0);
    await waitFor(() => expect(result.current.error).not.toBeNull());
    expect(result.current.expenses).toEqual([]);
  });

  it("does not retain another status's expenses or summary after changing filters", async () => {
    server.use(
      http.get("http://localhost:5000/api/v1/expenses", ({ request }) => {
        if (new URL(request.url).searchParams.has("status"))
          return HttpResponse.json(
            { detail: "Falha na consulta" },
            { status: 500 },
          );
        return HttpResponse.json({
          data: [
            {
              id: "original",
              category: {
                id: "category",
                name: "Categoria",
                isActive: true,
                subcategories: [],
              },
              amount: 9.9,
              dueDate: "2026-10-17",
              status: "Paid",
              createdAt: "2026-10-17T12:00:00Z",
              updatedAt: "2026-10-17T12:00:00Z",
            },
          ],
        });
      }),
    );
    const { result, rerender } = renderHook(
      ({ status }) => useExpenses({ status }),
      {
        initialProps: { status: undefined as ExpenseStatus | undefined },
        wrapper: testWrapper(),
      },
    );
    await waitFor(() => expect(result.current.summary.total).toBe(9.9));
    rerender({ status: "Pending" });
    expect(result.current.hasData).toBe(false);
    expect(result.current.expenses).toEqual([]);
    await waitFor(() => expect(result.current.error).not.toBeNull());
    expect(result.current.hasData).toBe(false);
    expect(result.current.expenses).toEqual([]);
  });
});
