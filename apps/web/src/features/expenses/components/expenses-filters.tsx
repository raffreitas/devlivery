import { Filter } from "lucide-react";
import type { DateRange } from "react-day-picker";
import { DateRangePicker } from "@/shared/components/date-range-picker";
import { Button } from "@/shared/components/ui/button";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { useExpenseCategories } from "../hooks/use-expenses";
import { ExpenseStatus } from "../types";

interface ExpensesFiltersProps {
  period?: DateRange;
  categoryId?: string;
  status?: string;
  onDuePeriodChange: (period: DateRange | undefined) => void;
  onCategoryChange: (categoryId: string | undefined) => void;
  onStatusChange: (status: ExpenseStatus | undefined) => void;
  onOpenFilters: () => void;
}

const statusLabels: Record<ExpenseStatus, string> = {
  [ExpenseStatus.PAID]: "Pago",
  [ExpenseStatus.PENDING]: "Pendente",
  [ExpenseStatus.OVERDUE]: "Vencido",
  [ExpenseStatus.DUE_TODAY]: "Vence Hoje",
  [ExpenseStatus.CANCELLED]: "Cancelado",
};

export function ExpensesFilters({
  period,
  categoryId,
  status,
  onDuePeriodChange,
  onCategoryChange,
  onStatusChange,
  onOpenFilters,
}: ExpensesFiltersProps) {
  const { data: categories } = useExpenseCategories();

  const activeFiltersCount =
    (categoryId ? 1 : 0) + (status ? 1 : 0) + (period ? 1 : 0);

  return (
    <>
      <div className="sm:hidden">
        <Button variant="secondary" onClick={onOpenFilters} className="w-full">
          <Filter data-icon="inline-start" />
          <span>Filtros</span>
          {activeFiltersCount > 0 && (
            <span className="ml-2 px-1.5 py-0.5 bg-orange-500 text-white text-xs font-bold rounded-full min-w-5 text-center">
              {activeFiltersCount}
            </span>
          )}
        </Button>
      </div>

      <div className="hidden sm:block bg-card rounded-lg border border-border shadow-sm p-4">
        <div className="flex flex-col sm:flex-row items-end gap-2 sm:gap-4 pb-2 sm:pb-1 px-1">
          <div className="w-full sm:flex-1 sm:min-w-50 flex flex-col gap-1.5">
            <Label className="text-xs font-medium text-muted-foreground">
              Categoria
            </Label>
            <Select
              value={categoryId ?? "all"}
              onValueChange={(value) =>
                onCategoryChange(
                  value === "all" ? undefined : (value ?? undefined),
                )
              }
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="Todas">
                  {categoryId
                    ? categories?.find((cat) => cat.id === categoryId)?.name
                    : "Todas"}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectItem value="all">Todas</SelectItem>
                  {categories
                    ?.filter((cat) => cat.isActive)
                    .map((cat) => (
                      <SelectItem key={cat.id} value={cat.id}>
                        {cat.name}
                      </SelectItem>
                    ))}
                </SelectGroup>
              </SelectContent>
            </Select>
          </div>

          <div className="w-full sm:flex-1 sm:min-w-50 flex flex-col gap-1.5">
            <Label className="text-xs font-medium text-muted-foreground">
              Status
            </Label>
            <Select
              value={status ?? "all"}
              onValueChange={(value) =>
                onStatusChange(
                  value === "all" ? undefined : (value as ExpenseStatus),
                )
              }
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="Todos">
                  {status ? statusLabels[status as ExpenseStatus] : "Todos"}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectItem value="all">Todos</SelectItem>
                  {Object.entries(statusLabels).map(([value, label]) => (
                    <SelectItem key={value} value={value}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectGroup>
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
    </>
  );
}
