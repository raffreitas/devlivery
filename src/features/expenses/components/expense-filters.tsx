import type { DateRange } from "react-day-picker";
import { DateRangePicker } from "@/shared/components/ui/date-range-picker";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { useExpenseCategories } from "../hooks/use-expenses";
import { ExpenseStatus } from "../types";

interface ExpenseFiltersProps {
  period?: DateRange;
  categoryId?: string;
  status?: string;
  onPeriodChange: (period: DateRange | undefined) => void;
  onCategoryChange: (categoryId: string | undefined) => void;
  onStatusChange: (status: ExpenseStatus | undefined) => void;
}

const statusLabels: Record<ExpenseStatus, string> = {
  [ExpenseStatus.PAID]: "Pago",
  [ExpenseStatus.PENDING]: "Pendente",
  [ExpenseStatus.OVERDUE]: "Vencido",
  [ExpenseStatus.SCHEDULED]: "Agendado",
};

export function ExpenseFiltersComponent({
  period,
  categoryId,
  status,
  onPeriodChange,
  onCategoryChange,
  onStatusChange,
}: ExpenseFiltersProps) {
  const { data: categories } = useExpenseCategories();

  const handleCategoryChange = (value: string) => {
    onCategoryChange(value === "all" ? undefined : value);
  };

  const handleStatusChange = (value: ExpenseStatus | "all") => {
    onStatusChange(value === "all" ? undefined : value);
  };

  return (
    <div className="hidden sm:block bg-card rounded-lg border border-border shadow-sm p-4">
      <div className="flex items-end gap-4 pb-2 sm:pb-1 px-1">
        <div className="flex-1 min-w-[200px] flex flex-col gap-1.5">
          <Label className="text-xs font-medium text-muted-foreground">
            Categoria
          </Label>
          <Select
            value={categoryId ?? "all"}
            onValueChange={handleCategoryChange}
          >
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Todas" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todas</SelectItem>
              {categories
                ?.filter((cat) => cat.isActive)
                .map((cat) => (
                  <SelectItem key={cat.id} value={cat.id}>
                    {cat.name}
                  </SelectItem>
                ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex-1 min-w-[200px] flex flex-col gap-1.5">
          <Label className="text-xs font-medium text-muted-foreground">
            Status
          </Label>
          <Select value={status ?? "all"} onValueChange={handleStatusChange}>
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Todos" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos</SelectItem>
              {Object.entries(statusLabels).map(([value, label]) => (
                <SelectItem key={value} value={value}>
                  {label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex flex-col gap-1.5 min-w-60">
          <Label className="text-xs font-medium text-muted-foreground">
            Período
          </Label>
          <DateRangePicker date={period} onDateChange={onPeriodChange} />
        </div>
      </div>
    </div>
  );
}
