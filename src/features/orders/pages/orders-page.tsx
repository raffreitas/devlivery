import { useState } from "react";
import {
  type AutocompleteOption,
  AutocompleteSelect,
} from "@/shared/components/autocomplete-select";
import { Button } from "@/shared/components/button";
import { DateRangeFilter } from "@/shared/components/date-range-filter";
import { LoadingSpinner } from "@/shared/components/loading-spinner";
import { Modal } from "@/shared/components/modal";
import { useDateRangeFilter } from "@/shared/hooks/use-date-range-filter";
import { OrderCard } from "../components/order-card";
import { OrderForm } from "../components/order-form";
import { getPaymentOptions } from "../constants/payment-methods";
import { useOrders } from "../hooks/use-orders";
import type { Order, OrderFormData } from "../types";

const statusOptions: AutocompleteOption<Order["status"] | "all">[] = [
  { value: "all", label: "Todos" },
  { value: "pending", label: "Pendente" },
  { value: "preparing", label: "Em Preparo" },
  { value: "ready", label: "Pronto" },
  { value: "delivered", label: "Entregue" },
  { value: "cancelled", label: "Cancelado" },
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
    updateOrderStatus,
    deleteOrder,
  } = useOrders(
    startDate,
    endDate,
    paymentFilter === "all" ? undefined : paymentFilter,
  );
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [statusFilter, setStatusFilter] = useState<Order["status"] | "all">(
    "all",
  );

  const handleCreateOrder = (data: OrderFormData) => {
    createOrder(data);
    setIsModalOpen(false);
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
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold text-gray-900">Pedidos</h1>
          {isFetching && (
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <LoadingSpinner size="sm" className="text-orange-500" />
              <span>Atualizando...</span>
            </div>
          )}
        </div>
        <Button onClick={() => setIsModalOpen(true)}>+ Novo Pedido</Button>
      </div>

      <div className="bg-white rounded-lg shadow-md p-3">
        <div className="flex items-end gap-4 flex-nowrap overflow-x-auto p-1">
          <div className="flex items-end gap-4">
            <AutocompleteSelect
              label="Status"
              value={statusFilter}
              options={statusOptions}
              onChange={(value) => setStatusFilter(value ?? "all")}
              placeholder="Selecione um status"
              autocomplete={false}
            />

            <AutocompleteSelect
              label="Pagamento"
              value={paymentFilter}
              options={paymentOptions}
              onChange={(value) => setPaymentFilter(value ?? "all")}
              placeholder="Selecione método"
              autocomplete={false}
            />
          </div>

          <div className="ml-auto">
            <DateRangeFilter
              startDate={inputStartDate}
              endDate={inputEndDate}
              onStartChange={setStartDate}
              onEndChange={setEndDate}
              onReset={resetToToday}
            />
          </div>
        </div>
      </div>

      {loading && orders.length === 0 ? (
        <div className="flex justify-center items-center h-64">
          <div className="text-xl text-gray-600">Carregando...</div>
        </div>
      ) : filteredByPayment.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">
            {orders.length === 0
              ? "Nenhum pedido cadastrado. Comece criando um novo pedido!"
              : "Nenhum pedido encontrado com o filtro aplicado."}
          </p>
        </div>
      ) : (
        <div
          className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          {filteredByPayment
            .map((o) => o)
            .sort(
              (a, b) =>
                new Date(b.createdAt).getTime() -
                new Date(a.createdAt).getTime(),
            )
            .map((order) => (
              <OrderCard
                key={order.id}
                order={order}
                onUpdateStatus={updateOrderStatus}
                onDelete={deleteOrder}
              />
            ))}
        </div>
      )}

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Novo Pedido"
      >
        <OrderForm
          onSubmit={handleCreateOrder}
          onCancel={() => setIsModalOpen(false)}
        />
      </Modal>
    </div>
  );
}
