import { PlusIcon } from "lucide-react";
import { useMemo } from "react";
import type { Product } from "@/features/products/types";
import { Button } from "@/shared/components/ui/button";
import { Combobox } from "@/shared/components/ui/combobox";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";

interface ProductSelectorProps {
  products: Product[];
  selectedProductId: string | undefined;
  quantity: number;
  notes: string;
  onProductChange: (productId: string | undefined) => void;
  onQuantityChange: (quantity: number) => void;
  onNotesChange: (notes: string) => void;
  onAddItem: () => void;
}

export function ProductSelector({
  products,
  selectedProductId,
  quantity,
  notes,
  onProductChange,
  onQuantityChange,
  onNotesChange,
  onAddItem,
}: ProductSelectorProps) {
  const productOptions = useMemo(
    () =>
      products.map((product) => ({
        value: product.id,
        label: `${product.name} - R$ ${product.price.toFixed(2)}`,
      })),
    [products],
  );

  return (
    <div className="space-y-3 sm:space-y-4">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        <div className="sm:col-span-2 lg:col-span-1 space-y-2">
          <Label htmlFor="product-select">Produto</Label>
          <Combobox
            value={selectedProductId}
            onChange={onProductChange}
            options={productOptions}
            placeholder="Selecione ou pesquise"
            className="w-full"
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="quantity">Quantidade</Label>
          <Input
            id="quantity"
            type="number"
            min="1"
            value={quantity}
            onChange={(e) =>
              onQuantityChange(Number.parseInt(e.target.value || "", 10) || 1)
            }
          />
        </div>

        <div className="sm:col-span-2 lg:col-span-1 space-y-2">
          <Label htmlFor="notes">Observações</Label>
          <Input
            id="notes"
            type="text"
            value={notes}
            onChange={(e) => onNotesChange(e.target.value)}
            placeholder="Ex: Sem cebola"
          />
        </div>
      </div>

      <Button
        type="button"
        variant="secondary"
        onClick={onAddItem}
        disabled={!selectedProductId}
        className="w-full sm:w-auto cursor-pointer"
      >
        <PlusIcon /> Adicionar Item
      </Button>
    </div>
  );
}
