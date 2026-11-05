import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { QueryState } from "@/shared/components/query-state";
import { Button } from "@/shared/components/ui/button";
import { queryKeys } from "@/shared/services/query-keys";
import { formatMoney } from "@/shared/utils/formatters";
import { cashHistoryService } from "../services/cash-history-service";
import { formatCashHistoryDateTime } from "../utils/format-cash-date-time";
import { CashComposition } from "./cash-composition";
import { CashSessionEntries } from "./cash-session-entries";

export function CashHistoryDetail({ sessionId }: { sessionId: string }) {
  const query = useQuery({
    queryKey: [...queryKeys.cash, "history-detail", sessionId],
    queryFn: ({ signal }) => cashHistoryService.get(sessionId, signal),
  });
  const session = query.data;
  return (
    <QueryState
      isPending={query.isPending}
      error={query.error}
      hasData={session !== undefined}
      refetch={query.refetch}
    >
      {session && (
        <div className="flex min-w-0 flex-col gap-4">
          <p className="text-sm text-muted-foreground">
            {formatCashHistoryDateTime(session.startAt)} até{" "}
            {session.endAt ? formatCashHistoryDateTime(session.endAt) : "agora"}{" "}
            · Aberto por {session.attendant}
          </p>
          <p className="text-sm font-medium">
            {session.status === "Open" ? "Caixa aberto" : "Caixa fechado"}
          </p>
          <CashComposition
            {...session}
            counted={
              session.status === "Closed"
                ? (session.closingAmount ?? undefined)
                : undefined
            }
          />
          <dl className="flex flex-col gap-2 text-sm">
            {(session.status === "Open" || session.closingAmount === null) && (
              <div className="flex justify-between gap-3">
                <dt>Contado</dt>
                <dd>—</dd>
              </div>
            )}
            <div className="flex justify-between gap-3">
              <dt>Diferença</dt>
              <dd
                className={
                  session.difference !== null && session.difference < 0
                    ? "text-destructive"
                    : "text-foreground"
                }
              >
                {session.status === "Closed" && session.difference !== null
                  ? formatMoney(session.difference)
                  : "—"}
              </dd>
            </div>
          </dl>
          <p className="text-sm text-muted-foreground">
            Observação do turno: {session.notes || "Nenhuma observação."}
          </p>
          {session.status === "Open" && (
            <Button variant="outline" render={<Link to="/cash" />}>
              Ir para o turno atual
            </Button>
          )}
          <CashSessionEntries key={sessionId} sessionId={sessionId} />
        </div>
      )}
    </QueryState>
  );
}
