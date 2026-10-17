import { zodResolver } from "@hookform/resolvers/zod";
import { Banknote, PlusIcon, TrendingUp } from "lucide-react";
import { useId } from "react";
import { Controller, useForm } from "react-hook-form";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { InputMoney } from "@/shared/components/ui/input-money";
import { Textarea } from "@/shared/components/ui/textarea";
import { formatMoney } from "@/shared/utils/formatters";
import {
  type CloseCashSessionFormData,
  closeCashSessionFormSchema,
} from "../types";
import { CashDiffAlertTitle } from "./cash-diff-alert-title";

interface CloseCashFormProps {
  expectedCashAmount: number;
  openingAmount: number;
  depositsTotal: number;
  cashSales: number;
  onSubmit: (dto: CloseCashSessionFormData) => Promise<void>;
  isSubmitting: boolean;
  onCancel: () => void;
}

export function CloseCashForm({
  expectedCashAmount,
  openingAmount,
  depositsTotal,
  cashSales,
  onSubmit,
  isSubmitting,
  onCancel,
}: CloseCashFormProps) {
  const formId = useId();
  const form = useForm<CloseCashSessionFormData>({
    resolver: zodResolver(closeCashSessionFormSchema),
    defaultValues: {
      closingAmount: 0,
      notes: "",
    },
  });

  const handleSubmit = async (data: CloseCashSessionFormData) => {
    await onSubmit({
      closingAmount: data.closingAmount,
      notes: data.notes?.trim() || undefined,
    });
  };

  const closingAmount = form.watch("closingAmount");
  const difference = closingAmount ? closingAmount - expectedCashAmount : 0;
  const hasDifference = closingAmount > 0 && difference !== 0;

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)}>
      <FieldGroup>
        {/* Breakdown do valor esperado */}
        <Alert>
          <Banknote className="w-5 h-5" />
          <AlertTitle>Composição do Valor Esperado</AlertTitle>
          <AlertDescription>
            <div className="space-y-2 text-sm mt-2 w-full">
              <div className="flex items-center justify-between">
                <span>Abertura</span>
                <span className="font-medium">
                  {formatMoney(openingAmount)}
                </span>
              </div>
              {depositsTotal > 0 && (
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-1">
                    <PlusIcon className="w-3 h-3" />
                    Aportes
                  </span>
                  <span className="font-medium">
                    {formatMoney(depositsTotal)}
                  </span>
                </div>
              )}
              <div className="flex items-center justify-between">
                <span className="flex items-center gap-1">
                  <TrendingUp className="w-3 h-3" />
                  Pagamentos em Dinheiro
                </span>
                <span className="font-medium">{formatMoney(cashSales)}</span>
              </div>
              <div className="pt-3 border-t border-border mt-3">
                <div className="flex items-center justify-between">
                  <p className="text-xs font-medium">Total Esperado</p>
                  <p className="text-lg font-bold">
                    {formatMoney(expectedCashAmount)}
                  </p>
                </div>
              </div>
            </div>
          </AlertDescription>
        </Alert>

        {/* Valor real contado */}
        <Controller
          control={form.control}
          name="closingAmount"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-close-cash-amount`}>
                Valor Real no Caixa
              </FieldLabel>
              <InputMoney
                {...field}
                id={`${formId}-close-cash-amount`}
                placeholder="0,00"
                aria-invalid={fieldState.invalid}
                aria-describedby={[
                  `${formId}-close-cash-amount-description`,
                  fieldState.invalid
                    ? `${formId}-close-cash-amount-error`
                    : undefined,
                ]
                  .filter(Boolean)
                  .join(" ")}
              />
              <FieldDescription id={`${formId}-close-cash-amount-description`}>
                Conte o dinheiro físico no caixa e informe o valor total
              </FieldDescription>
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-close-cash-amount-error`}
                  errors={[fieldState.error]}
                />
              )}
            </Field>
          )}
        />

        {/* Diferença - sempre visível quando há valor digitado */}
        {closingAmount > 0 && (
          <Alert>
            <AlertTitle>
              <CashDiffAlertTitle difference={difference} />
            </AlertTitle>
            {hasDifference && (
              <AlertDescription>
                {difference > 0
                  ? "Você pode registrar uma observação sobre a sobra no campo abaixo."
                  : "Por favor, verifique a contagem ou registre uma observação sobre a diferença."}
              </AlertDescription>
            )}
          </Alert>
        )}

        <Controller
          control={form.control}
          name="notes"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-close-cash-notes`}>
                Observações (opcional)
              </FieldLabel>
              <Textarea
                {...field}
                id={`${formId}-close-cash-notes`}
                placeholder="Observações sobre o fechamento do caixa..."
                rows={3}
                disabled={isSubmitting}
                className="resize-none"
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid
                    ? `${formId}-close-cash-notes-error`
                    : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-close-cash-notes-error`}
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
          <Button
            type="submit"
            disabled={isSubmitting}
            variant="destructive"
            className="flex-1"
          >
            {isSubmitting ? "Fechando..." : "Fechar Caixa"}
          </Button>
        </Field>
      </FieldGroup>
    </form>
  );
}
