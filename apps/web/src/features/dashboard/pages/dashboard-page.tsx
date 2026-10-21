import { subDays } from "date-fns";
import {
  AlertCircle,
  BarChart3,
  DollarSign,
  TrendingDown,
  TrendingUp,
} from "lucide-react";
import { useState } from "react";
import type { DateRange } from "react-day-picker";
import { QueryRegion } from "@/shared/components/query-region";
import { Button } from "@/shared/components/ui/button";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
} from "@/shared/components/ui/drawer";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/shared/components/ui/tabs";
import { formatMoney } from "@/shared/utils/formatters";
import { sumMoney } from "@/shared/utils/money";
import { StatCard } from "../../../shared/components/stat-card";
import { DashboardFiltersContent } from "../components/dashboard-filters-content";
import { DashboardHeader } from "../components/dashboard-header";
import { ExpenseStatusChart } from "../components/expense-status-chart";
import { ExpensesByCategoryChart } from "../components/expenses-by-category-chart";
import { ExpensesOverTimeChart } from "../components/expenses-over-time-chart";
import { PaymentBreakdownCard } from "../components/payment-breakdown-card";
import { RevenueVsExpensesChart } from "../components/revenue-vs-expenses-chart";
import { SalesChart } from "../components/sales-chart";
import { TopProductsCard } from "../components/top-products-card";
import { useDashboardExpenses } from "../hooks/use-dashboard-expenses";
import { useDashboardOverview } from "../hooks/use-dashboard-overview";
import { useDashboardSales } from "../hooks/use-dashboard-sales";
import { dashboardService } from "../services/dashboard-service";

