import { useState } from "react";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/auth-context";
import { NavbarDesktop } from "./navbar-desktop";
import { NavbarMobile } from "./navbar-mobile";
import { NavbarUserSection } from "./navbar-user-section";

export function Layout() {
  const location = useLocation();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);
  const { logout } = useAuth();

  const onLogout = async () => {
    await logout();
    setMobileOpen(false);
    navigate("/login", { replace: true });
  };

  const isLogin = location.pathname === "/login";

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-lg relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            {/* Logo e navegação */}
            <div className="flex items-center flex-1">
              <div className="shrink-0 flex items-center">
                <Link to="/">
                  <h1 className="text-xl sm:text-2xl font-bold text-orange-600">
                    🍕 Devlivery
                  </h1>
                </Link>
              </div>
              {!isLogin && <NavbarDesktop />}
            </div>

            {/* Seção do usuário e menu mobile */}
            <div className="flex items-center gap-2">
              <NavbarUserSection onLogout={onLogout} />
              {!isLogin && (
                <NavbarMobile
                  isOpen={mobileOpen}
                  onToggle={() => setMobileOpen((v) => !v)}
                  onClose={() => setMobileOpen(false)}
                />
              )}
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-4 px-4 sm:py-6 sm:px-6 lg:px-8">
        <Outlet />
      </main>
    </div>
  );
}
