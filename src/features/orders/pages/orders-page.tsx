import { useState } from "react";
import type { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import {
  GridSkeleton,
  LoadingOverlay,
  LoadingState,
} from "@/shared/components/loading";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Separator } from "@/shared/components/ui/separator";
import { OrderCard } from "../components/order-card";
import { OrderForm } from "../components/order-form";
import { OrdersFilters } from "../components/orders-filters";
import { OrdersFiltersContent } from "../components/orders-filters-content";
import { OrdersHeader } from "../components/orders-header";
import { getOrderStatusOptions } from "../constants/order-status";
import { getPaymentOptions } from "../constants/payment-methods";
import { useOrders } from "../hooks/use-orders";
import type { Order, OrderFormData } from "../types";

const statusOptions: Array<Order["status"]> = [
  ...getOrderStatusOptions().map((s) => s.value),
];

const paymentOptions: Array<Order["paymentMethod"]> = [
  ...getPaymentOptions().map((o) => o.value),
];

export function OrdersPage() {
  const [period, setPeriod] = useState<DateRange | undefined>({
    from: new Date(),
    to: new Date(),
  });

  const [paymentFilter, setPaymentFilter] = useState<
    Order["paymentMethod"] | "all"
  >("all");

  const {
    orders,
    loading,
    isFetching,
    createOrder,
    updateOrder,
    updateOrderStatus,
    deleteOrder,
    isCreating,
    isUpdating,
  } = useOrders(
    period?.from,
    period?.to,
    paymentFilter === "all" ? undefined : paymentFilter,
  );
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingOrder, setEditingOrder] = useState<Order | null>(null);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [statusFilter, setStatusFilter] = useState<Order["status"] | "all">(
    "all",
  );

  const handleCreateOrUpdateOrder = async (data: OrderFormData) => {
    try {
      if (editingOrder) {
        await updateOrder(editingOrder.id, data);
      } else {
        await createOrder(data);
      }
      setIsModalOpen(false);
      setEditingOrder(null);
      toast.success(
        `Pedido ${editingOrder ? "atualizado" : "criado"} com sucesso!`,
      );
    } catch {
      // Error is handled by the mutation's onError or toast
    }
  };

  const handleEditOrder = (order: Order) => {
    setEditingOrder(order);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingOrder(null);
  };

  const handlePeriodFilterChange = (dateRange: DateRange | undefined) => {
    if (!dateRange) {
      setPeriod({
        from: new Date(),
        to: new Date(),
      });
    } else {
      setPeriod(dateRange);
    }
  };

  const filteredOrders =
    statusFilter === "all"
      ? orders
      : orders.filter((order) => order.status === statusFilter);

  const filteredByPayment =
    paymentFilter === "all"
      ? filteredOrders
      : filteredOrders.filter((order) => order.paymentMethod === paymentFilter);

  return (
    <>
      <LoadingOverlay isFetching={isFetching} position="top-bar" />

      <div className="space-y-4 sm:space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
          <OrdersHeader
            isFetching={isFetching}
            onNewOrder={() => setIsModalOpen(true)}
          />
        </div>

        <OrdersFilters
          statusFilter={statusFilter}
          paymentFilter={paymentFilter}
          statusOptions={statusOptions}
          paymentOptions={paymentOptions}
          period={period}
          onDateChange={handlePeriodFilterChange}
          onStatusChange={setStatusFilter}
          onPaymentChange={setPaymentFilter}
          onOpenFilters={() => setIsFiltersOpen(true)}
        />

        <LoadingState
          isLoading={loading && orders.length === 0}
          skeleton={<GridSkeleton items={6} columns={3} />}
        >
          {filteredByPayment.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-500 text-sm sm:text-lg">
                {orders.length === 0
                  ? "Nenhum pedido cadastrado. Comece criando um novo pedido!"
                  : "Nenhum pedido encontrado com o filtro aplicado."}
              </p>
            </div>
          ) : (
            <div
              className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6 transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              {filteredByPayment
                .sort(
                  (a, b) =>
                    new Date(b.createdAt).getTime() -
                    new Date(a.createdAt).getTime(),
                )
                .map((order) => (
                  <OrderCard
                    key={order.id}
                    order={order}
                    onEdit={handleEditOrder}
                    onUpdateStatus={updateOrderStatus}
                    onDelete={deleteOrder}
                  />
                ))}
            </div>
          )}
        </LoadingState>
      </div>

      <Dialog open={isModalOpen} onOpenChange={handleCloseModal}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingOrder ? "Editar Pedido" : "Novo Pedido"}
            </DialogTitle>
          </DialogHeader>

          <Separator />

          <OrderForm
            initialData={editingOrder ? { ...editingOrder } : undefined}
            onSubmit={handleCreateOrUpdateOrder}
            onCancel={handleCloseModal}
            isSubmitting={isCreating || isUpdating}
          />
        </DialogContent>
      </Dialog>

      <BottomSheet
        isOpen={isFiltersOpen}
        onClose={() => setIsFiltersOpen(false)}
        title="Filtros"
      >
        <div className="space-y-4">
          <OrdersFiltersContent
            statusFilter={statusFilter}
            paymentFilter={paymentFilter}
            statusOptions={statusOptions}
            paymentOptions={paymentOptions}
            period={period}
            onStatusChange={setStatusFilter}
            onPaymentChange={setPaymentFilter}
            onDateChange={handlePeriodFilterChange}
          />

          <div className="pt-4 pb-2 border-t border-gray-200">
            <Button onClick={() => setIsFiltersOpen(false)} className="w-full">
              Aplicar Filtros
            </Button>
          </div>
        </div>
      </BottomSheet>
    </>
  );
}
