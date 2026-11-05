import { formatMoney } from "@/shared/utils/formatters";

export function CashComposition({
  openingAmount,
  depositsTotal,
  cashPayments,
  cashRefunds,
  changeAmount,
  expectedCashAmount,
  counted,
}: {
  openingAmount: number;
  depositsTotal: number;
  cashPayments: number;
  cashRefunds: number;
  changeAmount: number;
  expectedCashAmount: number;
  counted?: number;
}) {
  const rows = [
    ["Abertura", formatMoney(openingAmount)],
    ["Aportes", `+ ${formatMoney(depositsTotal)}`],
    ["Recebido em dinheiro", `+ ${formatMoney(cashPayments)}`],
    ["Trocos", `- ${formatMoney(changeAmount)}`],
    ["Estornos em dinheiro", `− ${formatMoney(cashRefunds)}`],
    ["Sangrias", "- · Em breve"],
  ];
  return (
    <dl className="flex flex-col gap-2 text-ui">
      {rows.map(([label, value]) => (
        <div key={label} className="flex justify-between gap-3">
          <dt className="text-meta text-muted-foreground">{label}</dt>
          <dd className="text-right tabular-nums">{value}</dd>
        </div>
      ))}
      <div className="flex justify-between gap-3 border-t pt-3 font-semibold">
        <dt>Esperado em dinheiro</dt>
        <dd className="tabular-nums">{formatMoney(expectedCashAmount)}</dd>
      </div>
      {counted !== undefined && (
        <div className="flex justify-between gap-3">
          <dt>Contado</dt>
          <dd className="tabular-nums">{formatMoney(counted)}</dd>
        </div>
      )}
    </dl>
  );
}
