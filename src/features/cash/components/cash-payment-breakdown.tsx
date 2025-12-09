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
    if (totalRevenue === 0) return 0;
    return (value / totalRevenue) * 100;
  };

  if (paymentBreakdown.length === 0) {
    return (
      <Card className="h-full">
        <CardHeader>
          <CardTitle className="text-lg">Formas de Pagamento</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-gray-500 text-center py-8">
            Nenhuma venda registrada neste período
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="h-full">
      <CardHeader>
        <CardTitle className="text-lg">Formas de Pagamento</CardTitle>
        <p className="text-sm text-gray-500 mt-1">
          Distribuição das vendas por método de pagamento.
        </p>
      </CardHeader>

      <CardContent className="space-y-4">
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
            <div key={item.method} className="space-y-2">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className={`p-1.5 rounded-lg ${style.bg}`}>
                    <Icon className={`w-4 h-4 ${style.text}`} />
                  </div>
                  <span className="text-sm font-medium text-gray-900">
                    {methodLabel}
                  </span>
                </div>
                <div className="text-right">
                  <div className="text-base font-bold text-gray-900">
                    R$ {item.amount?.toFixed(2)}
                  </div>
                  <div className="text-xs text-gray-500">
                    {item.count} {item.count === 1 ? "pedido" : "pedidos"}
                  </div>
                </div>
              </div>

              {/* Progress bar */}
              <div className="w-full bg-gray-100 rounded-full h-2 overflow-hidden">
                <div
                  className={`h-full rounded-full transition-all duration-300 ${style.bg.replace("-50", "-400")}`}
                  style={{ width: `${percentage}%` }}
                />
              </div>

              <div className="text-xs text-gray-500 text-right">
                {percentage.toFixed(1)}% do faturamento
              </div>
            </div>
          );
        })}
      </CardContent>
    </Card>
  );
}
