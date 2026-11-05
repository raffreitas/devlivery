import { zodResolver } from "@hookform/resolvers/zod";
import { LockKeyhole } from "lucide-react";
import { useId, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { FormError } from "@/shared/components/form-error";
import { InputMoney } from "@/shared/components/input-money";
import { LoadingButton } from "@/shared/components/loading";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Textarea } from "@/shared/components/ui/textarea";
import { applyFormError } from "@/shared/utils/form-errors";
import { formatMoney } from "@/shared/utils/formatters";
import { moneyDifference } from "@/shared/utils/money";
import {
  type CloseCashSessionFormData,
  closeCashSessionFormSchema,
} from "../types";
import { CashComposition } from "./cash-composition";

interface CloseCashFormProps {
  expectedCashAmount: number;
  openingAmount: number;
  depositsTotal: number;
  cashPayments: number;
  cashRefunds: number;
  changeAmount: number;
  onSubmit: (dto: CloseCashSessionFormData) => Promise<void>;
  isSubmitting: boolean;
}

export function CloseCashForm({
  onSubmit,
  isSubmitting,
  ...composition
}: CloseCashFormProps) {
  const formId = useId();
  const [confirmed, setConfirmed] = useState(false);
  const form = useForm<CloseCashSessionFormData>({
    resolver: zodResolver(closeCashSessionFormSchema),
    defaultValues: { closingAmount: 0, notes: "" },
  });
  const busy = form.formState.isSubmitting || isSubmitting;
  const counted = form.watch("closingAmount");
  const difference = moneyDifference(counted, composition.expectedCashAmount);

  return (
    <Card size="sm">
      <CardHeader>
        <CardTitle>
          <h2>Conferir e fechar</h2>
        </CardTitle>
        <CardAction>
          <CardDescription>Passo {confirmed ? "2" : "1"} de 2</CardDescription>
        </CardAction>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={form.handleSubmit(async (data) => {
            form.clearErrors();
            if (!confirmed) {
              setConfirmed(true);
              return;
            }
            try {
              await onSubmit({
                closingAmount: data.closingAmount,
                notes: data.notes?.trim() || undefined,
              });
            } catch (error) {
              applyFormError(form, error, ["closingAmount", "notes"]);
            }
          })}
        >
          <fieldset disabled={busy} inert={busy} className="min-w-0">
            <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
              <FormError
                message={form.formState.errors.root?.server?.message}
                errorType={form.formState.errors.root?.server?.type}
              />
              {confirmed ? (
                <>
                  <CashComposition {...composition} counted={counted} />
                  <Alert role="status">
                    <AlertTitle>
                      {difference === 0
                        ? "Caixa conferido"
                        : difference > 0
                          ? "Sobra no caixa"
                          : "Falta no caixa"}
                    </AlertTitle>
                    <AlertDescription
                      className={
                        difference === 0
                          ? "text-success"
                          : difference < 0
                            ? "text-destructive"
                            : "text-foreground"
                      }
                    >
                      Diferença: {formatMoney(difference)}
                    </AlertDescription>
                  </Alert>
                  <Controller
                    control={form.control}
                    name="notes"
                    render={({ field, fieldState }) => (
                      <Field data-invalid={fieldState.invalid}>
                        <FieldLabel htmlFor={`${formId}-notes`}>
                          Observação do fechamento (opcional)
                        </FieldLabel>
                        <Textarea
                          {...field}
                          id={`${formId}-notes`}
                          rows={2}
                          placeholder="Observação sobre a conferência"
                          aria-invalid={fieldState.invalid}
                          aria-describedby={
                            fieldState.invalid
                              ? `${formId}-notes-error`
                              : undefined
                          }
                        />
                        {fieldState.invalid && (
                          <FieldError
                            id={`${formId}-notes-error`}
                            errors={[fieldState.error]}
                          />
                        )}
                      </Field>
                    )}
                  />
                </>
              ) : (
                <>
                  <p className="text-sm">
                    Conte o dinheiro da gaveta e informe o total. Cartão e Pix
                    não entram.
                  </p>
                  <Controller
                    control={form.control}
                    name="closingAmount"
                    render={({ field, fieldState }) => (
                      <Field data-invalid={fieldState.invalid}>
                        <FieldLabel htmlFor={`${formId}-amount`}>
                          Dinheiro contado
                        </FieldLabel>
                        <InputMoney
                          {...field}
                          id={`${formId}-amount`}
                          aria-invalid={fieldState.invalid}
                          aria-describedby={
                            fieldState.invalid
                              ? `${formId}-amount-error`
                              : undefined
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
                  <Alert role="note">
                    <LockKeyhole aria-hidden="true" />
                    <AlertDescription>
                      O valor esperado aparece depois que você informar a
                      contagem.
                    </AlertDescription>
                  </Alert>
                </>
              )}
              <div className="flex items-center justify-between gap-2">
                {confirmed && (
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={() => {
                      setConfirmed(false);
                      form.clearErrors();
                    }}
                  >
                    Contar de novo
                  </Button>
                )}
                <LoadingButton
                  type="submit"
                  isLoading={busy}
                  loadingText="Fechando caixa…"
                  className={confirmed ? "ml-auto" : "w-full"}
                >
                  {confirmed ? "Fechar caixa" : "Conferir contagem"}
                </LoadingButton>
              </div>
            </FieldGroup>
          </fieldset>
        </form>
      </CardContent>
    </Card>
  );
}
