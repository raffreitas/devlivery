import { DateRangeFilter } from "@/shared/components/date-range-filter";

interface DashboardFiltersContentProps {
  inputStartDate: string;
  inputEndDate: string;
  onStartDateChange: (value: string) => void;
  onEndDateChange: (value: string) => void;
  onResetDates: () => void;
}

export function DashboardFiltersContent({
  inputStartDate,
  inputEndDate,
  onStartDateChange,
  onEndDateChange,
  onResetDates,
}: DashboardFiltersContentProps) {
  return (
    <DateRangeFilter
      startDate={inputStartDate}
      endDate={inputEndDate}
      onStartChange={onStartDateChange}
      onEndChange={onEndDateChange}
      onReset={onResetDates}
    />
  );
}
