import z from "zod";

export interface User {
  id: string;
  name: string;
  email: string;
}

export interface Credentials {
  email: string;
  password: string;
}

export interface AuthState {
  user: User | null;
  token: string | null;
}

export const authFormSchema = z.object({
  email: z.email("E-mail inválido"),
  password: z.string().min(1, "Senha é obrigatória"),
});

export type AuthFormData = z.infer<typeof authFormSchema>;
