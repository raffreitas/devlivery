import {
  Banknote,
  ChartNoAxesCombined,
  ClipboardList,
  Package,
  ReceiptText,
  Wallet,
} from "lucide-react";
import { Link } from "react-router-dom";

const steps = [
  {
    title: "Abra o caixa",
    text: "Registre o valor de abertura para começar o atendimento.",
  },
  {
    title: "Acompanhe os pedidos",
    text: "Registre os itens, o pagamento e atualize o status durante o preparo.",
  },
  {
    title: "Confira e feche",
    text: "Revise as movimentações e registre o fechamento do caixa.",
  },
];

const resources = [
  {
    icon: ClipboardList,
    title: "Pedidos e pagamentos",
    text: "Organize os itens de cada pedido, acompanhe o status e registre pagamentos e troco.",
  },
  {
    icon: Package,
    title: "Seu catálogo em dia",
    text: "Cadastre produtos, ajuste preços e controle o que está disponível para vender.",
  },
  {
    icon: Wallet,
    title: "Caixa do início ao fim",
    text: "Registre abertura, aportes e fechamento, com movimentações vinculadas ao operador.",
  },
  {
    icon: ReceiptText,
    title: "Despesas organizadas",
    text: "Acompanhe categorias, vencimentos e pagamentos das despesas da operação.",
  },
];

export function LandingSections() {
  return (
    <>
      <section
        id="como-funciona"
        className="lp-workflow lp-container"
        aria-labelledby="workflow-title"
      >
        <div className="lp-section-intro">
          <h2 id="workflow-title">
            Do primeiro pedido
            <br />
            ao fim do expediente.
          </h2>
          <p>
            Uma rotina conectada para acompanhar o que acontece no seu delivery.
          </p>
        </div>
        <ol className="lp-steps">
          {steps.map((step, index) => (
            <li key={step.title}>
              <span className="lp-step-number">{index + 1}</span>
              <h3>{step.title}</h3>
              <p>{step.text}</p>
            </li>
          ))}
        </ol>
      </section>
      <section
        id="recursos"
        className="lp-resources"
        aria-labelledby="resources-title"
      >
        <div className="lp-container">
          <div className="lp-section-intro">
            <h2 id="resources-title">
              Cada parte da operação.
              <br />
              No mesmo sistema.
            </h2>
            <p>
              Do que você vende ao que você paga, encontre as informações para
              tocar o dia a dia.
            </p>
          </div>
          <div className="lp-resource-grid">
            {resources.map(({ icon: Icon, title, text }) => (
              <article className="lp-resource" key={title}>
                <Icon size={25} strokeWidth={1.6} aria-hidden="true" />
                <h3>{title}</h3>
                <p>{text}</p>
              </article>
            ))}
          </div>
          <article className="lp-indicators">
            <div className="lp-indicator-visual" aria-hidden="true">
              <ChartNoAxesCombined size={38} strokeWidth={1.5} />
              <div className="lp-bars">
                <span />
                <span />
                <span />
                <span />
                <span />
                <span />
                <span />
              </div>
              <span className="lp-chart-caption">Visualização ilustrativa</span>
            </div>
            <div>
              <h3>Enxergue o movimento do seu negócio.</h3>
              <p>
                Consulte indicadores de vendas, pedidos, produtos e despesas no
                dashboard. Uma visão consolidada para acompanhar sua operação.
              </p>
              <span className="lp-inline-note">
                <Banknote size={18} aria-hidden="true" /> Informações que ajudam
                na rotina.
              </span>
            </div>
          </article>
        </div>
      </section>
      <section
        className="lp-closing lp-container"
        aria-labelledby="closing-title"
      >
        <div>
          <h2 id="closing-title">
            Mais clareza para
            <br />
            cuidar do seu delivery.
          </h2>
          <p>Conheça o que o Devlivery reúne para a sua operação.</p>
        </div>
        <div className="lp-closing-actions">
          <a className="lp-button lp-button-primary" href="#recursos">
            Explorar os recursos
          </a>
          <p>
            Já usa o Devlivery? <Link to="/login">Entrar</Link>
          </p>
        </div>
      </section>
    </>
  );
}
