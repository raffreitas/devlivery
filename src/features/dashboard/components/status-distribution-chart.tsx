import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";

interface StatusDistributionChartProps {
  data: { status: string; count: number }[];
}

const COLORS = {
  Pending: "#f59e0b", // amber-500
  Preparing: "#3b82f6", // blue-500
  Ready: "#22c55e", // green-500
  Delivered: "#a855f7", // purple-500
  Canceled: "#ef4444", // red-500
};

const STATUS_LABELS: Record<string, string> = {
  Pending: "Pendente",
  Preparing: "Preparando",
  Ready: "Pronto",
  Delivered: "Entregue",
  Canceled: "Cancelado",
};

export function StatusDistributionChart({
  data,
}: StatusDistributionChartProps) {
  const chartData = data.filter((d) => d.count > 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Status dos Pedidos</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-75 w-full">
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={chartData}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={80}
                  paddingAngle={5}
                  dataKey="count"
                >
                  {chartData.map((entry) => (
                    <Cell
                      key={`cell-${entry.status}`}
                      fill={
                        COLORS[entry.status as keyof typeof COLORS] || "#8884d8"
                      }
                    />
                  ))}
                </Pie>
                <Tooltip
                  content={({ active, payload }) => {
                    if (active && payload && payload.length) {
                      const data = payload[0].payload;
                      return (
                        <div className="rounded-lg border bg-background p-2 shadow-sm">
                          <div className="flex flex-col">
                            <span className="text-[0.70rem] uppercase text-muted-foreground">
                              {STATUS_LABELS[data.status] || data.status}
                            </span>
                            <span className="font-bold">
                              {data.count} pedidos
                            </span>
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
              Nenhum dado disponível
            </div>
          )}
        </div>
        <div className="mt-4 flex flex-wrap justify-center gap-4">
          {chartData.map((entry) => (
            <div key={entry.status} className="flex items-center gap-2">
              <div
                className="h-3 w-3 rounded-full"
                style={{
                  backgroundColor:
                    COLORS[entry.status as keyof typeof COLORS] || "#8884d8",
                }}
              />
              <span className="text-sm text-muted-foreground">
                {STATUS_LABELS[entry.status] || entry.status}
              </span>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
