import { Button } from "./button";
import { Input } from "./input";

interface DateRangeFilterProps {
  inputStart: string;
  inputEnd: string;
  onStartChange: (value: string) => void;
  onEndChange: (value: string) => void;
  onApply: () => void;
  onReset: () => void;
  isInvalid: boolean;
}

export function DateRangeFilter({
  inputStart,
  inputEnd,
  onStartChange,
  onEndChange,
  onApply,
  onReset,
  isInvalid,
}: DateRangeFilterProps) {
  return (
    <div className="flex items-end space-x-2">
      <Input
        id="startDate"
        label="Início"
        type="date"
        value={inputStart}
        onChange={(e) => onStartChange(e.target.value)}
        className="w-40"
      />
      <span className="text-sm text-gray-500 pb-2">até</span>
      <Input
        id="endDate"
        label="Fim"
        type="date"
        value={inputEnd}
        onChange={(e) => onEndChange(e.target.value)}
        className="w-40"
      />
      <div className="flex items-center space-x-2">
        <Button variant="secondary" size="md" onClick={onReset} type="button">
          Hoje
        </Button>
        <Button onClick={onApply} size="md" type="button" disabled={isInvalid}>
          Aplicar
        </Button>
      </div>
    </div>
  );
}
