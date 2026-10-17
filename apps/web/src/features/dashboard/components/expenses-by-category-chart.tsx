import { Pie, PieChart } from "recharts";
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
import type { ExpensesByCategory } from "../types";
import { categoryChartColor, categoryChartKey } from "../utils/chart-data";
import { ChartDataTable } from "./chart-data-table";
import { ChartTooltipValue } from "./chart-tooltip-value";

export function ExpensesByCategoryChart({
  data,
}: {
  data: ExpensesByCategory[];
}) {
  const chartData = data
    .filter((point) => point.total > 0)
    .map((point) => ({
      ...point,
      series: categoryChartKey(point.category),
      fill: `var(--color-${categoryChartKey(point.category)})`,
    }));
  const chartConfig: ChartConfig = Object.fromEntries(
    chartData.map((point) => [
      point.series,
      { label: point.category, color: categoryChartColor(point.category) },
    ]),
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Despesas por Categoria</CardTitle>
      </CardHeader>
      <CardContent>
        {chartData.length > 0 ? (
          <>
            <ChartContainer config={chartConfig} className="h-75 w-full">
              <PieChart accessibilityLayer title="Despesas por Categoria">
                <Pie
                  data={chartData}
                  dataKey="total"
                  nameKey="series"
                  innerRadius={60}
                  outerRadius={80}
                  paddingAngle={3}
                />
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      hideLabel
                      nameKey="series"
                      formatter={(value, _name, item) => {
                        const point =
                          item.payload as (typeof chartData)[number];
                        return (
                          <ChartTooltipValue
                            label={point.category}
                            value={formatMoney(Number(value))}
                            detail={`${point.percentage.toLocaleString("pt-BR", { maximumFractionDigits: 1 })}% do total`}
                          />
                        );
                      }}
                    />
                  }
                />
                <ChartLegend
                  content={
                    <ChartLegendContent
                      nameKey="series"
                      className="max-h-24 flex-wrap overflow-y-auto"
                    />
                  }
                />
              </PieChart>
            </ChartContainer>
            <ChartDataTable
              caption="Despesas por Categoria"
              columns={["Categoria", "Valor", "Percentual"]}
              rows={chartData.map((point) => ({
                key: point.series,
                values: [
                  point.category,
                  formatMoney(point.total),
                  `${point.percentage.toLocaleString("pt-BR", { maximumFractionDigits: 1 })}%`,
                ],
              }))}
            />
          </>
        ) : (
          <div className="flex h-75 items-center justify-center text-muted-foreground">
            Nenhuma despesa no período
          </div>
        )}
      </CardContent>
    </Card>
  );
}
