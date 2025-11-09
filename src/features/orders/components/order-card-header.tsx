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
    <div className="flex justify-between items-start mb-4">
      <div className="flex-1">
        <div className="flex items-center gap-2 mb-1">
          <h3 className="text-lg font-semibold text-gray-900">
            {order.customerName}
          </h3>
          <span
            className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium border ${paymentStyle.bg} ${paymentStyle.text} ${paymentStyle.border}`}
          >
            <PaymentIcon className="w-3 h-3" />
            {getPaymentOptionLabel(order.paymentMethod)}
          </span>
        </div>
        {order.customerPhone && (
          <p className="text-sm text-gray-600">{order.customerPhone}</p>
        )}
        <p className="text-sm text-gray-600">{order.deliveryAddress}</p>
      </div>
      <span
        className={`px-3 py-1 rounded-full text-sm font-medium ${statusStyle.badgeBg} ${statusStyle.badgeText}`}
      >
        {statusStyle.label}
      </span>
    </div>
  );
}
