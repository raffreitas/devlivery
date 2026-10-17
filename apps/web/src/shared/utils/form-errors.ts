import type { FieldPath, FieldValues, UseFormReturn } from "react-hook-form";
import { ApiError } from "@/shared/services/api";

export function applyFormError<T extends FieldValues>(
  form: UseFormReturn<T>,
  error: unknown,
  aliases: Record<string, string> = {},
) {
  const messages: string[] = [];
  const fieldErrors = error instanceof ApiError ? error.details?.errors : null;
  for (const [key, message] of Object.entries(fieldErrors ?? {})) {
    const normalized = key
      .replace(/\[(\d+)\]/g, ".$1")
      .split(".")
      .map((part) => part.charAt(0).toLowerCase() + part.slice(1))
      .join(".");
    const path = aliases[normalized] ?? normalized;
    // Only bind errors to fields present in this form, never arbitrary server paths.
    if (form.getValues(path as FieldPath<T>) !== undefined) {
      form.setError(path as FieldPath<T>, { type: "server", message });
    }
    // Keep a summary for errors on structured/hidden fields (e.g. order items),
    // as well as individual messages associated with editable controls.
    messages.push(message);
  }
  if (
    !fieldErrors ||
    Object.keys(fieldErrors).length === 0 ||
    messages.length > 0
  ) {
    form.setError("root.server", {
      message: messages.length
        ? [...new Set(messages)].join(" ")
        : error instanceof Error
          ? error.message
          : "Não foi possível salvar. Tente novamente.",
    });
  }
}
