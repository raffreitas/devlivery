import { TrashIcon, XIcon } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { formatMoney } from "@/shared/utils/formatters";

interface OrderItemsTableItem {
  fieldId: string;
  product: {
    id: string;
    name: string;
    price: number;
    description?: string;
    category?: string;
    available?: boolean;
  };
  quantity: number;
  notes?: string;
}

interface OrderItemsTableProps {
  items: OrderItemsTableItem[];
  subtotal: number;
  deliveryFee: number;
  total: number;
  onRemoveItem: (index: number) => void;
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
      {/* Mobile view */}
      <div className="sm:hidden">
        <div className="divide-y divide-gray-200">
          {items.map((item, index) => (
            <div key={item.fieldId} className="p-3 bg-white">
              <div className="flex justify-between items-start mb-2">
                <div className="flex-1">
                  <div className="font-medium text-gray-900 text-sm">
                    {item.product.name}
                  </div>
                  {item.notes && (
                    <div className="text-gray-500 text-xs mt-1">
                      Obs: {item.notes}
                    </div>
                  )}
                </div>
                <Button
                  type="button"
                  size="icon"
                  variant="destructive"
                  onClick={() => onRemoveItem(index)}
                >
                  <XIcon />
                </Button>
              </div>
              <div className="flex justify-between text-xs text-secondary-foreground">
                <span>Qtd: {item.quantity}</span>
                <span>Unit: {formatMoney(item.product.price)}</span>
                <span className="font-medium text-gray-900">
                  {formatMoney(item.product.price * item.quantity)}
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Desktop view */}
      <div className="hidden sm:block overflow-x-auto">
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
            {items.map((item, index) => (
              <tr key={item.fieldId}>
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
                  {formatMoney(item.product.price)}
                </td>
                <td className="px-4 py-3 text-sm font-medium text-gray-900">
                  {formatMoney(item.product.price * item.quantity)}
                </td>
                <td className="px-4 py-3 text-sm">
                  <Button
                    type="button"
                    size="icon"
                    variant="destructive"
                    onClick={() => onRemoveItem(index)}
                  >
                    <TrashIcon />
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="bg-gray-50 px-3 sm:px-4 py-3">
        <div className="flex justify-between items-center mb-1">
          <span className="text-xs sm:text-sm text-secondary-foreground">
            Subtotal
          </span>
          <span className="text-xs sm:text-sm font-medium text-gray-900">
            {formatMoney(subtotal)}
          </span>
        </div>

        <div className="flex justify-between items-center mb-1">
          <span className="text-xs sm:text-sm text-secondary-foreground">
            Taxa de Entrega
          </span>
          <span className="text-xs sm:text-sm font-medium text-gray-900">
            {formatMoney(deliveryFee)}
          </span>
        </div>

        <div className="flex justify-between items-center">
          <span className="text-base sm:text-lg font-semibold text-gray-900">
            Total:
          </span>
          <span className="text-xl sm:text-2xl font-bold text-primary">
            {formatMoney(total)}
          </span>
        </div>
      </div>
    </div>
  );
}
