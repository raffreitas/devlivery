import type { DateRange } from "react-day-picker";
import { LoadingOverlay } from "@/shared/components/loading";
import { DashboardFilters } from "./dashboard-filters";

interface DashboardHeaderProps {
  isFetching: boolean;
  period?: DateRange;
  onDateChange: (date: DateRange | undefined) => void;
  onOpenFilters: () => void;
}

export function DashboardHeader({
  isFetching,
  period,
  onDateChange,
  onOpenFilters,
}: DashboardHeaderProps) {
  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:justify-between sm:items-center">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground mt-1">
          Visão geral dos pedidos e métricas
        </p>
      </div>

      <LoadingOverlay isFetching={isFetching} position="top-bar" />

      <DashboardFilters
        period={period}
        onDateChange={onDateChange}
        onOpenFilters={onOpenFilters}
      />
    </div>
  );
}
