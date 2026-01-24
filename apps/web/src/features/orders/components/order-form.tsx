import { zodResolver } from "@hookform/resolvers/zod";
import {
  Banknote,
  Check,
  CreditCard,
  QrCode,
  Smartphone,
  Wallet,
} from "lucide-react";
import { useEffect, useState } from "react";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import { useProducts } from "@/features/products/hooks/use-products";
import { LoadingButton } from "@/shared/components/loading";
import { Button } from "@/shared/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/components/ui/form";
import { Input } from "@/shared/components/ui/input";
import { InputMoney } from "@/shared/components/ui/input-money";
import { Separator } from "@/shared/components/ui/separator";
import { Textarea } from "@/shared/components/ui/textarea";
import { cn } from "@/shared/lib/utils";
import {
  getPaymentOptionLabel,
  getPaymentOptions,
} from "../constants/payment-methods";
import {
  type OrderFormData,
  orderFormSchema,
  type PaymentMethod,
} from "../types";
import { OrderItemsTable } from "./order-form-items-table";
import { ProductSelector } from "./order-form-product-selector";

interface OrderFormProps {
  initialData?: OrderFormData & { id?: string };
  onSubmit: (data: OrderFormData) => void | Promise<void>;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function OrderForm({
  initialData,
  onSubmit,
  onCancel,
  isSubmitting: externalIsSubmitting,
}: OrderFormProps) {
  const { products } = useProducts();
  const [selectedProductId, setSelectedProductId] = useState<
    string | undefined
  >();
  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState("");

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

  const { fields, append, remove, update } = useFieldArray({
    control: form.control,
    name: "items",
  });

  const {
    fields: paymentFields,
    append: appendPayment,
    remove: removePayment,
  } = useFieldArray({
    control: form.control,
    name: "payments",
  });

  const handleAddItem = () => {
    if (!selectedProductId) return;

    const product = products.find((p) => p.id === selectedProductId);
    if (!product) return;

    const existingItemIndex = fields.findIndex(
      (item) => item.product.id === selectedProductId,
    );

    const productData = {
      id: product.id,
      name: product.name,
      price: product.price,
      description: product.description,
      category: product.category,
      available: product.available,
    };

    if (existingItemIndex >= 0) {
      const existingItem = fields[existingItemIndex];
      update(existingItemIndex, {
        product: productData,
        quantity: existingItem.quantity + quantity,
        notes: notes || existingItem.notes,
      });
    } else {
      append({
        product: productData,
        quantity,
        notes: notes || undefined,
      });
    }

    setSelectedProductId(undefined);
    setQuantity(1);
    setNotes("");
  };

  const handlePlaceOrder = (data: OrderFormData) => {
    onSubmit(data);
  };

  const handleAddPaymentMethod = (
    isSelected: boolean,
    method: PaymentMethod,
  ) => {
    if (isSelected) {
      if (paymentFields.length > 1) {
        const index = paymentFields.findIndex((p) => p.method === method);
        removePayment(index);
      }
    } else {
      appendPayment({
        method: method,
        amount: Math.max(0, remainingAmount),
      });
    }
  };

  const deliveryFee =
    useWatch({
      control: form.control,
      name: "deliveryFee",
    }) ?? 0;

  const payments = useWatch({
    control: form.control,
    name: "payments",
  });

  const subtotal = fields.reduce(
    (sum, item) => sum + item.product.price * item.quantity,
    0,
  );

  const total = subtotal + deliveryFee;

  const totalPaid = payments?.reduce((sum, p) => sum + (p.amount || 0), 0) ?? 0;
  const remainingAmount = total - totalPaid;

  useEffect(() => {
    if (
      paymentFields.length === 1 &&
      payments?.[0]?.amount !== total &&
      payments?.[0]?.method !== "Cash"
    ) {
      form.setValue(`payments.0.amount`, total, { shouldValidate: true });
    }
  }, [total, paymentFields.length, form, payments]);

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(handlePlaceOrder)}
        className="flex flex-col max-h-[calc(100vh-140px)]"
      >
        <div className="flex-1 overflow-y-auto px-2 space-y-4">
          <div className="space-y-3">
            <h3 className="text-lg font-semibold">Dados do Cliente</h3>

