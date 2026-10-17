import { testWrapper } from "@test/render";
import { server } from "@test/server";
import { act, renderHook, waitFor } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import type { PaymentMethod } from "../types";
import { useOrders } from "./use-orders";

describe("order filter identity", () => {
  it.each([
    { day: 10, payment: undefined },
    { day: 17, payment: "Pix" as PaymentMethod },
  ])(
    "does not present data from previous filters while loading or after failure ($day, $payment)",
    async (changed) => {
      let release: () => void = () => {};
      const pending = new Promise<void>((resolve) => {
        release = resolve;
      });
      server.use(
        http.get("http://localhost:5000/api/v1/orders", async ({ request }) => {
          const params = new URL(request.url).searchParams;
          if (
            params.get("start") !== "2026-10-17" ||
            params.has("paymentMethod")
          ) {
            await pending;
            return HttpResponse.json(
              { detail: "Falha na consulta" },
              { status: 500 },
            );
          }
          return HttpResponse.json({
            data: [
              {
                id: "original",
                items: [],
                payments: [],
                customerName: "Cliente",
                deliveryAddress: "Rua A",
                status: "Pending",
                total: 10,
                deliveryFee: 0,
                createdAt: "2026-10-17T12:00:00Z",
                updatedAt: "2026-10-17T12:00:00Z",
              },
            ],
          });
        }),
      );
      const initialProps = {
        day: 17,
        payment: undefined as PaymentMethod | undefined,
      };
      const { result, rerender } = renderHook(
        ({ day, payment }) =>
          useOrders(new Date(2026, 9, day), new Date(2026, 9, day), payment),
        { initialProps, wrapper: testWrapper() },
      );
      await waitFor(() =>
        expect(result.current.orders[0]?.id).toBe("original"),
      );
      rerender(changed);
      expect(result.current.hasData).toBe(false);
      expect(result.current.orders).toEqual([]);
      await act(async () => release());
      await waitFor(() => expect(result.current.error).not.toBeNull());
      expect(result.current.hasData).toBe(false);
      expect(result.current.orders).toEqual([]);
      rerender(initialProps);
      await waitFor(() =>
        expect(result.current.orders[0]?.id).toBe("original"),
      );
    },
  );
});
