import { describe, expect, it } from "vitest";
import { itemTotal, moneyDifference, sumMoney, toCents } from "./money";

describe("money", () => {
  it("calculates exact totals and change in cents", () => {
    expect(itemTotal(9.9, 3)).toBe(29.7);
    expect(sumMoney([0.1, 0.2])).toBe(0.3);
    expect(moneyDifference(30, 29.7)).toBe(0.3);
    expect(toCents(29.7)).toBe(2970);
  });

  it("includes zero when calculating cash shortages", () => {
    expect(moneyDifference(0, 500)).toBe(-500);
    expect(moneyDifference(500, 500)).toBe(0);
  });
});