export function DashboardPage() {
  const [period, setPeriod] = useState<DateRange | undefined>({
    from: subDays(new Date(), 7),
    to: new Date(),
  });

  const [activeTab, setActiveTab] = useState("vendas");
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

  const overview = useDashboardOverview(period?.from, period?.to);

  const sales = useDashboardSales(
    period?.from,
    period?.to,
    activeTab === "vendas",
  );
  const expenses = useDashboardExpenses(
    period?.from,
    period?.to,
    activeTab === "despesas",
  );

  const isFetching =
    overview.isFetching || sales.isFetching || expenses.isFetching;

  const {
    stats,
    expenseSummary,
    salesOverTime,
    expensesOverTime,
    expensesByStatus,
  } = overview;

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

  const netProfit = dashboardService.calculateNetProfit(
    stats.totalRevenue,
    expenseSummary.paid,
  );

  const pendingExpensesTotal = sumMoney([
    expenseSummary.pending,
    expenseSummary.overdue,
  ]);

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
          className={`grid grid-cols-1 lg:grid-cols-4 gap-3 sm:gap-4 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <QueryRegion queries={[overview.queries.stats]}>
            <StatCard
              title="Valor dos Pedidos"
              value={formatMoney(stats.totalRevenue)}
              icon={<DollarSign className="w-5 h-5 sm:w-6 sm:h-6" />}
              color="green"
            />
          </QueryRegion>

          <QueryRegion queries={[overview.queries.expenseSummary]}>
            <StatCard
              title="Despesas Pagas"
              value={formatMoney(expenseSummary.paid)}
              icon={<TrendingDown className="w-5 h-5 sm:w-6 sm:h-6" />}
              color="red"
            />
          </QueryRegion>

          <QueryRegion
            queries={[overview.queries.stats, overview.queries.expenseSummary]}
          >
            <StatCard
              title="Saldo Projetado"
              value={formatMoney(netProfit)}
              icon={<TrendingUp className="w-5 h-5 sm:w-6 sm:h-6" />}
              color={netProfit >= 0 ? "green" : "red"}
            />
          </QueryRegion>

          <QueryRegion queries={[overview.queries.stats]}>
            <StatCard
              title="Pedidos"
              value={stats.totalOrders}
              icon={<AlertCircle className="w-5 h-5 sm:w-6 sm:h-6" />}
              color="orange"
            />
          </QueryRegion>
        </div>

        <div
          className={`transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <QueryRegion
            queries={[
              overview.queries.salesOverTime,
              overview.queries.expensesOverTime,
            ]}
          >
            <RevenueVsExpensesChart
              revenueData={salesOverTime}
              expensesData={expensesOverTime}
            />
          </QueryRegion>
        </div>

        <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
          <TabsList className="grid w-full grid-cols-3 lg:w-fit">
            <TabsTrigger value="vendas">Vendas</TabsTrigger>
            <TabsTrigger value="despesas">Despesas</TabsTrigger>
            <TabsTrigger value="analises">Análises</TabsTrigger>
          </TabsList>

          <TabsContent value="vendas" className="mt-4 space-y-4 sm:space-y-6">
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                sales.isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <QueryRegion queries={[overview.queries.salesOverTime]}>
                <SalesChart data={salesOverTime} />
              </QueryRegion>
            </div>
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                sales.isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <QueryRegion queries={[sales.queries.paymentBreakdown]}>
                <PaymentBreakdownCard
                  paymentBreakdown={sales.paymentBreakdown}
                />
              </QueryRegion>
              <QueryRegion queries={[sales.queries.topProducts]}>
                <TopProductsCard data={sales.topProducts} />
              </QueryRegion>
            </div>
          </TabsContent>

          <TabsContent value="despesas" className="mt-4 space-y-4 sm:space-y-6">
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                expenses.isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <QueryRegion queries={[overview.queries.expensesByStatus]}>
                <ExpenseStatusChart data={expensesByStatus} />
              </QueryRegion>
              <QueryRegion queries={[expenses.queries.expensesByCategory]}>
                <ExpensesByCategoryChart data={expenses.expensesByCategory} />
              </QueryRegion>
            </div>
            <div
              className={`transition-opacity duration-200 ${
                expenses.isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <QueryRegion queries={[overview.queries.expensesOverTime]}>
                <ExpensesOverTimeChart data={expensesOverTime} />
              </QueryRegion>
            </div>
          </TabsContent>

          <TabsContent value="analises" className="mt-4 space-y-4 sm:space-y-6">
            <div
              className={`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 sm:gap-4 transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <QueryRegion queries={[overview.queries.stats]}>
                <StatCard
                  title="Total de Pedidos"
                  value={stats.totalOrders}
                  icon={<BarChart3 className="w-5 h-5 sm:w-6 sm:h-6" />}
                  color="orange"
                />
              </QueryRegion>
              <QueryRegion queries={[overview.queries.stats]}>
                <StatCard
                  title="Ticket Médio"
                  value={formatMoney(stats.averageOrderValue)}
                  icon={<DollarSign className="w-5 h-5 sm:w-6 sm:h-6" />}
                  color="green"
                />
              </QueryRegion>
              <QueryRegion queries={[overview.queries.expenseSummary]}>
                <StatCard
                  title="Despesas Pendentes"
                  value={formatMoney(pendingExpensesTotal)}
                  icon={<AlertCircle className="w-5 h-5 sm:w-6 sm:h-6" />}
                  color="amber"
                />
              </QueryRegion>
            </div>
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <QueryRegion queries={[overview.queries.salesOverTime]}>
                <SalesChart data={salesOverTime} />
              </QueryRegion>
              <QueryRegion queries={[overview.queries.expensesOverTime]}>
                <ExpensesOverTimeChart data={expensesOverTime} />
              </QueryRegion>
            </div>
          </TabsContent>
        </Tabs>
      </div>

      <Drawer
        open={isFiltersOpen}
        onOpenChange={setIsFiltersOpen}
        showSwipeHandle
      >
        <DrawerContent>
          <DrawerHeader>
            <DrawerTitle>Filtros</DrawerTitle>
          </DrawerHeader>
          <div className="min-h-0 flex-1 overflow-y-auto p-4">
            <DashboardFiltersContent
              period={period}
              onDateChange={handlePeriodFilterChange}
            />
          </div>
          <DrawerFooter>
            <DrawerClose render={<Button />}>Aplicar Filtros</DrawerClose>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>
    </>
  );
}
