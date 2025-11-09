import type { Product } from "@/features/products/types";
import { AutocompleteSelect } from "@/shared/components/autocomplete-select";
import { Button } from "@/shared/components/button";
import { Input } from "@/shared/components/input";

interface ProductSelectorProps {
  products: Product[];
  selectedProductId: string | null;
  quantity: number;
  notes: string;
  onProductChange: (productId: string | null) => void;
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
  const productOptions = products.map((product) => ({
    value: product.id,
    label: `${product.name} - R$ ${product.price.toFixed(2)}`,
  }));

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
        <div className="md:col-span-1">
          <AutocompleteSelect
            id="product-select"
            label="Produto"
            placeholder="Selecione ou pesquise um produto"
            value={selectedProductId}
            onChange={onProductChange}
            options={productOptions}
          />
        </div>

        <div>
          <Input
            label="Quantidade"
            type="number"
            min="1"
            value={quantity}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              onQuantityChange(Number.parseInt(e.target.value || "", 10) || 1)
            }
          />
        </div>

        <div>
          <Input
            label="Observações"
            type="text"
            value={notes}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              onNotesChange(e.target.value)
            }
            placeholder="Ex: sem cebola"
          />
        </div>
      </div>

      <Button
        type="button"
        variant="secondary"
        onClick={onAddItem}
        disabled={!selectedProductId}
        className="w-full md:w-auto"
      >
        + Adicionar Item
      </Button>
    </div>
  );
}
