// Telefone vazio é válido (campo opcional); com formatação ou não, exige 10 ou 11 dígitos.
export function isValidBrazilianPhone(
  phone: string | undefined | null,
): boolean {
  if (!phone || phone.trim() === "") return true;

  const digitsOnly = phone.replace(/\D/g, "");

  return digitsOnly.length >= 10 && digitsOnly.length <= 11;
}
