import type { Order } from "../types";

interface OrderPrintProps {
  order: Order;
}

export function OrderPrint({ order }: OrderPrintProps) {
  return (
    <div className="print-receipt">
      <div className="text-center mb-4">
        <h1 className="text-xl font-bold">DEVLIVERY</h1>
        <p className="text-sm">Pedido #{order.id.slice(0, 8).toUpperCase()}</p>
      </div>

      <div className="border-t-2 border-b-2 border-dashed border-gray-800 py-2 mb-3">
        <p className="font-semibold">Cliente: {order.customerName}</p>
        <p className="text-sm">Tel: {order.customerPhone}</p>
        <p className="text-sm">End: {order.deliveryAddress}</p>
      </div>

      <div className="mb-3">
        <p className="font-semibold mb-2">ITENS:</p>
        {order.items.map((item) => (
          <div key={`${item.product.id}-${item.quantity}`} className="mb-2">
            <div className="flex justify-between">
              <span>
                {item.quantity}x {item.product.name}
              </span>
              <span>R$ {(item.product.price * item.quantity).toFixed(2)}</span>
            </div>
            {item.notes && (
              <p className="text-xs text-gray-600 ml-4">Obs: {item.notes}</p>
            )}
          </div>
        ))}
      </div>

      <div className="border-t-2 border-dashed border-gray-800 pt-2 mb-3">
        {(() => {
          const subtotal = order.items.reduce(
            (s, it) => s + it.product.price * it.quantity,
            0,
          );
          return (
            <div>
              <div className="flex justify-between">
                <span>SUBTOTAL:</span>
                <span>R$ {subtotal.toFixed(2)}</span>
              </div>

              <div className="flex justify-between">
                <span>TAXA DE ENTREGA:</span>
                <span>R$ {order.deliveryFee.toFixed(2)}</span>
              </div>

              <div className="flex justify-between font-bold text-lg mt-2">
                <span>TOTAL:</span>
                <span>R$ {order.total.toFixed(2)}</span>
              </div>
            </div>
          );
        })()}
      </div>

      <div className="text-center text-xs border-t border-gray-800 pt-2">
        <p>Data: {new Date(order.createdAt).toLocaleString("pt-BR")}</p>
        <p className="mt-2">Obrigado pela preferência!</p>
      </div>
    </div>
  );
}
