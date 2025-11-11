import { LoadingSpinner } from "@/shared/components/loading-spinner";
import { DashboardFilters } from "./dashboard-filters";

interface DashboardHeaderProps {
  isFetching: boolean;
  inputStartDate: string;
  inputEndDate: string;
  onStartChange: (value: string) => void;
  onEndChange: (value: string) => void;
  onReset: () => void;
  onOpenFilters: () => void;
}

export function DashboardHeader({
  isFetching,
  inputStartDate,
  inputEndDate,
  onStartChange,
  onEndChange,
  onReset,
  onOpenFilters,
}: DashboardHeaderProps) {
  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <div className="flex items-center gap-3">
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">
            Dashboard
          </h1>
          {isFetching && (
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <LoadingSpinner size="sm" className="text-orange-500" />
              <span className="hidden sm:inline">Atualizando...</span>
            </div>
          )}
        </div>
        <p className="text-sm sm:text-base text-gray-600 mt-1">
          Visão geral dos pedidos
        </p>
      </div>

      <DashboardFilters
        inputStartDate={inputStartDate}
        inputEndDate={inputEndDate}
        onStartDateChange={onStartChange}
        onEndDateChange={onEndChange}
        onResetDates={onReset}
        onOpenFilters={onOpenFilters}
      />
    </div>
  );
}
