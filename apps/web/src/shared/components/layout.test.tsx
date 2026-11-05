import { fireEvent, render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ThemeProvider } from "@/shared/contexts/theme-context";
import { MorePage } from "@/shared/pages/more-page";
import { Layout } from "./layout";

const state = vi.hoisted(() => ({
  mobile: false,
  logout: vi.fn(async () => {}),
}));
vi.mock("@/shared/hooks/use-mobile", () => ({
  useIsMobile: () => state.mobile,
}));
vi.mock("@/features/cash/hooks/use-current-cash-session", () => ({
  useCurrentCashSession: () => ({
    currentSession: null,
    isLoading: false,
    error: null,
  }),
}));
vi.mock("@/shared/contexts/auth-context", () => ({
  useAuth: () => ({
    user: { name: "Operador de teste" },
    logout: state.logout,
  }),
}));

function renderLayout(path = "/orders") {
  const router = createMemoryRouter(
    [
      {
        element: <Layout />,
        children: [
          { path: "/more", element: <MorePage /> },
          {
            path: "*",
            element: (
              <>
                <h1>Tela existente</h1>
                <input aria-label="Observação" />
              </>
            ),
          },
        ],
      },
      { path: "/login", element: <h1>Login</h1> },
    ],
    { initialEntries: [path] },
  );
  const view = render(
    <ThemeProvider defaultTheme="dark" storageKey="devlivery@theme">
      <RouterProvider router={router} />
    </ThemeProvider>,
  );
  return { ...view, router };
}

beforeEach(() => {
  state.mobile = false;
  state.logout.mockClear();
});

describe("Layout", () => {
  it("preserves destinations and marks nested routes without selecting the root", () => {
    renderLayout("/orders/123");
    const nav = screen.getByRole("navigation", { name: "Navegação principal" });
    expect(within(nav).getByRole("link", { name: "Pedidos" })).toHaveAttribute(
      "aria-current",
      "page",
    );
    expect(
      within(nav).getByRole("link", { name: "Resumo" }),
    ).not.toHaveAttribute("aria-current");
    expect(within(nav).getByRole("link", { name: "Despesas" })).toHaveAttribute(
      "href",
      "/expenses",
    );
    expect(
      screen.getByRole("heading", { name: "Tela existente" }),
    ).toBeVisible();
  });

  it("persists the collapsed sidebar and restores it on remount", async () => {
    const user = userEvent.setup();
    const first = renderLayout();
    await user.click(
      screen.getByRole("button", { name: "Recolher navegação" }),
    );
    expect(localStorage.getItem("devlivery@sidebar-expanded")).toBe("false");
    first.unmount();
    renderLayout();
    expect(
      screen.getByRole("button", { name: "Expandir navegação" }),
    ).toHaveAttribute("aria-expanded", "false");
    await user.click(
      screen.getByRole("button", { name: "Expandir navegação" }),
    );
    expect(localStorage.getItem("devlivery@sidebar-expanded")).toBe("true");
  });

  it("does not collapse while typing the native shortcut in a field", () => {
    renderLayout();
    fireEvent.keyDown(screen.getByRole("textbox", { name: "Observação" }), {
      key: "b",
      ctrlKey: true,
    });
    expect(
      screen.getByRole("button", { name: "Recolher navegação" }),
    ).toHaveAttribute("aria-expanded", "true");
  });

  it("offers four mobile destinations and reaches products through More", async () => {
    state.mobile = true;
    const user = userEvent.setup();
    renderLayout("/expenses");
    expect(
      screen.queryByRole("navigation", { name: "Navegação principal" }),
    ).not.toBeInTheDocument();
    const nav = screen.getByRole("navigation", { name: "Navegação móvel" });
    expect(within(nav).getAllByRole("link")).toHaveLength(4);
    expect(within(nav).getByRole("link", { name: "Mais" })).toHaveAttribute(
      "aria-current",
      "page",
    );
    await user.click(within(nav).getByRole("link", { name: "Mais" }));
    expect(await screen.findByRole("heading", { name: "Mais" })).toBeVisible();
    await user.click(screen.getByRole("link", { name: "Produtos" }));
    expect(
      await screen.findByRole("heading", { name: "Tela existente" }),
    ).toBeVisible();
  });

  it("changes the theme and logs out from the sidebar", async () => {
    const user = userEvent.setup();
    renderLayout();
    await user.click(screen.getByRole("button", { name: "Alternar tema" }));
    expect(localStorage.getItem("devlivery@theme")).toBe("light");
    await user.click(screen.getByRole("button", { name: "Sair" }));
    expect(state.logout).toHaveBeenCalledOnce();
    expect(await screen.findByRole("heading", { name: "Login" })).toBeVisible();
  });

  it("opens More directly and offers theme and logout on mobile", async () => {
    state.mobile = true;
    const user = userEvent.setup();
    renderLayout("/more");
    expect(screen.getByRole("link", { name: "Despesas" })).toHaveAttribute(
      "href",
      "/expenses",
    );
    await user.click(screen.getByRole("button", { name: "Altere o tema" }));
    expect(localStorage.getItem("devlivery@theme")).toBe("light");
    await user.click(screen.getByRole("button", { name: "Sair" }));
    expect(state.logout).toHaveBeenCalledOnce();
    expect(await screen.findByRole("heading", { name: "Login" })).toBeVisible();
  });
});
