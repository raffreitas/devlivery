import { ORDER_STATUS_STYLES } from "@/shared/constants/ui-styles";
import type { OrdersByStatus } from "../types";

interface StatsSidebarProps {
  ordersByStatus: OrdersByStatus;
  averageOrderValue: number;
}

export function StatsSidebar({
  ordersByStatus,
  averageOrderValue,
}: StatsSidebarProps) {
  return (
    <div className="bg-white rounded-lg shadow-md p-4 sm:p-6">
      <h2 className="text-lg sm:text-xl font-semibold text-gray-900 mb-4">
        Status dos Pedidos
      </h2>
      <div className="space-y-3">
        {(
          Object.entries(ordersByStatus) as Array<
            [keyof typeof ORDER_STATUS_STYLES, number]
          >
        ).map(([status, count]) => {
          const style = ORDER_STATUS_STYLES[status];
          const Icon = style.icon;

          return (
            <div
              key={status}
              className={`flex justify-between items-center p-3 ${style.bg} rounded-lg`}
            >
              <div className="flex items-center gap-2">
                <Icon className={`w-4 h-4 ${style.text}`} />
                <span className="text-sm font-medium text-gray-700">
                  {style.label}
                </span>
              </div>
              <span className={`text-lg font-bold ${style.text}`}>{count}</span>
            </div>
          );
        })}
      </div>

      <div className="mt-6 pt-6 border-t border-gray-200">
        <div className="flex justify-between items-center">
          <span className="text-sm font-medium text-gray-700">
            Ticket Médio
          </span>
          <span className="text-lg sm:text-xl font-bold text-orange-600">
            R$ {averageOrderValue.toFixed(2)}
          </span>
        </div>
      </div>
    </div>
  );
}
