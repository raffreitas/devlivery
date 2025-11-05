import { AppRoutes } from "./app-routes";
import { AuthProvider } from "./shared/contexts/auth-context";

export function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
