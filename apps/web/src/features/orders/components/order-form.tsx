import { zodResolver } from "@hookform/resolvers/zod";
import {
  Banknote,
  Check,
  CreditCard,
  type LucideIcon,
  QrCode,
  Smartphone,
  Wallet,
} from "lucide-react";
import { useEffect, useId } from "react";
import { Controller, useForm } from "react-hook-form";
import { useProducts } from "@/features/products/hooks/use-products";
import { FormError } from "@/shared/components/form-error";
import { InputMoney } from "@/shared/components/input-money";
import { LoadingButton } from "@/shared/components/loading";
import { Button } from "@/shared/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { Textarea } from "@/shared/components/ui/textarea";
import {
  ToggleGroup,
  ToggleGroupItem,
} from "@/shared/components/ui/toggle-group";
import { applyFormError } from "@/shared/utils/form-errors";
import {
  getPaymentOptionLabel,
  getPaymentOptions,
} from "../constants/payment-methods";
import { useOrderItems } from "../hooks/use-order-items";
import { useOrderPayments } from "../hooks/use-order-payments";
import {
  type OrderFormData,
  orderFormSchema,
  type PaymentMethod,
} from "../types";
import { OrderItemsTable } from "./order-form-items-table";
import { ProductSelector } from "./order-form-product-selector";

const PAYMENT_ICONS: Partial<Record<PaymentMethod, LucideIcon>> = {
  Cash: Banknote,
  CreditCard: CreditCard,
  DebitCard: Wallet,
  Pix: QrCode,
};

interface OrderFormProps {
  initialData?: OrderFormData & { id?: string };
  onSubmit: (data: OrderFormData) => Promise<void>;
  onCancel: () => void;
  isSubmitting?: boolean;
  onDirtyChange?: (dirty: boolean) => void;
}

