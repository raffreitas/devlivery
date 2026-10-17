import { describe, expect, it } from "vitest";
import {
  categoryChartKey,
  combineFinancialSeries,
  formatChartDate,
  formatChartFullDate,
} from "./chart-data";

describe("dashboard chart data", () => {
  it("aligns disjoint dates chronologically, keeps zeros and does not mutate API data", () => {
    const revenue = [
      { date: "2026-10-17", total: 9.9 },
      { date: "2026-10-15", total: 0 },
    ];
    const expenses = [
      { date: "2026-10-16", total: 5 },
      { date: "2026-10-17", total: 1.1 },
    ];
    expect(combineFinancialSeries(revenue, expenses)).toEqual([
      { date: "2026-10-15", revenue: 0, expenses: 0 },
      { date: "2026-10-16", revenue: 0, expenses: 5 },
      { date: "2026-10-17", revenue: 9.9, expenses: 1.1 },
    ]);
    expect(revenue[0]).toEqual({ date: "2026-10-17", total: 9.9 });
    expect(expenses).toHaveLength(2);
  });

  it("formats date-only API values without shifting the day", () => {
    expect(formatChartDate("2026-10-17")).toBe("17/10");
    expect(formatChartFullDate("2026-10-17")).toBe("17/10/2026");
    expect(formatChartDate("indisponível")).toBe("indisponível");
  });

  it("encodes category identities safely and distinguishes punctuation and accents", () => {
    const names = [
      "Água / Gás",
      "Agua / Gas",
      "A-B",
      "AB",
      "</style><script>alert(1)</script>",
    ];
    const keys = names.map(categoryChartKey);
    expect(new Set(keys).size).toBe(names.length);
    for (const key of keys) expect(key).toMatch(/^category-[a-f0-9-]+$/);
  });
});
