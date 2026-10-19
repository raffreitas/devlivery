import type { FieldPath, FieldValues, UseFormReturn } from "react-hook-form";
import { ApiError } from "@/shared/services/api";
import { getErrorFeedback } from "@/shared/services/error-feedback";

export function applyFormError<T extends FieldValues>(
  form: UseFormReturn<T>,
  error: unknown,
  editableFields: readonly FieldPath<T>[],
) {
  form.clearErrors("root.server");
  const fields = new Set<string>(editableFields);
  const messages: string[] = [];
  const fieldErrors = error instanceof ApiError ? error.details?.errors : null;
  for (const [key, message] of Object.entries(fieldErrors ?? {})) {
    const normalized = key
      .replace(/\[(\d+)\]/g, ".$1")
      .split(".")
      .map((part) => part.charAt(0).toLowerCase() + part.slice(1))
      .join(".");
    // A registered optional control may legitimately have an undefined value.
    if (fields.has(normalized)) {
      form.setError(normalized as FieldPath<T>, { type: "server", message });
    } else {
      messages.push(message);
    }
  }
  if (
    !fieldErrors ||
    Object.keys(fieldErrors).length === 0 ||
    messages.length > 0
  ) {
    form.setError("root.server", {
      type: error instanceof ApiError ? error.kind : "server",
      message: messages.length
        ? [...new Set(messages)].join(" ")
        : getErrorFeedback(error).description,
    });
  }
}
