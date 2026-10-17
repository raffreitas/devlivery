import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/shared/components/ui/chart";
import { formatMoney } from "@/shared/utils/formatters";
import type { ExpensesByStatus } from "../types";
import { ChartDataTable } from "./chart-data-table";
import { ChartTooltipValue } from "./chart-tooltip-value";

const statusConfig = {
  Paid: { label: "Pago", color: "var(--chart-revenue)" },
  Pending: { label: "Pendente", color: "var(--chart-info)" },
  Overdue: { label: "Vencido", color: "var(--chart-expense)" },
  DueToday: { label: "Vence Hoje", color: "var(--chart-warning)" },
  Cancelled: { label: "Cancelado", color: "var(--chart-neutral)" },
} satisfies ChartConfig;

export function ExpenseStatusChart({ data }: { data: ExpensesByStatus[] }) {
  const chartConfig = statusConfig;
  const statuses = new Map(Object.entries(statusConfig));
  const chartData = data
    .filter((point) => point.count > 0)
    .map((point) => ({
      ...point,
      fill: statuses.get(point.status)?.color ?? "var(--chart-neutral)",
    }));
  const statusLabel = (status: string) => statuses.get(status)?.label ?? status;
  return (
    <Card>
      <CardHeader>
        <CardTitle>Status das Despesas</CardTitle>
      </CardHeader>
      <CardContent>
        {chartData.length > 0 ? (
          <>
            <ChartContainer config={chartConfig} className="h-75 w-full">
              <BarChart
                accessibilityLayer
                title="Status das Despesas"
                data={chartData}
                layout="vertical"
                margin={{ right: 16 }}
              >
                <CartesianGrid horizontal={false} />
                <XAxis
                  type="number"
                  tick={{ fill: "var(--muted-foreground)" }}
                  allowDecimals={false}
                  tickLine={false}
                  axisLine={false}
                />
                <YAxis
                  type="category"
                  tick={{ fill: "var(--muted-foreground)" }}
                  dataKey="status"
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={statusLabel}
                  width={90}
                />
                <ChartTooltip
                  cursor={false}
                  content={
                    <ChartTooltipContent
                      hideLabel
                      nameKey="status"
                      formatter={(value, _name, item) => {
                        const point = item.payload as ExpensesByStatus;
                        return (
                          <ChartTooltipValue
                            label={statusLabel(point.status)}
                            value={`${value} despesas`}
                            detail={formatMoney(point.total)}
                          />
                        );
                      }}
                    />
                  }
                />
                <Bar dataKey="count" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ChartContainer>
            <ChartDataTable
              caption="Status das Despesas"
              columns={["Status", "Quantidade", "Total"]}
              rows={chartData.map((point) => ({
                key: point.status,
                values: [
                  statusLabel(point.status),
                  point.count,
                  formatMoney(point.total),
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