export function OrderForm({
  initialData,
  onSubmit,
  onCancel,
  isSubmitting: externalIsSubmitting,
  onDirtyChange,
}: OrderFormProps) {
  const formId = useId();
  const { products } = useProducts();

  const form = useForm<OrderFormData>({
    resolver: zodResolver(orderFormSchema),
    defaultValues: {
      customerName: initialData?.customerName ?? "",
      customerPhone: initialData?.customerPhone ?? "",
      deliveryAddress: initialData?.deliveryAddress ?? "",
      deliveryFee: initialData?.deliveryFee ?? 0,
      payments: initialData?.payments.map((p) => ({
        id: p.id,
        method: p.method,
        amount: p.amount,
      })) ?? [{ method: "Cash", amount: 0 }],
      notes: initialData?.notes ?? "",
      items: initialData?.items ?? [],
    },
  });

  const {
    fields,
    removeItem,
    subtotal,
    selectableProducts,
    selectedProductId,
    quantity,
    notes,
    setSelectedProductId,
    setQuantity,
    setNotes,
    addSelectedItem,
    hasPendingSelection,
  } = useOrderItems(form, products, initialData?.items);

  const {
    paymentFields,
    deliveryFee,
    total,
    remainingAmount,
    hasElectronicExcess,
    changePaymentMethods,
  } = useOrderPayments(form, subtotal);

  const isDirty = form.formState.isDirty;
  useEffect(() => {
    onDirtyChange?.(isDirty || hasPendingSelection);
  }, [isDirty, hasPendingSelection, onDirtyChange]);

  const handlePlaceOrder = async (data: OrderFormData) => {
    await onSubmit(data);
  };

  return (
    <form
      onSubmit={form.handleSubmit(async (data) => {
        form.clearErrors();
        try {
          await handlePlaceOrder(data);
        } catch (error) {
          applyFormError(form, error, [
            "customerName",
            "deliveryAddress",
            "deliveryFee",
            "notes",
            ...paymentFields.map(
              (_, index) => `payments.${index}.amount` as const,
            ),
          ]);
        }
      })}
      className="flex flex-col sm:max-h-[calc(100vh-140px)]"
    >
      <fieldset
        inert={form.formState.isSubmitting || externalIsSubmitting === true}
        disabled={form.formState.isSubmitting || externalIsSubmitting === true}
        className="flex min-h-0 flex-1 flex-col gap-4"
      >
        <FormError
          message={form.formState.errors.root?.server?.message}
          errorType={form.formState.errors.root?.server?.type}
        />
        <div className="flex-1 overflow-y-auto px-2 space-y-4">
          <div className="space-y-3">
            <h3 className="text-lg font-semibold">Dados do Cliente</h3>

            <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
              <Controller
                control={form.control}
                name="customerName"
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <FieldLabel htmlFor={`${formId}-order-customer-name`}>
                      Nome do Cliente
                    </FieldLabel>
                    <Input
                      {...field}
                      id={`${formId}-order-customer-name`}
                      placeholder="Nome do cliente"
                      aria-invalid={fieldState.invalid}
                      aria-describedby={
                        fieldState.invalid
                          ? `${formId}-order-customer-name-error`
                          : undefined
                      }
                    />
                    {fieldState.invalid && (
                      <FieldError
                        id={`${formId}-order-customer-name-error`}
                        errors={[fieldState.error]}
                      />
                    )}
                  </Field>
                )}
              />

              <Controller
                control={form.control}
                name="deliveryAddress"
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <FieldLabel htmlFor={`${formId}-order-delivery-address`}>
                      Endereço de Entrega
                    </FieldLabel>
                    <Input
                      {...field}
                      id={`${formId}-order-delivery-address`}
                      placeholder="Endereço completo"
                      aria-invalid={fieldState.invalid}
                      aria-describedby={
                        fieldState.invalid
                          ? `${formId}-order-delivery-address-error`
                          : undefined
                      }
                    />
                    {fieldState.invalid && (
                      <FieldError
                        id={`${formId}-order-delivery-address-error`}
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
                    <FieldLabel htmlFor={`${formId}-order-notes`}>
                      Observações do Pedido
                    </FieldLabel>
                    <Textarea
                      {...field}
                      id={`${formId}-order-notes`}
                      placeholder="Ex: Troco necessário, preferências do cliente, etc."
                      className="resize-none"
                      rows={2}
                      aria-invalid={fieldState.invalid}
                      aria-describedby={
                        fieldState.invalid
                          ? `${formId}-order-notes-error`
                          : undefined
                      }
                    />
                    {fieldState.invalid && (
                      <FieldError
                        id={`${formId}-order-notes-error`}
                        errors={[fieldState.error]}
                      />
                    )}
                  </Field>
                )}
              />
            </FieldGroup>
          </div>

          <div className="space-y-3 sm:space-y-4">
            <h3 className="text-base sm:text-lg font-semibold">
              Itens do Pedido
            </h3>

            <ProductSelector
              products={selectableProducts}
              selectedProductId={selectedProductId}
              quantity={quantity}
              notes={notes}
              onProductChange={setSelectedProductId}
              onQuantityChange={setQuantity}
              onNotesChange={setNotes}
              onAddItem={addSelectedItem}
            />
          </div>

          <div className="space-y-4 pt-2">
            <div className="flex items-center justify-between border-b pb-2">
              <h3 className="text-base sm:text-lg font-semibold">
                Fechamento e Pagamento
              </h3>

              <Controller
                control={form.control}
                name="deliveryFee"
                render={({ field, fieldState }) => (
                  <Field
                    orientation="horizontal"
                    data-invalid={fieldState.invalid}
                    className="w-auto gap-2"
                  >
                    <FieldLabel
                      htmlFor={`${formId}-order-delivery-fee`}
                      className="truncate"
                    >
                      Taxa de entrega:
                    </FieldLabel>
                    <InputMoney
                      {...field}
                      id={`${formId}-order-delivery-fee`}
                      className="h-8 w-24"
                      aria-invalid={fieldState.invalid}
                      aria-describedby={
                        fieldState.invalid
                          ? `${formId}-order-delivery-fee-error`
                          : undefined
                      }
                    />
                    {fieldState.invalid && (
                      <FieldError
                        id={`${formId}-order-delivery-fee-error`}
                        errors={[fieldState.error]}
                      />
                    )}
                  </Field>
                )}
              />
            </div>

            <ToggleGroup
              multiple
              variant="outline"
              size="sm"
              value={paymentFields.map((p) => p.method)}
              onValueChange={changePaymentMethods}
              aria-label="Formas de pagamento"
              className="flex-wrap"
            >
              {getPaymentOptions().map((option) => {
                const Icon = PAYMENT_ICONS[option.value] ?? Smartphone;

                return (
                  <ToggleGroupItem key={option.value} value={option.value}>
                    <Icon data-icon="inline-start" />
                    {option.label}
                  </ToggleGroupItem>
                );
              })}
            </ToggleGroup>

            <div className="flex flex-col sm:flex-row gap-2 animate-in fade-in slide-in-from-top-1 duration-200">
              {paymentFields.map((field, index) => (
                <div
                  key={field.id}
                  className="group relative flex flex-1 flex-col p-2 rounded-lg border bg-muted/20 hover:border-primary/20 transition-colors shadow-none"
                >
                  <div className="flex items-center justify-between px-1 mb-0.5">
                    <span className="text-[9px] font-bold tracking-tight text-muted-foreground uppercase">
                      {getPaymentOptionLabel(field.method)}
                    </span>
                  </div>
                  <Controller
                    control={form.control}
                    name={`payments.${index}.amount`}
                    render={({ field: amountField, fieldState }) => (
                      <Field
                        data-invalid={fieldState.invalid}
                        className="gap-1"
                      >
                        <InputMoney
                          {...amountField}
                          id={`${formId}-payment-${field.id}-amount`}
                          aria-label={`Valor em ${getPaymentOptionLabel(field.method)}`}
                          aria-invalid={fieldState.invalid}
                          aria-describedby={
                            fieldState.invalid
                              ? `${formId}-payment-${field.id}-amount-error`
                              : undefined
                          }
                        />
                        {fieldState.invalid && (
                          <FieldError
                            id={`${formId}-payment-${field.id}-amount-error`}
                            errors={[fieldState.error]}
                            className="text-[10px]"
                          />
                        )}
                      </Field>
                    )}
                  />
                </div>
              ))}
            </div>

            {total > 0 && (
              <div className="flex flex-col gap-1.5 pt-1 border-t border-dashed mt-2">
                <div className="flex items-center justify-between text-[11px]">
                  <span className="text-muted-foreground">
                    Total Pedido: R$ {total.toFixed(2)}
                  </span>
                  {hasElectronicExcess ? (
                    <span className="text-destructive font-bold">
                      Revise os pagamentos eletrônicos
                    </span>
                  ) : remainingAmount > 0 ? (
                    <span className="text-amber-600 font-bold flex items-center gap-1">
                      <div className="size-1.5 rounded-full bg-amber-500 animate-pulse" />
                      Falta: R$ {remainingAmount.toFixed(2)}
                    </span>
                  ) : remainingAmount < 0 ? (
                    <span className="text-blue-600 font-bold">
                      Troco: R$ {Math.abs(remainingAmount).toFixed(2)}
                    </span>
                  ) : (
                    <span className="text-green-600 font-bold flex items-center gap-1">
                      <Check className="size-3" /> CONFERE
                    </span>
                  )}
                </div>
              </div>
            )}

            {(form.formState.errors.payments?.root?.message ??
              form.formState.errors.payments?.message) && (
              <p className="text-sm font-medium text-destructive mt-1">
                {form.formState.errors.payments?.root?.message ??
                  form.formState.errors.payments?.message}
              </p>
            )}
          </div>

          <OrderItemsTable
            items={fields.map((field) => ({
              ...field,
              fieldId: field.id,
            }))}
            subtotal={subtotal}
            deliveryFee={deliveryFee}
            total={total}
            onRemoveItem={(index) => {
              removeItem(index);
            }}
          />
          {form.formState.errors.items && (
            <p className="text-sm font-medium text-destructive">
              {form.formState.errors.items.message}
            </p>
          )}
        </div>

        <div className="flex gap-2 justify-center sm:justify-end pt-2">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            className="flex-1 sm:flex-initial"
            disabled={
              form.formState.isSubmitting || externalIsSubmitting === true
            }
          >
            Cancelar
          </Button>
          <LoadingButton
            type="submit"
            className="flex-1 sm:flex-initial"
            isLoading={
              form.formState.isSubmitting || externalIsSubmitting === true
            }
            loadingText={initialData?.id ? "Atualizando..." : "Criando..."}
          >
            {initialData?.id ? "Atualizar" : "Criar"} Pedido
          </LoadingButton>
        </div>
      </fieldset>
    </form>
  );
}
