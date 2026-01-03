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
            ? "border-primary text-foreground"
            : "border-transparent text-muted-foreground hover:border-border hover:text-foreground"
        } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2 transition-colors`}
      >
        <LayoutDashboard className="w-4 h-4" />
        Dashboard
      </Link>
      <Link
        to="/products"
        className={`${
          isActive("/products")
            ? "border-primary text-foreground"
            : "border-transparent text-muted-foreground hover:border-border hover:text-foreground"
        } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2 transition-colors`}
      >
        <Package className="w-4 h-4" />
        Produtos
      </Link>
      <Link
        to="/orders"
        className={`${
          isActive("/orders")
            ? "border-primary text-foreground"
            : "border-transparent text-muted-foreground hover:border-border hover:text-foreground"
        } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2 transition-colors`}
      >
        <ShoppingCart className="w-4 h-4" />
        Pedidos
      </Link>
    </div>
  );
}
