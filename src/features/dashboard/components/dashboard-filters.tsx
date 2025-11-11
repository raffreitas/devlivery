import { Filter } from "lucide-react";
import { Button } from "@/shared/components/button";
import { DashboardFiltersContent } from "./dashboard-filters-content";

interface DashboardFiltersProps {
  inputStartDate: string;
  inputEndDate: string;
  onStartDateChange: (value: string) => void;
  onEndDateChange: (value: string) => void;
  onResetDates: () => void;
  onOpenFilters: () => void;
}

export function DashboardFilters({
  inputStartDate,
  inputEndDate,
  onStartDateChange,
  onEndDateChange,
  onResetDates,
  onOpenFilters,
}: DashboardFiltersProps) {
  return (
    <>
      {/* Mobile: Botão para abrir Bottom Sheet */}
      <div className="lg:hidden">
        <Button
          variant="secondary"
          onClick={onOpenFilters}
          className="w-full sm:w-auto"
        >
          <Filter className="w-4 h-4" />
          Filtros
        </Button>
      </div>

      {/* Desktop: Filtros inline */}
      <div className="hidden lg:block">
        <DashboardFiltersContent
          inputStartDate={inputStartDate}
          inputEndDate={inputEndDate}
          onStartDateChange={onStartDateChange}
          onEndDateChange={onEndDateChange}
          onResetDates={onResetDates}
        />
      </div>
    </>
  );
}
