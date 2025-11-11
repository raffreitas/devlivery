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
        className="sm:hidden inline-flex items-center justify-center p-2 rounded-md text-gray-500 hover:text-gray-900 hover:bg-gray-100 focus:outline-none"
      >
        {isOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
      </button>

      {/* Mobile menu panel */}
      {isOpen && (
        <div
          className="sm:hidden absolute top-full left-0 right-0 z-50 bg-white shadow-lg border-t border-gray-200"
          id="mobile-menu"
        >
          <div className="px-2 pt-2 pb-3 space-y-1">
            <Link
              to="/"
              onClick={onClose}
              className={`block px-3 py-2 rounded-md text-base font-medium ${
                isActive("/")
                  ? "bg-orange-50 text-orange-700"
                  : "text-gray-700 hover:bg-gray-50 hover:text-gray-900"
              }`}
            >
              Dashboard
            </Link>
            <Link
              to="/products"
              onClick={onClose}
              className={`block px-3 py-2 rounded-md text-base font-medium ${
                isActive("/products")
                  ? "bg-orange-50 text-orange-700"
                  : "text-gray-700 hover:bg-gray-50 hover:text-gray-900"
              }`}
            >
              Produtos
            </Link>
            <Link
              to="/orders"
              onClick={onClose}
              className={`block px-3 py-2 rounded-md text-base font-medium ${
                isActive("/orders")
                  ? "bg-orange-50 text-orange-700"
                  : "text-gray-700 hover:bg-gray-50 hover:text-gray-900"
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
