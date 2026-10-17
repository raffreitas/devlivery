import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/shared/components/ui/chart";
import { formatMoney } from "@/shared/utils/formatters";
import {
  formatChartDate,
  formatChartFullDate,
  formatChartMoney,
  type TimeSeriesPoint,
} from "../utils/chart-data";
import { ChartDataTable } from "./chart-data-table";
import { ChartTooltipValue } from "./chart-tooltip-value";

interface TimeSeriesBarChartProps {
  data: TimeSeriesPoint[];
  title: string;
  label: string;
  color: string;
  emptyMessage: string;
  height?: "sales" | "default";
}

export function TimeSeriesBarChart({
  data,
  title,
  label,
  color,
  emptyMessage,
  height = "default",
}: TimeSeriesBarChartProps) {
  return (
    <Card className={height === "sales" ? "sm:col-span-2" : undefined}>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>
        {data.length > 0 ? (
          <>
            <ChartContainer
              config={{ total: { label, color } }}
              className={height === "sales" ? "h-87.5 w-full" : "h-75 w-full"}
            >
              <BarChart accessibilityLayer title={title} data={data}>
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
                  cursor={false}
                  content={
                    <ChartTooltipContent
                      labelFormatter={(value) =>
                        formatChartFullDate(String(value))
                      }
                      formatter={(value) => (
                        <ChartTooltipValue
                          label={label}
                          value={formatMoney(Number(value))}
                        />
                      )}
                    />
                  }
                />
                <Bar
                  dataKey="total"
                  name={label}
                  fill="var(--color-total)"
                  radius={[4, 4, 0, 0]}
                />
              </BarChart>
            </ChartContainer>
            <ChartDataTable
              caption={title}
              columns={["Data", label]}
              rows={data.map((point) => ({
                key: point.date,
                values: [
                  formatChartFullDate(point.date),
                  formatMoney(point.total),
                ],
              }))}
            />
          </>
        ) : (
          <div className="flex h-75 items-center justify-center text-muted-foreground">
            {emptyMessage}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
