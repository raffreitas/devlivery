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
  password: z
    .string({ error: "Deve ser informado um valor válido." })
    .min(6, "A senha deve ter no mínimo 6 caracteres"),
});

export type AuthFormData = z.infer<typeof authFormSchema>;