            <FormField
              control={form.control}
              name="customerName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Nome do Cliente</FormLabel>
                  <FormControl>
                    <Input placeholder="Nome do cliente" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="deliveryAddress"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Endereço de Entrega</FormLabel>
                  <FormControl>
                    <Input placeholder="Endereço completo" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Observações do Pedido</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Ex: Troco necessário, preferências do cliente, etc."
                      className="resize-none"
                      rows={2}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <div className="space-y-3 sm:space-y-4">
            <h3 className="text-base sm:text-lg font-semibold">
              Itens do Pedido
            </h3>

            <ProductSelector
              products={products.filter((p) => p.available)}
              selectedProductId={selectedProductId}
              quantity={quantity}
              notes={notes}
              onProductChange={setSelectedProductId}
              onQuantityChange={setQuantity}
              onNotesChange={setNotes}
              onAddItem={handleAddItem}
            />
          </div>

          <div className="space-y-4 pt-2">
            <div className="flex items-center justify-between border-b pb-2">
              <h3 className="text-base sm:text-lg font-semibold">
                Fechamento e Pagamento
              </h3>

              <FormField
                control={form.control}
                name="deliveryFee"
                render={({ field }) => (
                  <FormItem className="flex items-center gap-2 space-y-0">
                    <FormLabel className="truncate py-0.5">
                      Taxa de entrega:
                    </FormLabel>
                    <FormControl>
                      <InputMoney {...field} className="h-8 w-24" />
                    </FormControl>
                  </FormItem>
                )}
              />
            </div>

            <div className="flex flex-wrap gap-1.5">
              {getPaymentOptions().map((option) => {
                const isSelected = paymentFields.some(
                  (p) => p.method === option.value,
                );

                const Icon =
                  {
                    Cash: Banknote,
                    CreditCard: CreditCard,
                    DebitCard: Wallet,
                    Pix: QrCode,
                  }[option.value] || Smartphone;

                return (
                  <Button
                    key={option.value}
                    type="button"
                    variant={isSelected ? "default" : "outline"}
                    size="sm"
                    className={cn(
                      "rounded-full px-3 h-7 gap-1.5 transition-all duration-200 border",
                      isSelected
                        ? "bg-primary border-primary hover:bg-primary/90"
                        : "hover:border-primary/50 hover:bg-primary/5 text-muted-foreground",
                    )}
                    onClick={() =>
                      handleAddPaymentMethod(isSelected, option.value)
                    }
                  >
                    <Icon className="size-3.5" />
                    <span className="text-xs">{option.label}</span>
                    {isSelected && <Check className="size-3" />}
                  </Button>
                );
              })}
            </div>

            <div className="flex gap-2 animate-in fade-in slide-in-from-top-1 duration-200">
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
                  <FormField
                    control={form.control}
                    name={`payments.${index}.amount`}
                    render={({ field }) => (
                      <FormItem className="space-y-0">
                        <FormControl>
                          <InputMoney {...field} />
                        </FormControl>
                        <FormMessage className="text-[10px]" />
                      </FormItem>
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
                  {remainingAmount > 0 ? (
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

            {form.formState.errors.payments?.root?.message && (
              <p className="text-sm font-medium text-destructive mt-1">
                {form.formState.errors.payments.root?.message}
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
              remove(index);
            }}
          />
          {form.formState.errors.items && (
            <p className="text-sm font-medium text-destructive">
              {form.formState.errors.items.message}
            </p>
          )}

          <Separator />
        </div>

        <div className="flex flex-col-reverse sm:flex-row justify-end gap-2 sm:gap-3 pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={
              form.formState.isSubmitting || externalIsSubmitting === true
            }
          >
            Cancelar
          </Button>
          <LoadingButton
            type="submit"
            isLoading={
              form.formState.isSubmitting || externalIsSubmitting === true
            }
            loadingText={initialData?.id ? "Atualizando..." : "Criando..."}
          >
            {initialData?.id ? "Atualizar" : "Criar"} Pedido
          </LoadingButton>
        </div>
      </form>
    </Form>
  );
}
