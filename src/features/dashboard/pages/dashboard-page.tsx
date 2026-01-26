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
import { BottomSheet } from "@/shared/components/bottom-sheet";
import { Button } from "@/shared/components/ui/button";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/shared/components/ui/tabs";
import { formatMoney } from "@/shared/utils/formatters";
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

  // Dados sempre necessários (overview)
  const overview = useDashboardOverview(period?.from, period?.to);

  // Dados condicionais por tab
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

  // Combinar isFetching de todos os hooks
  const isFetching =
    overview.isFetching || sales.isFetching || expenses.isFetching;

  // Extrair dados do overview
  const {
    stats,
    expenseSummary,
    salesOverTime: overviewSalesOverTime,
    expensesOverTime: overviewExpensesOverTime,
    expensesByStatus: overviewExpensesByStatus,
  } = overview;

  // Usar salesOverTime do hook de vendas se disponível, senão do overview
  const salesOverTime =
    sales.salesOverTime.length > 0
      ? sales.salesOverTime
      : overviewSalesOverTime;

  // Usar expensesOverTime do hook de despesas se disponível, senão do overview
  const expensesOverTime =
    expenses.expensesOverTime.length > 0
      ? expenses.expensesOverTime
      : overviewExpensesOverTime;

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

  // Calcular lucro líquido
  const netProfit = dashboardService.calculateNetProfit(
    stats.totalRevenue,
    expenseSummary.paid,
  );

  // Usar expensesByStatus do hook de despesas se disponível, senão do overview
  const expensesByStatus =
    expenses.expensesByStatus.length > 0
      ? expenses.expensesByStatus
      : overviewExpensesByStatus;

  // Calcular despesas pendentes (Pending + Overdue)
  const pendingExpensesTotal = expenseSummary.pending + expenseSummary.overdue;

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
          className={`grid grid-cols-1 lg:grid-cols-4 gap-3 sm:gap-4 transition-opacity duration-200 ${
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
            title="Pedidos"
            value={stats.totalOrders}
            icon={<AlertCircle className="w-5 h-5 sm:w-6 sm:h-6" />}
            color="orange"
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

        {/* Tabs para organizar gráficos detalhados */}
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
              <SalesChart data={salesOverTime} />
            </div>
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                sales.isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <PaymentBreakdownCard paymentBreakdown={sales.paymentBreakdown} />
              <TopProductsCard data={sales.topProducts} />
            </div>
          </TabsContent>

          <TabsContent value="despesas" className="mt-4 space-y-4 sm:space-y-6">
            <div
              className={`grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6 transition-opacity duration-200 ${
                expenses.isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <ExpenseStatusChart data={expensesByStatus} />
              <ExpensesByCategoryChart data={expenses.expensesByCategory} />
            </div>
            <div
              className={`transition-opacity duration-200 ${
                expenses.isFetching ? "opacity-60" : "opacity-100"
              }`}
            >
              <ExpensesOverTimeChart data={expensesOverTime} />
            </div>
          </TabsContent>

          <TabsContent value="analises" className="mt-4 space-y-4 sm:space-y-6">
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

          <div className="pt-4 pb-2 border-t border-border">
            <Button onClick={() => setIsFiltersOpen(false)} className="w-full">
              Aplicar Filtros
            </Button>
          </div>
        </div>
      </BottomSheet>
    </>
  );
}
