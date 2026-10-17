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
import { ORDER_STATUS_STYLES } from "@/shared/constants/ui-styles";
import { ChartDataTable } from "./chart-data-table";
import { ChartTooltipValue } from "./chart-tooltip-value";

export function StatusDistributionChart({
  data,
}: {
  data: { status: string; count: number }[];
}) {
  const statuses = new Map(Object.entries(ORDER_STATUS_STYLES));
  const chartConfig: ChartConfig = Object.fromEntries(
    [...statuses].map(([status, style]) => [
      status,
      { label: style.label, color: style.chartColor },
    ]),
  );
  const chartData = data
    .filter((point) => point.count > 0)
    .map((point) => ({
      ...point,
      fill: statuses.get(point.status)?.chartColor ?? "var(--chart-neutral)",
    }));
  return (
    <Card>
      <CardHeader>
        <CardTitle>Status dos Pedidos</CardTitle>
      </CardHeader>
      <CardContent>
        {chartData.length > 0 ? (
          <>
            <ChartContainer config={chartConfig} className="h-75 w-full">
              <PieChart accessibilityLayer title="Status dos Pedidos">
                <Pie
                  data={chartData}
                  dataKey="count"
                  nameKey="status"
                  innerRadius={60}
                  outerRadius={80}
                  paddingAngle={3}
                />
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      hideLabel
                      nameKey="status"
                      formatter={(value, name) => (
                        <ChartTooltipValue
                          label={chartConfig[String(name)]?.label ?? name}
                          value={`${value} pedidos`}
                        />
                      )}
                    />
                  }
                />
                <ChartLegend
                  content={
                    <ChartLegendContent
                      nameKey="status"
                      className="flex-wrap"
                    />
                  }
                />
              </PieChart>
            </ChartContainer>
            <ChartDataTable
              caption="Status dos Pedidos"
              columns={["Status", "Quantidade"]}
              rows={chartData.map((point) => ({
                key: point.status,
                values: [
                  statuses.get(point.status)?.label ?? point.status,
                  point.count,
                ],
              }))}
            />
          </>
        ) : (
          <div className="flex h-75 items-center justify-center text-muted-foreground">
            Nenhum dado disponível
          </div>
        )}
      </CardContent>
    </Card>
  );
}
