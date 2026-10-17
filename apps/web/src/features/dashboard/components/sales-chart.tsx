import type { TimeSeriesPoint } from "../utils/chart-data";
import { TimeSeriesBarChart } from "./time-series-bar-chart";

export function SalesChart({ data }: { data: TimeSeriesPoint[] }) {
  return (
    <TimeSeriesBarChart
      data={data}
      title="Vendas no Período"
      label="Vendas"
      color="var(--chart-sales)"
      emptyMessage="Nenhum dado disponível para o período"
      height="sales"
    />
  );
}
