import { Card } from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { ORDER_STATUS_STYLES } from "@/shared/constants/ui-styles";
import { formatMoney } from "@/shared/utils/formatters";
import type { OrdersByStatus } from "../types";

interface StatsSidebarProps {
  ordersByStatus: OrdersByStatus;
  averageOrderValue: number;
}

export function StatsSidebar({
  ordersByStatus,
  averageOrderValue,
}: StatsSidebarProps) {
  const entries = Object.entries(ordersByStatus) as Array<
    [keyof typeof ORDER_STATUS_STYLES, number]
  >;
  return (
    <Card className="p-4 sm:p-6">
      <h4 className="scroll-m-20 text-xl font-semibold tracking-tight">
        Status dos Pedidos
      </h4>

      <div className="space-y-3">
        {entries.map(([status, count]) => {
          const style = ORDER_STATUS_STYLES[status];
          const Icon = style.icon;

          return (
            <div
              key={status}
              className={`flex items-center justify-between p-2 sm:p-3 rounded-lg ${style.bg} ${style.text}`}
            >
              <div className="flex items-center gap-2">
                <Icon className="w-4 h-4" />
                <span className="text-xs sm:text-sm font-medium">
                  {style.label}
                </span>
              </div>
              <span className="text-sm sm:text-base font-bold">{count}</span>
            </div>
          );
        })}
      </div>

      <Separator />

      <div className="flex justify-between items-center">
        <span className="text-sm font-medium text-secondary-foreground">
          Ticket Médio
        </span>
        <span className="text-lg sm:text-xl font-bold text-primary">
          {formatMoney(averageOrderValue)}
        </span>
      </div>
    </Card>
  );
}
