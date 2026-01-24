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
      <div className="hidden lg:flex items-center gap-3">
        <DashboardFiltersContent period={period} onDateChange={onDateChange} />
      </div>
    </>
  );
}
