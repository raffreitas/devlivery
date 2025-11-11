import { Filter } from "lucide-react";
import {
  type AutocompleteOption,
  AutocompleteSelect,
} from "@/shared/components/autocomplete-select";
import { Button } from "@/shared/components/button";
import { DateRangeFilter } from "@/shared/components/date-range-filter";
import type { Order } from "../types";

interface OrdersFiltersProps {
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
  onOpenFilters: () => void;
}

export function OrdersFilters({
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
          <div className="flex-1 min-w-[200px]">
            <AutocompleteSelect
              label="Status"
              value={statusFilter}
              options={statusOptions}
              onChange={(value) => onStatusChange(value ?? "all")}
              placeholder="Selecione um status"
              autocomplete={false}
            />
          </div>

          <div className="flex-1 min-w-[200px]">
            <AutocompleteSelect
              label="Pagamento"
              value={paymentFilter}
              options={paymentOptions}
              onChange={(value) => onPaymentChange(value ?? "all")}
              placeholder="Selecione método"
              autocomplete={false}
            />
          </div>

          <div className="w-auto">
            <DateRangeFilter
              startDate={inputStartDate}
              endDate={inputEndDate}
              onStartChange={onStartDateChange}
              onEndChange={onEndDateChange}
              onReset={onResetDates}
            />
          </div>
        </div>
      </div>
    </>
  );
}
