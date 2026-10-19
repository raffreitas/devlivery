import { describe, expect, it, vi } from "vitest";
import { toast } from "@/shared/components/ui/toast";
import { ApiError } from "./api";
import {
  getErrorFeedback,
  notifyError,
  UNKNOWN_OUTCOME_MESSAGE,
} from "./error-feedback";

describe("error feedback", () => {
  it.each([
    [
      new ApiError("x", undefined, undefined, "network"),
      "Sem conexão com o servidor",
    ],
    [
      new ApiError("x", undefined, undefined, "timeout"),
      "O servidor demorou a responder",
    ],
    [
      new ApiError(
        UNKNOWN_OUTCOME_MESSAGE,
        undefined,
        undefined,
        "unknown-outcome",
      ),
      "Resultado não confirmado",
    ],
    [
      new ApiError("x", undefined, undefined, "offline"),
      "Sem conexão com a internet",
    ],
    [new ApiError("x", 403), "Acesso negado"],
    [new ApiError("x", 404), "Registro não encontrado"],
    [new ApiError("x", 429), "Muitas tentativas"],
    [new ApiError("x", 503), "Erro no servidor"],
  ])("titles %o as %s", (error, title) => {
    expect(getErrorFeedback(error).title).toBe(title);
  });

  it("keeps the server detail as the description", () => {
    expect(
      getErrorFeedback(
        new ApiError("x", 422, { detail: "Caixa já está fechado." }),
      ),
    ).toEqual({
      title: "Não foi possível concluir",
      description: "Caixa já está fechado.",
    });
  });

  it("describes a bare status when the server sent no detail", () => {
    expect(getErrorFeedback(new ApiError("HTTP 502", 502))).toEqual({
      title: "Erro no servidor",
      description:
        "O servidor encontrou um erro. Tente novamente em instantes.",
    });
  });

  it("does not expose client exception messages", () => {
    const feedback = getErrorFeedback(
      new TypeError("Cannot read properties of undefined"),
      "Falha ao salvar",
    );
    expect(feedback.title).toBe("Falha ao salvar");
    expect(feedback.description).not.toContain("undefined");
  });

  it("raises error toasts with a title, description and high priority", () => {
    const toastSpy = vi.spyOn(toast, "add");
    notifyError(new ApiError("x", 404, { detail: "Pedido não encontrado." }));
    expect(toastSpy).toHaveBeenCalledWith({
      type: "error",
      title: "Registro não encontrado",
      description: "Pedido não encontrado.",
      priority: "high",
    });
  });
});
