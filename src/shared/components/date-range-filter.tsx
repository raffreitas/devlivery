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
    <div className="flex items-end space-x-2">
      <Input
        id="startDate"
        label="Início"
        type="date"
        value={startDate}
        onChange={(e) => onStartChange(e.target.value)}
        className="w-40"
      />
      <span className="text-sm text-gray-500 pb-2">até</span>
      <Input
        id="endDate"
        label="Fim"
        type="date"
        value={endDate}
        onChange={(e) => onEndChange(e.target.value)}
        className="w-40"
      />
      <Button variant="secondary" size="md" onClick={onReset} type="button">
        Hoje
      </Button>
    </div>
  );
}
