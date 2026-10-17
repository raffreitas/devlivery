import { testWrapper } from "@test/render";
import { server } from "@test/server";
import { renderHook, waitFor } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import type { ExpenseStatus } from "../types";
import { useExpenses } from "./use-expenses";

describe("expense filter identity", () => {
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
