import type { Order } from "../types";

interface OrderCardItemsProps {
  items: Order["items"];
}

export function OrderCardItems({ items }: OrderCardItemsProps) {
  return (
    <div className="border-t border-gray-200 pt-3 sm:pt-4 mb-3 sm:mb-4">
      <h4 className="text-xs sm:text-sm font-medium text-gray-900 mb-2">
        Itens:
      </h4>
      <ul className="space-y-2">
        {items.map((item) => (
          <li
            key={`${item.product.id}-${item.quantity}`}
            className="flex justify-between gap-2 text-xs sm:text-sm"
          >
            <span className="text-gray-700 flex-1 min-w-0">
              <span className="font-medium">{item.quantity}x</span>{" "}
              {item.product.name}
              {item.notes && (
                <span className="text-gray-500 text-xs block sm:inline sm:ml-2">
                  ({item.notes})
                </span>
              )}
            </span>
            <span className="text-gray-900 font-medium shrink-0">
              R$ {(item.product.price * item.quantity).toFixed(2)}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
