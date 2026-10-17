import {
  MutationObserver,
  QueryClient,
  QueryObserver,
} from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import { invalidateAfterMutation, queryKeys } from "./query-keys";

describe("query invalidation", () => {
  it.each([
    ["orders", ["orders", "dashboard", "cash-sessions"]],
    ["expenses", ["expenses", "dashboard", "cash-sessions"]],
    ["products", ["products", "orders", "dashboard"]],
    ["categories", ["expense-categories", "expenses", "dashboard"]],
    ["cash", ["cash-sessions"]],
  ] as const)(
    "invalidates dependent queries after %s writes",
    async (feature, expected) => {
      const client = new QueryClient();
      const keys = [
        queryKeys.orders,
        queryKeys.products,
        queryKeys.expenses,
        queryKeys.dashboard,
        queryKeys.categories,
        queryKeys.currentCash,
        queryKeys.deposits("session"),
      ];
      for (const key of keys) client.setQueryData(key, []);
      await invalidateAfterMutation(client, feature);
      for (const key of keys)
        expect(client.getQueryState(key)?.isInvalidated).toBe(
          expected.some((prefix) => prefix === key[0]),
        );
      client.clear();
    },
  );

  it("normalizes periods without time-of-day cache fragmentation", () => {
    expect(queryKeys.period(new Date(2026, 9, 17, 8))).toEqual(
      queryKeys.period(new Date(2026, 9, 17, 20)),
    );
  });

  it("resolves a confirmed mutation while dependent active reads are still pending", async () => {
    const client = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    let release: (value: number[]) => void = () => {};
    const pending = new Promise<number[]>((resolve) => {
      release = resolve;
    });
    client.setQueryData(queryKeys.dashboard, [1]);
    const query = new QueryObserver(client, {
      queryKey: queryKeys.dashboard,
      queryFn: () => pending,
      staleTime: Infinity,
    });
    const unsubscribe = query.subscribe(() => {});
    const mutation = new MutationObserver(client, {
      mutationFn: async () => "saved",
      onSuccess: () => invalidateAfterMutation(client, "orders"),
    });
    try {
      await expect(mutation.mutate()).resolves.toBe("saved");
      expect(mutation.getCurrentResult().isSuccess).toBe(true);
      expect(query.getCurrentResult().isFetching).toBe(true);
    } finally {
      release([2]);
      unsubscribe();
      client.clear();
    }
  });
});
