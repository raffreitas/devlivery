import { zodResolver } from "@hookform/resolvers/zod";
import { useId } from "react";
import { Controller, useForm } from "react-hook-form";
import { LoadingButton } from "@/shared/components/loading";
import { Button } from "@/shared/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { InputMoney } from "@/shared/components/ui/input-money";
import { Textarea } from "@/shared/components/ui/textarea";
import {
  type CreateCashDepositFormData,
  createCashDepositFormSchema,
} from "../types";

interface CashDepositFormProps {
  onSubmit: (data: CreateCashDepositFormData) => Promise<void>;
  isLoading?: boolean;
}

export function CashDepositForm({
  onSubmit,
  isLoading = false,
}: CashDepositFormProps) {
  const formId = useId();
  const form = useForm<CreateCashDepositFormData>({
    resolver: zodResolver(createCashDepositFormSchema),
    defaultValues: {
      amount: 0,
      notes: "",
    },
  });

  const handleFormSubmit = async (data: CreateCashDepositFormData) => {
    await onSubmit(data);
    form.reset();
  };

  return (
    <form onSubmit={form.handleSubmit(handleFormSubmit)}>
      <FieldGroup>
        <Controller
          control={form.control}
          name="amount"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-cash-deposit-amount`}>
                Valor do Aporte (R$) <span className="text-destructive">*</span>
              </FieldLabel>
              <InputMoney
                {...field}
                id={`${formId}-cash-deposit-amount`}
                disabled={isLoading}
                placeholder="0.00"
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid
                    ? `${formId}-cash-deposit-amount-error`
                    : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-cash-deposit-amount-error`}
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
                Motivo ou Observações (Opcional)
              </FieldLabel>
              <Textarea
                {...field}
                id={`${formId}-cash-deposit-notes`}
                disabled={isLoading}
                placeholder="Motivo do aporte (ex: Troco, Falta de dinheiro...)"
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

        <Field orientation="horizontal">
          <Button
            type="button"
            variant="outline"
            disabled={isLoading}
            onClick={() => form.reset()}
            className="flex-1"
          >
            Limpar
          </Button>
          <LoadingButton
            type="submit"
            isLoading={isLoading}
            loadingText="Adicionando..."
            variant="default"
            className="flex-1"
          >
            Adicionar Aporte
          </LoadingButton>
        </Field>
      </FieldGroup>
    </form>
  );
}
