import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff } from "lucide-react";
import { useId, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { Navigate, useNavigate } from "react-router-dom";
import { LoadingButton } from "@/shared/components/loading";
import { Card } from "@/shared/components/ui/card";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/shared/components/ui/input-group";
import { toast } from "@/shared/components/ui/toast";
import { useAuth } from "@/shared/contexts/auth-context";
import { type AuthFormData, authFormSchema } from "../types";

export function LoginPage() {
  const formId = useId();
  const { login, loading, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);

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
    } catch (error) {
      const limited =
        error &&
        typeof error === "object" &&
        "status" in error &&
        error.status === 429;
      toast.add({
        type: "error",
        title: limited
          ? "Muitas tentativas. Aguarde antes de tentar novamente."
          : "Credenciais inválidas",
      });
    }
  };

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="min-h-screen bg-linear-to-br from-orange-50 via-background to-orange-50 dark:from-orange-950/20 dark:via-background dark:to-orange-950/20 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold tracking-[-0.048em] text-[#182230] dark:text-foreground">
            Devlivery
            <span className="text-[#c2410c] dark:text-orange-400">.</span>
          </h1>
        </div>

        <Card className="p-8">
          <form onSubmit={form.handleSubmit(handleAuthenticate)} noValidate>
            <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
              <Controller
                control={form.control}
                name="email"
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <FieldLabel htmlFor={`${formId}-login-email`}>
                      E-mail
                    </FieldLabel>
                    <Input
                      {...field}
                      id={`${formId}-login-email`}
                      type="email"
                      placeholder="seu@email.com"
                      autoComplete="email"
                      aria-invalid={fieldState.invalid}
                      aria-describedby={
                        fieldState.invalid
                          ? `${formId}-login-email-error`
                          : undefined
                      }
                    />
                    {fieldState.invalid && (
                      <FieldError
                        id={`${formId}-login-email-error`}
                        errors={[fieldState.error]}
                      />
                    )}
                  </Field>
                )}
              />

              <Controller
                control={form.control}
                name="password"
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <FieldLabel htmlFor={`${formId}-login-password`}>
                      Senha
                    </FieldLabel>
                    <InputGroup>
                      <InputGroupInput
                        {...field}
                        id={`${formId}-login-password`}
                        type={showPassword ? "text" : "password"}
                        placeholder="••••••••"
                        autoComplete="current-password"
                        aria-invalid={fieldState.invalid}
                        aria-describedby={
                          fieldState.invalid
                            ? `${formId}-login-password-error`
                            : undefined
                        }
                      />
                      <InputGroupAddon align="inline-end">
                        <InputGroupButton
                          size="icon-xs"
                          aria-label={
                            showPassword ? "Ocultar senha" : "Mostrar senha"
                          }
                          onClick={() => setShowPassword(!showPassword)}
                        >
                          {showPassword ? <EyeOff /> : <Eye />}
                        </InputGroupButton>
                      </InputGroupAddon>
                    </InputGroup>
                    {fieldState.invalid && (
                      <FieldError
                        id={`${formId}-login-password-error`}
                        errors={[fieldState.error]}
                      />
                    )}
                  </Field>
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
            </FieldGroup>
          </form>
        </Card>

        <p className="text-center text-sm text-muted-foreground mt-6">
          © {currentYear} Devlivery. Todos os direitos reservados.
        </p>
      </div>
    </div>
  );
}
