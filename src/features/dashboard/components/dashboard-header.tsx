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
        <div className="flex items-center gap-3">
          <h1 className="text-2xl sm:text-3xl font-bold">Dashboard</h1>
          {isFetching && (
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Spinner />
              <span className="hidden sm:inline">Atualizando...</span>
            </div>
          )}
        </div>
        <p className="text-sm sm:text-base text-muted-foreground mt-1">
          Visão geral dos pedidos
        </p>
      </div>

      <DashboardFilters
        period={period}
        onDateChange={onDateChange}
        onOpenFilters={onOpenFilters}
      />
    </div>
  );
}
