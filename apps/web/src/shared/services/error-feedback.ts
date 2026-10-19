import { toast } from "@/shared/components/ui/toast";
import { ApiError, type ApiErrorKind } from "./api";

export type ErrorFeedback = { title: string; description: string };

export const UNKNOWN_OUTCOME_MESSAGE =
  "A conexão caiu antes da resposta do servidor. Confira se a alteração foi registrada antes de enviar novamente.";

const GENERIC_DESCRIPTION = "Ocorreu um erro inesperado. Tente novamente.";

const KIND_FEEDBACK: Partial<Record<ApiErrorKind, ErrorFeedback>> = {
  "unknown-outcome": {
    title: "Resultado não confirmado",
    description: UNKNOWN_OUTCOME_MESSAGE,
  },
  network: {
    title: "Sem conexão com o servidor",
    description:
      "Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.",
  },
  offline: {
    title: "Sem conexão com a internet",
    description: "Você está offline. Verifique sua rede e tente novamente.",
  },
  timeout: {
    title: "O servidor demorou a responder",
    description: "A requisição demorou demais. Tente novamente.",
  },
};

const STATUS_FEEDBACK: Record<number, ErrorFeedback> = {
  400: {
    title: "Dados inválidos",
    description: "Os dados enviados são inválidos.",
  },
  401: {
    title: "Sessão expirada",
    description: "Sua sessão expirou. Por favor, faça login novamente.",
  },
  403: {
    title: "Acesso negado",
    description: "Você não tem permissão para realizar esta ação.",
  },
  404: {
    title: "Registro não encontrado",
    description: "O registro não foi encontrado.",
  },
  409: {
    title: "Operação em conflito",
    description: "A operação conflita com o estado atual do registro.",
  },
  422: {
    title: "Não foi possível concluir",
    description: "Os dados enviados são inválidos.",
  },
  429: {
    title: "Muitas tentativas",
    description: "Muitas tentativas. Aguarde antes de tentar novamente.",
  },
};

const SERVER_FEEDBACK: ErrorFeedback = {
  title: "Erro no servidor",
  description: "O servidor encontrou um erro. Tente novamente em instantes.",
};

function lookup(kind?: string, status?: number): ErrorFeedback | undefined {
  const byKind = KIND_FEEDBACK[kind as ApiErrorKind];
  if (byKind || status === undefined) return byKind;
  return (
    STATUS_FEEDBACK[status] ?? (status >= 500 ? SERVER_FEEDBACK : undefined)
  );
}

/** Title for a failure kind or status, or undefined when only the caller's context can name it. */
export function getErrorTitle(kind?: string, status?: number) {
  return lookup(kind, status)?.title;
}

export function getErrorFeedback(
  error: unknown,
  fallbackTitle = "Não foi possível concluir a operação",
): ErrorFeedback {
  if (error instanceof ApiError) {
    const known = lookup(error.kind, error.status);
    return {
      title: known?.title ?? fallbackTitle,
      // The API writes its problem detail for operators, so it beats the generic text.
      description:
        error.details?.detail || known?.description || GENERIC_DESCRIPTION,
    };
  }
  // Anything else is a client bug; its message is not meant for operators.
  return { title: fallbackTitle, description: GENERIC_DESCRIPTION };
}

export function notifyError(error: unknown, fallbackTitle?: string) {
  const { title, description } = getErrorFeedback(error, fallbackTitle);
  toast.add({ type: "error", title, description, priority: "high" });
}
