import {
  LayoutDashboard,
  Package,
  Receipt,
  ShoppingCart,
  Wallet,
} from "lucide-react";
import { Link, useLocation } from "react-router-dom";
import { cn } from "@/shared/lib/utils";

export function NavbarBottom() {
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  const navItems = [
    {
      label: "Dashboard",
      icon: LayoutDashboard,
      path: "/",
    },
    {
      label: "Pedidos",
      icon: ShoppingCart,
      path: "/orders",
    },
    {
      label: "Caixa",
      icon: Wallet,
      path: "/cash",
    },
    {
      label: "Produtos",
      icon: Package,
      path: "/products",
    },
    {
      label: "Despesas",
      icon: Receipt,
      path: "/expenses",
    },
    // Future Menu Management Placeholder
    // {
    //   label: "Cardápio",
    //   icon: Menu,
    //   path: "/menu",
    // },
  ];

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 bg-card border-t border-border pb-safe sm:hidden">
      <div className="flex justify-around items-center h-16">
        {navItems.map((item) => {
          const active = isActive(item.path);
          return (
            <Link
              key={item.path}
              to={item.path}
              className={cn(
                "flex flex-col items-center justify-center w-full h-full space-y-1 transition-colors duration-200",
                active
                  ? "text-primary font-medium"
                  : "text-muted-foreground hover:text-foreground",
              )}
            >
              <item.icon
                className={cn(
                  "w-6 h-6",
                  active ? "stroke-[2.5px]" : "stroke-2",
                )}
              />
              <span className="text-[10px]">{item.label}</span>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
