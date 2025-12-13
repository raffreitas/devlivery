import { useState } from "react";
import { toast } from "sonner";
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
  const [alert, setAlert] = useState({
    open: false,
    type: "",
    action: () => {},
  });

  const handleEdit = () => {
    onEdit(order);
  };

  const handleNextStatus = () => {
    const next = NEXT_STATUS[order.status];
    if (next) {
      onUpdateStatus(order.id, next);
    }
  };

  const handleOpenAlert = (type: string, action: () => void) => {
    setAlert({ open: true, type, action });
  };

  const handleCancel = () => {
    onUpdateStatus(order.id, "Canceled");
    setAlert({ open: false, type: "", action: () => {} });
    toast.success("Pedido cancelado com sucesso");
  };

  const handleDelete = async () => {
    onDelete(order.id);
    setAlert({ open: false, type: "", action: () => {} });
    toast.success("Pedido excluído com sucesso");
  };

  return (
    <Card className="p-4 sm:p-6 hover:shadow-lg transition-shadow flex flex-col h-full gap-2">
      <OrderCardHeader order={order} />
      <Separator className="bg-gray-100" />

      <CardContent className="p-0 flex-1 flex flex-col justify-between gap-3">
        <OrderCardItems items={order.items} />

        <Separator className="bg-gray-100" />

        <OrderCardTotal order={order} />
      </CardContent>

      <Separator className="bg-gray-100" />

      <OrderCardActions
        order={order}
        onPrint={handlePrint}
        onEdit={handleEdit}
        onNextStatus={handleNextStatus}
        onCancel={() => handleOpenAlert("cancelar", handleCancel)}
        onDelete={() => handleOpenAlert("excluir", handleDelete)}
        hasNextStatus={NEXT_STATUS[order.status] !== null}
      />

      <div style={{ display: "none" }}>
        <div ref={contentRef}>
          <OrderPrint order={order} />
        </div>
      </div>

      <AlertDialog
        open={alert.open}
        onOpenChange={() =>
          setAlert({ open: false, type: "", action: () => {} })
        }
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
