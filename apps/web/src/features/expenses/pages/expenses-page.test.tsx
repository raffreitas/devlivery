import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import { ExpensesPage } from "./expenses-page";

const url = "http://localhost:5000/api/v1/expenses";

describe("ExpensesPage", () => {
  it("preserves a new expense after the API rejects it", async () => {
    const user = userEvent.setup();
    server.use(
      http.get(url, () => HttpResponse.json({ data: [] })),
      http.get(`${url}/categories`, () =>
        HttpResponse.json({
          data: [
            {
              id: "123e4567-e89b-42d3-a456-426614174000",
              name: "Aluguel",
              isActive: true,
              subcategories: [],
            },
          ],
        }),
      ),
      http.post(url, () =>
        HttpResponse.json(
          { detail: "Falha ao salvar despesa" },
          { status: 500 },
        ),
      ),
    );
    renderWithProviders(<ExpensesPage />);
    await user.click(screen.getByRole("button", { name: "Nova Despesa" }));
    const dialog = await screen.findByRole("dialog");
    const category = await within(dialog).findByRole("combobox", {
      name: "Categoria",
    });
    await user.type(category, "Aluguel");
    await user.click(await screen.findByRole("option", { name: "Aluguel" }));
    await user.type(
      within(dialog).getByLabelText(/Fornecedor/),
      "Fornecedor teste",
    );
    await user.type(within(dialog).getByLabelText(/Valor/), "10000");
    await user.type(
      within(dialog).getByLabelText(/Data de Vencimento/),
      "2026-10-17",
    );
    await user.click(
      within(dialog).getByRole("button", { name: "Criar Despesa" }),
    );
    expect(await within(dialog).findByRole("alert")).toHaveTextContent(
      "Falha ao salvar despesa",
    );
    expect(within(dialog).getByLabelText(/Fornecedor/)).toHaveValue(
      "Fornecedor teste",
    );
    expect(category).toHaveValue("Aluguel");
    expect(within(dialog).getByLabelText(/Data de Vencimento/)).toHaveValue(
      "2026-10-17",
    );
  });
});
