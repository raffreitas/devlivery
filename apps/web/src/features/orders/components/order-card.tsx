import { useState } from "react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { toast } from "@/shared/components/ui/toast";
import { usePrintOrder } from "../hooks/use-print-order";
import type { Order, OrderStatus } from "../types";
import { OrderCardActions } from "./order-card-actions";
import { OrderCardHeader } from "./order-card-header";
import { OrderCardItems } from "./order-card-items";
import { OrderCardTotal } from "./order-card-total";
import { OrderPrint } from "./order-print";

interface OrderCardProps {
  order: Order;
  onEdit: (order: Order) => void;
  onUpdateStatus: (id: string, status: OrderStatus) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
}

const CLOSED_ALERT = { open: false, type: "", action: () => {} };

const NEXT_STATUS: Record<OrderStatus, OrderStatus | null> = {
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
  const [isAdvancing, setIsAdvancing] = useState(false);
  const { contentRef, handlePrint } = usePrintOrder();
  const [alert, setAlert] = useState({
    open: false,
    type: "",
    action: () => {},
  });

  const handleEdit = () => {
    onEdit(order);
  };

  // Failed mutations are already reported by the global MutationCache error toast,
  // so the handlers below only need to avoid announcing success when the API rejects.
  const handleNextStatus = async () => {
    const next = NEXT_STATUS[order.status];
    if (!next || isAdvancing) return;
    setIsAdvancing(true);
    try {
      await onUpdateStatus(order.id, next);
    } catch {
      // Reported by the global MutationCache.
    } finally {
      setIsAdvancing(false);
    }
  };

  const handleOpenAlert = (type: string, action: () => void) => {
    setAlert({ open: true, type, action });
  };

  const handleCancel = async () => {
    setAlert(CLOSED_ALERT);
    try {
      await onUpdateStatus(order.id, "Canceled");
      toast.add({ type: "success", title: "Pedido cancelado com sucesso" });
    } catch {
      // Reported by the global MutationCache.
    }
  };

  const handleDelete = async () => {
    setAlert(CLOSED_ALERT);
    try {
      await onDelete(order.id);
      toast.add({ type: "success", title: "Pedido excluído com sucesso" });
    } catch {
      // Reported by the global MutationCache.
    }
  };

  return (
    <Card className="p-4 sm:p-6 hover:shadow-lg transition-shadow flex flex-col h-full gap-2">
      <OrderCardHeader order={order} />
      <Separator className="bg-border" />

      <CardContent className="p-0 flex-1 flex flex-col justify-between gap-3">
        <OrderCardItems items={order.items} />

        <Separator className="bg-border" />

        <OrderCardTotal order={order} />
      </CardContent>

      <Separator className="bg-border" />

      <OrderCardActions
        order={order}
        onPrint={handlePrint}
        onEdit={handleEdit}
        onNextStatus={handleNextStatus}
        onCancel={() => handleOpenAlert("cancelar", handleCancel)}
        onDelete={() => handleOpenAlert("excluir", handleDelete)}
        hasNextStatus={NEXT_STATUS[order.status] !== null}
        isAdvancing={isAdvancing}
      />

      <div style={{ display: "none" }}>
        <div ref={contentRef}>
          <OrderPrint order={order} />
        </div>
      </div>

      <AlertDialog
        open={alert.open}
        onOpenChange={() => setAlert(CLOSED_ALERT)}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              Tem certeza que deseja {alert.type} este pedido?
            </AlertDialogTitle>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={alert.action}>
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </Card>
  );
}
