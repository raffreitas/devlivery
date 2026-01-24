import type { ReactNode } from "react";
import { createContext, useContext, useEffect } from "react";
import { useAuthQuery } from "@/features/auth/hooks/use-auth";
import type { Credentials, User } from "@/features/auth/types";
import { authEvents } from "@/shared/services/auth-events";

interface AuthContextData {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (credentials: Credentials) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextData | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const auth = useAuthQuery();

  // Escuta eventos de erro de autenticação (ex: 401 Unauthorized)
  useEffect(() => {
    const unsubscribe = authEvents.subscribe(() => {
      void auth.logout();
    });
    return unsubscribe;
  }, [auth]);

  return <AuthContext.Provider value={auth}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
