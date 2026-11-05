import { QueryClientProvider } from "@tanstack/react-query";
import { createTestClient } from "@test/render";
import { server } from "@test/server";
import { act, render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { SidebarProvider } from "@/shared/components/ui/sidebar";
import type { CashHistoryResponse } from "../services/cash-history-service";
import type { CashSessionSummary } from "../services/cash-overview-service";
import { CashHistoryPage } from "./cash-history-page";

const viewport = vi.hoisted(() => ({ desktop: true }));
vi.mock("@/shared/hooks/use-media-query", () => ({
  useMediaQuery: () => viewport.desktop,
}));
vi.mock("@/shared/components/date-range-picker", () => ({
  DateRangePicker: ({
    onDateChange,
  }: {
    onDateChange: (value: { from: Date; to?: Date }) => void;
  }) => (
    <>
      <button
        type="button"
        onClick={() => onDateChange({ from: new Date(2026, 9, 1) })}
      >
        Escolher início
      </button>
      <button
        type="button"
        onClick={() =>
          onDateChange({
            from: new Date(2026, 9, 1),
            to: new Date(2026, 9, 31),
          })
        }
      >
        Escolher período
      </button>
    </>
  ),
}));
const base = "http://localhost:5000/api/v2/cash-register/sessions";
const session: CashSessionSummary = {
  id: "turn-1",
  status: "Closed",
  attendant: "Ana Lima",
  startAt: "2026-11-01T21:00:00Z",
  endAt: "2026-11-02T02:00:00Z",
  openingAmount: 100,
  closingAmount: 170,
  expectedCashAmount: 180,
  difference: -10,
  totalRevenue: 80,
  totalOrders: 1,
  depositsTotal: 0,
  cashPayments: 100,
  cashRefunds: 0,
  changeAmount: 20,
  netCashReceived: 80,
  notes: "Turno conferido",
  paymentBreakdown: [],
};
const summary = {
  closedCount: 2,
  openCount: 1,
  totalRevenue: 180,
  totalOrders: 2,
  totalDifference: -10,
  sessionsWithDifference: 1,
};
const metadata = {
  currentPage: 1,
  pageSize: 50,
  totalPages: 2,
  totalCount: 51,
};
const resource: CashHistoryResponse = {
  data: { sessions: [session], summary },
  meta: metadata,
};
const movement = {
  id: "move-1",
  type: "Payment",
  createdAt: session.startAt,
  signedAmount: 100,
  method: "Cash",
  relatedOrderId: "12345678-1111-1111-1111-12345678abcd",
  authorName: "Ana Lima",
  createdBy: "operator",
  reason: null,
};
function setup(url = "/cash/history?start=2026-11-01&end=2026-11-05&page=1") {
  const client = createTestClient();
  const router = createMemoryRouter(
    [
      {
        path: "/cash/history",
        element: (
          <SidebarProvider>
            <CashHistoryPage />
          </SidebarProvider>
        ),
      },
      { path: "/cash", element: <h1>Caixa atual</h1> },
    ],
    { initialEntries: [url] },
  );
  render(
    <QueryClientProvider client={client}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  );
  return { router, client };
}
beforeEach(() => {
  viewport.desktop = true;
  server.use(
    http.get(base, ({ request }) => {
      const page = Number(new URL(request.url).searchParams.get("page"));
      return HttpResponse.json({
        ...resource,
        meta: { ...metadata, currentPage: page },
      });
    }),
    http.get(`${base}/:id`, () => HttpResponse.json({ data: session })),
    http.get(`${base}/:id/movements`, ({ request }) => {
      const page = Number(new URL(request.url).searchParams.get("page"));
      return HttpResponse.json({
        data: [
          {
            ...movement,
            id: `move-${page}`,
            type: page === 1 ? "Payment" : "Change",
          },
        ],
        meta: { ...metadata, currentPage: page },
      });
    }),
  );
});

describe("CashHistoryPage", () => {
  it("initializes a 30-day URL and shows consolidated totals without selecting a turn", async () => {
    const { router } = setup("/cash/history");
    expect(await screen.findByText("Turnos fechados")).toBeVisible();
    const params = new URLSearchParams(router.state.location.search);
    const first = new Date(`${params.get("start")}T12:00:00Z`);
    const last = new Date(`${params.get("end")}T12:00:00Z`);
    expect((last.getTime() - first.getTime()) / 86400000).toBe(29);
    expect(params.get("page")).toBe("1");
    expect(params.get("end")).toBe(
      new Intl.DateTimeFormat("en-CA", {
        timeZone: "America/Sao_Paulo",
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
      }).format(new Date()),
    );
    expect(screen.getByText("1 turno aberto no período")).toBeVisible();
    expect(screen.getByText("R$ 180,00")).toBeVisible();
    expect(
      screen.queryByRole("region", { name: "Detalhes do turno" }),
    ).not.toBeInTheDocument();
    expect(
      within(screen.getByRole("navigation", { name: "Breadcrumb" })).getByRole(
        "link",
        { name: "Caixa" },
      ),
    ).toHaveAttribute("href", "/cash");
  });

  it("opens detail and its independent ledger, restores focus and supports browser back", async () => {
    const user = userEvent.setup();
    const { router } = setup();
    const trigger = await screen.findByRole("link", { name: /Ver turno de/ });
    await user.click(trigger);
    const detail = screen.getByRole("region", { name: "Detalhes do turno" });
    expect(
      await within(detail).findByText("Observação do turno: Turno conferido"),
    ).toBeVisible();
    expect(await within(detail).findByText("Pedido #5678ABCD")).toBeVisible();
    await user.click(within(detail).getByRole("button", { name: "Próxima" }));
    expect(await within(detail).findByText("Troco")).toBeVisible();
    expect(new URLSearchParams(router.state.location.search).get("page")).toBe(
      "1",
    );
    await user.click(
      within(detail).getByRole("button", { name: "Fechar detalhes" }),
    );
    await waitFor(() => expect(trigger).toHaveFocus());
    expect(router.state.location.search).not.toContain("sessionId");
    await act(() => router.navigate(-1));
    expect(
      await screen.findByRole("region", { name: "Detalhes do turno" }),
    ).toBeVisible();
  });

  it("resets selection on pagination and applies only a complete new period", async () => {
    const user = userEvent.setup();
    const { router } = setup(
      "/cash/history?start=2026-11-01&end=2026-11-05&page=1&sessionId=turn-1",
    );
    await screen.findByText("Turnos fechados");
    const table = screen.getByRole("table", { name: "Turnos do caixa" });
    const list = table.closest('[data-slot="card"]') as HTMLElement;
    await user.click(within(list).getByRole("button", { name: "Próxima" }));
    await waitFor(() =>
      expect(router.state.location.search).toContain("page=2"),
    );
    expect(router.state.location.search).not.toContain("sessionId");
    expect(await screen.findByText("R$ 180,00")).toBeVisible();
    await user.click(screen.getByRole("button", { name: "Escolher início" }));
    expect(router.state.location.search).toContain("start=2026-11-01");
    await user.click(screen.getByRole("button", { name: "Escolher período" }));
    expect(router.state.location.search).toContain("start=2026-10-01");
    expect(router.state.location.search).toContain("end=2026-10-31");
    expect(router.state.location.search).toContain("page=1");
  });

  it("keeps the list usable when detail fails and distinguishes an empty list from a failure", async () => {
    server.use(
      http.get(`${base}/:id`, () =>
        HttpResponse.json({ detail: "Caixa não encontrado." }, { status: 404 }),
      ),
    );
    const { client } = setup(
      "/cash/history?start=2026-11-01&end=2026-11-05&page=1&sessionId=missing",
    );
    expect(await screen.findByText("Caixa não encontrado.")).toBeVisible();
    expect(
      screen.getByRole("table", { name: "Turnos do caixa" }),
    ).toBeVisible();
    server.use(
      http.get(base, () =>
        HttpResponse.json({ detail: "Consulta indisponível" }, { status: 500 }),
      ),
    );
    await act(async () => {
      await client.invalidateQueries({
        queryKey: ["cash-sessions", "history"],
      });
    });
    expect(await screen.findByText("Dados desatualizados")).toBeVisible();
    expect(
      screen.getByRole("table", { name: "Turnos do caixa" }),
    ).toBeVisible();
    server.use(
      http.get(base, () =>
        HttpResponse.json({
          data: { sessions: [], summary: { ...summary, closedCount: 0 } },
          meta: { ...metadata, totalCount: 0, totalPages: 0 },
        }),
      ),
    );
    await act(async () => {
      await client.invalidateQueries({
        queryKey: ["cash-sessions", "history"],
      });
    });
    expect(await screen.findByText("Nenhum turno no período")).toBeVisible();
  });

  it("uses a drawer on smaller screens and keeps an open turn read-only", async () => {
    viewport.desktop = false;
    server.use(
      http.get(`${base}/:id`, () =>
        HttpResponse.json({
          data: {
            ...session,
            status: "Open",
            closingAmount: null,
            difference: null,
            endAt: null,
          },
        }),
      ),
    );
    const user = userEvent.setup();
    setup();
    const trigger = await screen.findByRole("link", { name: /Ver turno de/ });
    await user.click(trigger);
    const drawer = await screen.findByRole("dialog", {
      name: "Detalhes do turno",
    });
    expect(await within(drawer).findByText("Caixa aberto")).toBeVisible();
    expect(
      within(drawer).getByRole("link", { name: "Ir para o turno atual" }),
    ).toHaveAttribute("href", "/cash");
    expect(
      within(drawer).queryByRole("button", { name: "Fechar caixa" }),
    ).not.toBeInTheDocument();
    await user.click(
      within(drawer).getByRole("button", { name: "Fechar detalhes" }),
    );
    await waitFor(() => expect(trigger).toHaveFocus());
  });

  it("shows initial loading and failure without treating them as empty history", async () => {
    let finish!: () => void;
    const waiting = new Promise<void>((resolve) => {
      finish = resolve;
    });
    server.use(
      http.get(base, async () => {
        await waiting;
        return HttpResponse.json(
          { detail: "Histórico indisponível" },
          { status: 500 },
        );
      }),
    );
    setup();
    expect(await screen.findByText("Carregando...")).toBeVisible();
    expect(
      screen.queryByText("Nenhum turno no período"),
    ).not.toBeInTheDocument();
    finish();
    expect(await screen.findByText("Histórico indisponível")).toBeVisible();
    expect(screen.queryByText("Turnos fechados")).not.toBeInTheDocument();
  });

  it("refreshes history and details through the existing cash invalidation prefix", async () => {
    const { client } = setup(
      "/cash/history?start=2026-11-01&end=2026-11-05&page=1&sessionId=turn-1",
    );
    await screen.findByText("Observação do turno: Turno conferido");
    server.use(
      http.get(base, () =>
        HttpResponse.json({
          ...resource,
          data: {
            ...resource.data,
            summary: { ...summary, totalRevenue: 250 },
          },
        }),
      ),
      http.get(`${base}/:id`, () =>
        HttpResponse.json({
          data: { ...session, notes: "Atualizado após operação" },
        }),
      ),
    );
    await act(async () => {
      await client.invalidateQueries({ queryKey: ["cash-sessions"] });
    });
    expect(await screen.findByText("R$ 250,00")).toBeVisible();
    expect(
      await screen.findByText("Observação do turno: Atualizado após operação"),
    ).toBeVisible();
  });
});
