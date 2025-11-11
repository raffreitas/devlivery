import { LogOut } from "lucide-react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/auth-context";

interface NavbarUserSectionProps {
  onLogout: () => void;
}

export function NavbarUserSection({ onLogout }: NavbarUserSectionProps) {
  const location = useLocation();
  const { isAuthenticated, user } = useAuth();
  const isLogin = location.pathname === "/login";

  if (isLogin) {
    return null;
  }

  return (
    <div className="flex items-center space-x-2 sm:space-x-4">
      {isAuthenticated ? (
        <>
          <span className="text-xs sm:text-sm text-gray-700 truncate max-w-[120px] sm:max-w-none">
            Olá, {user?.name}
          </span>
          <button
            type="button"
            onClick={onLogout}
            className="inline-flex text-xs sm:text-sm text-gray-600 hover:text-gray-900 items-center gap-1 cursor-pointer"
            title="Sair"
          >
            <LogOut className="w-4 h-4" />
            <span className="hidden sm:inline">Sair</span>
          </button>
        </>
      ) : (
        <Link
          to="/login"
          className="text-xs sm:text-sm text-gray-600 hover:text-gray-900"
        >
          Entrar
        </Link>
      )}
    </div>
  );
}
