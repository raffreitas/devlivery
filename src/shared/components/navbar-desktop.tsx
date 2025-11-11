import { LayoutDashboard, Package, ShoppingCart } from "lucide-react";
import { Link, useLocation } from "react-router-dom";

export function NavbarDesktop() {
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
      <Link
        to="/"
        className={`${
          isActive("/")
            ? "border-orange-500 text-gray-900"
            : "border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700"
        } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2`}
      >
        <LayoutDashboard className="w-4 h-4" />
        Dashboard
      </Link>
      <Link
        to="/products"
        className={`${
          isActive("/products")
            ? "border-orange-500 text-gray-900"
            : "border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700"
        } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2`}
      >
        <Package className="w-4 h-4" />
        Produtos
      </Link>
      <Link
        to="/orders"
        className={`${
          isActive("/orders")
            ? "border-orange-500 text-gray-900"
            : "border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700"
        } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2`}
      >
        <ShoppingCart className="w-4 h-4" />
        Pedidos
      </Link>
    </div>
  );
}
