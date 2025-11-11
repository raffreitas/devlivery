import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Button } from "@/shared/components/button";
import { Input } from "@/shared/components/input";
import { LoadingSpinner } from "@/shared/components/loading-spinner";
import { useAuth } from "@/shared/contexts/auth-context";

export function LoginPage() {
  const { login, loading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation() as {
    state?: { from?: { pathname?: string } };
  };

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  const from = location.state?.from?.pathname || "/";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await login({ email, password });
      navigate(from, { replace: true });
    } catch {
      setError("Credenciais inválidas");
    }
  };

  return (
    <div className="min-h-screen bg-linear-to-br from-orange-50 via-white to-orange-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Branding */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-orange-600 rounded-2xl shadow-lg mb-4">
            <span className="text-5xl">🍕</span>
          </div>
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Devlivery</h1>
          {/* <p className="text-gray-600">Sistema de Gestão de Pedidos</p> */}
        </div>

        {/* Login Card */}
        <div className="bg-white rounded-2xl shadow-xl p-8 border border-gray-100">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Entrar</h2>

          {error && (
            <div className="mb-6 p-3 rounded-lg bg-red-50 text-red-700 border border-red-200 text-sm flex items-center gap-2">
              <span className="text-base">⚠️</span>
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <Input
              id="email"
              label="E-mail"
              type="email"
              value={email}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setEmail(e.target.value)
              }
              required
              autoComplete="email"
              placeholder="seu@email.com"
            />

            <Input
              id="password"
              label="Senha"
              type="password"
              value={password}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setPassword(e.target.value)
              }
              required
              autoComplete="current-password"
              placeholder="••••••••"
            />

            <Button type="submit" disabled={loading} className="w-full mt-6">
              {loading ? (
                <span className="flex items-center justify-center gap-2">
                  <LoadingSpinner size="sm" className="text-white" />
                  Entrando...
                </span>
              ) : (
                "Entrar"
              )}
            </Button>
          </form>
        </div>

        {/* Footer */}
        <p className="text-center text-sm text-gray-500 mt-6">
          © 2025 Devlivery. Todos os direitos reservados.
        </p>
      </div>
    </div>
  );
}
