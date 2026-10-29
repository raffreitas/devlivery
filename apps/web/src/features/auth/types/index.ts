import z from "zod";

export type {
  SessionUser as User,
  StoredAuth as AuthState,
} from "@/shared/services/auth-storage";

export interface Credentials {
  email: string;
  password: string;
}

export const authFormSchema = z.object({
  email: z.email("E-mail inválido"),
  password: z.string().min(1, "Senha é obrigatória"),
});

export type AuthFormData = z.infer<typeof authFormSchema>;
