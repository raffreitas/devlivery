import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { Card } from "@/shared/components/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Spinner } from "@/shared/components/ui/spinner";
import { useAuth } from "@/shared/contexts/auth-context";
import { type AuthFormData, authFormSchema } from "../types";

export function LoginPage() {
  const { login, loading } = useAuth();
  const navigate = useNavigate();

  const currentYear = new Date().getFullYear();

  const { register, handleSubmit } = useForm<AuthFormData>({
    resolver: zodResolver(authFormSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const handleAuthenticate = async ({ email, password }: AuthFormData) => {
    try {
      await login({ email, password });
      navigate("/", { replace: true });
    } catch {
      toast.error("Credenciais inválidas");
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
        <Card className="p-8">
          <form
            onSubmit={handleSubmit(handleAuthenticate)}
            className="space-y-5"
          >
            <Label htmlFor="email">E-mail</Label>
            <Input
              id="email"
              type="email"
              required
              autoComplete="email"
              placeholder="seu@email.com"
              {...register("email")}
            />

            <Label htmlFor="password">Senha</Label>
            <Input
              id="password"
              type="password"
              required
              autoComplete="current-password"
              placeholder="••••••••"
              {...register("password")}
            />

            <Button type="submit" disabled={loading} className="w-full mt-6">
              {loading ? (
                <span className="flex items-center justify-center gap-2">
                  <Spinner />
                  Entrando...
                </span>
              ) : (
                "Entrar"
              )}
            </Button>
          </form>
        </Card>

        {/* Footer */}
        <p className="text-center text-sm text-gray-500 mt-6">
          © {currentYear} Devlivery. Todos os direitos reservados.
        </p>
      </div>
    </div>
  );
}
