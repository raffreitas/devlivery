import { Separator } from "@/shared/components/ui/separator";
import { formatMoney } from "@/shared/utils/formatters";

export function CashComposition({
  openingAmount,
  depositsTotal,
  cashPayments,
  cashRefunds,
  changeAmount,
  withdrawalsTotal,
  expectedCashAmount,
  counted,
}: {
  openingAmount: number;
  depositsTotal: number;
  cashPayments: number;
  cashRefunds: number;
  changeAmount: number;
  withdrawalsTotal: number;
  expectedCashAmount: number;
  counted?: number;
}) {
  const rows = [
    ["Abertura", formatMoney(openingAmount)],
    ["Aportes", `+ ${formatMoney(depositsTotal)}`],
    ["Recebido em dinheiro", `+ ${formatMoney(cashPayments)}`],
    ["Trocos", `− ${formatMoney(changeAmount)}`],
    ["Estornos em dinheiro", `− ${formatMoney(cashRefunds)}`],
    [
      "Sangrias",
      withdrawalsTotal > 0
        ? `− ${formatMoney(withdrawalsTotal)}`
        : formatMoney(0),
    ],
  ];
  return (
    <div className="flex flex-col gap-4 text-sm">
      <dl className="flex flex-col gap-2">
        {rows.map(([label, value]) => (
          <div key={label} className="flex justify-between gap-3">
            <dt className="text-muted-foreground">{label}</dt>
            <dd className="text-right tabular-nums">{value}</dd>
          </div>
        ))}
      </dl>
      <Separator />
      <dl className="flex flex-col gap-2">
        <div className="flex justify-between gap-3 font-medium">
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
    </div>
  );
}
