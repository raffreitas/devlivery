import { useId } from "react";
import { Area, AreaChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  type ChartConfig,
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
} from "@/shared/components/ui/chart";
import { formatMoney } from "@/shared/utils/formatters";
import { moneyDifference } from "@/shared/utils/money";
import {
  combineFinancialSeries,
  formatChartDate,
  formatChartFullDate,
  formatChartMoney,
  type TimeSeriesPoint,
} from "../utils/chart-data";
import { ChartDataTable } from "./chart-data-table";
import { ChartTooltipValue } from "./chart-tooltip-value";

const chartConfig = {
  revenue: { label: "Receitas", color: "var(--chart-revenue)" },
  expenses: { label: "Despesas", color: "var(--chart-expense)" },
} satisfies ChartConfig;

export function RevenueVsExpensesChart({
  revenueData,
  expensesData,
}: {
  revenueData: TimeSeriesPoint[];
  expensesData: TimeSeriesPoint[];
}) {
  const id = useId().replace(/:/g, "");
  const combinedData = combineFinancialSeries(revenueData, expensesData);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Receitas vs Despesas</CardTitle>
      </CardHeader>
      <CardContent>
        {combinedData.length > 0 ? (
          <>
            <ChartContainer config={chartConfig} className="h-75 w-full">
              <AreaChart
                accessibilityLayer
                title="Receitas vs Despesas"
                data={combinedData}
              >
                <defs>
                  <linearGradient
                    id={`${id}-revenue`}
                    x1="0"
                    y1="0"
                    x2="0"
                    y2="1"
                  >
                    <stop
                      offset="5%"
                      stopColor="var(--color-revenue)"
                      stopOpacity={0.4}
                    />
                    <stop
                      offset="95%"
                      stopColor="var(--color-revenue)"
                      stopOpacity={0.05}
                    />
                  </linearGradient>
                  <linearGradient
                    id={`${id}-expenses`}
                    x1="0"
                    y1="0"
                    x2="0"
                    y2="1"
                  >
                    <stop
                      offset="5%"
                      stopColor="var(--color-expenses)"
                      stopOpacity={0.4}
                    />
                    <stop
                      offset="95%"
                      stopColor="var(--color-expenses)"
                      stopOpacity={0.05}
                    />
                  </linearGradient>
                </defs>
                <CartesianGrid vertical={false} />
                <XAxis
                  dataKey="date"
                  tick={{ fill: "var(--muted-foreground)" }}
                  tickLine={false}
                  axisLine={false}
                  tickMargin={8}
                  tickFormatter={formatChartDate}
                />
                <YAxis
                  tickLine={false}
                  axisLine={false}
                  width={72}
                  tick={{ fill: "var(--muted-foreground)" }}
                  tickFormatter={formatChartMoney}
                />
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      labelFormatter={(value) =>
                        formatChartFullDate(String(value))
                      }
                      formatter={(value, name, item, index) => {
                        const point =
                          item.payload as (typeof combinedData)[number];
                        return (
                          <>
                            <ChartTooltipValue
                              label={
                                name === "revenue" ? "Receitas" : "Despesas"
                              }
                              value={formatMoney(Number(value))}
                            />
                            {index === 1 && (
                              <ChartTooltipValue
                                label="Saldo projetado"
                                value={formatMoney(
                                  moneyDifference(
                                    point.revenue,
                                    point.expenses,
                                  ),
                                )}
                              />
                            )}
                          </>
                        );
                      }}
                    />
                  }
                />
                <Area
                  type="linear"
                  dataKey="revenue"
                  stroke="var(--color-revenue)"
                  strokeWidth={2}
                  fill={`url(#${id}-revenue)`}
                />
                <Area
                  type="linear"
                  dataKey="expenses"
                  stroke="var(--color-expenses)"
                  strokeWidth={2}
                  strokeDasharray="5 3"
                  fill={`url(#${id}-expenses)`}
                />
                <ChartLegend content={<ChartLegendContent />} />
              </AreaChart>
            </ChartContainer>
            <ChartDataTable
              caption="Receitas vs Despesas"
              columns={["Data", "Receitas", "Despesas", "Saldo projetado"]}
              rows={combinedData.map((point) => ({
                key: point.date,
                values: [
                  formatChartFullDate(point.date),
                  formatMoney(point.revenue),
                  formatMoney(point.expenses),
                  formatMoney(moneyDifference(point.revenue, point.expenses)),
                ],
              }))}
            />
          </>
        ) : (
          <div className="flex h-75 items-center justify-center text-muted-foreground">
            Nenhum dado disponível para o período
          </div>
        )}
      </CardContent>
    </Card>
  );
}
