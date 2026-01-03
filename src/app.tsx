import { AppRoutes } from "./app-routes";
import { AuthProvider } from "./shared/contexts/auth-context";
import { ThemeProvider } from "./shared/contexts/theme-context";

export function App() {
  return (
    <ThemeProvider defaultTheme="system" storageKey="devlivery@theme">
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </ThemeProvider>
  );
}
