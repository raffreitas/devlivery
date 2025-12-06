import type { DateRange } from "react-day-picker";
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

interface OrdersFiltersContentProps {
  statusFilter: Order["status"] | "all";
  paymentFilter: Order["paymentMethod"] | "all";
  statusOptions: Array<Order["status"] | "all">;
  paymentOptions: Array<Order["paymentMethod"] | "all">;
  period?: DateRange;
  onDateChange: (date: DateRange | undefined) => void;
  onStatusChange: (status: Order["status"] | "all") => void;
  onPaymentChange: (payment: Order["paymentMethod"] | "all") => void;
}

export function OrdersFiltersContent({
  statusFilter,
  paymentFilter,
  statusOptions,
  paymentOptions,
  period,
  onDateChange,
  onStatusChange,
  onPaymentChange,
}: OrdersFiltersContentProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-2">
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

      <div className="flex flex-col gap-2">
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

      <div className="flex flex-col gap-2">
        <Label>Período</Label>
        <DateRangePicker date={period} onDateChange={onDateChange} />
      </div>
    </div>
  );
}
