import { zodResolver } from "@hookform/resolvers/zod";
import { Wallet } from "lucide-react";
import { useId } from "react";
import { Controller, useForm } from "react-hook-form";
import { FormError } from "@/shared/components/form-error";
import { InputMoney } from "@/shared/components/input-money";
import { Button } from "@/shared/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Textarea } from "@/shared/components/ui/textarea";
import { applyFormError } from "@/shared/utils/form-errors";
import {
  type CreateCashSessionFormData,
  createCashSessionFormSchema,
} from "../types";

interface OpenCashFormProps {
  onSubmit: (dto: CreateCashSessionFormData) => Promise<void>;
  isSubmitting: boolean;
  onCancel: () => void;
}

export function OpenCashForm({
  onSubmit,
  isSubmitting,
  onCancel,
}: OpenCashFormProps) {
  const formId = useId();
  const form = useForm<CreateCashSessionFormData>({
    resolver: zodResolver(createCashSessionFormSchema),
    defaultValues: {
      openingAmount: 0,
      notes: "",
    },
  });

  const handleSubmit = async (data: CreateCashSessionFormData) => {
    await onSubmit({
      openingAmount: data.openingAmount,
      notes: data.notes?.trim() || undefined,
    });
  };

  return (
    <form
      onSubmit={form.handleSubmit(async (data) => {
        form.clearErrors();
        try {
          await handleSubmit(data);
        } catch (error) {
          applyFormError(form, error, {});
        }
      })}
    >
      <fieldset
        inert={form.formState.isSubmitting || isSubmitting}
        disabled={form.formState.isSubmitting || isSubmitting}
        className="flex min-h-0 flex-1 flex-col gap-4"
      >
        <FormError message={form.formState.errors.root?.server?.message} />
        <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
          <Controller
            control={form.control}
            name="openingAmount"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-open-cash-amount`}>
                  Valor de Abertura
                </FieldLabel>
                <InputMoney
                  {...field}
                  id={`${formId}-open-cash-amount`}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={[
                    `${formId}-open-cash-amount-description`,
                    fieldState.invalid
                      ? `${formId}-open-cash-amount-error`
                      : undefined,
                  ]
                    .filter(Boolean)
                    .join(" ")}
                />
                <FieldDescription id={`${formId}-open-cash-amount-description`}>
                  Informe o valor inicial em dinheiro no caixa
                </FieldDescription>
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-open-cash-amount-error`}
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
                <FieldLabel htmlFor={`${formId}-open-cash-notes`}>
                  Observações (opcional)
                </FieldLabel>
                <Textarea
                  {...field}
                  id={`${formId}-open-cash-notes`}
                  placeholder="Observações sobre a abertura do caixa..."
                  rows={3}
                  disabled={isSubmitting}
                  className="resize-none"
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-open-cash-notes-error`
                      : undefined
                  }
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-open-cash-notes-error`}
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
              onClick={onCancel}
              disabled={isSubmitting}
              className="flex-1"
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={isSubmitting} className="flex-1">
              <Wallet data-icon="inline-start" />
              {isSubmitting ? "Abrindo..." : "Abrir Caixa"}
            </Button>
          </Field>
        </FieldGroup>
      </fieldset>
    </form>
  );
}
