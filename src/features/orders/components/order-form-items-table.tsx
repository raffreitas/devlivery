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
    <div className="border border-border rounded-lg overflow-hidden flex flex-col">
      {/* Mobile view */}
      <div className="sm:hidden flex-1 overflow-y-auto">
        <div className="divide-y divide-border">
          {items.map((item, index) => (
            <div
              key={item.fieldId}
              className="p-3 bg-card hover:bg-accent/50 transition-colors"
            >
              <div className="flex justify-between items-start mb-2">
                <div className="flex-1">
                  <div className="font-medium text-foreground text-sm">
                    {item.product.name}
                  </div>
                  {item.notes && (
                    <div className="text-muted-foreground text-xs mt-1">
                      Obs: {item.notes}
                    </div>
                  )}
                </div>
                <Button
                  type="button"
                  size="icon"
                  variant="ghost"
                  className="ml-2"
                  onClick={() => onRemoveItem(index)}
                >
                  <XIcon className="w-4 h-4 text-destructive" />
                </Button>
              </div>
              <div className="flex justify-between text-xs text-secondary-foreground">
                <span>Qtd: {item.quantity}</span>
                <span>Unit: {formatMoney(item.product.price)}</span>
                <span className="font-medium text-foreground">
                  {formatMoney(item.product.price * item.quantity)}
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Desktop view */}
      <div className="hidden sm:flex flex-col flex-1">
        <div className="overflow-x-auto overflow-y-auto flex-1">
          <table className="min-w-full divide-y divide-border">
            <thead className="bg-muted/60 sticky top-0 z-10">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Produto
                </th>
                <th className="px-4 py-3 text-center text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Qtd
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Preço Unit.
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Subtotal
                </th>
                <th className="px-4 py-3 text-center text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {items.map((item, index) => (
                <tr
                  key={item.fieldId}
                  className="hover:bg-accent/50 transition-colors"
                >
                  <td className="px-4 py-3 text-sm">
                    <div>
                      <div className="font-medium text-foreground">
                        {item.product.name}
                      </div>
                      {item.notes && (
                        <div className="text-muted-foreground text-xs mt-1">
                          Obs: {item.notes}
                        </div>
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm text-foreground text-center">
                    {item.quantity}
                  </td>
                  <td className="px-4 py-3 text-sm text-foreground text-right">
                    {formatMoney(item.product.price)}
                  </td>
                  <td className="px-4 py-3 text-sm font-semibold text-foreground text-right">
                    {formatMoney(item.product.price * item.quantity)}
                  </td>
                  <td className="px-4 py-3 text-sm text-center">
                    <Button
                      type="button"
                      size="icon"
                      variant="ghost"
                      onClick={() => onRemoveItem(index)}
                      className="hover:bg-destructive/10"
                    >
                      <TrashIcon className="w-4 h-4 text-destructive" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div className="border-t border-border bg-muted/40 px-3 sm:px-4 py-3 space-y-2">
        <div className="flex justify-between items-center">
          <span className="text-xs sm:text-sm text-muted-foreground font-medium">
            Subtotal
          </span>
          <span className="text-xs sm:text-sm font-semibold text-foreground">
            {formatMoney(subtotal)}
          </span>
        </div>

        <div className="flex justify-between items-center">
          <span className="text-xs sm:text-sm text-muted-foreground font-medium">
            Taxa de Entrega
          </span>
          <span className="text-xs sm:text-sm font-semibold text-foreground">
            {formatMoney(deliveryFee)}
          </span>
        </div>

        <div className="flex justify-between items-center pt-2 border-t border-border">
          <span className="text-base sm:text-lg font-bold text-foreground">
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
