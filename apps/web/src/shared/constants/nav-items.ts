import {
  ChartNoAxesCombined,
  Package,
  Receipt,
  ShoppingCart,
  Wallet,
} from "lucide-react";
import type { ComponentType, SVGProps } from "react";

export type NavItem = {
  id: string;
  label: string;
  mobileLabel: string;
  icon: ComponentType<SVGProps<SVGSVGElement>>;
  href: string;
  group: "Operação" | "Financeiro" | "Cadastros";
  mobile: "bar" | "more";
};

export const navGroups = ["Operação", "Financeiro", "Cadastros"] as const;
export const navItems: NavItem[] = [
  {
    id: "orders",
    label: "Pedidos",
    mobileLabel: "Pedidos",
    icon: ShoppingCart,
    href: "/orders",
    group: "Operação",
    mobile: "bar",
  },
  {
    id: "cash",
    label: "Caixa",
    mobileLabel: "Caixa",
    icon: Wallet,
    href: "/cash",
    group: "Operação",
    mobile: "bar",
  },
  {
    id: "summary",
    label: "Resumo",
    mobileLabel: "Resumo",
    icon: ChartNoAxesCombined,
    href: "/",
    group: "Operação",
    mobile: "bar",
  },
  {
    id: "expenses",
    label: "Despesas",
    mobileLabel: "Despesas",
    icon: Receipt,
    href: "/expenses",
    group: "Financeiro",
    mobile: "more",
  },
  {
    id: "products",
    label: "Produtos",
    mobileLabel: "Produtos",
    icon: Package,
    href: "/products",
    group: "Cadastros",
    mobile: "more",
  },
];

export function isNavItemActive(pathname: string, href: string) {
  return pathname === href || (href !== "/" && pathname.startsWith(`${href}/`));
}
