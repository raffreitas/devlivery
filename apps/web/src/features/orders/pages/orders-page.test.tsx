import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { beforeEach, describe, expect, it } from "vitest";
import { OrdersPage } from "./orders-page";

describe("order draft dismissal", () => {
  beforeEach(() => {
    server.use(
      http.get("http://localhost:5000/api/v1/orders", () =>
        HttpResponse.json({ data: [] }),
      ),
      http.get("http://localhost:5000/api/v1/products", () =>
        HttpResponse.json({ data: [] }),
      ),
      http.get(
        "http://localhost:5000/api/v1/cash-register/sessions/active",
        () => new HttpResponse(null, { status: 404 }),
      ),
    );
  });

  it("preserves a filled order after Escape until the operator explicitly discards it", async () => {
    const user = userEvent.setup();
    renderWithProviders(<OrdersPage />);
    await user.click(screen.getByRole("button", { name: "Novo Pedido" }));
    await user.type(
      screen.getByRole("textbox", { name: "Nome do Cliente" }),
      "Cliente WhatsApp",
    );
    await user.keyboard("{Escape}");
    expect(await screen.findByRole("alertdialog")).toHaveTextContent(
      "Descartar alterações do pedido?",
    );
    await user.click(
      screen.getByRole("button", { name: "Continuar preenchendo" }),
    );
    expect(
      screen.getByRole("textbox", { name: "Nome do Cliente" }),
    ).toHaveValue("Cliente WhatsApp");
    await user.click(screen.getByRole("button", { name: "Cancelar" }));
    await user.click(await screen.findByRole("button", { name: "Descartar" }));
    await waitFor(() =>
      expect(
        screen.queryByRole("textbox", { name: "Nome do Cliente" }),
      ).not.toBeInTheDocument(),
    );
    await user.click(screen.getByRole("button", { name: "Novo Pedido" }));
    expect(
      screen.getByRole("textbox", { name: "Nome do Cliente" }),
    ).toHaveValue("");
  });

  it("closes an untouched form without asking for discard", async () => {
    const user = userEvent.setup();
    renderWithProviders(<OrdersPage />);
    await user.click(screen.getByRole("button", { name: "Novo Pedido" }));
    await user.keyboard("{Escape}");
    await waitFor(() =>
      expect(
        screen.queryByRole("textbox", { name: "Nome do Cliente" }),
      ).not.toBeInTheDocument(),
    );
    expect(screen.queryByRole("alertdialog")).not.toBeInTheDocument();
  });
});
