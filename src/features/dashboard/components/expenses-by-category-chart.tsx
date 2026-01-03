import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";
import type { ExpensesByCategory } from "../types";

interface ExpensesByCategoryChartProps {
  data: ExpensesByCategory[];
}

const COLORS = [
  "#3b82f6", // blue-500
  "#22c55e", // green-500
  "#f59e0b", // amber-500
  "#ef4444", // red-500
  "#a855f7", // purple-500
  "#06b6d4", // cyan-500
  "#f97316", // orange-500
  "#8b5cf6", // violet-500
];

export function ExpensesByCategoryChart({
  data,
}: ExpensesByCategoryChartProps) {
  const chartData = data.filter((d) => d.total > 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Despesas por Categoria</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-75 w-full">
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={chartData as unknown as Array<Record<string, unknown>>}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={80}
                  paddingAngle={5}
                  dataKey="total"
                >
                  {chartData.map((entry, index) => (
                    <Cell
                      key={`cell-${entry.category}`}
                      fill={COLORS[index % COLORS.length]}
                    />
                  ))}
                </Pie>
                <Tooltip
                  content={({ active, payload }) => {
                    if (active && payload && payload.length) {
                      const data = payload[0].payload as ExpensesByCategory;
                      return (
                        <div className="rounded-lg border bg-background p-2 shadow-sm">
                          <div className="space-y-1">
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Categoria
                              </span>
                              <span className="font-bold text-muted-foreground">
                                {data.category}
                              </span>
                            </div>
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Valor
                              </span>
                              <span className="font-bold">
                                {formatMoney(data.total)}
                              </span>
                            </div>
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Percentual
                              </span>
                              <span className="font-bold">
                                {data.percentage.toFixed(1)}%
                              </span>
                            </div>
                          </div>
                        </div>
                      );
                    }
                    return null;
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <div className="flex h-full items-center justify-center text-muted-foreground">
              Nenhuma despesa no período
            </div>
          )}
        </div>
        <div className="mt-4 flex flex-wrap justify-center gap-4">
          {chartData.slice(0, 6).map((entry, index) => (
            <div key={entry.category} className="flex items-center gap-2">
              <div
                className="h-3 w-3 rounded-full"
                style={{
                  backgroundColor: COLORS[index % COLORS.length],
                }}
              />
              <span className="text-sm text-muted-foreground">
                {entry.category}
              </span>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
