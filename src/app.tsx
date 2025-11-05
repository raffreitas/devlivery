import { AppRoutes } from "./app-routes";
import { AuthProvider } from "./shared/contexts/AuthContext";

export function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
