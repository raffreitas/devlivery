import { Card, CardContent } from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { usePrintOrder } from "../hooks/use-print-order";
import type { Order } from "../types";
import { OrderCardActions } from "./order-card-actions";
import { OrderCardHeader } from "./order-card-header";
import { OrderCardItems } from "./order-card-items";
import { OrderCardTotal } from "./order-card-total";
import { OrderPrint } from "./order-print";

interface OrderCardProps {
  order: Order;
  onEdit: (order: Order) => void;
  onUpdateStatus: (id: string, status: Order["status"]) => void;
  onDelete: (id: string) => void;
}

const NEXT_STATUS: Record<Order["status"], Order["status"] | null> = {
  Pending: "Preparing",
  Preparing: "Ready",
  Ready: "Delivered",
  Delivered: null,
  Canceled: null,
};

export function OrderCard({
  order,
  onEdit,
  onUpdateStatus,
  onDelete,
}: OrderCardProps) {
  const { contentRef, handlePrint } = usePrintOrder();

  const handleEdit = () => {
    onEdit(order);
  };

  const handleNextStatus = () => {
    const next = NEXT_STATUS[order.status];
    if (next) {
      onUpdateStatus(order.id, next);
    }
  };

  const handleCancel = () => {
    if (window.confirm("Tem certeza que deseja cancelar este pedido?")) {
      onUpdateStatus(order.id, "Canceled");
    }
  };

  const handleDelete = () => {
    if (window.confirm("Tem certeza que deseja excluir este pedido?")) {
      onDelete(order.id);
    }
  };

  return (
    <Card className="p-4 sm:p-6 hover:shadow-lg transition-shadow flex flex-col h-full gap-2">
      <OrderCardHeader order={order} />
      <Separator />

      <CardContent className="p-0 flex-1 flex flex-col justify-between gap-2">
        <OrderCardItems items={order.items} />

        <Separator />

        <OrderCardTotal order={order} />
      </CardContent>

      <Separator />

      <OrderCardActions
        order={order}
        onPrint={handlePrint}
        onEdit={handleEdit}
        onCancel={handleCancel}
        onNextStatus={handleNextStatus}
        onDelete={handleDelete}
        hasNextStatus={NEXT_STATUS[order.status] !== null}
      />

      <div style={{ display: "none" }}>
        <div ref={contentRef}>
          <OrderPrint order={order} />
        </div>
      </div>
    </Card>
  );
}
