import { cn } from "cn";
import { Home, RefreshCw } from "lucide-react";
import { useEffect } from "react";
import { isRouteErrorResponse, Link, useRouteError } from "react-router-dom";
import { env } from "@/env";
import { Button } from "../components/ui/button";
import { NotFoundPage } from "./not-found-page";

const RELOAD_KEY = "devlivery@chunk-reload-at";
const RELOAD_COOLDOWN_MS = 10_000;

// Browsers word a failed dynamic import differently; all mean the deployed chunk is gone.
const CHUNK_LOAD_ERROR =
  /Failed to fetch dynamically imported module|error loading dynamically imported module|Importing a module script failed|Unable to preload CSS/i;

export function isChunkLoadError(error: unknown) {
  return error instanceof Error && CHUNK_LOAD_ERROR.test(error.message);
}

/** Reloads to fetch the new build, unless a reload just happened and failed the same way. */
export function reloadForNewVersion(): boolean {
  try {
    const last = Number(sessionStorage.getItem(RELOAD_KEY));
    if (last && Date.now() - last < RELOAD_COOLDOWN_MS) return false;
    sessionStorage.setItem(RELOAD_KEY, String(Date.now()));
  } catch {
    return false;
  }
  window.location.reload();
  return true;
}

export function RouteErrorPage({
  fullScreen = false,
}: {
  fullScreen?: boolean;
}) {
  const error = useRouteError();
  const chunkError = isChunkLoadError(error);

  useEffect(() => {
    if (chunkError) reloadForNewVersion();
  }, [chunkError]);

  if (isRouteErrorResponse(error) && error.status === 404) {
    return <NotFoundPage />;
  }

  return (
    <div
      role="alert"
      className={cn(
        "flex items-center justify-center p-4",
        fullScreen ? "min-h-screen bg-background" : "min-h-[60vh]",
      )}
    >
      <div className="flex max-w-md flex-col items-center gap-4 text-center">
        <h1 className="text-2xl font-semibold text-foreground">
          {chunkError ? "Nova versão disponível" : "Algo deu errado"}
        </h1>
        <p className="text-muted-foreground">
          {chunkError
            ? "O sistema foi atualizado. Recarregue a página para continuar."
            : "Ocorreu um erro inesperado nesta tela. Recarregue a página ou volte ao início."}
        </p>
        {env.MODE === "development" && error instanceof Error && (
          <pre className="max-h-48 w-full overflow-auto rounded-lg bg-muted p-3 text-left text-xs">
            {error.stack ?? error.message}
          </pre>
        )}
        <div className="flex flex-wrap justify-center gap-2">
          <Button onClick={() => window.location.reload()}>
            <RefreshCw data-icon="inline-start" />
            Recarregar
          </Button>
          {!chunkError && (
            <Button
              variant="outline"
              nativeButton={false}
              role="link"
              render={<Link to="/" />}
            >
              <Home data-icon="inline-start" />
              Voltar ao início
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
