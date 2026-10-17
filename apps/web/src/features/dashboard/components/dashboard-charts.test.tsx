import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { categoryChartKey } from "../utils/chart-data";
import { ExpenseStatusChart } from "./expense-status-chart";
import { ExpensesByCategoryChart } from "./expenses-by-category-chart";
import { RevenueVsExpensesChart } from "./revenue-vs-expenses-chart";
import { SalesChart } from "./sales-chart";

describe("dashboard charts", () => {
  beforeEach(() => {
    // jsdom does not calculate layout. Keep Recharts real and provide only
    // the browser measurement it needs to render its SVG and legend.
    vi.spyOn(HTMLElement.prototype, "getBoundingClientRect").mockImplementation(
      function (this: HTMLElement) {
        const width = this.tagName === "SPAN" ? 40 : 640;
        const height = this.classList.contains("recharts-legend-wrapper")
          ? 40
          : this.tagName === "SPAN"
            ? 12
            : 350;
        return {
          x: 0,
          y: 0,
          top: 0,
          left: 0,
          right: width,
          bottom: height,
          width,
          height,
          toJSON: () => ({}),
        };
      },
    );
  });
  it("keeps every category in the legend, including categories after the sixth", async () => {
    const data = Array.from({ length: 9 }, (_, index) => ({
      category: `Categoria ${index + 1}`,
      total: index + 1,
      percentage: 10,
    }));
    const { container } = render(<ExpensesByCategoryChart data={data} />);
    await waitFor(() => {
      const chart = container.querySelector<HTMLElement>('[data-slot="chart"]');
      expect(chart).not.toBeNull();
      for (const point of data)
        expect(
          within(chart as HTMLElement).getByText(point.category),
        ).toBeInTheDocument();
    });
  });

  it("keeps the same category color after reordering and filtering", () => {
    const gas = { category: "Gás", total: 100, percentage: 50 };
    const water = { category: "Água", total: 100, percentage: 50 };
    const { container, rerender } = render(
      <ExpensesByCategoryChart data={[gas, water]} />,
    );
    const readColor = () =>
      container
        .querySelector("style")
        ?.textContent?.match(
          new RegExp(`--color-${categoryChartKey(gas.category)}: ([^;]+);`),
        )?.[1];
    const original = readColor();
    expect(original).toBeTruthy();
    rerender(<ExpensesByCategoryChart data={[water, gas]} />);
    expect(readColor()).toBe(original);
    rerender(<ExpensesByCategoryChart data={[gas]} />);
    expect(readColor()).toBe(original);
  });

  it("offers exact values in a table without depending on hover or color", async () => {
    render(
      <SalesChart
        data={[
          { date: "2026-10-17", total: 9.9 },
          { date: "2026-10-18", total: 0 },
        ]}
      />,
    );
    expect(
      screen.getByText("Ver dados em tabela").closest("details"),
    ).not.toHaveAttribute("open");
    await userEvent.click(screen.getByText("Ver dados em tabela"));
    const table = screen.getByRole("table", { name: "Vendas no Período" });
    expect(within(table).getByText("17/10/2026")).toBeInTheDocument();
    expect(
      within(table).getByRole("cell", { name: /R\$\s*9,90/ }),
    ).toBeInTheDocument();
    expect(
      within(table).getByRole("cell", { name: /R\$\s*0,00/ }),
    ).toBeInTheDocument();
  });

  it("creates independent gradient references when two financial charts are mounted", async () => {
    const data = [
      { date: "2026-10-17", total: 10 },
      { date: "2026-10-18", total: 12 },
    ];
    const { container } = render(
      <>
        <RevenueVsExpensesChart revenueData={data} expensesData={data} />
        <RevenueVsExpensesChart revenueData={data} expensesData={data} />
      </>,
    );
    await waitFor(() =>
      expect(container.querySelectorAll("linearGradient")).toHaveLength(4),
    );
    const gradients = [...container.querySelectorAll("linearGradient")];
    expect(new Set(gradients.map((gradient) => gradient.id)).size).toBe(4);
    await waitFor(() => {
      for (const gradient of gradients) {
        const svg = gradient.closest("svg");
        expect(
          svg?.querySelector(`[fill="url(#${gradient.id})"]`),
        ).not.toBeNull();
      }
    });
  });

  it("shows an unfamiliar expense status as text with its values", async () => {
    render(
      <ExpenseStatusChart
        data={[{ status: "Novo status", count: 2, total: 45 }]}
      />,
    );
    await userEvent.click(screen.getByText("Ver dados em tabela"));
    const table = screen.getByRole("table", { name: "Status das Despesas" });
    expect(within(table).getByText("Novo status")).toBeInTheDocument();
    expect(
      within(table).getByRole("cell", { name: /R\$\s*45,00/ }),
    ).toBeInTheDocument();
  });
});
