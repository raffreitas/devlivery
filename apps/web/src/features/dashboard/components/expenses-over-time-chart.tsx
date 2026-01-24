import {
  Bar,
  BarChart,
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
import { formatMoney, parseLocalDate } from "@/shared/utils/formatters";
import type { ExpenseTimeSeries } from "../types";

interface ExpensesOverTimeChartProps {
  data: ExpenseTimeSeries[];
}

export function ExpensesOverTimeChart({ data }: ExpensesOverTimeChartProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Despesas no Período</CardTitle>
      </CardHeader>
      <CardContent className="pl-2">
        <div className="h-75 w-full">
          {data.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={data}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis
                  dataKey="date"
                  stroke="#888888"
                  fontSize={12}
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={(value) => {
                    const d = parseLocalDate(value);
                    if (d) {
                      return new Intl.DateTimeFormat("pt-BR", {
                        day: "2-digit",
                        month: "2-digit",
                      }).format(d);
                    }
                    return value;
                  }}
                />
                <YAxis
                  stroke="#888888"
                  fontSize={12}
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={(value) => `R$${value}`}
                />
                <Tooltip
                  cursor={{ fill: "transparent" }}
                  content={({ active, payload }) => {
                    if (active && payload && payload.length) {
                      return (
                        <div className="rounded-lg border bg-background p-2 shadow-sm">
                          <div className="grid grid-cols-2 gap-2">
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Data
                              </span>
                              <span className="font-bold text-muted-foreground">
                                {(() => {
                                  const v = payload[0].payload.date;
                                  const d = parseLocalDate(v);
                                  if (d) {
                                    return new Intl.DateTimeFormat("pt-BR", {
                                      day: "2-digit",
                                      month: "2-digit",
                                      year: "numeric",
                                    }).format(d);
                                  }
                                  return v;
                                })()}
                              </span>
                            </div>
                            <div className="flex flex-col">
                              <span className="text-[0.70rem] uppercase text-muted-foreground">
                                Total
                              </span>
                              <span className="font-bold">
                                {formatMoney(payload[0].value as number)}
                              </span>
                            </div>
                          </div>
                        </div>
                      );
                    }
                    return null;
                  }}
                />
                <Bar
                  dataKey="total"
                  fill="currentColor"
                  radius={[4, 4, 0, 0]}
                  className="fill-destructive"
                />
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
