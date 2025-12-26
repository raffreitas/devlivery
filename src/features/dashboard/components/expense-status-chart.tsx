import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";
import type { ExpensesByStatus } from "../types";

interface ExpenseStatusChartProps {
  data: ExpensesByStatus[];
}

const STATUS_LABELS: Record<string, string> = {
  Paid: "Pago",
  Pending: "Pendente",
  Overdue: "Vencido",
  DueToday: "Vence Hoje",
  Cancelled: "Cancelado",
};

const STATUS_COLORS: Record<string, string> = {
  Paid: "#22c55e", // green-500
  Pending: "#3b82f6", // blue-500
  Overdue: "#ef4444", // red-500
  DueToday: "#f59e0b", // amber-500
  Cancelled: "#6b7280", // gray-500
};

export function ExpenseStatusChart({ data }: ExpenseStatusChartProps) {
  const chartData = data.filter((d) => d.count > 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Status das Despesas</CardTitle>
      </CardHeader>
      <CardContent className="pl-2">
        <div className="h-[300px] w-full">
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={chartData}
                layout="vertical"
                margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" horizontal={false} />
                <XAxis type="number" stroke="#888888" fontSize={12} />
                <YAxis
                  type="category"
                  dataKey="status"
                  stroke="#888888"
                  fontSize={12}
                  tickFormatter={(value) => STATUS_LABELS[value] || value}
                  width={100}
                />
                <Tooltip
                  cursor={{ fill: "transparent" }}
                  content={({ active, payload }) => {
                    if (active && payload && payload.length) {
                      const data = payload[0].payload as ExpensesByStatus;
                      return (
                        <div className="rounded-lg border bg-background p-2 shadow-sm">
                          <div className="space-y-1">
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Status
                              </span>
                              <span className="font-bold text-muted-foreground">
                                {STATUS_LABELS[data.status] || data.status}
                              </span>
                            </div>
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Quantidade
                              </span>
                              <span className="font-bold">{data.count}</span>
                            </div>
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Total
                              </span>
                              <span className="font-bold">
                                {formatMoney(data.total)}
                              </span>
                            </div>
                          </div>
                        </div>
                      );
                    }
                    return null;
                  }}
                />
                <Bar dataKey="count" radius={[0, 4, 4, 0]}>
                  {chartData.map((entry) => (
                    <Cell
                      key={entry.status}
                      fill={STATUS_COLORS[entry.status] || "#8884d8"}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="flex h-full items-center justify-center text-muted-foreground">
              Nenhuma despesa no período
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
