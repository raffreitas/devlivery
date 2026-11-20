import { zodResolver } from "@hookform/resolvers/zod";
import { useId, useRef, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { FormError } from "@/shared/components/form-error";
import { InputMoney } from "@/shared/components/input-money";
import { Alert, AlertDescription } from "@/shared/components/ui/alert";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Textarea } from "@/shared/components/ui/textarea";
import { ApiError } from "@/shared/services/api";
import { readAuth } from "@/shared/services/auth-storage";
import { applyFormError } from "@/shared/utils/form-errors";
import {
  type CashWithdrawalRequest,
  type CreateCashWithdrawalFormData,
  cashWithdrawalRequestSchema,
  createCashWithdrawalFormSchema,
} from "../types";
import { CashMovementModal } from "./cash-movement-modal";

const initialValues = { amount: 0, reason: "" };

function readPending(key: string) {
  try {
    const raw = sessionStorage.getItem(key);
    if (!raw) return null;
    return cashWithdrawalRequestSchema.safeParse(JSON.parse(raw)).data ?? null;
  } catch {
    return null;
  }
}

export function CashWithdrawalModal({
  isOpen,
  onClose,
  sessionId,
  onSubmit,
}: {
  isOpen: boolean;
  onClose: () => void;
  sessionId: string;
  onSubmit: (request: CashWithdrawalRequest) => Promise<void>;
}) {
  const formId = useId();
  const storageKey = `devlivery@cash-withdrawal:${readAuth().user?.id}:${sessionId}`;
  const [pending, setPending] = useState(() => readPending(storageKey));
  const submitting = useRef(false);
  const form = useForm<CreateCashWithdrawalFormData>({
    resolver: zodResolver(createCashWithdrawalFormSchema),
    defaultValues: pending ?? initialValues,
  });
  const busy = form.formState.isSubmitting;

  const savePending = (next: CashWithdrawalRequest | null) => {
    setPending(next);
    try {
      if (next) sessionStorage.setItem(storageKey, JSON.stringify(next));
      else sessionStorage.removeItem(storageKey);
    } catch {
      // The mounted form keeps the request available for retry if storage is unavailable.
    }
  };
  const dismiss = () => {
    if (submitting.current) return;
    form.reset(pending ?? initialValues);
    onClose();
  };

  return (
    <CashMovementModal
      title="Sangria"
      description="Tirar dinheiro da gaveta durante o turno."
      formId={formId}
      isOpen={isOpen}
      isBusy={busy}
      onClose={dismiss}
      submitLabel={pending ? "Reenviar sangria" : "Registrar sangria"}
      loadingText="Registrando sangria…"
      onSubmit={form.handleSubmit(async (values) => {
        if (submitting.current) return;
        submitting.current = true;
        form.clearErrors();
        try {
          const request = pending ?? {
            ...values,
            requestId: crypto.randomUUID(),
          };
          savePending(request);
          await onSubmit(request);
          savePending(null);
          form.reset(initialValues);
          onClose();
        } catch (error) {
          if (
            error instanceof ApiError &&
            [400, 404, 409, 422].includes(error.status ?? 0)
          ) {
            savePending(null);
          }
          applyFormError(form, error, ["amount", "reason"]);
        } finally {
          submitting.current = false;
        }
      })}
    >
      <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
        {pending && !busy && (
          <Alert role="status">
            <AlertDescription>
              Reenvie esta mesma sangria para confirmar o registro.
            </AlertDescription>
          </Alert>
        )}
        <FormError
          message={form.formState.errors.root?.server?.message}
          errorType={form.formState.errors.root?.server?.type}
        />
        <Controller
          control={form.control}
          name="amount"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-amount`}>Valor</FieldLabel>
              <InputMoney
                {...field}
                id={`${formId}-amount`}
                className="max-md:min-h-11"
                readOnly={pending !== null}
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid ? `${formId}-amount-error` : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-amount-error`}
                  errors={[fieldState.error]}
                />
              )}
            </Field>
          )}
        />
        <Controller
          control={form.control}
          name="reason"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-reason`}>Motivo</FieldLabel>
              <Textarea
                {...field}
                id={`${formId}-reason`}
                readOnly={pending !== null}
                placeholder="Ex.: dinheiro guardado no cofre"
                rows={3}
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid ? `${formId}-reason-error` : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-reason-error`}
                  errors={[fieldState.error]}
                />
              )}
            </Field>
          )}
        />
      </FieldGroup>
    </CashMovementModal>
  );
}
