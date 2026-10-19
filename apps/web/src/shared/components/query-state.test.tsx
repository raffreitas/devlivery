import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ApiError } from "@/shared/services/api";
import { QueryState } from "./query-state";

describe("QueryState", () => {
  it("does not show empty business states during initial loading or errors", () => {
    const props = {
      isPending: true,
      error: null,
      hasData: false,
      refetch: vi.fn(),
    };
    const { rerender } = render(
      <QueryState {...props}>Nenhum registro</QueryState>,
    );
    expect(screen.getByRole("status")).toBeInTheDocument();
    expect(screen.queryByText("Nenhum registro")).not.toBeInTheDocument();
    rerender(
      <QueryState
        {...props}
        isPending={false}
        error={
          new ApiError("HTTP 503", 503, { detail: "Servidor indisponível" })
        }
      >
        Nenhum registro
      </QueryState>,
    );
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Servidor indisponível",
    );
    expect(screen.queryByText("Nenhum registro")).not.toBeInTheDocument();
  });

  it("retains successful data with a persistent warning and a retry action", async () => {
    const refetch = vi.fn();
    render(
      <QueryState
        isPending={false}
        hasData
        error={new Error("Falha ao atualizar")}
        refetch={refetch}
      >
        R$ 500,00
      </QueryState>,
    );
    expect(screen.getByRole("alert")).toHaveTextContent("Dados desatualizados");
    expect(screen.getByText("R$ 500,00")).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Tentar novamente" }),
    );
    expect(refetch).toHaveBeenCalledOnce();
  });
});
