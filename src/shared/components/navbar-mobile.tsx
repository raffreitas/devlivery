import { Menu, X } from "lucide-react";
import { Link, useLocation } from "react-router-dom";

interface NavbarMobileProps {
  isOpen: boolean;
  onToggle: () => void;
  onClose: () => void;
}

export function NavbarMobile({ isOpen, onToggle, onClose }: NavbarMobileProps) {
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <>
      {/* Mobile menu button */}
      <button
        type="button"
        aria-controls="mobile-menu"
        aria-expanded={isOpen}
        onClick={onToggle}
        className="sm:hidden inline-flex items-center justify-center p-2 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent/10 focus:outline-none"
      >
        {isOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
      </button>

      {/* Mobile menu panel */}
      {isOpen && (
        <div
          className="sm:hidden absolute top-full left-0 right-0 z-50 bg-card shadow-lg border-t border-border"
          id="mobile-menu"
        >
          <div className="px-2 pt-2 pb-3 space-y-1">
            <Link
              to="/"
              onClick={onClose}
              className={`block px-3 py-2 rounded-md text-base font-medium ${
                isActive("/")
                  ? "bg-accent text-accent-foreground"
                  : "text-foreground hover:bg-accent/10 hover:text-foreground"
              }`}
            >
              Dashboard
            </Link>
            <Link
              to="/products"
              onClick={onClose}
              className={`block px-3 py-2 rounded-md text-base font-medium ${
                isActive("/products")
                  ? "bg-accent text-accent-foreground"
                  : "text-foreground hover:bg-accent/10 hover:text-foreground"
              }`}
            >
              Produtos
            </Link>
            <Link
              to="/orders"
              onClick={onClose}
              className={`block px-3 py-2 rounded-md text-base font-medium ${
                isActive("/orders")
                  ? "bg-accent text-accent-foreground"
                  : "text-foreground hover:bg-accent/10 hover:text-foreground"
              }`}
            >
              Pedidos
            </Link>
          </div>
        </div>
      )}
    </>
  );
}
