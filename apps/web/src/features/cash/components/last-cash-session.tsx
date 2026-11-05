import { StatTile } from "@/shared/components/stat-tile";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";
import type { CashSessionSummary } from "../services/cash-overview-service";
import { formatCashDateTime } from "../utils/format-cash-date-time";
import { CashComposition } from "./cash-composition";

export function LastCashSession({
  session,
}: {
  session: CashSessionSummary | null;
}) {
  return (
    <Card size="sm" className="gap-0 py-0 shadow-none">
      <CardHeader className="border-b py-4">
        <CardTitle>Último turno</CardTitle>
      </CardHeader>
      <CardContent className="gap-4 py-4">
        {session ? (
          <>
            <div className="grid gap-3 sm:grid-cols-3">
              <StatTile
                label="Recebido"
                value={formatMoney(session.totalRevenue)}
                foot={`${session.totalOrders} pedidos com recebimento`}
              />
              <StatTile
                label="Contado no fechamento"
                value={
                  session.closingAmount === null
                    ? "—"
                    : formatMoney(session.closingAmount)
                }
                foot="Em dinheiro"
              />
              <StatTile
                label="Diferença"
                value={
                  session.difference === null
                    ? "—"
                    : formatMoney(session.difference)
                }
                foot={
                  session.difference === null
                    ? "Não informada"
                    : session.difference < 0
                      ? "Faltou na gaveta"
                      : session.difference > 0
                        ? "Sobrou na gaveta"
                        : "Caixa conferido"
                }
              />
            </div>
            <p className="flex flex-wrap justify-between gap-2 text-meta">
              <span className="text-muted-foreground">Turno</span>
              <span>
                {formatCashDateTime(session.startAt)} até{" "}
                {session.endAt ? formatCashDateTime(session.endAt) : "—"} ·{" "}
                {session.attendant}
              </span>
            </p>
            <CashComposition
              {...session}
              counted={session.closingAmount ?? undefined}
            />
            <div className="flex justify-between gap-3 text-ui">
              <span>Diferença</span>
              <span
                className={
                  session.difference !== null && session.difference < 0
                    ? "text-destructive"
                    : "text-foreground"
                }
              >
                {session.difference === null
                  ? "—"
                  : formatMoney(session.difference)}
              </span>
            </div>
            {session.notes && (
              <p className="text-meta text-muted-foreground">
                Observação do turno: {session.notes}
              </p>
            )}
          </>
        ) : (
          <p className="py-6 text-ui text-muted-foreground">
            Nenhum turno encerrado. O resumo aparecerá após o primeiro
            fechamento.
          </p>
        )}
      </CardContent>
    </Card>
  );
}
