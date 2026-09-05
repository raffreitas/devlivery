import { ReceiptText } from "lucide-react";
import { useEffect } from "react";
import { Link } from "react-router-dom";
import { LandingSections } from "../components/landing-sections";
import { ProductPreview } from "../components/product-preview";
import "../landing.css";

export function LandingPage() {
  useEffect(() => {
    const previousTitle = document.title;
    const existingDescription = document.querySelector<HTMLMetaElement>(
      'meta[name="description"]',
    );
    const previousDescription = existingDescription?.getAttribute("content");
    const description = existingDescription ?? document.createElement("meta");
    description.name = "description";
    description.content =
      "Organize pedidos, produtos, caixa e despesas do seu delivery com o Devlivery. Conheça os recursos para acompanhar sua operação em um só lugar.";
    if (!existingDescription) document.head.append(description);
    document.title = "Devlivery | Seu delivery organizado";
    return () => {
      document.title = previousTitle;
      if (!existingDescription) description.remove();
      else if (previousDescription === null)
        description.removeAttribute("content");
      else description.setAttribute("content", previousDescription ?? "");
    };
  }, []);

  return (
    <div className="lp">
      <a className="lp-skip-link" href="#conteudo">
        Pular para o conteúdo
      </a>
      <header className="lp-header lp-container">
        <a className="lp-brand" href="#conteudo" aria-label="Devlivery, início">
          <span className="lp-brand-icon">
            <ReceiptText size={23} aria-hidden="true" />
          </span>
          Devlivery<span className="lp-brand-dot">.</span>
        </a>
        <nav aria-label="Navegação principal">
          <a className="lp-nav-section" href="#como-funciona">
            Como funciona
          </a>
          <a className="lp-nav-section" href="#recursos">
            Recursos
          </a>
          <Link className="lp-button lp-button-outline" to="/login">
            Entrar
          </Link>
        </nav>
      </header>
      <main id="conteudo" tabIndex={-1}>
        <section className="lp-hero lp-container" aria-labelledby="hero-title">
          <div className="lp-hero-copy">
            <p className="lp-audience">Para quem faz o delivery acontecer</p>
            <h1 id="hero-title">
              Do pedido ao fechamento do caixa, seu delivery organizado.
            </h1>
            <p className="lp-hero-description">
              Pedidos, produtos, caixa e despesas em um só lugar. Cuide da
              rotina da sua pizzaria, hamburgueria ou lanchonete com uma visão
              clara da operação.
            </p>
            <a className="lp-button lp-button-primary" href="#previa">
              Conhecer o sistema
            </a>
            <p className="lp-hero-note">
              Seu atendimento começa aqui. Sua gestão também.
            </p>
          </div>
          <ProductPreview />
        </section>
        <LandingSections />
      </main>
      <footer className="lp-footer lp-container">
        <a className="lp-brand" href="#conteudo">
          Devlivery<span className="lp-brand-dot">.</span>
        </a>
        <p>
          © {new Date().getFullYear()} Devlivery. Todos os direitos reservados.
        </p>
        <Link to="/login">Acessar o sistema</Link>
      </footer>
    </div>
  );
}
