import { useState } from "react";
import { Button } from "@/shared/components/button";
import { Modal } from "@/shared/components/modal";
import { OrderCard } from "../components/order-card";
import { OrderForm } from "../components/order-form";
import { useOrders } from "../hooks/use-orders";
import type { Order, OrderFormData } from "../types";

export function OrdersPage() {
  const { orders, loading, createOrder, updateOrderStatus, deleteOrder } =
    useOrders();
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

  const sortedOrders = [...filteredOrders].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  );

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-xl text-gray-600">Carregando...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold text-gray-900">Pedidos</h1>
        <Button onClick={() => setIsModalOpen(true)}>+ Novo Pedido</Button>
      </div>

      <div className="flex items-center space-x-4">
        <span className="text-sm font-medium text-gray-700">
          Filtrar por status:
        </span>
        <select
          value={statusFilter}
          onChange={(e) =>
            setStatusFilter(e.target.value as Order["status"] | "all")
          }
          className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-orange-500"
        >
          <option value="all">Todos</option>
          <option value="pending">Pendente</option>
          <option value="preparing">Em Preparo</option>
          <option value="ready">Pronto</option>
          <option value="delivered">Entregue</option>
          <option value="cancelled">Cancelado</option>
        </select>
      </div>

      {sortedOrders.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">
            {orders.length === 0
              ? "Nenhum pedido cadastrado. Comece criando um novo pedido!"
              : "Nenhum pedido encontrado com o filtro aplicado."}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {sortedOrders.map((order) => (
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
