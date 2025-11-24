import { CheckCircle, ClipboardList, Clock, DollarSign } from "lucide-react";
import { useEffect, useState } from "react";
import type { DateRange } from "react-day-picker";
import { OrderForm } from "@/features/orders/components/order-form";
import { useOrders } from "@/features/orders/hooks/use-orders";
import type { Order, OrderFormData } from "@/features/orders/types";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import { Button } from "@/shared/components/button";
import { Modal } from "@/shared/components/modal";
import { ActiveOrdersSection } from "../components/active-orders-section";
import { DashboardFiltersContent } from "../components/dashboard-filters-content";
import { DashboardHeader } from "../components/dashboard-header";
import { PaymentBreakdownCard } from "../components/payment-breakdown-card";
import { StatCard } from "../components/stat-card";
import { StatsSidebar } from "../components/stats-sidebar";
import { dashboardService } from "../services/dashboard-service";

export function DashboardPage() {
  const [period, setPeriod] = useState<DateRange | undefined>({
    from: new Date(),
    to: new Date(),
  });
  const { orders, isFetching, updateOrder, updateOrderStatus, deleteOrder } =
    useOrders(period?.from, period?.to);

  const [todayOrders, setTodayOrders] = useState<Order[]>([]);
  const [editingOrder, setEditingOrder] = useState<Order | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

  useEffect(() => {
    setTodayOrders(orders);
  }, [orders]);

  const handleEditOrder = (order: Order) => {
    setEditingOrder(order);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingOrder(null);
  };

  const handleUpdateOrder = (data: OrderFormData) => {
    if (editingOrder) {
      updateOrder(editingOrder.id, data);
    }
    handleCloseModal();
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

  const stats = dashboardService.calculateStats(todayOrders);
  const ordersByStatus = dashboardService.getOrdersByStatus(todayOrders);
  const paymentBreakdown = dashboardService.getPaymentBreakdown(todayOrders);

  const activeOrders = todayOrders
    .filter((o) => o.status !== "Delivered" && o.status !== "Canceled")
    .sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );

  return (
    <>
      <div className="space-y-4 sm:space-y-6">
        <DashboardHeader
          isFetching={isFetching}
          period={period}
          onDateChange={handlePeriodFilterChange}
          onOpenFilters={() => setIsFiltersOpen(true)}
        />

        <div
          className={`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4 lg:gap-6 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <StatCard
            title="Total de Pedidos"
            value={stats.totalOrders}
            icon={<ClipboardList className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="orange"
          />

          <StatCard
            title="Receita Total"
            value={`R$ ${stats.totalRevenue.toFixed(2)}`}
            icon={<DollarSign className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="green"
          />

          <StatCard
            title="Pedidos Ativos"
            value={stats.pendingOrders}
            icon={<Clock className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="blue"
          />

          <StatCard
            title="Pedidos Entregues"
            value={stats.deliveredOrders}
            icon={<CheckCircle className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="purple"
          />
        </div>

        <div
          className={`grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <div className="lg:col-span-2">
            <ActiveOrdersSection
              orders={activeOrders}
              onEdit={handleEditOrder}
              onUpdateStatus={updateOrderStatus}
              onDelete={deleteOrder}
            />
          </div>

          <div className="space-y-4 sm:space-y-6">
            <StatsSidebar
              ordersByStatus={ordersByStatus}
              averageOrderValue={stats.averageOrderValue}
            />

            <PaymentBreakdownCard paymentBreakdown={paymentBreakdown} />
          </div>
        </div>
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title="Editar Pedido"
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
          onSubmit={handleUpdateOrder}
          onCancel={handleCloseModal}
        />
      </Modal>

      <BottomSheet
        isOpen={isFiltersOpen}
        onClose={() => setIsFiltersOpen(false)}
        title="Filtros"
      >
        <div className="space-y-4">
          <DashboardFiltersContent
            period={period}
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
