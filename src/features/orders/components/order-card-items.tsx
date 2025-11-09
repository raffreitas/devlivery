import type { Order } from "../types";

interface OrderCardItemsProps {
  items: Order["items"];
}

export function OrderCardItems({ items }: OrderCardItemsProps) {
  return (
    <div className="border-t border-gray-200 pt-4 mb-4">
      <h4 className="text-sm font-medium text-gray-900 mb-2">Itens:</h4>
      <ul className="space-y-2">
        {items.map((item) => (
          <li
            key={`${item.product.id}-${item.quantity}`}
            className="flex justify-between text-sm"
          >
            <span className="text-gray-700">
              {item.quantity}x {item.product.name}
              {item.notes && (
                <span className="text-gray-500 text-xs ml-2">
                  ({item.notes})
                </span>
              )}
            </span>
            <span className="text-gray-900 font-medium">
              R$ {(item.product.price * item.quantity).toFixed(2)}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
