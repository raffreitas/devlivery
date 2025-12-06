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
  const paymentStyle = PAYMENT_METHOD_STYLES[order.paymentMethod];
  const PaymentIcon = paymentStyle.icon;
  const statusStyle = ORDER_STATUS_STYLES[order.status];

  return (
    <CardHeader className="p-0 flex flex-col gap-3 sm:flex-row sm:justify-between sm:items-start">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1 flex-wrap">
          <h3 className="text-base sm:text-lg font-semibold text-gray-900 truncate">
            {order.customerName}
          </h3>
          <span
            className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium border ${paymentStyle.bg} ${paymentStyle.text} ${paymentStyle.border} shrink-0`}
          >
            <PaymentIcon className="w-3 h-3" />
            <span className="hidden sm:inline">
              {getPaymentOptionLabel(order.paymentMethod)}
            </span>
          </span>
        </div>
        {order.customerPhone && (
          <p className="text-xs sm:text-sm text-secondary-foreground truncate">
            {order.customerPhone}
          </p>
        )}
        <p className="text-xs sm:text-sm text-secondary-foreground line-clamp-2">
          {order.deliveryAddress}
        </p>
      </div>
      <span
        className={`px-2 sm:px-3 py-1 rounded-full text-xs sm:text-sm font-medium ${statusStyle.badgeBg} ${statusStyle.badgeText} shrink-0 self-start`}
      >
        {statusStyle.label}
      </span>
    </CardHeader>
  );
}
