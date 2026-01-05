import { ChevronLeft, ChevronRight } from "lucide-react";
import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { Button } from "@/shared/components/ui/button";
import { navItems } from "@/shared/constants/nav-items";
import { cn } from "@/shared/lib/utils";

export function Sidebar() {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <aside
      className={cn(
        "hidden sm:flex flex-col h-screen bg-sidebar border-r border-sidebar-border transition-all duration-300 sticky top-0",
        isCollapsed ? "w-16" : "w-64",
      )}
    >
      <div className="flex items-center justify-between p-4 h-16 border-b border-sidebar-border">
        {!isCollapsed && (
          <h1 className="text-xl font-bold text-primary truncate">Devlivery</h1>
        )}
        <Button
          variant="ghost"
          size="icon"
          className="ml-auto"
          onClick={() => setIsCollapsed(!isCollapsed)}
        >
          {isCollapsed ? (
            <ChevronRight className="w-4 h-4" />
          ) : (
            <ChevronLeft className="w-4 h-4" />
          )}
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto py-4 flex flex-col gap-6">
        <div className="px-3">
          {!isCollapsed && (
            <h4 className="mb-2 px-4 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Operação
            </h4>
          )}
          <nav className="space-y-1">
            {navItems.map((item) => (
              <Link
                key={item.path}
                to={item.path}
                className={cn(
                  "flex items-center gap-3 px-3 py-2 rounded-md transition-colors text-sm font-medium",
                  isActive(item.path)
                    ? "bg-accent text-accent-foreground"
                    : "text-muted-foreground hover:bg-accent/10 hover:text-foreground",
                  isCollapsed && "justify-center px-2",
                )}
                title={isCollapsed ? item.label : undefined}
              >
                <item.icon
                  className={cn(
                    "w-5 h-5",
                    isActive(item.path) && "stroke-[2.5px]",
                  )}
                />
                {!isCollapsed && <span>{item.label}</span>}
              </Link>
            ))}
          </nav>
        </div>
      </div>

      {/* <div className="p-4 border-t border-gray-100">
        <nav className="space-y-1">
          <button
            type="button"
            onClick={onLogout}
            className={cn(
              "w-full flex items-center gap-3 px-3 py-2 rounded-md transition-colors text-sm font-medium text-red-600 hover:bg-red-50",
              isCollapsed && "justify-center px-2",
            )}
            title={isCollapsed ? "Sair" : undefined}
          >
            <LogOut className="w-5 h-5" />
            {!isCollapsed && <span>Sair</span>}
          </button>
        </nav>
      </div> */}
    </aside>
  );
}
