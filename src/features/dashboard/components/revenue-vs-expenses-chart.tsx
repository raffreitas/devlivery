import { useMemo } from "react";
import {
  Area,
  AreaChart,
  CartesianGrid,
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

interface RevenueVsExpensesChartProps {
  revenueData: { date: string; total: number }[];
  expensesData: { date: string; total: number }[];
}

export function RevenueVsExpensesChart({
  revenueData,
  expensesData,
}: RevenueVsExpensesChartProps) {
  // Combina os dados de receitas e despesas por data
  const combinedData = useMemo(() => {
    const dataMap = new Map<
      string,
      { date: string; revenue: number; expenses: number }
    >();

    revenueData.forEach((item) => {
      if (!dataMap.has(item.date)) {
        dataMap.set(item.date, { date: item.date, revenue: 0, expenses: 0 });
      }
      const entry = dataMap.get(item.date);
      if (entry) {
        entry.revenue = item.total;
      }
    });

    expensesData.forEach((item) => {
      if (!dataMap.has(item.date)) {
        dataMap.set(item.date, { date: item.date, revenue: 0, expenses: 0 });
      }
      const entry = dataMap.get(item.date);
      if (entry) {
        entry.expenses = item.total;
      }
    });

    return Array.from(dataMap.values()).sort((a, b) => {
      const [dayA, monthA] = a.date.split("/").map(Number);
      const [dayB, monthB] = b.date.split("/").map(Number);
      return monthA - monthB || dayA - dayB;
    });
  }, [revenueData, expensesData]);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Receitas vs Despesas</CardTitle>
      </CardHeader>
      <CardContent className="pl-2">
        <div className="h-[300px] w-full">
          {combinedData.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={combinedData}>
                <defs>
                  <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#22c55e" stopOpacity={0.8} />
                    <stop offset="95%" stopColor="#22c55e" stopOpacity={0.1} />
                  </linearGradient>
                  <linearGradient
                    id="colorExpenses"
                    x1="0"
                    y1="0"
                    x2="0"
                    y2="1"
                  >
                    <stop offset="5%" stopColor="#ef4444" stopOpacity={0.8} />
                    <stop offset="95%" stopColor="#ef4444" stopOpacity={0.1} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis
                  dataKey="date"
                  stroke="#888888"
                  fontSize={12}
                  tickLine={false}
                  axisLine={false}
                />
                <YAxis
                  stroke="#888888"
                  fontSize={12}
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={(value) => `R$${value}`}
                />
                <Tooltip
                  cursor={{ stroke: "#888888", strokeWidth: 1 }}
                  content={({ active, payload }) => {
                    if (active && payload && payload.length) {
                      const data = payload[0].payload as {
                        date: string;
                        revenue: number;
                        expenses: number;
                      };
                      return (
                        <div className="rounded-lg border bg-background p-2 shadow-sm">
                          <div className="space-y-2">
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Data
                              </span>
                              <span className="font-bold text-muted-foreground">
                                {data.date}
                              </span>
                            </div>
                            <div className="flex items-center justify-between gap-4">
                              <div className="flex flex-col">
                                <span className="text-[0.70rem] uppercase text-muted-foreground">
                                  Receitas
                                </span>
                                <span className="font-bold text-green-600">
                                  {formatMoney(data.revenue)}
                                </span>
                              </div>
                              <div className="flex flex-col">
                                <span className="text-[0.70rem] uppercase text-muted-foreground">
                                  Despesas
                                </span>
                                <span className="font-bold text-red-600">
                                  {formatMoney(data.expenses)}
                                </span>
                              </div>
                            </div>
                            <div className="flex flex-col border-t pt-2">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Lucro Líquido
                              </span>
                              <span
                                className={`font-bold ${
                                  data.revenue - data.expenses >= 0
                                    ? "text-green-600"
                                    : "text-red-600"
                                }`}
                              >
                                {formatMoney(data.revenue - data.expenses)}
                              </span>
                            </div>
                          </div>
                        </div>
                      );
                    }
                    return null;
                  }}
                />
                <Area
                  type="monotone"
                  dataKey="revenue"
                  stroke="#22c55e"
                  fillOpacity={1}
                  fill="url(#colorRevenue)"
                  name="Receitas"
                />
                <Area
                  type="monotone"
                  dataKey="expenses"
                  stroke="#ef4444"
                  fillOpacity={1}
                  fill="url(#colorExpenses)"
                  name="Despesas"
                />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <div className="flex h-full items-center justify-center text-muted-foreground">
              Nenhum dado disponível para o período
            </div>
          )}
        </div>
        <div className="mt-4 flex justify-center gap-6">
          <div className="flex items-center gap-2">
            <div className="h-3 w-3 rounded-full bg-green-500" />
            <span className="text-sm text-muted-foreground">Receitas</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="h-3 w-3 rounded-full bg-red-500" />
            <span className="text-sm text-muted-foreground">Despesas</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
