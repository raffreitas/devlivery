import { AppRoutes } from "./app-routes";
import { Toaster } from "./shared/components/ui/sonner";
import { AuthProvider } from "./shared/contexts/auth-context";
import { ThemeProvider } from "./shared/contexts/theme-context";

export function App() {
  return (
    <ThemeProvider defaultTheme="system" storageKey="devlivery@theme">
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
      <Toaster richColors />
    </ThemeProvider>
  );
}
