import { Spinner } from "@/shared/components/ui/spinner";
import { cn } from "@/shared/lib/utils";

interface LoadingOverlayProps {
  isFetching: boolean;
  message?: string;
  position?: "top-bar" | "inline" | "badge";
  className?: string;
}

/**
 * LoadingOverlay - Componente para indicar refetch em background
 *
 * Usado quando há dados existentes e está fazendo refetch.
 * Não bloqueia a UI, apenas mostra feedback discreto.
 *
 * PADRÃO RECOMENDADO: Use `position="top-bar"` (progress bar) para refetch de páginas completas.
 *
 * Variantes:
 * - top-bar: Progress bar no topo (PADRÃO - use para refetch de páginas)
 * - inline: Spinner inline discreto (use para seções específicas)
 * - badge: Badge com spinner (use quando precisa de mais destaque)
 *
 * @example
 * // Padrão: Progress bar no topo
 * <LoadingOverlay isFetching={isFetching} position="top-bar" />
 *
 * // Spinner inline em header
 * <LoadingOverlay isFetching={isFetching} position="inline" />
 */
export function LoadingOverlay({
  isFetching,
  message = "Atualizando...",
  position = "top-bar",
  className,
}: LoadingOverlayProps) {
  if (!isFetching) return null;

  // Barra sutil no topo (estilo moderno como Linear, Vercel)
  if (position === "top-bar") {
    return (
      <div
        className="fixed top-0 left-0 right-0 z-50 h-[3px]"
        aria-live="polite"
      >
        <span className="sr-only">{message}</span>
        <div className="h-full w-full bg-primary/20 relative overflow-hidden">
          <div
            className="absolute top-0 left-0 h-full w-1/3 bg-primary rounded-full animate-[progress_1.5s_ease-in-out_infinite]"
            style={{
              boxShadow: "0 0 10px hsl(var(--primary) / 0.5)",
            }}
          />
        </div>
      </div>
    );
  }

  // Badge discreto (para casos onde precisa de mais destaque)
  if (position === "badge") {
    return (
      <div
        className={cn(
          "inline-flex items-center gap-1.5 px-2 py-1 rounded-md bg-muted/50 text-xs text-muted-foreground border border-border/50",
          className,
        )}
        aria-live="polite"
      >
        <Spinner className="w-3 h-3" aria-hidden="true" />
        <span>{message}</span>
      </div>
    );
  }

  // Inline discreto (padrão)
  return (
    <div
      className={cn(
        "inline-flex items-center gap-1.5 text-xs text-muted-foreground",
        className,
      )}
      aria-live="polite"
    >
      <Spinner className="w-3 h-3" aria-hidden="true" />
      <span className="sr-only">{message}</span>
    </div>
  );
}
