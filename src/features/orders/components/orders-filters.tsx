import { Filter } from "lucide-react";
import type { DateRange } from "react-day-picker";
import { Button } from "@/shared/components/button";
import { DateRangePicker } from "@/shared/components/ui/date-range-picker";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/shared/components/ui/select";
import { getOrderStatusOptionLabel } from "../constants/order-status";
import { getPaymentOptionLabel } from "../constants/payment-methods";
import type { Order } from "../types";

interface OrdersFiltersProps {
  statusFilter: Order["status"] | "all";
  paymentFilter: Order["paymentMethod"] | "all";
  statusOptions: Array<Order["status"] | "all">;
  paymentOptions: Array<Order["paymentMethod"] | "all">;
  period?: DateRange;
  onDateChange: (date: DateRange | undefined) => void;
  onStatusChange: (status: Order["status"] | "all") => void;
  onPaymentChange: (payment: Order["paymentMethod"] | "all") => void;
  onOpenFilters: () => void;
}

export function OrdersFilters({
  statusFilter,
  paymentFilter,
  statusOptions,
  paymentOptions,
  period,
  onStatusChange,
  onPaymentChange,
  onDateChange,
  onOpenFilters,
}: OrdersFiltersProps) {
  // Contagem de filtros ativos
  const activeFiltersCount =
    (statusFilter !== "all" ? 1 : 0) + (paymentFilter !== "all" ? 1 : 0);

  return (
    <>
      {/* Mobile: Botão Filtros */}
      <div className="sm:hidden">
        <Button
          variant="secondary"
          onClick={onOpenFilters}
          size="md"
          className="w-full"
        >
          <Filter className="w-4 h-4" />
          <span className="ml-2">Filtros</span>
          {activeFiltersCount > 0 && (
            <span className="ml-2 px-1.5 py-0.5 bg-orange-500 text-white text-xs font-bold rounded-full min-w-5 text-center">
              {activeFiltersCount}
            </span>
          )}
        </Button>
      </div>

      {/* Desktop: Inline Filters */}
      <div className="hidden sm:block bg-white rounded-lg shadow-md p-3 sm:p-4">
        <div className="flex flex-row flex-wrap items-end gap-4">
          <div className="flex-1 flex flex-col gap-2 min-w-[200px]">
            <Label>Status</Label>
            <Select onValueChange={onStatusChange}>
              <SelectTrigger className="w-full cursor-pointer">
                <span>
                  {statusFilter === "all"
                    ? "Todos"
                    : getOrderStatusOptionLabel(statusFilter)}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all" onSelect={() => onStatusChange("all")}>
                  Todos
                </SelectItem>
                {statusOptions.map((option) => (
                  <SelectItem
                    key={option}
                    value={option}
                    onSelect={() => onStatusChange(option)}
                    className="cursor-pointer"
                  >
                    {getOrderStatusOptionLabel(option)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex-1 flex flex-col gap-2 min-w-[200px]">
            <Label>Pagamento</Label>
            <Select onValueChange={onPaymentChange}>
              <SelectTrigger className="w-full  cursor-pointer">
                <span>
                  {paymentFilter === "all"
                    ? "Todos"
                    : getPaymentOptionLabel(paymentFilter)}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all" onSelect={() => onPaymentChange("all")}>
                  Todos
                </SelectItem>
                {paymentOptions.map((option) => (
                  <SelectItem
                    key={option}
                    value={option}
                    onSelect={() => onPaymentChange(option)}
                    className="cursor-pointer"
                  >
                    {getPaymentOptionLabel(option)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="w-auto flex flex-col gap-2">
            <Label>Período</Label>
            <DateRangePicker date={period} onDateChange={onDateChange} />
          </div>
        </div>
      </div>
    </>
  );
}
