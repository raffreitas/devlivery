import { Button } from "@/shared/components/button";
import type { OrderItem } from "../types";

interface OrderItemsTableProps {
  items: OrderItem[];
  subtotal: number;
  deliveryFee: number;
  total: number;
  onRemoveItem: (productId: string) => void;
}

export function OrderItemsTable({
  items,
  subtotal,
  deliveryFee,
  total,
  onRemoveItem,
}: OrderItemsTableProps) {
  if (items.length === 0) {
    return null;
  }

  return (
    <div className="border border-gray-200 rounded-lg overflow-hidden">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
              Produto
            </th>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
              Qtd
            </th>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
              Preço Unit.
            </th>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
              Subtotal
            </th>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
              Ações
            </th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {items.map((item) => (
            <tr key={item.product.id}>
              <td className="px-4 py-3 text-sm">
                <div>
                  <div className="font-medium text-gray-900">
                    {item.product.name}
                  </div>
                  {item.notes && (
                    <div className="text-gray-500 text-xs">
                      Obs: {item.notes}
                    </div>
                  )}
                </div>
              </td>
              <td className="px-4 py-3 text-sm text-gray-900">
                {item.quantity}
              </td>
              <td className="px-4 py-3 text-sm text-gray-900">
                R$ {item.product.price.toFixed(2)}
              </td>
              <td className="px-4 py-3 text-sm font-medium text-gray-900">
                R$ {(item.product.price * item.quantity).toFixed(2)}
              </td>
              <td className="px-4 py-3 text-sm">
                <Button
                  type="button"
                  size="sm"
                  variant="danger"
                  onClick={() => onRemoveItem(item.product.id)}
                >
                  Remover
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <div className="bg-gray-50 px-4 py-3">
        <div className="flex justify-between items-center mb-1">
          <span className="text-sm text-gray-600">Subtotal</span>
          <span className="text-sm font-medium text-gray-900">
            R$ {subtotal.toFixed(2)}
          </span>
        </div>

        <div className="flex justify-between items-center mb-1">
          <span className="text-sm text-gray-600">Taxa de Entrega</span>
          <span className="text-sm font-medium text-gray-900">
            R$ {deliveryFee.toFixed(2)}
          </span>
        </div>

        <div className="flex justify-between items-center">
          <span className="text-lg font-semibold text-gray-900">Total:</span>
          <span className="text-2xl font-bold text-orange-600">
            R$ {total.toFixed(2)}
          </span>
        </div>
      </div>
    </div>
  );
}
