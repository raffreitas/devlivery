import { Button } from "@/shared/components/button";
import type { Product } from "../types";

interface ProductCardProps {
  product: Product;
  onEdit: (product: Product) => void;
  onDelete: (id: string) => void;
}

export function ProductCard({ product, onEdit, onDelete }: ProductCardProps) {
  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow">
      <div className="p-4">
        <div className="flex justify-between items-start mb-2">
          <h3 className="text-lg font-semibold text-gray-900">
            {product.name}
          </h3>
          <span
            className={`px-2 py-1 text-xs rounded-full ${
              product.available
                ? "bg-green-100 text-green-800"
                : "bg-red-100 text-red-800"
            }`}
          >
            {product.available ? "Disponível" : "Indisponível"}
          </span>
        </div>

        <p className="text-sm text-gray-600 mb-2">{product.description}</p>
        <p className="text-sm text-gray-500 mb-3">
          Categoria: {product.category}
        </p>

        <div className="flex justify-between items-center">
          <span className="text-xl font-bold text-orange-600">
            R$ {product.price.toFixed(2)}
          </span>

          <div className="flex space-x-2">
            <Button
              size="sm"
              variant="secondary"
              onClick={() => onEdit(product)}
            >
              Editar
            </Button>
            <Button
              size="sm"
              variant="danger"
              onClick={() => onDelete(product.id)}
            >
              Excluir
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
