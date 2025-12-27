import type { ReactNode } from "react";
import { Spinner } from "@/shared/components/ui/spinner";

interface LoadingStateProps {
  isLoading: boolean;
  children: ReactNode;
  skeleton?: ReactNode;
  spinner?: boolean;
  emptyMessage?: string;
  className?: string;
}

/**
 * LoadingState - Componente para gerenciar estados de loading inicial
 *
 * Usado quando não há dados ainda (primeira carga).
 * Mostra skeleton por padrão, ou spinner centralizado se especificado.
 */
export function LoadingState({
  isLoading,
  children,
  skeleton,
  spinner = false,
  emptyMessage,
  className,
}: LoadingStateProps) {
  if (isLoading) {
    if (spinner) {
      return (
        <div
          className={`flex flex-col items-center justify-center h-64 gap-3 ${className ?? ""}`}
          aria-live="polite"
        >
          <Spinner className="w-6 h-6" aria-hidden="true" />
          {emptyMessage ? (
            <p className="text-sm text-muted-foreground">{emptyMessage}</p>
          ) : (
            <span className="sr-only">Carregando</span>
          )}
        </div>
      );
    }

    if (skeleton) {
      return <>{skeleton}</>;
    }

    // Fallback: spinner centralizado
    return (
      <div
        className={`flex flex-col items-center justify-center h-64 gap-3 ${className ?? ""}`}
        aria-live="polite"
      >
        <Spinner className="w-6 h-6" aria-hidden="true" />
        {emptyMessage ? (
          <p className="text-sm text-muted-foreground">{emptyMessage}</p>
        ) : (
          <span className="sr-only">Carregando</span>
        )}
      </div>
    );
  }

  return <>{children}</>;
}
