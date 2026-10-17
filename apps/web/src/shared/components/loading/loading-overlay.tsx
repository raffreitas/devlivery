import { cn } from "cn";
import { Spinner } from "@/shared/components/ui/spinner";

interface LoadingOverlayProps {
  isFetching: boolean;
  message?: string;
  position?: "top-bar" | "inline" | "badge";
  className?: string;
}

export function LoadingOverlay({
  isFetching,
  message = "Atualizando...",
  position = "top-bar",
  className,
}: LoadingOverlayProps) {
  if (!isFetching) return null;

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
