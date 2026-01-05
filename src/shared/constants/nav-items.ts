import {
  LayoutDashboard,
  Package,
  Receipt,
  ShoppingCart,
  Wallet,
} from "lucide-react";
import type { ComponentType, SVGProps } from "react";

export type NavItem = {
  label: string;
  icon: ComponentType<SVGProps<SVGSVGElement>>;
  path: string;
};

export const navItems: NavItem[] = [
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
];
