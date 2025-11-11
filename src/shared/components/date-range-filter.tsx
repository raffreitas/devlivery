import { Button } from "./button";
import { Input } from "./input";

interface DateRangeFilterProps {
  startDate: string;
  endDate: string;
  onStartChange: (value: string) => void;
  onEndChange: (value: string) => void;
  onReset: () => void;
}

export function DateRangeFilter({
  startDate,
  endDate,
  onStartChange,
  onEndChange,
  onReset,
}: DateRangeFilterProps) {
  return (
    <div className="flex items-end gap-2 flex-wrap sm:flex-nowrap">
      <Input
        id="startDate"
        label="Início"
        type="date"
        value={startDate}
        onChange={(e) => onStartChange(e.target.value)}
        className="w-full sm:w-32 lg:w-40"
      />
      <span className="hidden sm:inline text-sm text-gray-500 pb-3">até</span>
      <Input
        id="endDate"
        label="Fim"
        type="date"
        value={endDate}
        onChange={(e) => onEndChange(e.target.value)}
        className="w-full sm:w-32 lg:w-40"
      />
      <Button
        variant="secondary"
        onClick={onReset}
        type="button"
        className="w-full sm:w-auto h-[42px]"
      >
        Hoje
      </Button>
    </div>
  );
}
