import { subDays } from "date-fns";
import { ClipboardListIcon, DollarSignIcon } from "lucide-react";
import { useEffect, useState } from "react";
import type { DateRange } from "react-day-picker";
import { useOrders } from "@/features/orders/hooks/use-orders";
import type { Order } from "@/features/orders/types";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import { Button } from "@/shared/components/ui/button";
import { formatMoney } from "@/shared/utils/formatters";
import { DashboardFiltersContent } from "../components/dashboard-filters-content";
import { DashboardHeader } from "../components/dashboard-header";
import { PaymentBreakdownCard } from "../components/payment-breakdown-card";
import { SalesChart } from "../components/sales-chart";
import { StatCard } from "../components/stat-card";
import { StatusDistributionChart } from "../components/status-distribution-chart";
import { TopProductsCard } from "../components/top-products-card";
import { dashboardService } from "../services/dashboard-service";

export function DashboardPage() {
  const [period, setPeriod] = useState<DateRange | undefined>({
    from: subDays(new Date(), 7),
    to: new Date(),
  });
  const { orders, isFetching } = useOrders(period?.from, period?.to);

  const [todayOrders, setTodayOrders] = useState<Order[]>([]);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

  useEffect(() => {
    setTodayOrders(orders);
  }, [orders]);

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
          className={`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 sm:gap-4 lg:gap-6 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <StatCard
            title="Total de Pedidos"
            value={stats.totalOrders}
            icon={<ClipboardListIcon className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="orange"
          />

          <StatCard
            title="Receita Total"
            value={formatMoney(stats.totalRevenue)}
            icon={<DollarSignIcon className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="green"
          />

          <StatCard
            title="Ticket Médio"
            value={formatMoney(stats.averageOrderValue)}
            icon={<DollarSignIcon className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="green"
          />
        </div>

        <div
          className={`grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <SalesChart data={dashboardService.getSalesOverTime(todayOrders)} />
          <StatusDistributionChart
            data={Object.entries(ordersByStatus).map(([status, count]) => ({
              status,
              count,
            }))}
          />
        </div>

        <div
          className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <PaymentBreakdownCard paymentBreakdown={paymentBreakdown} />
          <TopProductsCard
            data={dashboardService.getTopProducts(todayOrders)}
          />
        </div>
      </div>

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
