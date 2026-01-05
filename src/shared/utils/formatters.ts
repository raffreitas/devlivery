/**
 * Formats a Date object into a string in the format `YYYY-MM-DD`.
 * @param {Date} d - The Date object to format
 * @returns `YYYY-MM-DD`
 */
export const formatDate = (d: Date) => {
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
};

/**
 * Formats a number or string into Brazilian Real currency format.
 * @param value - The numeric value or string to format
 * @returns The formatted currency string in BRL
 */
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

/**
 * Removes all non-digit characters from the input string.
 * @param value - The string from which to remove non-digit characters
 * @returns The string containing only digit characters
 */
export const removeChars = (value: string) => {
  return value.replace(/\D/g, "");
};

/**
 * Parses an ISO date string (yyyy-MM-dd) as a local date without UTC conversion.
 * This prevents timezone offset issues where "2026-01-04" would be parsed as
 * 2026-01-03 23:00 in UTC-3 timezone.
 * @param dateString - ISO date string (yyyy-MM-dd) or legacy dd/MM format
 * @returns Date object in local timezone, or null if invalid
 */
export const parseLocalDate = (dateString: string): Date | null => {
  // Try ISO format first (yyyy-MM-dd)
  const isoMatch = dateString.match(/^(\d{4})-(\d{2})-(\d{2})$/);
  if (isoMatch) {
    const [, year, month, day] = isoMatch;
    return new Date(Number(year), Number(month) - 1, Number(day));
  }

  // Fallback for legacy dd/MM format
  const legacyMatch = dateString.match(/^(\d{1,2})\/(\d{1,2})$/);
  if (legacyMatch) {
    const [, day, month] = legacyMatch;
    return new Date(new Date().getFullYear(), Number(month) - 1, Number(day));
  }

  return null;
};
