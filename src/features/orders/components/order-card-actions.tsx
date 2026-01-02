import { Button } from "@/shared/components/ui/button";
import type { Order, OrderStatus } from "../types";

interface OrderCardActionsProps {
  order: Order;
  onPrint: () => void;
  onEdit: () => void;
  onCancel: () => void;
  onNextStatus: () => void;
  onDelete: () => void;
  hasNextStatus: boolean;
}

function getNextStatusLabel(status: OrderStatus): string {
  const labels: Record<OrderStatus, string> = {
    Pending: "Iniciar Preparo",
    Preparing: "Marcar como Pronto",
    Ready: "Marcar como Entregue",
    Delivered: "",
    Canceled: "",
  };
  return labels[status];
}

export function OrderCardActions({
  order,
  onPrint,
  onEdit,
  onCancel,
  onNextStatus,
  onDelete,
  hasNextStatus,
}: OrderCardActionsProps) {
  const showEdit = order.status !== "Canceled" && order.status !== "Delivered";
  const showCancel =
    order.status !== "Canceled" && order.status !== "Delivered";
  const showDelete =
    order.status === "Delivered" || order.status === "Canceled";
  return (
    <div className="mt-auto">
      <div className="flex flex-col sm:flex-row sm:flex-wrap items-stretch sm:items-center justify-end gap-2">
        <Button size="sm" variant="outline" onClick={onPrint}>
          Imprimir
        </Button>
        {showEdit && (
          <Button size="sm" variant="outline" onClick={onEdit}>
            Editar
          </Button>
        )}
        {showCancel && (
          <Button size="sm" variant="destructive" onClick={onCancel}>
            Cancelar
          </Button>
        )}
        {hasNextStatus && (
          <Button size="sm" onClick={onNextStatus}>
            <span className="hidden sm:inline">
              {getNextStatusLabel(order.status)}
            </span>
            <span className="sm:hidden">Avançar</span>
          </Button>
        )}
        {showDelete && (
          <Button size="sm" variant="destructive" onClick={onDelete}>
            Excluir
          </Button>
        )}
      </div>

      <div className="mt-2 sm:mt-3 text-xs text-gray-500">
        Criado em: {new Date(order.createdAt).toLocaleString("pt-BR")}
      </div>
    </div>
  );
}
