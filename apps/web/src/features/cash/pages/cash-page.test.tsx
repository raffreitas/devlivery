import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { screen } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import { CashPage } from "./cash-page";

const url = "http://localhost:5000/api/v1/cash-register/sessions/active";

describe("CashPage query state", () => {
  it("only offers opening a register after the API confirms there is none", async () => {
    server.use(http.get(url, () => new HttpResponse(null, { status: 404 })));
    renderWithProviders(<CashPage />);
    expect(await screen.findByText("Nenhum caixa aberto")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Abrir Caixa" })).toBeEnabled();
  });

  it("does not treat a server failure as an absent cash register", async () => {
    server.use(
      http.get(url, () =>
        HttpResponse.json({ detail: "Servidor indisponível" }, { status: 500 }),
      ),
    );
    renderWithProviders(<CashPage />);
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Servidor indisponível",
    );
    expect(screen.queryByText("Nenhum caixa aberto")).not.toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Abrir Caixa" }),
    ).not.toBeInTheDocument();
  });
});
