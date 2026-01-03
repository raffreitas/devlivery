import { Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/auth-context";
import { NavbarBottom } from "./navbar-bottom";
import { NavbarUserSection } from "./navbar-user-section";
import { Sidebar } from "./sidebar";

export function Layout() {
  const navigate = useNavigate();
  const { logout } = useAuth();

  const onLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex min-h-screen bg-background">
      {/* Sidebar for Desktop */}
      <Sidebar />

      <div className="flex-1 flex flex-col min-h-screen relative">
        {/* Mobile Header */}
        <header className="sm:hidden bg-card border-b border-border h-16 px-4 flex items-center justify-between sticky top-0 z-40">
          <h1 className="text-xl font-bold text-primary">🍕 Devlivery</h1>
          <NavbarUserSection onLogout={onLogout} />
        </header>

        {/* Desktop Header (Minimal - mainly for user profile if not in sidebar, but we put generic actions here if needed) */}
        {/* For now, Sidebar handles most, but we can keep a top bar for Profile if we want.
             In this design, let's put Profile in Sidebar?
             Actually, let's keep a top bar on desktop for "Search" or "Notifications" in future,
             but for now let's just keep the content area clean.
             Wait, where is the User Profile on Desktop? The Sidebar has "Logout" but maybe not the full User Menu.
             Let's put the User Section in the top right of the main content area for Desktop as well.
         */}
        <header className="hidden sm:flex bg-card border-b border-border h-16 px-6 items-center justify-between sticky top-0 z-40">
          <h2 className="text-lg font-semibold text-foreground">
            {/* Contextual Title could go here, leveraging a context or route matching.
                   For now, let's leave it empty or show Breadcrumbs. */}
            Bem-vindo
          </h2>
          <div className="flex items-center gap-4">
            <NavbarUserSection onLogout={onLogout} />
          </div>
        </header>

        <main className="flex-1 p-4 sm:p-6 lg:p-8 overflow-y-auto pb-24 sm:pb-8">
          <div className="max-w-screen-2xl mx-auto">
            <Outlet />
          </div>
        </main>

        {/* Bottom Nav for Mobile */}
        <NavbarBottom />
      </div>
    </div>
  );
}
