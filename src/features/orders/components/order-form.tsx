import { useState } from "react";
import { useProducts } from "@/features/products/hooks/use-products";
import { Button } from "@/shared/components/button";
import type { OrderFormData, OrderItem, PaymentMethod } from "../types";
import { CustomerInfoSection } from "./order-form-customer-info";
import { OrderItemsTable } from "./order-form-items-table";
import { ProductSelector } from "./order-form-product-selector";

interface OrderFormProps {
  onSubmit: (data: OrderFormData) => void;
  onCancel: () => void;
}

export function OrderForm({ onSubmit, onCancel }: OrderFormProps) {
  const { products } = useProducts();
  const [customerName, setCustomerName] = useState("");
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod | null>(
    null,
  );
  const [deliveryAddress, setDeliveryAddress] = useState("");
  const [deliveryFee, setDeliveryFee] = useState(0);
  const [items, setItems] = useState<OrderItem[]>([]);
  const [selectedProductId, setSelectedProductId] = useState<string | null>(
    null,
  );
  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState("");

  const availableProducts = products.filter((p) => p.available);

  const handleAddItem = () => {
    if (!selectedProductId) return;

    const product = products.find((p) => p.id === selectedProductId);
    if (!product) return;

    const existingItemIndex = items.findIndex(
      (item) => item.product.id === selectedProductId,
    );

    if (existingItemIndex >= 0) {
      const newItems = [...items];
      newItems[existingItemIndex].quantity += quantity;
      if (notes) {
        newItems[existingItemIndex].notes = notes;
      }
      setItems(newItems);
    } else {
      setItems([...items, { product, quantity, notes: notes || undefined }]);
    }

    setSelectedProductId(null);
    setQuantity(1);
    setNotes("");
  };

  const handleRemoveItem = (productId: string) => {
    setItems(items.filter((item) => item.product.id !== productId));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!paymentMethod) {
      alert("Selecione um método de pagamento");
      return;
    }

    if (items.length === 0) {
      alert("Adicione pelo menos um produto ao pedido");
      return;
    }

    onSubmit({
      items,
      customerName,
      deliveryAddress,
      deliveryFee,
      paymentMethod,
    });
  };

  const subtotal = items.reduce(
    (sum, item) => sum + item.product.price * item.quantity,
    0,
  );

  const total = subtotal + deliveryFee;

  return (
    <form onSubmit={handleSubmit} className="space-y-4 sm:space-y-6">
      <CustomerInfoSection
        customerName={customerName}
        deliveryAddress={deliveryAddress}
        deliveryFee={deliveryFee}
        paymentMethod={paymentMethod}
        onCustomerNameChange={setCustomerName}
        onDeliveryAddressChange={setDeliveryAddress}
        onDeliveryFeeChange={setDeliveryFee}
        onPaymentMethodChange={setPaymentMethod}
      />

      <div className="space-y-3 sm:space-y-4">
        <h3 className="text-base sm:text-lg font-semibold">Itens do Pedido</h3>

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
          items={items}
          subtotal={subtotal}
          deliveryFee={deliveryFee}
          total={total}
          onRemoveItem={handleRemoveItem}
        />
      </div>

      <div className="flex flex-col-reverse sm:flex-row justify-end gap-2 sm:gap-3 pt-4 border-t">
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary">
          Criar Pedido
        </Button>
      </div>
    </form>
  );
}
