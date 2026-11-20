import { zodResolver } from "@hookform/resolvers/zod";
import { useId, useRef } from "react";
import { Controller, useForm } from "react-hook-form";
import { FormError } from "@/shared/components/form-error";
import { InputMoney } from "@/shared/components/input-money";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Textarea } from "@/shared/components/ui/textarea";
import { applyFormError } from "@/shared/utils/form-errors";
import {
  type CreateCashDepositFormData,
  createCashDepositFormSchema,
} from "../types";
import { CashMovementModal } from "./cash-movement-modal";

interface CashDepositModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateCashDepositFormData) => Promise<void>;
  isLoading?: boolean;
}

export function CashDepositModal({
  isOpen,
  onClose,
  onSubmit,
  isLoading = false,
}: CashDepositModalProps) {
  const formId = useId();
  const submitting = useRef(false);
  const form = useForm<CreateCashDepositFormData>({
    resolver: zodResolver(createCashDepositFormSchema),
    defaultValues: {
      amount: 0,
      notes: "",
    },
  });

  const dismiss = () => {
    if (submitting.current || isLoading) return;
    form.reset();
    onClose();
  };

  return (
    <CashMovementModal
      title="Aporte"
      description="Colocar dinheiro na gaveta durante o turno."
      formId={formId}
      isOpen={isOpen}
      isBusy={form.formState.isSubmitting || isLoading}
      onClose={dismiss}
      submitLabel="Registrar aporte"
      loadingText="Registrando aporte…"
      onSubmit={form.handleSubmit(async (data) => {
        if (submitting.current || isLoading) return;
        submitting.current = true;
        form.clearErrors();
        try {
          await onSubmit(data);
          form.reset();
          onClose();
        } catch (error) {
          applyFormError(form, error, ["amount", "notes"]);
        } finally {
          submitting.current = false;
        }
      })}
    >
      <FormError
        message={form.formState.errors.root?.server?.message}
        errorType={form.formState.errors.root?.server?.type}
      />
      <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
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
          name="notes"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-cash-deposit-notes`}>
                Observação (opcional)
              </FieldLabel>
              <Textarea
                {...field}
                id={`${formId}-cash-deposit-notes`}
                placeholder="Ex.: moedas para troco"
                className="resize-none"
                rows={3}
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid
                    ? `${formId}-cash-deposit-notes-error`
                    : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-cash-deposit-notes-error`}
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
