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
  onDuePeriodChange: (period: DateRange | undefined) => void;
  onCategoryChange: (categoryId: string | undefined) => void;
  onStatusChange: (status: ExpenseStatus | undefined) => void;
}

const statusLabels: Record<ExpenseStatus, string> = {
  [ExpenseStatus.PAID]: "Pago",
  [ExpenseStatus.PENDING]: "Pendente",
  [ExpenseStatus.OVERDUE]: "Vencido",
  [ExpenseStatus.DUE_TODAY]: "Vence Hoje",
  [ExpenseStatus.CANCELLED]: "Cancelado",
};

export function ExpenseFiltersComponent({
  period,
  categoryId,
  status,
  onDuePeriodChange,
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
      <div className="flex flex-col sm:flex-row items-end gap-2 sm:gap-4 pb-2 sm:pb-1 px-1">
        <div className="w-full sm:flex-1 sm:min-w-50 flex flex-col gap-1.5">
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

        <div className="w-full sm:flex-1 sm:min-w-50 flex flex-col gap-1.5">
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

        <div className="w-full sm:w-auto sm:min-w-60 flex flex-col gap-1.5">
          <Label className="text-xs font-medium text-muted-foreground">
            Período de Vencimento
          </Label>
          <DateRangePicker date={period} onDateChange={onDuePeriodChange} />
        </div>
      </div>
    </div>
  );
}
