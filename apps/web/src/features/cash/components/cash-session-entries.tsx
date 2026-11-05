import { useQuery } from "@tanstack/react-query";
import { Banknote, CreditCard, Smartphone } from "lucide-react";
import { useState } from "react";
import { getPaymentOptionLabel } from "@/features/orders/constants/payment-methods";
import { QueryState } from "@/shared/components/query-state";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { queryKeys } from "@/shared/services/query-keys";
import { formatMoney } from "@/shared/utils/formatters";
import { cashOverviewService } from "../services/cash-overview-service";
import {
  formatCashDateTime,
  formatCashTime,
} from "../utils/format-cash-date-time";

const labels: Record<string, string> = {
  Opening: "Abertura",
  Payment: "Pagamento",
  Change: "Troco",
  Refund: "Estorno",
  Deposit: "Aporte",
  Adjustment: "Ajuste",
};

export function CashSessionEntries({ sessionId }: { sessionId: string }) {
  const [page, setPage] = useState(1);
  const query = useQuery({
    queryKey: [...queryKeys.cash, "movements-v2", sessionId, page],
    queryFn: ({ signal }) =>
      cashOverviewService.getMovements(sessionId, page, signal),
  });
  return (
    <Card size="sm" className="gap-0 py-0 shadow-none">
      <CardHeader className="flex flex-row items-center justify-between gap-3 border-b py-4">
        <CardTitle>Movimentações do turno</CardTitle>
        <span className="text-meta text-muted-foreground">
          Mais recentes primeiro
        </span>
      </CardHeader>
      <CardContent className="px-0">
        <QueryState
          isPending={query.isPending}
          error={query.error}
          hasData={query.data !== undefined}
          refetch={query.refetch}
        >
          <Table aria-label="Movimentações do turno">
            <TableHeader>
              <TableRow>
                {["Hora", "Tipo", "Referência", "Forma", "Valor"].map(
                  (label) => (
                    <TableHead
                      key={label}
                      className={`px-4 text-meta font-normal text-muted-foreground ${label === "Valor" ? "text-right" : ""}`}
                    >
                      {label}
                    </TableHead>
                  ),
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data?.data.map((entry) => (
                <TableRow key={entry.id} className="h-11 text-ui">
                  <TableCell className="px-4 text-meta text-muted-foreground">
                    <time
                      dateTime={entry.createdAt}
                      title={formatCashDateTime(entry.createdAt)}
                    >
                      {formatCashTime(entry.createdAt)}
                    </time>
                  </TableCell>
                  <TableCell className="px-4 font-medium">
                    {labels[entry.type] || entry.type}
                  </TableCell>
                  <TableCell className="max-w-64 whitespace-normal px-4 text-meta text-muted-foreground">
                    {entry.relatedOrderId ? (
                      <span title={entry.relatedOrderId}>
                        Pedido #{entry.relatedOrderId.slice(-8).toUpperCase()}
                      </span>
                    ) : (
                      entry.authorName
                    )}
                    {entry.reason && (
                      <span className="block">{entry.reason}</span>
                    )}
                  </TableCell>
                  <TableCell className="px-4 text-meta text-muted-foreground">
                    {entry.method ? (
                      <span className="flex items-center gap-2">
                        {entry.method === "Cash" ? (
                          <Banknote className="size-4" aria-hidden="true" />
                        ) : entry.method === "Pix" ? (
                          <Smartphone className="size-4" aria-hidden="true" />
                        ) : (
                          <CreditCard className="size-4" aria-hidden="true" />
                        )}
                        {getPaymentOptionLabel(entry.method)}
                      </span>
                    ) : (
                      "—"
                    )}
                  </TableCell>
                  <TableCell
                    className={`whitespace-nowrap px-4 text-right tabular-nums ${entry.signedAmount < 0 ? "text-destructive" : "text-success"}`}
                  >
                    {entry.signedAmount < 0 ? "−" : "+"}{" "}
                    {formatMoney(Math.abs(entry.signedAmount))}
                  </TableCell>
                </TableRow>
              ))}
              {query.data?.data.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={5}
                    className="py-6 text-center text-muted-foreground"
                  >
                    Nenhuma movimentação nesta página.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
          {query.data && query.data.meta.totalPages > 1 && (
            <div className="flex items-center justify-end gap-3 border-t p-3">
              <span className="text-meta text-muted-foreground">
                Página {page} de {query.data.meta.totalPages}
              </span>
              <Button
                size="sm"
                variant="ghost"
                disabled={page === 1 || query.isFetching}
                onClick={() => setPage(page - 1)}
              >
                Anterior
              </Button>
              <Button
                size="sm"
                variant="ghost"
                disabled={
                  page >= query.data.meta.totalPages || query.isFetching
                }
                onClick={() => setPage(page + 1)}
              >
                Próxima
              </Button>
            </div>
          )}
        </QueryState>
      </CardContent>
    </Card>
  );
}
