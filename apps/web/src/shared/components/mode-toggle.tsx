import { Moon, Sun } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { useTheme } from "@/shared/contexts/theme-context";

export function ModeToggle() {
  const { theme, setTheme } = useTheme();

  const handleToggleTheme = () => {
    if (theme === "light") {
      setTheme("dark");
    } else {
      setTheme("light");
    }
  };

  return (
    <Button
      variant="ghost"
      size="icon"
      className="min-h-11 min-w-11"
      onClick={handleToggleTheme}
    >
      {theme === "light" ? <Moon /> : <Sun />}
      <span className="sr-only">Altere o tema</span>
    </Button>
  );
}
