import { type ApiResponse, api } from "@/shared/services/api";
import { clearAuth, readAuth, writeAuth } from "@/shared/services/auth-storage";
import type { AuthState, Credentials, User } from "../types";

interface LoginResponseDto {
  userId: string;
  userName: string;
  token: string;
}

export const authService = {
  getAuth: (): AuthState => readAuth(),

  isAuthenticated: (): boolean => {
    const { token } = authService.getAuth();
    return Boolean(token);
  },

  login: async ({ email, password }: Credentials): Promise<AuthState> => {
    // Login has no lasting side effect, so a lost response is a plain connection failure.
    const res = await api.post<ApiResponse<LoginResponseDto>>(
      "/auth/login",
      { email, password },
      { idempotent: true },
    );

    const user: User = {
      id: res.data.userId,
      name: res.data.userName,
      email,
    };
    const auth: AuthState = { user, token: res.data.token };

    writeAuth(auth);
    return auth;
  },

  logout: async (): Promise<void> => {
    // não há endpoint de logout no backend; apenas limpa o storage
    clearAuth();
  },
};
