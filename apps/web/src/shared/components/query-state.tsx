import type { ReactNode } from "react";
import { getErrorFeedback } from "@/shared/services/error-feedback";
import { Alert, AlertDescription, AlertTitle } from "./ui/alert";
import { Button } from "./ui/button";
import { Spinner } from "./ui/spinner";

export interface QueryStateProps {
  isPending: boolean;
  error: Error | null;
  hasData: boolean;
  refetch: () => unknown;
  children: ReactNode;
}

export function QueryState({
  isPending,
  error,
  hasData,
  refetch,
  children,
}: QueryStateProps) {
  if (isPending && !hasData) {
    return (
      <div role="status" className="flex items-center justify-center gap-2 p-6">
        <Spinner aria-hidden="true" />
        Carregando...
      </div>
    );
  }

  return (
    <>
      {error && (
        <Alert variant="destructive" role="alert">
          <AlertTitle>
            {hasData
              ? "Dados desatualizados"
              : "Não foi possível carregar os dados"}
          </AlertTitle>
          <AlertDescription>
            <p>{getErrorFeedback(error).description}</p>
            {hasData && (
              <p>Exibindo a última consulta concluída com sucesso.</p>
            )}
            <Button
              type="button"
              variant="outline"
              onClick={() => void refetch()}
            >
              Tentar novamente
            </Button>
          </AlertDescription>
        </Alert>
      )}
      {(!error || hasData) && children}
    </>
  );
}
