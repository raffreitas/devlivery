import { cn } from "cn";
import { LogOut, Moon, Sun } from "lucide-react";
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
              <span
                aria-hidden="true"
                className={cn(
                  "size-2 shrink-0 rounded-full",
                  currentSession && !error && !isLoading
                    ? "bg-success"
                    : isClosed
                      ? "border border-dashed border-muted-foreground"
                      : "bg-muted-foreground",
                )}
              />
              <span className="flex-1 text-meta text-foreground group-data-[collapsible=icon]:sr-only">
                {label}
              </span>
              {since && (
                <span className="text-meta text-muted-foreground group-data-[collapsible=icon]:hidden">
                  desde {since}
                </span>
              )}
              {isClosed && (
                <span className="text-meta text-muted-foreground group-data-[collapsible=icon]:hidden">
                  Abrir
                </span>
              )}
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      )}
      <div className="flex min-w-0 items-center gap-1 group-data-[collapsible=icon]:flex-col">
        <div
          className="flex min-h-8 min-w-0 flex-1 items-center gap-2 px-2 group-data-[collapsible=icon]:flex-none group-data-[collapsible=icon]:px-0"
          title={name}
        >
          <Avatar size="sm">
            <AvatarFallback>{initials}</AvatarFallback>
          </Avatar>
          <span className="truncate text-ui group-data-[collapsible=icon]:hidden">
            {name}
          </span>
        </div>
        <Tooltip>
          <TooltipTrigger
            render={<Button variant="ghost" size="icon-sm" />}
            aria-label="Alternar tema"
            onClick={() => setTheme(theme === "light" ? "dark" : "light")}
          >
            {theme === "light" ? <Sun /> : <Moon />}
          </TooltipTrigger>
          <TooltipContent>Alternar tema</TooltipContent>
        </Tooltip>
        <Separator
          orientation="vertical"
          className="my-2 group-data-[collapsible=icon]:hidden"
        />
        <Tooltip>
          <TooltipTrigger
            render={<Button variant="ghost" size="icon-sm" />}
            aria-label="Sair"
            onClick={() => void onLogout()}
          >
            <LogOut />
          </TooltipTrigger>
          <TooltipContent>Sair</TooltipContent>
        </Tooltip>
      </div>
    </>
  );
}
