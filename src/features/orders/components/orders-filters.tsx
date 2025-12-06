import { Filter } from "lucide-react";
import type { DateRange } from "react-day-picker";
import { Button } from "@/shared/components/ui/button";
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
        <Button variant="secondary" onClick={onOpenFilters} className="w-full">
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
      <div className="hidden sm:block bg-card rounded-lg border border-border shadow-sm p-4">
        <div className="flex items-end gap-4 pb-2 sm:pb-1 px-1">
          <div className="flex-1 min-w-[200px] flex flex-col gap-1.5">
            <Label className="text-xs font-medium text-muted-foreground">
              Status do Pedido
            </Label>
            <Select onValueChange={onStatusChange}>
              <SelectTrigger className="w-full">
                <span>
                  {statusFilter === "all"
                    ? "Todos os status"
                    : getOrderStatusOptionLabel(statusFilter)}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all" onSelect={() => onStatusChange("all")}>
                  Todos os status
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

          <div className="flex-1 min-w-[200px] flex flex-col gap-1.5">
            <Label className="text-xs font-medium text-muted-foreground">
              Forma de Pagamento
            </Label>
            <Select onValueChange={onPaymentChange}>
              <SelectTrigger className="w-full">
                <span>
                  {paymentFilter === "all"
                    ? "Todas as formas"
                    : getPaymentOptionLabel(paymentFilter)}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all" onSelect={() => onPaymentChange("all")}>
                  Todas as formas
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

          <div className="flex flex-col gap-1.5 min-w-60">
            <Label className="text-xs font-medium text-muted-foreground">
              Período
            </Label>
            <DateRangePicker date={period} onDateChange={onDateChange} />
          </div>
        </div>
      </div>
    </>
  );
}
