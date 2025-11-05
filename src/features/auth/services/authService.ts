import { type ApiResponse, api } from "@/shared/services/api";
import type { AuthState, Credentials, User } from "../types";

const AUTH_STORAGE_KEY = "devlivery@auth";

interface LoginResponseDto {
  userId: string;
  userName: string;
  token: string;
}

export const authService = {
  getAuth: (): AuthState => {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) return { user: null, token: null };
    try {
      const parsed = JSON.parse(raw) as AuthState;
      return parsed;
    } catch {
      return { user: null, token: null };
    }
  },

  isAuthenticated: (): boolean => {
    const { token } = authService.getAuth();
    return Boolean(token);
  },

  login: async ({ email, password }: Credentials): Promise<AuthState> => {
    const res = await api.post<ApiResponse<LoginResponseDto>>(
      "/api/auth/login",
      { email, password },
    );

    if (!res.success || !res.data) {
      throw new Error(res.message || "Falha no login");
    }

    const user: User = {
      id: res.data.userId,
      name: res.data.userName,
      email,
    };
    const auth: AuthState = { user, token: res.data.token };

    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
    return auth;
  },

  logout: async (): Promise<void> => {
    // não há endpoint de logout no backend; apenas limpa o storage
    localStorage.removeItem(AUTH_STORAGE_KEY);
  },
};
