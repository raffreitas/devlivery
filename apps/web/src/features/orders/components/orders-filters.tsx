import { Filter } from "lucide-react";
import type { DateRange } from "react-day-picker";
import { DateRangePicker } from "@/shared/components/date-range-picker";
import { Button } from "@/shared/components/ui/button";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/shared/components/ui/select";
import { getOrderStatusOptionLabel } from "../constants/order-status";
import { getPaymentOptionLabel } from "../constants/payment-methods";
import type { OrderStatus, PaymentMethod } from "../types";

interface OrdersFiltersProps {
  statusFilter: OrderStatus | "all";
  paymentFilter: PaymentMethod | "all";
  statusOptions: Array<OrderStatus | "all">;
  paymentOptions: Array<PaymentMethod | "all">;
  period?: DateRange;
  onDateChange: (date: DateRange | undefined) => void;
  onStatusChange: (status: OrderStatus | "all") => void;
  onPaymentChange: (payment: PaymentMethod | "all") => void;
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
          <Filter data-icon="inline-start" />
          <span>Filtros</span>
          {activeFiltersCount > 0 && (
            <span className="ml-2 px-1.5 py-0.5 bg-orange-500 text-white text-xs font-bold rounded-full min-w-5 text-center">
              {activeFiltersCount}
            </span>
          )}
        </Button>
      </div>

      {/* Desktop: Inline Filters */}
      <div className="hidden sm:block bg-card rounded-lg border border-border shadow-sm p-4">
        <div className="flex flex-col sm:flex-row items-end gap-2 sm:gap-4 pb-2 sm:pb-1 px-1">
          <div className="w-full sm:flex-1 sm:min-w-50 flex flex-col gap-1.5">
            <Label className="text-xs font-medium text-muted-foreground">
              Status do Pedido
            </Label>
            <Select<OrderStatus | "all">
              value={statusFilter}
              onValueChange={(value) => {
                if (value !== null)
                  onStatusChange(value as OrderStatus | "all");
              }}
            >
              <SelectTrigger className="w-full">
                <span>
                  {statusFilter === "all"
                    ? "Todos os status"
                    : getOrderStatusOptionLabel(statusFilter)}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos os status</SelectItem>
                {statusOptions.map((option) => (
                  <SelectItem
                    key={option}
                    value={option}
                    className="cursor-pointer"
                  >
                    {getOrderStatusOptionLabel(option)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="w-full sm:flex-1 sm:min-w-50 flex flex-col gap-1.5">
            <Label className="text-xs font-medium text-muted-foreground">
              Forma de Pagamento
            </Label>
            <Select<PaymentMethod | "all">
              value={paymentFilter}
              onValueChange={(value) => {
                if (value !== null)
                  onPaymentChange(value as PaymentMethod | "all");
              }}
            >
              <SelectTrigger className="w-full">
                <span>
                  {paymentFilter === "all"
                    ? "Todas as formas"
                    : getPaymentOptionLabel(paymentFilter)}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas as formas</SelectItem>
                {paymentOptions.map((option) => (
                  <SelectItem
                    key={option}
                    value={option}
                    className="cursor-pointer"
                  >
                    {getPaymentOptionLabel(option)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="w-full sm:w-auto sm:min-w-60 flex flex-col gap-1.5">
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
