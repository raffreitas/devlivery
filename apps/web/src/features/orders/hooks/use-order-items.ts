import { useState } from "react";
import { type UseFormReturn, useFieldArray } from "react-hook-form";
import type { Product } from "@/features/products/types";
import { itemsSubtotal } from "@/shared/utils/money";
import type { OrderFormData } from "../types";

export function useOrderItems(
  form: UseFormReturn<OrderFormData>,
  products: Product[],
  initialItems: OrderFormData["items"] = [],
) {
  const recordedPrices = new Map(
    initialItems.map((item) => [item.product.id, item.unitPrice]),
  );
  const [selectedProductId, setSelectedProductId] = useState<
    string | undefined
  >();
  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState("");

  const { fields, append, remove, update } = useFieldArray({
    control: form.control,
    name: "items",
  });

  const selectableProducts = products
    .filter((product) => product.available)
    .map((product) => ({
      ...product,
      price: recordedPrices.get(product.id) ?? product.price,
    }));

  const addSelectedItem = () => {
    if (!selectedProductId) return;

    const product = products.find((p) => p.id === selectedProductId);
    if (!product) return;

    const existingItemIndex = fields.findIndex(
      (item) =>
        item.product.id === selectedProductId &&
        (item.notes?.trim() ?? "") === notes.trim(),
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
        unitPrice: existingItem.unitPrice,
        quantity: existingItem.quantity + quantity,
        notes: notes.trim() || undefined,
      });
    } else {
      append({
        product: productData,
        unitPrice: recordedPrices.get(product.id) ?? product.price,
        quantity,
        notes: notes.trim() || undefined,
      });
    }

    setSelectedProductId(undefined);
    setQuantity(1);
    setNotes("");
  };

  return {
    fields,
    removeItem: remove,
    subtotal: itemsSubtotal(fields),
    selectableProducts,
    selectedProductId,
    quantity,
    notes,
    setSelectedProductId,
    setQuantity,
    setNotes,
    addSelectedItem,
    hasPendingSelection: !!selectedProductId || quantity !== 1 || notes !== "",
  };
}
