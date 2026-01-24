import { CardHeader } from "@/shared/components/ui/card";
import {
  ORDER_STATUS_STYLES,
  PAYMENT_METHOD_STYLES,
} from "@/shared/constants/ui-styles";
import { getPaymentOptionLabel } from "../constants/payment-methods";
import type { Order } from "../types";

interface OrderCardHeaderProps {
  order: Order;
}

export function OrderCardHeader({ order }: OrderCardHeaderProps) {
  const statusStyle = ORDER_STATUS_STYLES[order.status];
  return (
    <CardHeader className="p-0 flex flex-col gap-3 sm:flex-row sm:justify-between sm:items-start">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1 flex-wrap">
          <h3 className="text-base sm:text-lg font-semibold text-foreground truncate">
            {order.customerName}
          </h3>
          {order.payments.map((payment) => {
            const paymentStyle = PAYMENT_METHOD_STYLES[payment.method];
            const PaymentIcon = paymentStyle.icon;
            return (
              <div
                key={payment.id}
                className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium shrink-0 ${paymentStyle.className}`}
              >
                <PaymentIcon className="w-3 h-3" />
                <span className="hidden sm:inline">
                  {getPaymentOptionLabel(payment.method)}
                </span>
              </div>
            );
          })}
        </div>
        {order.customerPhone && (
          <p className="text-xs sm:text-sm text-muted-foreground truncate">
            {order.customerPhone}
          </p>
        )}
        <p className="text-xs sm:text-sm text-muted-foreground line-clamp-2">
          {order.deliveryAddress}
        </p>
      </div>
      <div
        className={`px-2 sm:px-3 py-1 rounded-full text-xs sm:text-sm font-medium shrink-0 self-start ${statusStyle.className}`}
      >
        {statusStyle.label}
      </div>
    </CardHeader>
  );
}
