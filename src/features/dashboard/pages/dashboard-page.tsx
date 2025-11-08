import { CheckCircle, ClipboardList, Clock, DollarSign } from "lucide-react";
import { useEffect, useState } from "react";
import { OrderCard } from "@/features/orders/components/order-card";
import { useOrders } from "@/features/orders/hooks/use-orders";
import type { Order } from "@/features/orders/types";
import { DateRangeFilter } from "@/shared/components/date-range-filter";
import { useDateRangeFilter } from "@/shared/hooks/use-date-range-filter";
import { StatCard } from "../components/stat-card";
import { dashboardService } from "../services/dashboard-service";

export function DashboardPage() {
  const {
    inputStart,
    setInputStart,
    inputEnd,
    setInputEnd,
    startDate,
    endDate,
    applyRange,
    resetToToday,
    isInvalid,
  } = useDateRangeFilter();

  const { orders, loading, updateOrderStatus, deleteOrder } = useOrders(
    startDate,
    endDate,
  );

  const [todayOrders, setTodayOrders] = useState<Order[]>([]);

  useEffect(() => {
    setTodayOrders(orders);
  }, [orders]);

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-xl text-gray-600">Carregando...</div>
      </div>
    );
  }

  const stats = dashboardService.calculateStats(todayOrders);
  const ordersByStatus = dashboardService.getOrdersByStatus(todayOrders);

  const activeOrders = todayOrders
    .filter((o) => o.status !== "delivered" && o.status !== "cancelled")
    .sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600 mt-1">Visão geral dos pedidos</p>
        </div>

        <DateRangeFilter
          inputStart={inputStart}
          inputEnd={inputEnd}
          onStartChange={setInputStart}
          onEndChange={setInputEnd}
          onApply={applyRange}
          onReset={resetToToday}
          isInvalid={isInvalid}
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          title="Total de Pedidos"
          value={stats.totalOrders}
          icon={<ClipboardList className="w-6 h-6" />}
          color="orange"
        />

        <StatCard
          title="Receita Total"
          value={`R$ ${stats.totalRevenue.toFixed(2)}`}
          icon={<DollarSign className="w-6 h-6" />}
          color="green"
        />

        <StatCard
          title="Pedidos Ativos"
          value={stats.pendingOrders}
          icon={<Clock className="w-6 h-6" />}
          color="blue"
        />

        <StatCard
          title="Pedidos Entregues"
          value={stats.deliveredOrders}
          icon={<CheckCircle className="w-6 h-6" />}
          color="purple"
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Pedidos Ativos ({activeOrders.length})
            </h2>

            {activeOrders.length === 0 ? (
              <p className="text-gray-500 text-center py-8">
                Nenhum pedido ativo no momento
              </p>
            ) : (
              <div className="space-y-4">
                {activeOrders.map((order) => (
                  <OrderCard
                    key={order.id}
                    order={order}
                    onUpdateStatus={updateOrderStatus}
                    onDelete={deleteOrder}
                  />
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">
            Status dos Pedidos
          </h2>
          <div className="space-y-3">
            <div className="flex justify-between items-center p-3 bg-yellow-50 rounded-lg">
              <span className="text-sm font-medium text-gray-700">
                Pendentes
              </span>
              <span className="text-lg font-bold text-yellow-600">
                {ordersByStatus.pending}
              </span>
            </div>
            <div className="flex justify-between items-center p-3 bg-blue-50 rounded-lg">
              <span className="text-sm font-medium text-gray-700">
                Em Preparo
              </span>
              <span className="text-lg font-bold text-blue-600">
                {ordersByStatus.preparing}
              </span>
            </div>
            <div className="flex justify-between items-center p-3 bg-purple-50 rounded-lg">
              <span className="text-sm font-medium text-gray-700">Prontos</span>
              <span className="text-lg font-bold text-purple-600">
                {ordersByStatus.ready}
              </span>
            </div>
            <div className="flex justify-between items-center p-3 bg-green-50 rounded-lg">
              <span className="text-sm font-medium text-gray-700">
                Entregues
              </span>
              <span className="text-lg font-bold text-green-600">
                {ordersByStatus.delivered}
              </span>
            </div>
            <div className="flex justify-between items-center p-3 bg-red-50 rounded-lg">
              <span className="text-sm font-medium text-gray-700">
                Cancelados
              </span>
              <span className="text-lg font-bold text-red-600">
                {ordersByStatus.cancelled}
              </span>
            </div>
          </div>

          <div className="mt-6 pt-6 border-t border-gray-200">
            <div className="flex justify-between items-center">
              <span className="text-sm font-medium text-gray-700">
                Ticket Médio
              </span>
              <span className="text-xl font-bold text-orange-600">
                R$ {stats.averageOrderValue.toFixed(2)}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
