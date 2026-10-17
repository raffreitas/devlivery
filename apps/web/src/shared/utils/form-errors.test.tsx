import { act, renderHook } from "@testing-library/react";
import { useForm } from "react-hook-form";
import { describe, expect, it } from "vitest";
import { ApiError } from "@/shared/services/api";
import { applyFormError } from "./form-errors";

describe("applyFormError", () => {
  it("binds nested server validation to existing controls and exposes unknown paths at form level", () => {
    const { result } = renderHook(() => {
      const form = useForm({ defaultValues: { items: [{ quantity: 1 }] } });
      return { form, errors: form.formState.errors };
    });
    act(() =>
      applyFormError(
        result.current.form,
        new ApiError("Validation", 422, {
          errors: {
            "items[0].Quantity": "Quantidade inválida",
            unknown: "Erro de negócio",
          },
        }),
      ),
    );
    expect(result.current.errors.items?.[0]?.quantity?.message).toBe(
      "Quantidade inválida",
    );
    expect(result.current.errors.root?.server?.message).toContain(
      "Erro de negócio",
    );
    expect(result.current.errors.root?.server?.message).toContain(
      "Quantidade inválida",
    );
  });
});
