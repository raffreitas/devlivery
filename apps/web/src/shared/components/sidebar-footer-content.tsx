import { CircleAlert, CircleDashed, LogOut, Moon, Sun } from "lucide-react";
import { Link } from "react-router-dom";
import { useCurrentCashSession } from "@/features/cash/hooks/use-current-cash-session";
import { useAuth } from "@/shared/contexts/auth-context";
import { useTheme } from "@/shared/contexts/theme-context";
import { Avatar, AvatarFallback } from "./ui/avatar";
import { Button } from "./ui/button";
import { Separator } from "./ui/separator";
import {
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "./ui/sidebar";
import { Spinner } from "./ui/spinner";
import { Tooltip, TooltipContent, TooltipTrigger } from "./ui/tooltip";

const openingTime = new Intl.DateTimeFormat("pt-BR", {
  hour: "2-digit",
  minute: "2-digit",
  timeZone: "America/Sao_Paulo",
});

export function SidebarFooterContent({
  onLogout,
}: {
  onLogout: () => Promise<void>;
}) {
  const { user } = useAuth();
  const { theme, setTheme } = useTheme();
  const { open } = useSidebar();
  const { currentSession, isLoading, error } = useCurrentCashSession();
  const name = user?.name?.trim() || "Operador";
  const parts = name.split(/\s+/);
  const initials =
    `${parts[0][0]}${parts.length > 1 ? parts[parts.length - 1][0] : ""}`.toLocaleUpperCase(
      "pt-BR",
    );
  const label = isLoading
    ? "Consultando caixa…"
    : error
      ? "Caixa indisponível"
      : currentSession
        ? "Caixa aberto"
        : "Caixa fechado";
  const since =
    !isLoading && !error && currentSession
      ? openingTime.format(new Date(currentSession.startAt))
      : null;
  const isClosed = !isLoading && !error && !currentSession;

  return (
    <>
      {open && (
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              render={<Link to="/cash" />}
              tooltip={since ? `${label}, desde ${since}` : label}
              aria-label={since ? `${label}, desde ${since}` : label}
              className="border border-sidebar-border bg-card"
            >
              {isLoading ? (
                <Spinner aria-hidden="true" />
              ) : error ? (
                <CircleAlert aria-hidden="true" className="text-destructive" />
              ) : currentSession ? (
                <span
                  aria-hidden="true"
                  className="size-2 shrink-0 rounded-full bg-success"
                />
              ) : (
                <CircleDashed
                  aria-hidden="true"
                  className="text-muted-foreground"
                />
              )}
              <span className="min-w-0 flex-1 truncate">{label}</span>
              {since && (
                <span className="shrink-0 text-xs text-muted-foreground">
                  desde {since}
                </span>
              )}
              {isClosed && (
                <span className="shrink-0 text-sm text-muted-foreground">
                  Abrir
                </span>
              )}
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      )}
      <div className="flex min-w-0 items-center gap-1 group-data-[collapsible=icon]:flex-col">
        <div className="flex min-h-8 min-w-0 flex-1 items-center gap-2 px-2 group-data-[collapsible=icon]:flex-none group-data-[collapsible=icon]:px-0">
          <Tooltip>
            <TooltipTrigger
              render={<Avatar role="img" aria-label={name} />}
              tabIndex={open ? undefined : 0}
              className="outline-none focus-visible:ring-2 focus-visible:ring-sidebar-ring"
            >
              <AvatarFallback>{initials}</AvatarFallback>
            </TooltipTrigger>
            <TooltipContent side="right">{name}</TooltipContent>
          </Tooltip>
          <span className="truncate text-sm font-medium group-data-[collapsible=icon]:hidden">
            {name}
          </span>
        </div>
        <Tooltip>
          <TooltipTrigger
            render={<Button variant="ghost" size="icon-sm" />}
            aria-label="Alternar tema"
            onClick={() => setTheme(theme === "light" ? "dark" : "light")}
          >
            {theme === "light" ? (
              <Sun aria-hidden="true" />
            ) : (
              <Moon aria-hidden="true" />
            )}
          </TooltipTrigger>
          <TooltipContent side="right">Alternar tema</TooltipContent>
        </Tooltip>
        <Separator
          orientation="vertical"
          className="data-vertical:h-4 data-vertical:self-center group-data-[collapsible=icon]:hidden"
        />
        <Tooltip>
          <TooltipTrigger
            render={<Button variant="ghost" size="icon-sm" />}
            aria-label="Sair"
            onClick={() => void onLogout()}
          >
            <LogOut aria-hidden="true" />
          </TooltipTrigger>
          <TooltipContent side="right">Sair</TooltipContent>
        </Tooltip>
      </div>
    </>
  );
}
