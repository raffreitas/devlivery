import { Check, ClipboardList, ReceiptText, Wallet } from "lucide-react";

const orders = [
  {
    number: "1043",
    status: "Pendente",
    tone: "pending",
    item: "1 × Burger da casa",
    detail: "1 × Batata frita",
    total: "R$ 42,00",
  },
  {
    number: "1042",
    status: "Em preparo",
    tone: "preparing",
    item: "1 × Pizza margherita",
    detail: "1 × Refrigerante 1 L",
    total: "R$ 68,00",
  },
  {
    number: "1041",
    status: "Pronto",
    tone: "ready",
    item: "2 × Cheeseburger",
    detail: "2 × Suco de laranja",
    total: "R$ 76,00",
  },
];

export function ProductPreview() {
  return (
    <section
      id="previa"
      className="lp-preview"
      aria-label="Prévia ilustrativa do sistema"
    >
      <div className="lp-preview-top">
        <span className="lp-preview-brand">
          <ReceiptText size={19} aria-hidden="true" /> Devlivery
        </span>
        <span className="lp-demo-label">Dados ilustrativos</span>
      </div>
      <div className="lp-preview-body">
        <div className="lp-preview-heading">
          <div>
            <p className="lp-preview-context">Sua operação, em um só lugar</p>
            <h2>Pedidos do dia</h2>
          </div>
          <ClipboardList size={25} aria-hidden="true" />
        </div>
        <div className="lp-order-list">
          {orders.map((order) => (
            <article className="lp-order" key={order.number}>
              <div className="lp-order-top">
                <strong>#{order.number}</strong>
                <span className={`lp-status lp-status-${order.tone}`}>
                  {order.status}
                </span>
              </div>
              <p>
                {order.item}
                <span>{order.detail}</span>
              </p>
              <div className="lp-order-bottom">
                <span>Total do pedido</span>
                <strong>{order.total}</strong>
              </div>
            </article>
          ))}
        </div>
        <div className="lp-cash-preview">
          <div className="lp-cash-icon">
            <Wallet size={21} aria-hidden="true" />
          </div>
          <div>
            <span>Saldo do caixa</span>
            <strong>R$ 386,00</strong>
          </div>
          <span className="lp-cash-open">
            <Check size={14} aria-hidden="true" /> Aberto
          </span>
        </div>
        <p className="lp-preview-note">
          Exemplo de pedidos e caixa. Valores fictícios.
        </p>
      </div>
    </section>
  );
}
