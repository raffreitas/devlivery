import { OrderCard } from "@/features/orders/components/order-card";
import type { Order } from "@/features/orders/types";

interface ActiveOrdersSectionProps {
  orders: Order[];
  onUpdateStatus: (orderId: string, status: Order["status"]) => Promise<void>;
  onDelete: (orderId: string) => Promise<void>;
}

export function ActiveOrdersSection({
  orders,
  onUpdateStatus,
  onDelete,
}: ActiveOrdersSectionProps) {
  return (
    <div className="bg-white rounded-lg shadow-md p-4 sm:p-6">
      <h2 className="text-lg sm:text-xl font-semibold text-gray-900 mb-4">
        Pedidos Ativos ({orders.length})
      </h2>

      {orders.length === 0 ? (
        <p className="text-gray-500 text-center py-8 text-sm sm:text-base">
          Nenhum pedido ativo no momento
        </p>
      ) : (
        <div className="space-y-3 sm:space-y-4">
          {orders.map((order) => (
            <OrderCard
              key={order.id}
              order={order}
              onUpdateStatus={onUpdateStatus}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
}
