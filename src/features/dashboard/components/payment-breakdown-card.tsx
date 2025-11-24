import { PAYMENT_METHODS } from "@/features/orders/constants/payment-methods";
import { Card } from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { PAYMENT_METHOD_STYLES } from "@/shared/constants/ui-styles";
import type { PaymentBreakdown } from "../types";

interface PaymentBreakdownCardProps {
  paymentBreakdown: PaymentBreakdown;
}

export function PaymentBreakdownCard({
  paymentBreakdown,
}: PaymentBreakdownCardProps) {
  const { breakdown, total } = paymentBreakdown;

  const entries = Object.keys(breakdown) as Array<keyof typeof breakdown>;

  const getPercentage = (value: number) => {
    if (total === 0) return 0;
    return ((value / total) * 100).toFixed(1);
  };

  return (
    <Card className="p-4 sm:p-6">
      <h4 className="scroll-m-20 text-xl font-semibold tracking-tight">
        Resumo de Vendas
      </h4>

      <div className="space-y-3">
        {entries.map((method) => {
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
        })}
      </div>

      <Separator />

      <div className="flex justify-between items-center">
        <span className="text-sm font-medium text-secondary-foreground">
          Total
        </span>
        <span className="text-lg sm:text-xl font-bold text-primary">
          R$ {total.toFixed(2)}
        </span>
      </div>
    </Card>
  );
}
