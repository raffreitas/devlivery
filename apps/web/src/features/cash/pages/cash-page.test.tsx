import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { fireEvent, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it, vi } from "vitest";
import { SidebarProvider } from "@/shared/components/ui/sidebar";
import type {
  CashOverview,
  CashSessionSummary,
} from "../services/cash-overview-service";
import { CashPage as CashPageComponent } from "./cash-page";

function CashPage() {
  return (
    <SidebarProvider>
      <CashPageComponent />
    </SidebarProvider>
  );
}

const base = "http://localhost:5000/api/v1/cash-register/sessions";
const overviewUrl = "http://localhost:5000/api/v2/cash-register/overview";
const movementsUrl =
  "http://localhost:5000/api/v2/cash-register/sessions/session-1/movements";
const session: CashSessionSummary = {
  id: "session-1",
  status: "Open",
  attendant: "Ana Lima",
  openingAmount: 100,
  closingAmount: null,
  expectedCashAmount: 180,
  totalRevenue: 180,
  totalOrders: 2,
  depositsTotal: 0,
  cashPayments: 100,
  cashRefunds: 0,
  changeAmount: 20,
  netCashReceived: 80,
  difference: null,
  paymentBreakdown: [
    { method: "Cash", amount: 80, count: 1 },
    { method: "Pix", amount: 100, count: 1 },
  ],
  startAt: "2026-11-05T21:20:00Z",
  endAt: null,
  notes: null,
};
const openOverview: CashOverview = {
  activeSession: session,
  lastClosedSession: null,
  receivableCash: { amount: 80, orderCount: 1 },
};
const emptyOverview: CashOverview = {
  activeSession: null,
  lastClosedSession: null,
  receivableCash: null,
};
const entry = {
  id: "entry-1",
  type: "Payment",
  createdAt: "2026-11-05T22:00:00Z",
  signedAmount: 100,
  method: "Cash",
  relatedOrderId: "12345678-1111-1111-1111-12345678abcd",
  orderPaymentId: "payment-1",
  createdBy: "operator-1",
  authorName: "Ana Lima",
  reason: null,
};
const meta = { currentPage: 1, totalCount: 1, pageSize: 50, totalPages: 1 };
function mockOpenSession() {
  server.use(
    http.get(overviewUrl, () => HttpResponse.json({ data: openOverview })),
    http.get(movementsUrl, () => HttpResponse.json({ data: [entry], meta })),
  );
}
function v1Session() {
  return { ...session, attendantName: session.attendant, status: "Open" };
}

