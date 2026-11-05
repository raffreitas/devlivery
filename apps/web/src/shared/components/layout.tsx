import { useEffect, useState } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/auth-context";
import { useIsMobile } from "../hooks/use-mobile";
import { Brand } from "./brand";
import { NavbarBottom } from "./navbar-bottom";
import { Sidebar } from "./sidebar";
import { SidebarInset, SidebarProvider, SidebarTrigger } from "./ui/sidebar";

const SIDEBAR_STORAGE_KEY = "devlivery@sidebar-expanded";
function readSidebarPreference() {
  try {
    return localStorage.getItem(SIDEBAR_STORAGE_KEY) !== "false";
  } catch {
    return true;
  }
}

export function Layout() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const isMobile = useIsMobile();
  const [expanded, setExpanded] = useState(readSidebarPreference);

  useEffect(() => {
    // The official provider owns Ctrl/Cmd+B; keep it out of editable controls.
    const guardSidebarShortcut = (event: KeyboardEvent) => {
      if (event.key.toLowerCase() !== "b" || !(event.ctrlKey || event.metaKey))
        return;
      const target = event.target;
      if (
        isMobile ||
        (target instanceof HTMLElement &&
          target.closest(
            "input, textarea, select, [contenteditable]:not([contenteditable='false']), [role='textbox']",
          ))
      ) {
        event.stopImmediatePropagation();
      }
    };
    window.addEventListener("keydown", guardSidebarShortcut, true);
    return () =>
      window.removeEventListener("keydown", guardSidebarShortcut, true);
  }, [isMobile]);
  const onLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };
  const onExpandedChange = (open: boolean) => {
    setExpanded(open);
    try {
      localStorage.setItem(SIDEBAR_STORAGE_KEY, String(open));
    } catch {
      // Navigation still works when browser storage is unavailable.
    }
  };

  return (
    <SidebarProvider
      open={expanded}
      onOpenChange={onExpandedChange}
      style={
        { "--sidebar-width": "var(--app-sidebar-width)" } as React.CSSProperties
      }
      className="app-shell text-foreground"
    >
      {!isMobile && <Sidebar onLogout={onLogout} />}
      <SidebarInset className="min-w-0 md:border md:border-border md:bg-card">
        <header className="flex h-12 shrink-0 items-center gap-3 border-b border-border px-4">
          {isMobile ? (
            <Brand />
          ) : (
            <SidebarTrigger
              aria-label={
                expanded ? "Recolher navegação" : "Expandir navegação"
              }
              aria-expanded={expanded}
            />
          )}
        </header>
        <div className="app-shell-content flex-1 p-4 md:p-6">
          <div className="mx-auto w-full max-w-screen-2xl">
            <Outlet />
          </div>
        </div>
      </SidebarInset>
      <NavbarBottom />
    </SidebarProvider>
  );
}
