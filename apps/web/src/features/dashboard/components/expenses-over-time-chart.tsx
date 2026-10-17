import type { ExpenseTimeSeries } from "../types";
import { TimeSeriesBarChart } from "./time-series-bar-chart";

export function ExpensesOverTimeChart({ data }: { data: ExpenseTimeSeries[] }) {
  return (
    <TimeSeriesBarChart
      data={data}
      title="Despesas no Período"
      label="Despesas"
      color="var(--chart-expense)"
      emptyMessage="Nenhuma despesa no período"
    />
  );
}
