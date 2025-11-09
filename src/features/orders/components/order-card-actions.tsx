import { Button } from "@/shared/components/button";
import type { Order } from "../types";

interface OrderCardActionsProps {
  order: Order;
  onPrint: () => void;
  onCancel: () => void;
  onNextStatus: () => void;
  onDelete: () => void;
  hasNextStatus: boolean;
}

function getNextStatusLabel(status: Order["status"]): string {
  const labels: Record<Order["status"], string> = {
    pending: "Iniciar Preparo",
    preparing: "Marcar como Pronto",
    ready: "Marcar como Entregue",
    delivered: "",
    cancelled: "",
  };
  return labels[status];
}

export function OrderCardActions({
  order,
  onPrint,
  onCancel,
  onNextStatus,
  onDelete,
  hasNextStatus,
}: OrderCardActionsProps) {
  const showCancel =
    order.status !== "cancelled" && order.status !== "delivered";
  const showDelete =
    order.status === "delivered" || order.status === "cancelled";

  return (
    <div className="border-t border-gray-200 pt-4 mt-auto">
      <div className="flex flex-wrap items-center justify-end gap-2">
        <Button size="sm" variant="secondary" onClick={onPrint}>
          Imprimir
        </Button>
        {showCancel && (
          <Button size="sm" variant="danger" onClick={onCancel}>
            Cancelar
          </Button>
        )}
        {hasNextStatus && (
          <Button size="sm" variant="success" onClick={onNextStatus}>
            {getNextStatusLabel(order.status)}
          </Button>
        )}
        {showDelete && (
          <Button size="sm" variant="danger" onClick={onDelete}>
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
