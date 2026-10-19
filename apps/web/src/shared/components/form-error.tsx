import { getErrorTitle } from "@/shared/services/error-feedback";
import { Alert, AlertDescription, AlertTitle } from "./ui/alert";

export function FormError({
  message,
  errorType,
  fallbackTitle = "Não foi possível salvar",
}: {
  message?: string;
  errorType?: string | number;
  fallbackTitle?: string;
}) {
  if (!message) return null;
  return (
    <Alert variant="destructive" role="alert">
      <AlertTitle>
        {getErrorTitle(String(errorType)) ?? fallbackTitle}
      </AlertTitle>
      <AlertDescription>{message}</AlertDescription>
    </Alert>
  );
}
