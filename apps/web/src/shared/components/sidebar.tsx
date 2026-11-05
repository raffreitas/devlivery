import { Link, useLocation } from "react-router-dom";
import {
  isNavItemActive,
  navGroups,
  navItems,
} from "@/shared/constants/nav-items";
import { Brand } from "./brand";
import { SidebarFooterContent } from "./sidebar-footer-content";
import {
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  Sidebar as SidebarPrimitive,
} from "./ui/sidebar";

export function Sidebar({ onLogout }: { onLogout: () => Promise<void> }) {
  const { pathname } = useLocation();

  return (
    <SidebarPrimitive variant="inset" collapsible="icon">
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              render={<Link to="/" />}
              size="lg"
              tooltip="Devlivery"
              aria-label="Devlivery — início"
            >
              <span
                className="flex size-8 shrink-0 items-center justify-center rounded-md bg-sidebar-accent font-bold text-sidebar-accent-foreground"
                aria-hidden="true"
              >
                D<span className="text-primary">.</span>
              </span>
              <Brand />
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <nav aria-label="Navegação principal">
          {navGroups.map((group) => (
            <SidebarGroup key={group}>
              <SidebarGroupLabel>{group}</SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {navItems
                    .filter((item) => item.group === group)
                    .map((item) => {
                      const active = isNavItemActive(pathname, item.href);
                      return (
                        <SidebarMenuItem key={item.id}>
                          <SidebarMenuButton
                            render={<Link to={item.href} />}
                            isActive={active}
                            tooltip={item.label}
                            aria-label={item.label}
                            aria-current={active ? "page" : undefined}
                          >
                            <item.icon aria-hidden="true" />
                            <span>{item.label}</span>
                          </SidebarMenuButton>
                        </SidebarMenuItem>
                      );
                    })}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          ))}
        </nav>
      </SidebarContent>
      <SidebarFooter>
        <SidebarFooterContent onLogout={onLogout} />
      </SidebarFooter>
    </SidebarPrimitive>
  );
}
