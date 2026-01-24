/**
 * Valida se um número de telefone brasileiro é válido.
 * Aceita números com 10 ou 11 dígitos (com ou sem formatação).
 *
 * @param phone - O número de telefone a ser validado
 * @returns true se o telefone é válido, false caso contrário
 *
 * @example
 * isValidBrazilianPhone("11987654321") // true
 * isValidBrazilianPhone("(11) 98765-4321") // true
 * isValidBrazilianPhone("1198765432") // false (9 dígitos)
 */
export function isValidBrazilianPhone(
  phone: string | undefined | null,
): boolean {
  if (!phone || phone.trim() === "") return true;

  // Remove todos os caracteres não numéricos
  const digitsOnly = phone.replace(/\D/g, "");

  return digitsOnly.length >= 10 && digitsOnly.length <= 11;
}
