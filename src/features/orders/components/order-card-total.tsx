import { formatMoney } from "@/shared/utils/formatters";
import type { Order } from "../types";

interface OrderCardTotalProps {
  order: Order;
}

export function OrderCardTotal({ order }: OrderCardTotalProps) {
  const subtotal = order.items.reduce(
    (s, it) => s + it.product.price * it.quantity,
    0,
  );
  return (
    <div className="flex justify-between items-center">
      <div className="text-sm text-secondary-foreground space-y-1">
        <div>Subtotal: {formatMoney(subtotal)}</div>
        <div>Taxa de Entrega: {formatMoney(order.deliveryFee)}</div>
      </div>
      <div className="text-right">
        <div className="text-sm text-secondary-foreground">Total:</div>
        <div className="text-2xl font-bold text-primary">
          {formatMoney(order.total)}
        </div>
      </div>
    </div>
  );
}
