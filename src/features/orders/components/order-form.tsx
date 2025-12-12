import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import { useProducts } from "@/features/products/hooks/use-products";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Separator } from "@/shared/components/ui/separator";
import { Textarea } from "@/shared/components/ui/textarea";
import { getPaymentOptions } from "../constants/payment-methods";
import { type OrderFormData, orderFormSchema } from "../types";
import { OrderItemsTable } from "./order-form-items-table";
import { ProductSelector } from "./order-form-product-selector";

interface OrderFormProps {
  initialData?: OrderFormData & { id?: string };
  onSubmit: (data: OrderFormData) => void;
  onCancel: () => void;
}

export function OrderForm({ initialData, onSubmit, onCancel }: OrderFormProps) {
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
      paymentMethod: initialData?.paymentMethod ?? "Cash",
      notes: initialData?.notes ?? "",
      items: initialData?.items ?? [],
    },
  });

  const { fields, append, remove, update } = useFieldArray({
    control: form.control,
    name: "items",
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

  const deliveryFee = useWatch({
    control: form.control,
    name: "deliveryFee",
    defaultValue: 0,
  });

  const subtotal = fields.reduce(
    (sum, item) => sum + item.product.price * item.quantity,
    0,
  );

  const availableProducts = products.filter((p) => p.available);

  const total = subtotal + deliveryFee;

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

            <div className="flex flex-col gap-3 sm:flex-row">
              <FormField
                control={form.control}
                name="deliveryFee"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Taxa de Entrega</FormLabel>
                    <FormControl className="w-full">
                      <InputMoney {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="paymentMethod"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Método de Pagamento</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder="Selecione um método de pagamento" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {getPaymentOptions().map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

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
              products={availableProducts}
              selectedProductId={selectedProductId}
              quantity={quantity}
              notes={notes}
              onProductChange={setSelectedProductId}
              onQuantityChange={setQuantity}
              onNotesChange={setNotes}
              onAddItem={handleAddItem}
            />

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
          </div>
          <Separator />
        </div>

        <div className="flex flex-col-reverse sm:flex-row justify-end gap-2 sm:gap-3 pt-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancelar
          </Button>
          <Button type="submit">
            {initialData?.id ? "Atualizar" : "Criar"} Pedido
          </Button>
        </div>
      </form>
    </Form>
  );
}
