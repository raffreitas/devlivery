import { PAYMENT_METHODS } from "@/features/orders/constants/payment-methods";
import { PAYMENT_METHOD_STYLES } from "@/shared/constants/ui-styles";
import type { PaymentBreakdown } from "../types";

interface PaymentBreakdownCardProps {
  paymentBreakdown: PaymentBreakdown;
}

export function PaymentBreakdownCard({
  paymentBreakdown,
}: PaymentBreakdownCardProps) {
  const { breakdown, total } = paymentBreakdown;

  const getPercentage = (value: number) => {
    if (total === 0) return 0;
    return ((value / total) * 100).toFixed(1);
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-4 sm:p-6">
      <div className="flex items-center gap-2 mb-4">
        <h2 className="text-lg sm:text-xl font-semibold text-gray-900">
          Resumo de Vendas
        </h2>
      </div>

      <div className="space-y-2 sm:space-y-3 mb-4 sm:mb-6">
        {(Object.keys(breakdown) as Array<keyof typeof breakdown>).map(
          (method) => {
            const style = PAYMENT_METHOD_STYLES[method];
            const Icon = style.icon;
            const value = breakdown[method];
            const percentage = getPercentage(value);

            return (
              <div
                key={method}
                className={`flex items-center justify-between p-2 sm:p-3 rounded-lg ${style.bg} ${style.text} ${style.border}`}
              >
                <div className="flex items-center gap-2">
                  <Icon className="w-4 h-4" />
                  <span className="text-xs sm:text-sm font-medium">
                    {PAYMENT_METHODS[method]}
                  </span>
                </div>
                <div className="flex items-center gap-2 sm:gap-3">
                  <span className="text-xs font-medium opacity-75">
                    {percentage}%
                  </span>
                  <span className="text-sm sm:text-base font-bold">
                    R$ {value.toFixed(2)}
                  </span>
                </div>
              </div>
            );
          },
        )}
      </div>

      <div className="mt-4 sm:mt-6 pt-4 sm:pt-6 border-t border-gray-200">
        <div className="flex justify-between items-center">
          <span className="text-sm font-medium text-gray-700">Total</span>
          <span className="text-lg sm:text-xl font-bold text-orange-600">
            R$ {total.toFixed(2)}
          </span>
        </div>
      </div>
    </div>
  );
}
