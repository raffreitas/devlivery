import { LogOut } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { ModeToggle } from "@/shared/components/mode-toggle";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { navGroups, navItems } from "@/shared/constants/nav-items";
import { useAuth } from "@/shared/contexts/auth-context";

export function MorePage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const onLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-title-lg font-semibold">Mais</h1>
      {navGroups
        .filter((group) =>
          navItems.some(
            (item) => item.group === group && item.mobile === "more",
          ),
        )
        .map((group) => (
          <Card key={group}>
            <CardHeader>
              <CardTitle>{group}</CardTitle>
            </CardHeader>
            <CardContent>
              {navItems
                .filter(
                  (item) => item.group === group && item.mobile === "more",
                )
                .map((item) => (
                  <Button
                    key={item.id}
                    variant="ghost"
                    render={<Link to={item.href} />}
                    className="min-h-11 w-full justify-start"
                  >
                    <item.icon data-icon="inline-start" />
                    {item.label}
                  </Button>
                ))}
            </CardContent>
          </Card>
        ))}
      <Card>
        <CardHeader>
          <CardTitle>Conta e aparência</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="truncate text-ui text-muted-foreground">{user?.name}</p>
          <div className="flex min-h-11 items-center justify-between gap-3">
            <span className="text-ui">Tema</span>
            <ModeToggle />
          </div>
          <Button
            variant="ghost"
            className="min-h-11 justify-start"
            onClick={() => void onLogout()}
          >
            <LogOut data-icon="inline-start" />
            Sair
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
