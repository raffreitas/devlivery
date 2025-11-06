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
      <div className="text-sm text-gray-600">
        <div>Subtotal: R$ {subtotal.toFixed(2)}</div>
        <div>Taxa de Entrega: R$ {order.deliveryFee.toFixed(2)}</div>
      </div>
      <div>
        <span className="text-sm text-gray-600">Total:</span>
        <span className="text-xl font-bold text-orange-600 ml-2">
          R$ {order.total.toFixed(2)}
        </span>
      </div>
    </div>
  );
}
