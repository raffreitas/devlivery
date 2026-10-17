import { Alert, AlertDescription, AlertTitle } from "./ui/alert";

export function FormError({
  message,
  errorType,
}: {
  message?: string;
  errorType?: string | number;
}) {
  if (!message) return null;
  return (
    <Alert variant="destructive" role="alert">
      <AlertTitle>
        {errorType === "unknown-outcome"
          ? "Resultado da operação não confirmado"
          : "Não foi possível salvar"}
      </AlertTitle>
      <AlertDescription>{message}</AlertDescription>
    </Alert>
  );
}
