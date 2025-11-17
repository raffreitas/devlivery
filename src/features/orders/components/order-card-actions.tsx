import { Button } from "@/shared/components/button";
import type { Order } from "../types";

interface OrderCardActionsProps {
  order: Order;
  onPrint: () => void;
  onEdit: () => void;
  onCancel: () => void;
  onNextStatus: () => void;
  onDelete: () => void;
  hasNextStatus: boolean;
}

function getNextStatusLabel(status: Order["status"]): string {
  const labels: Record<Order["status"], string> = {
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
    <div className="border-t border-gray-200 pt-3 sm:pt-4 mt-auto">
      <div className="flex flex-col sm:flex-row sm:flex-wrap items-stretch sm:items-center justify-end gap-2">
        <Button size="sm" variant="secondary" onClick={onPrint}>
          Imprimir
        </Button>
        {showEdit && (
          <Button size="sm" variant="secondary" onClick={onEdit}>
            Editar
          </Button>
        )}
        {showCancel && (
          <Button size="sm" variant="danger" onClick={onCancel}>
            Cancelar
          </Button>
        )}
        {hasNextStatus && (
          <Button size="sm" variant="success" onClick={onNextStatus}>
            <span className="hidden sm:inline">
              {getNextStatusLabel(order.status)}
            </span>
            <span className="sm:hidden">Avançar</span>
          </Button>
        )}
        {showDelete && (
          <Button size="sm" variant="danger" onClick={onDelete}>
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
