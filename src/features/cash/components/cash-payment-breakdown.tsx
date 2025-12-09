import { PAYMENT_METHODS } from "@/features/orders/constants/payment-methods";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { PAYMENT_METHOD_STYLES } from "@/shared/constants/ui-styles";
import type { PaymentMethodTotal } from "../types";

interface CashPaymentBreakdownProps {
  paymentBreakdown: PaymentMethodTotal[];
  totalRevenue: number;
}

export function CashPaymentBreakdown({
  paymentBreakdown,
  totalRevenue,
}: CashPaymentBreakdownProps) {
  const getPercentage = (value: number) => {
    if (totalRevenue === 0) return "0.0";
    return ((value / totalRevenue) * 100).toFixed(1);
  };

  if (paymentBreakdown.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Vendas por Forma de Pagamento</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-gray-500 text-center py-4">
            Nenhuma venda registrada neste período
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Vendas por Forma de Pagamento</CardTitle>
      </CardHeader>

      <CardContent className="space-y-3">
        {paymentBreakdown.map((item) => {
          const style =
            PAYMENT_METHOD_STYLES[
              item.method as keyof typeof PAYMENT_METHOD_STYLES
            ] || PAYMENT_METHOD_STYLES.Cash;
          const Icon = style.icon;
          const percentage = getPercentage(item.amount);
          const methodLabel =
            PAYMENT_METHODS[item.method as keyof typeof PAYMENT_METHODS] ||
            item.method;

          return (
            <div
              key={item.method}
              className={`flex items-center justify-between p-2 sm:p-3 rounded-lg ${style.bg} ${style.text} ${style.border}`}
            >
              <div className="flex items-center gap-2">
                <Icon className="w-4 h-4" />
                <div className="flex flex-col">
                  <span className="text-xs sm:text-sm font-medium">
                    {methodLabel}
                  </span>
                  <span className="text-xs opacity-75">
                    {item.count} {item.count === 1 ? "pedido" : "pedidos"}
                  </span>
                </div>
              </div>
              <div className="flex items-center gap-2 sm:gap-3">
                <span className="text-xs font-medium opacity-75">
                  {percentage}%
                </span>
                <span className="text-sm sm:text-base font-bold">
                  R$ {item.amount?.toFixed(2)}
                </span>
              </div>
            </div>
          );
        })}
      </CardContent>
    </Card>
  );
}
