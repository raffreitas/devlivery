import { formatMoney } from "@/shared/utils/formatters";
import { getPaymentOptionLabel } from "../constants/payment-methods";
import type { Order } from "../types";

interface OrderPrintProps {
  order: Order;
}

export function OrderPrint({ order }: OrderPrintProps) {
  return (
    <div className="print-receipt">
      <div className="text-center mb-4">
        <h1 className="text-xl font-bold">DEVLIVERY</h1>
        <p>Pedido #{order.id.slice(0, 8).toUpperCase()}</p>
      </div>

      <div className="border-t-2 border-b-2 border-dashed border-foreground py-2 mb-3">
        <p className="font-semibold">Cliente: {order.customerName}</p>
        {order.customerPhone && <p>Tel: {order.customerPhone}</p>}
        <p>End: {order.deliveryAddress}</p>
        {order.payments.length > 0 && (
          <div>
            <p className="font-semibold">
              Pagamento{order.payments.length > 1 ? "s" : ""}:
            </p>
            {order.payments.map((p) => (
              <p key={p.id} className="ml-2 text-sm flex justify-between">
                <span>- {getPaymentOptionLabel(p.method)}</span>
                {p.method === "Cash" && <span>{formatMoney(p.amount)}</span>}
              </p>
            ))}
          </div>
        )}
      </div>

      <div className="mb-3">
        <p className="font-semibold mb-2">ITENS:</p>
        {order.items.map((item) => (
          <div key={`${item.product.id}-${item.quantity}`} className="mb-2">
            <div className="flex justify-between">
              <span>
                {item.quantity}x {item.product.name}
              </span>
              <span className="text-end whitespace-nowrap">
                {formatMoney(item.product.price * item.quantity)}
              </span>
            </div>
            {item.notes && <p className="text-sm ml-4">Obs: {item.notes}</p>}
          </div>
        ))}
      </div>

      {order.notes && (
        <div className="border-t-2 border-dashed border-foreground py-2 mb-3">
          <p className="font-semibold text-sm">OBSERVAÇÕES:</p>
          <p className="text-sm whitespace-pre-wrap">{order.notes}</p>
        </div>
      )}

      <div className="border-t-2 border-dashed border-foreground pt-2 mb-3">
        {(() => {
          const subtotal = order.items.reduce(
            (s, it) => s + it.product.price * it.quantity,
            0,
          );
          return (
            <div>
              <div className="flex justify-between">
                <span>SUBTOTAL:</span>
                <span>{formatMoney(subtotal)}</span>
              </div>

              <div className="flex justify-between">
                <span>TAXA DE ENTREGA:</span>
                <span>{formatMoney(order.deliveryFee)}</span>
              </div>

              <div className="flex justify-between font-bold text-lg mt-2">
                <span>TOTAL:</span>
                <span>{formatMoney(order.total)}</span>
              </div>
            </div>
          );
        })()}
      </div>

      <div className="text-center text-sm border-t border-foreground pt-2">
        <p>Data: {new Date(order.createdAt).toLocaleString("pt-BR")}</p>
        <p className="mt-2">Obrigado pela preferência!</p>
      </div>
    </div>
  );
}
