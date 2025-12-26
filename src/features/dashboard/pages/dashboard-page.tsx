import { subDays } from "date-fns";
import {
  AlertCircle,
  BarChart3,
  DollarSign,
  TrendingDown,
  TrendingUp,
} from "lucide-react";
import { useMemo, useState } from "react";
import type { DateRange } from "react-day-picker";
import { useExpenses } from "@/features/expenses/hooks/use-expenses";
import { ExpenseStatus } from "@/features/expenses/types";
import { useOrders } from "@/features/orders/hooks/use-orders";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import { Button } from "@/shared/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";
import { formatMoney } from "@/shared/utils/formatters";
import { StatCard } from "../../../shared/components/stat-card";
import { DashboardFiltersContent } from "../components/dashboard-filters-content";
import { DashboardHeader } from "../components/dashboard-header";
import { ExpenseAlertsCard } from "../components/expense-alerts-card";
import { ExpenseStatusChart } from "../components/expense-status-chart";
import { ExpensesByCategoryChart } from "../components/expenses-by-category-chart";
import { ExpensesOverTimeChart } from "../components/expenses-over-time-chart";
import { PaymentBreakdownCard } from "../components/payment-breakdown-card";
import { RevenueVsExpensesChart } from "../components/revenue-vs-expenses-chart";
import { SalesChart } from "../components/sales-chart";
import { StatusDistributionChart } from "../components/status-distribution-chart";
import { TopProductsCard } from "../components/top-products-card";
import { dashboardService } from "../services/dashboard-service";

export function DashboardPage() {
  const [period, setPeriod] = useState<DateRange | undefined>({
    from: subDays(new Date(), 7),
    to: new Date(),
  });
  const { orders, isFetching: isOrdersFetching } = useOrders(
    period?.from,
    period?.to,
  );
  const {
    expenses,
    summary: expenseSummary,
    isFetching: isExpensesFetching,
  } = useExpenses({
    duePeriod: period,
  });

  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

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

  const isFetching = isOrdersFetching || isExpensesFetching;

  const stats = dashboardService.calculateStats(orders);
  const ordersByStatus = dashboardService.getOrdersByStatus(orders);
  const paymentBreakdown = dashboardService.getPaymentBreakdown(orders);

  // Calcular métricas de despesas
  const expensesByStatus = dashboardService.getExpensesByStatus(expenses);
  const expensesByCategory = dashboardService.getExpensesByCategory(expenses);
  const expensesOverTime = dashboardService.getExpensesOverTime(expenses);
  const salesOverTime = dashboardService.getSalesOverTime(orders);

  // Calcular lucro líquido
  const netProfit = dashboardService.calculateNetProfit(
    stats.totalRevenue,
    expenseSummary.paid,
  );

  // Separar despesas por alertas
  const overdueExpenses = useMemo(
    () => expenses.filter((e) => e.status === ExpenseStatus.OVERDUE),
    [expenses],
  );

  const dueTodayExpenses = useMemo(
    () => expenses.filter((e) => e.status === ExpenseStatus.DUE_TODAY),
    [expenses],
  );

  const upcomingExpenses = useMemo(
    () => dashboardService.getUpcomingExpenses(expenses, 7),
    [expenses],
  );

  // Calcular despesas pendentes (Pending + Overdue)
  const pendingExpensesTotal =
    expenseSummary.pending + expenseSummary.overdue;

  // Verificar se há alertas importantes
  const hasImportantAlerts =
    overdueExpenses.length > 0 || dueTodayExpenses.length > 0;

  return (
    <>
      <div className="space-y-4 sm:space-y-6">
        <DashboardHeader
          isFetching={isFetching}
          period={period}
          onDateChange={handlePeriodFilterChange}
          onOpenFilters={() => setIsFiltersOpen(true)}
        />

        {/* Cards Principais - Apenas 4 métricas essenciais */}
        <div
          className={`grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <StatCard
            title="Receita Total"
            value={formatMoney(stats.totalRevenue)}
            icon={<DollarSign className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="green"
          />

          <StatCard
            title="Despesas Pagas"
            value={formatMoney(expenseSummary.paid)}
            icon={<TrendingDown className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="red"
          />

          <StatCard
            title="Lucro Líquido"
            value={formatMoney(netProfit)}
            icon={<TrendingUp className="w-5 h-5 sm:w-6 sm:h-6" />}
            color={netProfit >= 0 ? "green" : "red"}
          />

          <StatCard
            title={hasImportantAlerts ? "Alertas" : "Pedidos"}
            value={
              hasImportantAlerts
                ? `${overdueExpenses.length + dueTodayExpenses.length}`
                : stats.totalOrders
            }
            icon={<AlertCircle className="w-5 h-5 sm:w-6 sm:h-6" />}
            color={hasImportantAlerts ? "red" : "orange"}
          />
        </div>

        {/* Gráfico Principal - Receitas vs Despesas */}
        <div
          className={`transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          <RevenueVsExpensesChart
            revenueData={salesOverTime}
            expensesData={expensesOverTime}
          />
        </div>

        {/* Alertas de Despesas (se houver) */}
        {(overdueExpenses.length > 0 ||
          dueTodayExpenses.length > 0 ||
          upcomingExpenses.length > 0) && (
          <div
            className={`transition-opacity duration-200 ${
              isFetching ? "opacity-60" : "opacity-100"
            }`}
          >
            <ExpenseAlertsCard
              overdueExpenses={overdueExpenses}
              dueTodayExpenses={dueTodayExpenses}
              upcomingExpenses={upcomingExpenses}
            />
          </div>
        )}

        {/* Tabs para organizar gráficos detalhados */}
        <Tabs defaultValue="vendas" className="w-full">
          <TabsList className="grid w-full grid-cols-3 lg:w-fit">
            <TabsTrigger value="vendas">Vendas</TabsTrigger>
            <TabsTrigger value="despesas">Despesas</TabsTrigger>
            <TabsTrigger value="analises">Análises</TabsTrigger>
          </TabsList>

          <TabsContent
            value="vendas"
            className="mt-4 space-y-4 sm:space-y-6"
          >
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <SalesChart data={salesOverTime} />
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
              <TopProductsCard data={dashboardService.getTopProducts(orders)} />
            </div>
          </TabsContent>

          <TabsContent
            value="despesas"
            className="mt-4 space-y-4 sm:space-y-6"
          >
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <ExpenseStatusChart data={expensesByStatus} />
              <ExpensesByCategoryChart data={expensesByCategory} />
            </div>
            <div
              className={`transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <ExpensesOverTimeChart data={expensesOverTime} />
            </div>
          </TabsContent>

          <TabsContent
            value="analises"
            className="mt-4 space-y-4 sm:space-y-6"
          >
            <div
              className={`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 sm:gap-4 transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <StatCard
                title="Total de Pedidos"
                value={stats.totalOrders}
                icon={<BarChart3 className="w-5 h-5 sm:w-6 sm:h-6" />}
                color="orange"
              />
              <StatCard
                title="Ticket Médio"
                value={formatMoney(stats.averageOrderValue)}
                icon={<DollarSign className="w-5 h-5 sm:w-6 sm:h-6" />}
                color="green"
              />
              <StatCard
                title="Despesas Pendentes"
                value={formatMoney(pendingExpensesTotal)}
                icon={<AlertCircle className="w-5 h-5 sm:w-6 sm:h-6" />}
                color="amber"
              />
            </div>
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <SalesChart data={salesOverTime} />
              <ExpensesOverTimeChart data={expensesOverTime} />
            </div>
          </TabsContent>
        </Tabs>
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
