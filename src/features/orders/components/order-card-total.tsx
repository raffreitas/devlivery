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
      <div className="text-sm text-gray-600 space-y-1">
        <div>Subtotal: R$ {subtotal.toFixed(2)}</div>
        <div>Taxa de Entrega: R$ {order.deliveryFee.toFixed(2)}</div>
      </div>
      <div className="text-right">
        <div className="text-sm text-gray-600">Total:</div>
        <div className="text-2xl font-bold text-orange-600">
          R$ {order.total.toFixed(2)}
        </div>
      </div>
    </div>
  );
}
