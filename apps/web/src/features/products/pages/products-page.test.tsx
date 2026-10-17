import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import { ProductsPage } from "./products-page";

const product = {
  id: "product-1",
  name: "Pizza",
  description: "Pizza da casa",
  price: 40,
  category: "Pizzas",
  available: true,
  createdAt: "2026-10-17T12:00:00Z",
  updatedAt: "2026-10-17T12:00:00Z",
};
const url = "http://localhost:5000/api/v1/products";

describe("ProductsPage saving", () => {
  it("keeps the editor and input after a rejected save, and closes only after a successful retry", async () => {
    const user = userEvent.setup();
    server.use(
      http.get(url, () => HttpResponse.json({ data: [product] })),
      http.put(`${url}/${product.id}`, () =>
        HttpResponse.json(
          { detail: "Produto não pôde ser salvo" },
          { status: 500 },
        ),
      ),
    );
    renderWithProviders(<ProductsPage />);
    await user.click(
      await screen.findByRole("button", { name: "Editar Pizza" }),
    );
    const dialog = await screen.findByRole("dialog");
    const name = within(dialog).getByLabelText("Nome do Produto");
    await user.clear(name);
    await user.type(name, "Pizza especial");
    await user.click(
      within(dialog).getByRole("button", { name: "Atualizar Produto" }),
    );
    expect(await within(dialog).findByRole("alert")).toHaveTextContent(
      "Produto não pôde ser salvo",
    );
    expect(name).toHaveValue("Pizza especial");
    server.use(
      http.put(
        `${url}/${product.id}`,
        () => new HttpResponse(null, { status: 204 }),
      ),
    );
    await user.click(
      within(dialog).getByRole("button", { name: "Atualizar Produto" }),
    );
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );
  });

  it("prevents duplicate submissions and dismissal while the API is pending", async () => {
    const user = userEvent.setup();
    let finish: () => void = () => {};
    let requests = 0;
    const response = new Promise<void>((resolve) => {
      finish = resolve;
    });
    server.use(
      http.get(url, () => HttpResponse.json({ data: [product] })),
      http.put(`${url}/${product.id}`, async () => {
        requests++;
        await response;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    renderWithProviders(<ProductsPage />);
    await user.click(
      await screen.findByRole("button", { name: "Editar Pizza" }),
    );
    const dialog = await screen.findByRole("dialog");
    await user.click(
      within(dialog).getByRole("button", { name: "Atualizar Produto" }),
    );
    await waitFor(() => expect(requests).toBe(1));
    expect(
      within(dialog).getByRole("button", { name: /Atualizando/ }),
    ).toBeDisabled();
    expect(within(dialog).getByLabelText("Nome do Produto")).toBeDisabled();
    await user.keyboard("{Escape}");
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    finish();
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );
    expect(requests).toBe(1);
  });
});
