import { parseLocalDate } from "@/shared/utils/formatters";

export interface TimeSeriesPoint {
  date: string;
  total: number;
}

const shortDate = new Intl.DateTimeFormat("pt-BR", {
  day: "2-digit",
  month: "2-digit",
});
const fullDate = new Intl.DateTimeFormat("pt-BR", {
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
});
const compactMoney = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
  notation: "compact",
  maximumFractionDigits: 1,
});

export function formatChartDate(value: string) {
  const date = parseLocalDate(value);
  return date ? shortDate.format(date) : value;
}

export function formatChartFullDate(value: string) {
  const date = parseLocalDate(value);
  return date ? fullDate.format(date) : value;
}

export function formatChartMoney(value: number) {
  return compactMoney.format(value);
}

export function combineFinancialSeries(
  revenue: TimeSeriesPoint[],
  expenses: TimeSeriesPoint[],
) {
  const points = new Map<
    string,
    { date: string; revenue: number; expenses: number }
  >();
  for (const [series, items] of [
    ["revenue", revenue],
    ["expenses", expenses],
  ] as const) {
    for (const item of items) {
      const point = points.get(item.date) ?? {
        date: item.date,
        revenue: 0,
        expenses: 0,
      };
      point[series] = item.total;
      points.set(item.date, point);
    }
  }
  return [...points.values()].sort((a, b) => {
    const left = parseLocalDate(a.date);
    const right = parseLocalDate(b.date);
    return left && right ? left.getTime() - right.getTime() : 0;
  });
}

// Category names are the identity supplied by the API. Encode them before
// using ChartConfig keys, which become CSS custom properties.
export function categoryChartKey(category: string) {
  return `category-${Array.from(category, (char) => char.codePointAt(0)?.toString(16)).join("-")}`;
}

export function categoryChartColor(category: string) {
  let hash = 0;
  for (const char of category) {
    hash = (Math.imul(hash, 31) + (char.codePointAt(0) ?? 0)) >>> 0;
  }
  return `var(--chart-category-${(hash % 8) + 1})`;
}
