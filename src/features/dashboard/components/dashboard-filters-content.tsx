import type { DateRange } from "react-day-picker";
import { DateRangePicker } from "@/shared/components/ui/date-range-picker";
import { Label } from "@/shared/components/ui/label";

interface DashboardFiltersContentProps {
  period?: DateRange;
  onDateChange: (date: DateRange | undefined) => void;
}

export function DashboardFiltersContent({
  period,
  onDateChange,
}: DashboardFiltersContentProps) {
  return (
    <>
      <Label>Período</Label>
      <DateRangePicker date={period} onDateChange={onDateChange} />
    </>
  );
}
