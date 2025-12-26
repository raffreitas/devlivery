import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { Button } from "@/shared/components/ui/button";
import { Card } from "@/shared/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/components/ui/form";
import { Input } from "@/shared/components/ui/input";
import { LoadingButton } from "@/shared/components/loading";
import { useAuth } from "@/shared/contexts/auth-context";
import { type AuthFormData, authFormSchema } from "../types";

export function LoginPage() {
  const { login, loading } = useAuth();
  const navigate = useNavigate();

  const currentYear = new Date().getFullYear();

  const form = useForm<AuthFormData>({
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
          {/* <p className="text-secondary-foreground">Sistema de Gestão de Pedidos</p> */}
        </div>

        {/* Login Card */}
        <Card className="p-8">
          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(handleAuthenticate)}
              className="space-y-5"
            >
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>E-mail</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="seu@email.com"
                        autoComplete="email"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Senha</FormLabel>
                    <FormControl>
                      <Input
                        type="password"
                        placeholder="••••••••"
                        autoComplete="current-password"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <LoadingButton
                type="submit"
                isLoading={loading}
                loadingText="Entrando..."
                className="w-full"
              >
                Entrar
              </LoadingButton>
            </form>
          </Form>
        </Card>

        {/* Footer */}
        <p className="text-center text-sm text-gray-500 mt-6">
          © {currentYear} Devlivery. Todos os direitos reservados.
        </p>
      </div>
    </div>
  );
}
