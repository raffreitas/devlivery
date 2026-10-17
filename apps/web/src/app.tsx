import { AppRoutes } from "./app-routes";
import { Toaster } from "./shared/components/ui/toast";
import { AuthProvider } from "./shared/contexts/auth-context";
import { ThemeProvider } from "./shared/contexts/theme-context";

export function App() {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="devlivery@theme">
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
      <Toaster />
    </ThemeProvider>
  );
}
