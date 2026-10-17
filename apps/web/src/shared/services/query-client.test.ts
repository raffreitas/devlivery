import { describe, expect, it, vi } from "vitest";
import { toast } from "@/shared/components/ui/toast";
import { ApiError, UnauthorizedError } from "./api";
import { createAppQueryClient } from "./query-client";

describe("application query feedback", () => {
  it("does not stack global toasts for failures already rendered inline", async () => {
    const toastSpy = vi.spyOn(toast, "add");
    const client = createAppQueryClient(vi.fn());
    await Promise.allSettled(
      ["stats", "expense-summary", "sales-over-time"].map((metric) =>
        client.fetchQuery({
          queryKey: ["dashboard", metric],
          retry: false,
          queryFn: async () => {
            throw new ApiError("Indisponível", 503);
          },
        }),
      ),
    );
    expect(toastSpy).not.toHaveBeenCalled();
    client.clear();
  });

  it("still handles unauthorized queries globally", async () => {
    const onUnauthorized = vi.fn();
    const client = createAppQueryClient(onUnauthorized);
    await expect(
      client.fetchQuery({
        queryKey: ["protected"],
        queryFn: async () => {
          throw new UnauthorizedError();
        },
      }),
    ).rejects.toBeInstanceOf(UnauthorizedError);
    expect(onUnauthorized).toHaveBeenCalledOnce();
    client.clear();
  });

  it.each([true, false])(
    "respects mutation-local feedback with skipGlobalErrorToast=%s",
    async (skipGlobalErrorToast) => {
      const toastSpy = vi.spyOn(toast, "add");
      const client = createAppQueryClient(vi.fn());
      const mutation = client.getMutationCache().build(client, {
        meta: { skipGlobalErrorToast },
        mutationFn: async () => {
          throw new ApiError("Falha ao salvar", 422);
        },
      });
      await expect(mutation.execute(undefined)).rejects.toBeInstanceOf(
        ApiError,
      );
      expect(toastSpy).toHaveBeenCalledTimes(skipGlobalErrorToast ? 0 : 1);
      client.clear();
    },
  );
});
