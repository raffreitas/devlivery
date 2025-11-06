import { Button } from "@/shared/components/button";
import type { Order } from "../types";
import { printOrder } from "../utils/print-order";

interface OrderCardProps {
  order: Order;
  onUpdateStatus: (id: string, status: Order["status"]) => void;
  onDelete: (id: string) => void;
}

const statusColors = {
  pending: "bg-yellow-100 text-yellow-800",
  preparing: "bg-blue-100 text-blue-800",
  ready: "bg-purple-100 text-purple-800",
  delivered: "bg-green-100 text-green-800",
  cancelled: "bg-red-100 text-red-800",
};

const statusLabels = {
  pending: "Pendente",
  preparing: "Em Preparo",
  ready: "Pronto",
  delivered: "Entregue",
  cancelled: "Cancelado",
};

export function OrderCard({ order, onUpdateStatus, onDelete }: OrderCardProps) {
  const nextStatus: Record<Order["status"], Order["status"] | null> = {
    pending: "preparing",
    preparing: "ready",
    ready: "delivered",
    delivered: null,
    cancelled: null,
  };

  const handleNextStatus = () => {
    const next = nextStatus[order.status];
    if (next) {
      onUpdateStatus(order.id, next);
    }
  };

  const handleCancel = () => {
    if (window.confirm("Tem certeza que deseja cancelar este pedido?")) {
      onUpdateStatus(order.id, "cancelled");
    }
  };

  const handleDelete = () => {
    if (window.confirm("Tem certeza que deseja excluir este pedido?")) {
      onDelete(order.id);
    }
  };

  const handlePrint = () => {
    printOrder(order);
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">
            {order.customerName}
          </h3>
          <p className="text-sm text-gray-600">{order.customerPhone}</p>
          <p className="text-sm text-gray-600">{order.deliveryAddress}</p>
        </div>
        <span
          className={`px-3 py-1 rounded-full text-sm font-medium ${statusColors[order.status]}`}
        >
          {statusLabels[order.status]}
        </span>
      </div>

      <div className="border-t border-gray-200 pt-4 mb-4">
        <h4 className="text-sm font-medium text-gray-900 mb-2">Itens:</h4>
        <ul className="space-y-2">
          {order.items.map((item) => (
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

      <div className="flex justify-between items-center pt-4 border-t border-gray-200">
        <div>
          <span className="text-sm text-gray-600">Total:</span>
          <span className="text-xl font-bold text-orange-600 ml-2">
            R$ {order.total.toFixed(2)}
          </span>
        </div>
      </div>

      <div className="flex justify-end space-x-2 mt-4">
        <Button size="sm" variant="secondary" onClick={handlePrint}>
          Imprimir
        </Button>
        {order.status !== "cancelled" && order.status !== "delivered" && (
          <Button size="sm" variant="danger" onClick={handleCancel}>
            Cancelar
          </Button>
        )}
        {nextStatus[order.status] && (
          <Button size="sm" variant="success" onClick={handleNextStatus}>
            {order.status === "pending" && "Iniciar Preparo"}
            {order.status === "preparing" && "Marcar como Pronto"}
            {order.status === "ready" && "Marcar como Entregue"}
          </Button>
        )}
        {(order.status === "delivered" || order.status === "cancelled") && (
          <Button size="sm" variant="danger" onClick={handleDelete}>
            Excluir
          </Button>
        )}
      </div>

      <div className="mt-3 text-xs text-gray-500">
        Pedido criado em: {new Date(order.createdAt).toLocaleString("pt-BR")}
      </div>
    </div>
  );
}
