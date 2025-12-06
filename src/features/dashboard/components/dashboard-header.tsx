import type { DateRange } from "react-day-picker";
import { Spinner } from "@/shared/components/ui/spinner";
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

      {isFetching && (
        <div className="fixed top-4 right-4 z-50 bg-white/80 backdrop-blur px-3 py-1.5 rounded-full shadow-sm border border-gray-100 flex items-center gap-2 text-sm text-muted-foreground">
          <Spinner className="w-4 h-4" />
          <span>Atualizando...</span>
        </div>
      )}

      <DashboardFilters
        period={period}
        onDateChange={onDateChange}
        onOpenFilters={onOpenFilters}
      />
    </div>
  );
}
