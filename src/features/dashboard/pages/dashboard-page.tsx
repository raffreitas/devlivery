import { CheckCircle, ClipboardList, Clock, DollarSign } from "lucide-react";
import { useEffect, useState } from "react";
import { useOrders } from "@/features/orders/hooks/use-orders";
import type { Order } from "@/features/orders/types";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import { Button } from "@/shared/components/button";
import { useDateRangeFilter } from "@/shared/hooks/use-date-range-filter";
import { ActiveOrdersSection } from "../components/active-orders-section";
import { DashboardFiltersContent } from "../components/dashboard-filters-content";
import { DashboardHeader } from "../components/dashboard-header";
import { PaymentBreakdownCard } from "../components/payment-breakdown-card";
import { StatCard } from "../components/stat-card";
import { StatsSidebar } from "../components/stats-sidebar";
import { dashboardService } from "../services/dashboard-service";

export function DashboardPage() {
  const {
    inputStartDate,
    inputEndDate,
    startDate,
    endDate,
    setStartDate,
    setEndDate,
    resetToToday,
  } = useDateRangeFilter({ debounceMs: 500 });

  const { orders, isFetching, updateOrderStatus, deleteOrder } = useOrders(
    startDate,
    endDate,
  );

  const [todayOrders, setTodayOrders] = useState<Order[]>([]);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

  useEffect(() => {
    setTodayOrders(orders);
  }, [orders]);

  const stats = dashboardService.calculateStats(todayOrders);
  const ordersByStatus = dashboardService.getOrdersByStatus(todayOrders);
  const paymentBreakdown = dashboardService.getPaymentBreakdown(todayOrders);

  const activeOrders = todayOrders
    .filter((o) => o.status !== "delivered" && o.status !== "cancelled")
    .sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );

  return (
    <>
      <div className="space-y-4 sm:space-y-6">
        <DashboardHeader
          isFetching={isFetching}
          inputStartDate={inputStartDate}
          inputEndDate={inputEndDate}
          onStartChange={setStartDate}
          onEndChange={setEndDate}
          onReset={resetToToday}
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

      {/* Bottom Sheet de Filtros (fora do container space-y) */}
      <BottomSheet
        isOpen={isFiltersOpen}
        onClose={() => setIsFiltersOpen(false)}
        title="Filtros"
      >
        <div className="space-y-4">
          <DashboardFiltersContent
            inputStartDate={inputStartDate}
            inputEndDate={inputEndDate}
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
