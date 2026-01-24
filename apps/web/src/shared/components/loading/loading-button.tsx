import type * as React from "react";
import type { ReactNode } from "react";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { cn } from "@/shared/lib/utils";

interface LoadingButtonProps extends React.ComponentProps<typeof Button> {
  isLoading?: boolean;
  loadingText?: string;
  children: ReactNode;
}

/**
 * LoadingButton - Botão com estado de loading integrado
 *
 * Usado para mutações e ações do usuário.
 * Mostra spinner inline e desabilita o botão durante o loading.
 */
export function LoadingButton({
  isLoading = false,
  loadingText,
  children,
  className,
  disabled,
  ...props
}: LoadingButtonProps) {
  return (
    <Button
      className={cn(className)}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading && <Spinner className="w-4 h-4" />}
      <span>{isLoading && loadingText ? loadingText : children}</span>
    </Button>
  );
}