describe("CashPage", () => {
  it("shows the opening form and last-turn empty state only after a successful overview", async () => {
    server.use(
      http.get(overviewUrl, () => HttpResponse.json({ data: emptyOverview })),
    );
    renderWithProviders(<CashPage />);
    expect(
      await screen.findByRole("button", { name: "Abrir caixa" }),
    ).toBeEnabled();
    expect(screen.getByText(/Nenhum turno encerrado/)).toBeVisible();
    expect(
      screen.getByText(/Pedidos funcionam sem caixa aberto/),
    ).toBeVisible();
  });

  it("does not turn a failed overview into a closed register", async () => {
    server.use(
      http.get(overviewUrl, () =>
        HttpResponse.json({ detail: "Servidor indisponível" }, { status: 500 }),
      ),
    );
    renderWithProviders(<CashPage />);
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Servidor indisponível",
    );
    expect(
      screen.queryByRole("button", { name: "Abrir caixa" }),
    ).not.toBeInTheDocument();
  });

  it("shows the full ledger, pending cash and a non-operational withdrawal placeholder", async () => {
    mockOpenSession();
    renderWithProviders(<CashPage />);
    expect(await screen.findByText("Recebido no turno")).toBeVisible();
    expect(
      within(
        screen.getByRole("navigation", { name: "Breadcrumb" }),
      ).queryByText(/Aberto|Fechado/),
    ).not.toBeInTheDocument();
    expect(screen.getByText("1 pedido com dinheiro a receber")).toBeVisible();
    expect(screen.getByRole("button", { name: "Sangria" })).toBeDisabled();
    const table = await screen.findByRole("table", {
      name: "Movimentações do turno",
    });
    expect(await within(table).findByText("Pedido #5678ABCD")).toBeVisible();
    expect(within(table).getByText("Pagamento")).toBeVisible();
    expect(screen.queryByText("Esperado em dinheiro")).not.toBeInTheDocument();
    expect(
      screen.queryByRole("button", {
        name: "Recebimentos por forma de pagamento",
      }),
    ).not.toBeInTheDocument();
  });

  it("shows last closed results and session notes", async () => {
    const last = {
      ...session,
      closingAmount: 170,
      difference: -10,
      endAt: "2026-11-05T23:00:00Z",
      notes: "Conferência final",
    };
    server.use(
      http.get(overviewUrl, () =>
        HttpResponse.json({
          data: { ...emptyOverview, lastClosedSession: last },
        }),
      ),
    );
    renderWithProviders(<CashPage />);
    expect(await screen.findByText("Último turno")).toBeVisible();
    expect(screen.getByText("Faltou na gaveta")).toBeVisible();
    expect(
      screen.getByText("Observação do turno: Conferência final"),
    ).toBeVisible();
  });

  it("opens through v1 and refreshes the v2 overview", async () => {
    const user = userEvent.setup();
    let opened = false;
    const create = vi.fn();
    server.use(
      http.get(overviewUrl, () =>
        HttpResponse.json({ data: opened ? openOverview : emptyOverview }),
      ),
      http.get(movementsUrl, () => HttpResponse.json({ data: [entry], meta })),
      http.post(base, async ({ request }) => {
        create(await request.json());
        opened = true;
        return HttpResponse.json({ data: v1Session() }, { status: 201 });
      }),
    );
    renderWithProviders(<CashPage />);
    await screen.findByRole("button", { name: "Abrir caixa" });
    fireEvent.change(
      screen.getByRole("textbox", { name: "Dinheiro na gaveta" }),
      { target: { value: "10000" } },
    );
    await user.click(screen.getByRole("button", { name: "Abrir caixa" }));
    expect(await screen.findByText("Recebido no turno")).toBeVisible();
    expect(create).toHaveBeenCalledExactlyOnceWith({ openingAmount: 100 });
  });

  it("closes with difference and no note only after confirmation, then shows the last turn", async () => {
    const user = userEvent.setup();
    let closed = false;
    const close = vi.fn();
    server.use(
      http.get(overviewUrl, () =>
        HttpResponse.json({
          data: closed
            ? {
                ...emptyOverview,
                lastClosedSession: {
                  ...session,
                  closingAmount: 170,
                  difference: -10,
                  endAt: "2026-11-05T23:00:00Z",
                },
              }
            : openOverview,
        }),
      ),
      http.get(movementsUrl, () => HttpResponse.json({ data: [entry], meta })),
      http.patch(`${base}/session-1/close`, async ({ request }) => {
        close(await request.json());
        closed = true;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    renderWithProviders(<CashPage />);
    fireEvent.change(
      await screen.findByRole("textbox", { name: "Dinheiro contado" }),
      { target: { value: "17000" } },
    );
    await user.click(screen.getByRole("button", { name: "Conferir contagem" }));
    expect(close).not.toHaveBeenCalled();
    await user.click(screen.getByRole("button", { name: "Fechar caixa" }));
    expect(
      await screen.findByRole("button", { name: "Abrir caixa" }),
    ).toBeEnabled();
    expect(screen.getByText("Faltou na gaveta")).toBeVisible();
    expect(close).toHaveBeenCalledExactlyOnceWith({ closingAmount: 170 });
  });

  it("adds an aporte through v1 and refreshes the movements and summary", async () => {
    const user = userEvent.setup();
    let deposited = false;
    const create = vi.fn();
    server.use(
      http.get(overviewUrl, () =>
        HttpResponse.json({
          data: {
            ...openOverview,
            activeSession: { ...session, depositsTotal: deposited ? 50 : 0 },
          },
        }),
      ),
      http.get(movementsUrl, () =>
        HttpResponse.json({
          data: deposited
            ? [
                {
                  ...entry,
                  type: "Deposit",
                  reason: "Troco para o turno",
                  relatedOrderId: null,
                },
              ]
            : [entry],
          meta,
        }),
      ),
      http.post(`${base}/session-1/deposits`, async ({ request }) => {
        create(await request.json());
        deposited = true;
        return HttpResponse.json(
          {
            data: {
              id: "deposit-1",
              cashSessionId: "session-1",
              attendantName: "Ana Lima",
              amount: 50,
              depositedAt: entry.createdAt,
              notes: "Troco para o turno",
            },
          },
          { status: 201 },
        );
      }),
    );
    renderWithProviders(<CashPage />);
    await user.click(await screen.findByRole("button", { name: "Aporte" }));
    const dialog = screen.getByRole("dialog");
    fireEvent.change(
      within(dialog).getByRole("textbox", { name: "Valor do aporte" }),
      { target: { value: "5000" } },
    );
    await user.type(
      within(dialog).getByRole("textbox", {
        name: "Observação (opcional)",
      }),
      "Troco para o turno",
    );
    await user.click(
      within(dialog).getByRole("button", { name: "Adicionar aporte" }),
    );
    expect(await screen.findByText("Troco para o turno")).toBeVisible();
    expect(create).toHaveBeenCalledExactlyOnceWith({
      amount: 50,
      notes: "Troco para o turno",
    });
  });

  it("paginates without altering summary totals and exposes ledger failures separately", async () => {
    const user = userEvent.setup();
    mockOpenSession();
    server.use(
      http.get(movementsUrl, ({ request }) => {
        const page = Number(new URL(request.url).searchParams.get("page"));
        return HttpResponse.json({
          data: [
            {
              ...entry,
              id: `entry-${page}`,
              type: page === 1 ? "Payment" : "Change",
              signedAmount: page === 1 ? 100 : -20,
            },
          ],
          meta: { ...meta, currentPage: page, totalCount: 51, totalPages: 2 },
        });
      }),
    );
    renderWithProviders(<CashPage />);
    await user.click(await screen.findByRole("button", { name: "Próxima" }));
    expect(await screen.findByText("Troco")).toBeVisible();
    expect(screen.getByText("2 pedidos com recebimento")).toBeVisible();
    server.use(
      http.get(movementsUrl, () =>
        HttpResponse.json({ detail: "Falha no extrato" }, { status: 500 }),
      ),
    );
    await user.click(screen.getByRole("button", { name: "Anterior" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Falha no extrato",
    );
    expect(screen.getByText("Recebido no turno")).toBeVisible();
  });
});
