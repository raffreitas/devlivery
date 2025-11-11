import {
  type AutocompleteOption,
  AutocompleteSelect,
} from "@/shared/components/autocomplete-select";
import { DateRangeFilter } from "@/shared/components/date-range-filter";
import type { Order } from "../types";

interface OrdersFiltersContentProps {
  statusFilter: Order["status"] | "all";
  paymentFilter: Order["paymentMethod"] | "all";
  statusOptions: AutocompleteOption<Order["status"] | "all">[];
  paymentOptions: AutocompleteOption<Order["paymentMethod"] | "all">[];
  inputStartDate: string;
  inputEndDate: string;
  onStatusChange: (status: Order["status"] | "all") => void;
  onPaymentChange: (payment: Order["paymentMethod"] | "all") => void;
  onStartDateChange: (date: string) => void;
  onEndDateChange: (date: string) => void;
  onResetDates: () => void;
}

export function OrdersFiltersContent({
  statusFilter,
  paymentFilter,
  statusOptions,
  paymentOptions,
  inputStartDate,
  inputEndDate,
  onStatusChange,
  onPaymentChange,
  onStartDateChange,
  onEndDateChange,
  onResetDates,
}: OrdersFiltersContentProps) {
  return (
    <div className="flex flex-col gap-4">
      <div>
        <AutocompleteSelect
          label="Status"
          value={statusFilter}
          options={statusOptions}
          onChange={(value) => onStatusChange(value ?? "all")}
          placeholder="Selecione um status"
          autocomplete={false}
        />
      </div>

      <div>
        <AutocompleteSelect
          label="Pagamento"
          value={paymentFilter}
          options={paymentOptions}
          onChange={(value) => onPaymentChange(value ?? "all")}
          placeholder="Selecione método"
          autocomplete={false}
        />
      </div>

      <div>
        <DateRangeFilter
          startDate={inputStartDate}
          endDate={inputEndDate}
          onStartChange={onStartDateChange}
          onEndChange={onEndDateChange}
          onReset={onResetDates}
        />
      </div>
    </div>
  );
}
