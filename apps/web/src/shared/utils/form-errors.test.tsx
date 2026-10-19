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
        ["items.0.quantity"],
      ),
    );
    expect(result.current.errors.items?.[0]?.quantity?.message).toBe(
      "Quantidade inválida",
    );
    expect(result.current.errors.root?.server?.message).toContain(
      "Erro de negócio",
    );
    expect(result.current.errors.root?.server?.message).not.toContain(
      "Quantidade inválida",
    );
  });

  it("binds optional controls even when their current values are undefined, without repeating their errors in the summary", () => {
    const { result } = renderHook(() => {
      const form = useForm<{ paymentDate?: string; subcategoryId?: string }>({
        defaultValues: { paymentDate: undefined, subcategoryId: undefined },
      });
      return { form, errors: form.formState.errors };
    });
    act(() =>
      applyFormError(
        result.current.form,
        new ApiError("Validation", 422, {
          errors: {
            paymentDate: "Data inválida",
            subcategoryId: "Subcategoria inválida",
          },
        }),
        ["paymentDate", "subcategoryId"],
      ),
    );
    expect(result.current.errors.paymentDate?.message).toBe("Data inválida");
    expect(result.current.errors.subcategoryId?.message).toBe(
      "Subcategoria inválida",
    );
    expect(result.current.errors.root?.server).toBeUndefined();
  });

  it("keeps errors on structured or hidden fields in the summary", () => {
    const { result } = renderHook(() => {
      const form = useForm({ defaultValues: { items: [{ quantity: 1 }] } });
      return { form, errors: form.formState.errors };
    });
    act(() =>
      applyFormError(
        result.current.form,
        new ApiError("Validation", 422, {
          errors: { "items[0].Quantity": "Quantidade inválida" },
        }),
        [],
      ),
    );
    expect(result.current.errors.items).toBeUndefined();
    expect(result.current.errors.root?.server?.message).toBe(
      "Quantidade inválida",
    );
  });

  it("exposes errors without field details at form level", () => {
    const { result } = renderHook(() => {
      const form = useForm({ defaultValues: { name: "" } });
      return { form, errors: form.formState.errors };
    });
    act(() =>
      applyFormError(
        result.current.form,
        new ApiError("HTTP 500", 500, { detail: "Falha ao salvar" }),
        ["name"],
      ),
    );
    expect(result.current.errors.root?.server?.message).toBe("Falha ao salvar");
  });
});
