import { Edit2, Trash2 } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
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
      <CardHeader className="flex flex-col">
        <div className="flex justify-between w-full">
          <CardTitle
            className="text-base font-semibold line-clamp-1"
            title={product.name}
          >
            {product.name}
          </CardTitle>

          <CardAction>
            <Badge
              variant={product.available ? "default" : "destructive"}
              className="px-2 py-1 text-xs font-medium"
            >
              {product.available ? "Disponível" : "Indisponível"}
            </Badge>
          </CardAction>
        </div>
        <CardDescription className="text-sm min-h-10 mt-3">
          {product.description}
        </CardDescription>
      </CardHeader>

      <CardContent className="px-6 pt-0">
        <div className="flex items-center gap-2">
          <Badge variant="outline" className="text-xs px-2 py-1">
            {product.category}
          </Badge>
        </div>
      </CardContent>

      <CardFooter className="px-6 justify-between items-center">
        <span className="text-lg font-bold text-foreground">
          {formatMoney(product.price)}
        </span>

        <div className="flex gap-2 opacity-100 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity">
          <Button
            size="icon"
            variant="ghost"
            aria-label={`Editar ${product.name}`}
            title="Editar"
            onClick={() => onEdit(product)}
          >
            <Edit2 className="w-4 h-4" />
          </Button>

          <Button
            size="icon"
            variant="ghost"
            aria-label={`Excluir ${product.name}`}
            title="Excluir"
            onClick={() => onDelete(product.id)}
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      </CardFooter>
    </Card>
  );
}
