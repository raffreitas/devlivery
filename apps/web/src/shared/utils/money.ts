/** HTTP amounts remain in reais; arithmetic is performed in integer cents. */
export function toCents(amount: number): number {
  return Math.round(amount * 100);
}

export function fromCents(cents: number): number {
  return cents / 100;
}

export function sumMoney(amounts: readonly number[]): number {
  return fromCents(
    amounts.reduce((total, amount) => total + toCents(amount), 0),
  );
}

export function itemTotal(unitPrice: number, quantity: number): number {
  return fromCents(toCents(unitPrice) * quantity);
}

export function moneyDifference(amount: number, expected: number): number {
  return fromCents(toCents(amount) - toCents(expected));
}
