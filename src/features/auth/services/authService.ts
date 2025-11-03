import type { AuthState, Credentials, User } from "../types";

const AUTH_STORAGE_KEY = "auth";

const mockUser: User = {
  id: "1",
  name: "Atendente",
  email: "admin@pizza.com",
};

const mockPassword = "123456";

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
    // Simula uma chamada à API
    await new Promise((r) => setTimeout(r, 300));

    if (email === mockUser.email && password === mockPassword) {
      const token = "mock-token-" + crypto.randomUUID();
      const auth: AuthState = { user: mockUser, token };
      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
      return auth;
    }
    throw new Error("Credenciais inválidas");
  },

  logout: async (): Promise<void> => {
    await new Promise((r) => setTimeout(r, 100));
    localStorage.removeItem(AUTH_STORAGE_KEY);
  },
};
