import { useId } from "react";
import type { DateRange } from "react-day-picker";
import { DateRangePicker } from "@/shared/components/date-range-picker";
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
import { ExpenseSubcategoryFilter } from "./expense-subcategory-filter";

interface ExpensesFiltersContentProps {
  period?: DateRange;
  categoryId?: string;
  subcategoryId?: string;
  status?: string;
  onDuePeriodChange: (period: DateRange | undefined) => void;
  onCategoryChange: (categoryId: string | undefined) => void;
  onSubcategoryChange: (subcategoryId: string | undefined) => void;
  onStatusChange: (status: ExpenseStatus | undefined) => void;
}

const statusLabels: Record<ExpenseStatus, string> = {
  [ExpenseStatus.PAID]: "Pago",
  [ExpenseStatus.PENDING]: "Pendente",
  [ExpenseStatus.OVERDUE]: "Vencido",
  [ExpenseStatus.DUE_TODAY]: "Vence Hoje",
  [ExpenseStatus.CANCELLED]: "Cancelado",
};

export function ExpensesFiltersContent({
  period,
  categoryId,
  subcategoryId,
  status,
  onDuePeriodChange,
  onCategoryChange,
  onSubcategoryChange,
  onStatusChange,
}: ExpensesFiltersContentProps) {
  const id = useId();
  const { data: categories } = useExpenseCategories();

  const handleCategoryChange = (value: string) => {
    onCategoryChange(value === "all" ? undefined : (value ?? undefined));
  };

  const handleStatusChange = (value: ExpenseStatus | "all") => {
    onStatusChange(value === "all" ? undefined : (value ?? undefined));
  };

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-2">
        <Label htmlFor={`${id}-category`}>Categoria</Label>
        <Select
          value={categoryId ?? "all"}
          onValueChange={(value) => {
            if (value !== null) handleCategoryChange(value as string);
          }}
        >
          <SelectTrigger
            id={`${id}-category`}
            className="w-full cursor-pointer"
          >
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

      <ExpenseSubcategoryFilter
        categoryId={categoryId}
        subcategoryId={subcategoryId}
        onSubcategoryChange={onSubcategoryChange}
      />

      <div className="flex flex-col gap-2">
        <Label htmlFor={`${id}-status`}>Status</Label>
        <Select
          value={status ?? "all"}
          onValueChange={(value) => {
            if (value !== null)
              handleStatusChange(value as ExpenseStatus | "all");
          }}
        >
          <SelectTrigger id={`${id}-status`} className="w-full cursor-pointer">
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

      <div className="flex flex-col gap-2">
        <Label>Período de Vencimento</Label>
        <DateRangePicker date={period} onDateChange={onDuePeriodChange} />
      </div>
    </div>
  );
}
