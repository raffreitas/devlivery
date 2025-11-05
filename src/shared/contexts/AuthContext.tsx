import type { ReactNode } from "react";
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { authService } from "@/features/auth/services/authService";
import type { AuthState, Credentials, User } from "@/features/auth/types";

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
  const [state, setState] = useState<AuthState>({ user: null, token: null });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initial = authService.getAuth();
    setState(initial);
    setLoading(false);
  }, []);

  const login = useCallback(async (credentials: Credentials) => {
    setLoading(true);
    try {
      const auth = await authService.login(credentials);
      setState(auth);
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    setLoading(true);
    try {
      await authService.logout();
      setState({ user: null, token: null });
    } finally {
      setLoading(false);
    }
  }, []);

  const value: AuthContextData = useMemo(
    () => ({
      user: state.user,
      token: state.token,
      loading,
      login,
      logout,
      isAuthenticated: Boolean(state.token),
    }),
    [state.user, state.token, loading, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
