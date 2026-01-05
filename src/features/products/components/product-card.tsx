import { Edit2, Trash2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Card } from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";
import type { Product } from "../types";

interface ProductCardProps {
  product: Product;
  onEdit: (product: Product) => void;
  onDelete: (id: string) => void;
}

export function ProductCard({ product, onEdit, onDelete }: ProductCardProps) {
  return (
    <Card className="group overflow-hidden hover:shadow-xl transition-all duration-300 border-border flex flex-col h-full">
      {/* <div className="relative aspect-video bg-background flex items-center justify-center overflow-hidden">
        Placeholder for Product Image - Future: product.image
        <div className="text-muted-foreground flex flex-col items-center gap-2">
          <Package className="w-12 h-12" />
        </div>

        <div className="absolute top-2 right-2">
          <span
            className={`px-2 py-1 text-xs font-medium rounded-full ${
              product.available
                ? "bg-green-100 text-green-700 border border-green-200"
                : "bg-red-100 text-red-700 border border-red-200"
            }`}
          >
            {product.available ? "Disponível" : "Indisponível"}
          </span>
        </div>
      </div> */}

      <div className="relative p-4 flex flex-col flex-1 gap-3">
        <div className="absolute top-2 right-2">
          <span
            className={`px-2 py-1 text-xs font-medium rounded-full ${
              product.available
                ? "bg-green-100 dark:bg-green-950 text-green-800 dark:text-green-200 border border-green-300 dark:border-green-700"
                : "bg-red-100 dark:bg-red-950 text-red-800 dark:text-red-200 border border-red-300 dark:border-red-700"
            }`}
          >
            {product.available ? "Disponível" : "Indisponível"}
          </span>
        </div>
        <div>
          <div className="flex justify-between items-start gap-2 mb-1">
            <h3
              className="text-base font-semibold text-foreground line-clamp-1"
              title={product.name}
            >
              {product.name}
            </h3>
          </div>
          <p className="text-sm text-muted-foreground line-clamp-2 min-h-10">
            {product.description}
          </p>
        </div>

        <div className="flex items-center gap-2 mt-auto pt-2">
          <span className="text-xs px-2 py-1 bg-accent/10 rounded text-accent-foreground font-medium">
            {product.category}
          </span>
        </div>

        <div className="flex justify-between items-center pt-2 border-t border-border mt-2">
          <span className="text-lg font-bold text-foreground">
            {formatMoney(product.price)}
          </span>

          <div className="flex gap-2 opacity-100 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity">
            <Button
              size="icon"
              variant="ghost"
              className="h-8 w-8 hover:bg-orange-100 dark:hover:bg-orange-950 hover:text-orange-600 dark:hover:text-orange-400"
              onClick={() => onEdit(product)}
            >
              <Edit2 className="w-4 h-4" />
            </Button>
            <Button
              size="icon"
              variant="ghost"
              className="h-8 w-8 hover:bg-red-100 dark:hover:bg-red-950 hover:text-red-600 dark:hover:text-red-400"
              onClick={() => onDelete(product.id)}
            >
              <Trash2 className="w-4 h-4" />
            </Button>
          </div>
        </div>
      </div>
    </Card>
  );
}
