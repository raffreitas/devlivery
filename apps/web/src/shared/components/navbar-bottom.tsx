import { cn } from "cn";
import { Menu } from "lucide-react";
import { Link, useLocation } from "react-router-dom";
import { isNavItemActive, navItems } from "@/shared/constants/nav-items";

export function NavbarBottom() {
  const { pathname } = useLocation();
  const items = [
    ...navItems.filter((item) => item.mobile === "bar"),
    { id: "more", href: "/more", mobileLabel: "Mais", icon: Menu },
  ];
  const moreActive =
    pathname === "/more" ||
    navItems.some(
      (item) => item.mobile === "more" && isNavItemActive(pathname, item.href),
    );

  return (
    <nav
      aria-label="Navegação móvel"
      className="mobile-navigation fixed inset-x-0 bottom-0 z-40 border-t border-sidebar-border bg-sidebar md:hidden"
    >
      <div className="grid grid-cols-4 px-2 py-1">
        {items.map((item) => {
          const active =
            item.id === "more"
              ? moreActive
              : isNavItemActive(pathname, item.href);
          return (
            <Link
              key={item.id}
              to={item.href}
              aria-current={active ? "page" : undefined}
              className={cn(
                "flex min-h-14 flex-col items-center justify-center gap-1 rounded-md text-meta text-muted-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring",
                active && "bg-sidebar-accent text-sidebar-accent-foreground",
              )}
            >
              <item.icon className="size-5" aria-hidden="true" />
              <span>{item.mobileLabel}</span>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
