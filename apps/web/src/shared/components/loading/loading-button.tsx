import { cn } from "cn";
import type * as React from "react";
import type { ReactNode } from "react";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";

interface LoadingButtonProps extends React.ComponentProps<typeof Button> {
  isLoading?: boolean;
  loadingText?: string;
  children: ReactNode;
}

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
      {isLoading && <Spinner data-icon="inline-start" />}
      <span>{isLoading && loadingText ? loadingText : children}</span>
    </Button>
  );
}
