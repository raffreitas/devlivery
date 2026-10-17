export const formatDate = (d: Date) => {
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
};

export const formatMoney = (value: number | string | undefined) => {
  if (value === undefined || value === null) return "";

  const formatter = new Intl.NumberFormat("pt-br", {
    style: "currency",
    currency: "BRL",
  });

  // If a number is provided, treat it as a value in reais already
  // (e.g. 2 -> R$ 2,00). If a string is provided (from raw input),
  // strip non-digits, parse cents and divide by 100.
  if (typeof value === "number") {
    return formatter.format(value);
  }

  const valueWithoutChars = value.toString().replace(/\D/g, "").trim() || "0";
  const numericValue = parseInt(valueWithoutChars, 10) / 100;

  return formatter.format(numericValue);
};

export const removeChars = (value: string) => {
  return value.replace(/\D/g, "");
};

// Parses yyyy-MM-dd as a local date; `new Date("2026-01-04")` would be UTC and
// shift to the previous day in UTC-3.
export const parseLocalDate = (dateString: string): Date | null => {
  const isoMatch = dateString.match(/^(\d{4})-(\d{2})-(\d{2})$/);
  if (isoMatch) {
    const [, year, month, day] = isoMatch;
    return new Date(Number(year), Number(month) - 1, Number(day));
  }

  // Fallback for legacy dd/MM format (e.g. "04/01" for January 4th)
  const legacyMatch = dateString.match(/^(\d{1,2})\/(\d{1,2})$/);
  if (legacyMatch) {
    const [, day, month] = legacyMatch;
    return new Date(new Date().getFullYear(), Number(month) - 1, Number(day));
  }

  return null;
};
