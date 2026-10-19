import { Outlet, useNavigate } from "react-router-dom";
import { env } from "@/env";
import { useAuth } from "../contexts/auth-context";
import { NavbarBottom } from "./navbar-bottom";
import { NavbarUserSection } from "./navbar-user-section";
import { Sidebar } from "./sidebar";
import { Badge } from "./ui/badge";

export function Layout() {
  const navigate = useNavigate();
  const { logout } = useAuth();

  const onLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      <Sidebar />

      <div className="flex-1 flex flex-col min-w-0 min-h-screen relative">
        <header className="sm:hidden bg-sidebar border-b border-sidebar-border h-16 px-4 flex items-center justify-between sticky top-0 z-40">
          <h1 className="text-xl font-bold tracking-[-0.048em] text-[#182230] dark:text-foreground">
            Devlivery
            <span className="text-[#c2410c] dark:text-orange-400">.</span>
          </h1>
          {env.MODE === "development" && <Badge>DEV ENV</Badge>}
          <NavbarUserSection onLogout={onLogout} />
        </header>

        <header className="hidden sm:flex bg-sidebar border-b border-sidebar-border h-16 px-6 items-center justify-between sticky top-0 z-40">
          <h2 className="text-lg font-semibold text-foreground">Bem-vindo</h2>
          {env.MODE === "development" && <Badge>DEV ENV</Badge>}
          <div className="flex items-center gap-4">
            <NavbarUserSection onLogout={onLogout} />
          </div>
        </header>

        <main className="flex-1 p-4 sm:p-6 lg:p-8 overflow-y-auto pb-24 sm:pb-8">
          <div className="max-w-screen-2xl mx-auto">
            <Outlet />
          </div>
        </main>

        <NavbarBottom />
      </div>
    </div>
  );
}
