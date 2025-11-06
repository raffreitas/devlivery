import { LayoutDashboard, LogOut, Package, ShoppingCart } from "lucide-react";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/auth-context";

export function Layout() {
  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated, user, logout } = useAuth();

  const isActive = (path: string) => location.pathname === path;

  const onLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  const isLogin = location.pathname === "/login";

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex">
              <div className="shrink-0 flex items-center">
                <h1 className="text-2xl font-bold text-orange-600">
                  🍕 Devlivery
                </h1>
              </div>
              {!isLogin && (
                <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
                  <Link
                    to="/"
                    className={`${
                      isActive("/")
                        ? "border-orange-500 text-gray-900"
                        : "border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700"
                    } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2`}
                  >
                    <LayoutDashboard className="w-4 h-4" />
                    Dashboard
                  </Link>
                  <Link
                    to="/products"
                    className={`${
                      isActive("/products")
                        ? "border-orange-500 text-gray-900"
                        : "border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700"
                    } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2`}
                  >
                    <Package className="w-4 h-4" />
                    Produtos
                  </Link>
                  <Link
                    to="/orders"
                    className={`${
                      isActive("/orders")
                        ? "border-orange-500 text-gray-900"
                        : "border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700"
                    } inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium gap-2`}
                  >
                    <ShoppingCart className="w-4 h-4" />
                    Pedidos
                  </Link>
                </div>
              )}
            </div>
            <div className="flex items-center space-x-4">
              {isAuthenticated ? (
                <>
                  <span className="text-sm text-gray-700 hidden sm:inline">
                    Olá, {user?.name}
                  </span>
                  <button
                    type="button"
                    onClick={onLogout}
                    className="text-sm text-gray-600 hover:text-gray-900 inline-flex items-center gap-1"
                  >
                    <LogOut className="w-4 h-4" />
                    Sair
                  </button>
                </>
              ) : (
                !isLogin && (
                  <Link
                    to="/login"
                    className="text-sm text-gray-600 hover:text-gray-900"
                  >
                    Entrar
                  </Link>
                )
              )}
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <Outlet />
      </main>
    </div>
  );
}
