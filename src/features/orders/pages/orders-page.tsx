import { useState } from "react";
import type { AutocompleteOption } from "@/shared/components/autocomplete-select";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import { Button } from "@/shared/components/button";
import { Modal } from "@/shared/components/modal";
import { useDateRangeFilter } from "@/shared/hooks/use-date-range-filter";
import { OrderCard } from "../components/order-card";
import { OrderForm } from "../components/order-form";
import { OrdersFilters } from "../components/orders-filters";
import { OrdersFiltersContent } from "../components/orders-filters-content";
import { OrdersHeader } from "../components/orders-header";
import { getPaymentOptions } from "../constants/payment-methods";
import { useOrders } from "../hooks/use-orders";
import type { Order, OrderFormData } from "../types";

const statusOptions: AutocompleteOption<Order["status"] | "all">[] = [
  { value: "all", label: "Todos" },
  { value: "Pending", label: "Pendente" },
  { value: "Preparing", label: "Em Preparo" },
  { value: "Ready", label: "Pronto" },
  { value: "Delivered", label: "Entregue" },
  { value: "Canceled", label: "Cancelado" },
];

const paymentOptions: AutocompleteOption<Order["paymentMethod"] | "all">[] = [
  { value: "all", label: "Todos" },
  ...getPaymentOptions().map((o) => ({
    value: o.value as Order["paymentMethod"],
    label: o.label,
  })),
];

export function OrdersPage() {
  const {
    inputStartDate,
    inputEndDate,
    startDate,
    endDate,
    setStartDate,
    setEndDate,
    resetToToday,
  } = useDateRangeFilter({ defaultDaysBack: 2, debounceMs: 500 });
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
  } = useOrders(
    startDate,
    endDate,
    paymentFilter === "all" ? undefined : paymentFilter,
  );
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingOrder, setEditingOrder] = useState<Order | null>(null);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [statusFilter, setStatusFilter] = useState<Order["status"] | "all">(
    "all",
  );

  const handleCreateOrUpdateOrder = (data: OrderFormData) => {
    if (editingOrder) {
      updateOrder(editingOrder.id, data);
    } else {
      createOrder(data);
    }
    setIsModalOpen(false);
    setEditingOrder(null);
  };

  const handleEditOrder = (order: Order) => {
    setEditingOrder(order);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingOrder(null);
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
      <div className="space-y-4 sm:space-y-6">
        <OrdersHeader
          isFetching={isFetching}
          onNewOrder={() => setIsModalOpen(true)}
        />

        <OrdersFilters
          statusFilter={statusFilter}
          paymentFilter={paymentFilter}
          statusOptions={statusOptions}
          paymentOptions={paymentOptions}
          inputStartDate={inputStartDate}
          inputEndDate={inputEndDate}
          onStatusChange={setStatusFilter}
          onPaymentChange={setPaymentFilter}
          onStartDateChange={setStartDate}
          onEndDateChange={setEndDate}
          onResetDates={resetToToday}
          onOpenFilters={() => setIsFiltersOpen(true)}
        />

        {loading && orders.length === 0 ? (
          <div className="flex justify-center items-center h-64">
            <div className="text-base sm:text-xl text-secondary-foreground">
              Carregando...
            </div>
          </div>
        ) : filteredByPayment.length === 0 ? (
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
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={editingOrder ? "Editar Pedido" : "Novo Pedido"}
      >
        <OrderForm
          initialData={
            editingOrder
              ? {
                  id: editingOrder.id,
                  items: editingOrder.items,
                  customerName: editingOrder.customerName,
                  customerPhone: editingOrder.customerPhone,
                  deliveryAddress: editingOrder.deliveryAddress,
                  deliveryFee: editingOrder.deliveryFee,
                  paymentMethod: editingOrder.paymentMethod,
                }
              : undefined
          }
          onSubmit={handleCreateOrUpdateOrder}
          onCancel={handleCloseModal}
        />
      </Modal>

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
            inputStartDate={inputStartDate}
            inputEndDate={inputEndDate}
            onStatusChange={setStatusFilter}
            onPaymentChange={setPaymentFilter}
            onStartDateChange={setStartDate}
            onEndDateChange={setEndDate}
            onResetDates={resetToToday}
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
