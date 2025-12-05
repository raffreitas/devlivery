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
