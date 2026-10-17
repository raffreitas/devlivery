import { Filter } from "lucide-react";
import type { DateRange } from "react-day-picker";
import { Button } from "@/shared/components/ui/button";
import { DashboardFiltersContent } from "./dashboard-filters-content";

interface DashboardFiltersProps {
  period?: DateRange;
  onDateChange: (date: DateRange | undefined) => void;
  onOpenFilters: () => void;
}

export function DashboardFilters({
  period,
  onDateChange,
  onOpenFilters,
}: DashboardFiltersProps) {
  return (
    <>
      <div className="lg:hidden">
        <Button
          variant="secondary"
          onClick={onOpenFilters}
          className="w-full sm:w-auto"
        >
          <Filter data-icon="inline-start" />
          Filtros
        </Button>
      </div>

      <div className="hidden lg:flex items-center gap-3">
        <DashboardFiltersContent period={period} onDateChange={onDateChange} />
      </div>
    </>
  );
}
