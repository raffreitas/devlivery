import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it, vi } from "vitest";
import { SidebarFooterContent } from "./sidebar-footer-content";
import { SidebarProvider } from "./ui/sidebar";

vi.mock("@/shared/contexts/auth-context", () => ({
  useAuth: () => ({ user: { name: "Ana Lima" } }),
}));

const url = "http://localhost:5000/api/v1/cash-register/sessions/active";

function renderFooter() {
  return renderWithProviders(
    <SidebarProvider>
      <SidebarFooterContent onLogout={async () => {}} />
    </SidebarProvider>,
  );
}

describe("SidebarFooterContent", () => {
  it("shows the current cash opening time in Sao Paulo and the operator initials", async () => {
    server.use(
      http.get(url, () =>
        HttpResponse.json({
          data: {
            id: "session-1",
            attendantId: "operator-1",
            attendantName: "Ana Lima",
            openingAmount: 0,
            closingAmount: null,
            expectedCashAmount: 0,
            totalRevenue: 0,
            totalOrders: 0,
            paymentBreakdown: [],
            startAt: "2026-11-05T21:20:00Z",
            endAt: null,
            status: "Open",
            notes: null,
          },
        }),
      ),
    );
    const { client } = renderFooter();
    expect(
      await screen.findByRole("link", { name: "Caixa aberto, desde 18:20" }),
    ).toHaveAttribute("href", "/cash");
    expect(screen.getByText("AL")).toBeInTheDocument();
    expect(
      screen.queryByRole("menuitem", { name: "Sair" }),
    ).not.toBeInTheDocument();

    server.use(http.get(url, () => new HttpResponse(null, { status: 404 })));
    await client.invalidateQueries({ queryKey: ["cash-sessions"] });
    expect(
      await screen.findByRole("link", { name: "Caixa fechado" }),
    ).toBeInTheDocument();
    expect(screen.queryByText("desde 18:20")).not.toBeInTheDocument();
  });

  it("treats only a confirmed missing session as closed", async () => {
    server.use(http.get(url, () => new HttpResponse(null, { status: 404 })));
    renderFooter();
    expect(
      await screen.findByRole("link", { name: "Caixa fechado" }),
    ).toBeInTheDocument();
  });

  it("does not report a failed cash lookup as closed", async () => {
    server.use(http.get(url, () => new HttpResponse(null, { status: 500 })));
    renderFooter();
    expect(
      await screen.findByRole("link", { name: "Caixa indisponível" }),
    ).toBeInTheDocument();
    expect(screen.queryByText("Caixa fechado")).not.toBeInTheDocument();
  });

  it("hides the cash indicator and keeps logout and theme accessible when collapsed", async () => {
    server.use(http.get(url, () => new HttpResponse(null, { status: 404 })));
    const user = userEvent.setup();
    renderWithProviders(
      <SidebarProvider defaultOpen={false}>
        <SidebarFooterContent onLogout={async () => {}} />
      </SidebarProvider>,
    );
    expect(
      screen.queryByRole("link", { name: "Caixa fechado" }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Alternar tema" }),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Sair" }));
    expect(screen.queryByRole("menu")).not.toBeInTheDocument();
  });
});
