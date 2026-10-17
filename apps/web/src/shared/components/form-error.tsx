import { Alert, AlertDescription, AlertTitle } from "./ui/alert";

export function FormError({ message }: { message?: string }) {
  if (!message) return null;
  return (
    <Alert variant="destructive" role="alert">
      <AlertTitle>Não foi possível salvar</AlertTitle>
      <AlertDescription>{message}</AlertDescription>
    </Alert>
  );
}
