import { useQuery } from "@tanstack/react-query";
import { format } from "date-fns";
import { X } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import type { DateRange } from "react-day-picker";
import { Link, useSearchParams } from "react-router-dom";
import { DateRangePicker } from "@/shared/components/date-range-picker";
import { LoadingOverlay } from "@/shared/components/loading";
import { PageHeader } from "@/shared/components/page-header";
import { QueryState } from "@/shared/components/query-state";
import { StatTile } from "@/shared/components/stat-tile";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
} from "@/shared/components/ui/drawer";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/shared/components/ui/empty";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { useMediaQuery } from "@/shared/hooks/use-media-query";
import { queryKeys } from "@/shared/services/query-keys";
import { formatMoney } from "@/shared/utils/formatters";
import { CashHistoryDetail } from "../components/cash-history-detail";
import { cashHistoryService } from "../services/cash-history-service";
import { formatCashHistoryDateTime } from "../utils/format-cash-date-time";

function defaultPeriod() {
  const today = new Intl.DateTimeFormat("en-CA", {
    timeZone: "America/Sao_Paulo",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(new Date());
  const start = new Date(`${today}T12:00:00Z`);
  start.setUTCDate(start.getUTCDate() - 29);
  return { start: start.toISOString().slice(0, 10), end: today };
}
function validDate(value: string | null): value is string {
  if (!value || !/^\d{4}-\d{2}-\d{2}$/.test(value) || value.startsWith("0000"))
    return false;
  const date = new Date(`${value}T12:00:00Z`);
  return (
    Number.isFinite(date.getTime()) && date.toISOString().slice(0, 10) === value
  );
}
function calendarDate(value: string) {
  return new Date(`${value}T12:00:00`);
}

function HistoryPeriod({
  start,
  end,
  onChange,
}: {
  start: string;
  end: string;
  onChange: (start: string, end: string) => void;
}) {
  const [date, setDate] = useState<DateRange | undefined>({
    from: calendarDate(start),
    to: calendarDate(end),
  });
  return (
    <DateRangePicker
      date={date}
      onDateChange={(value) => {
        setDate(value);
        if (value?.from && value.to)
          onChange(
            format(value.from, "yyyy-MM-dd"),
            format(value.to, "yyyy-MM-dd"),
          );
      }}
    />
  );
}

export function CashHistoryPage() {
  const [params, setParams] = useSearchParams();
  const [defaults] = useState(defaultPeriod);
  const rawStart = params.get("start");
  const rawEnd = params.get("end");
  const validPeriod =
    validDate(rawStart) &&
    validDate(rawEnd) &&
    rawStart <= rawEnd &&
    rawEnd !== "9999-12-31";
  const start = validPeriod ? rawStart : defaults.start;
  const end = validPeriod ? rawEnd : defaults.end;
  const rawPage = Number(params.get("page") ?? 1);
  const page =
    Number.isInteger(rawPage) && rawPage > 0 && rawPage <= 42949673
      ? rawPage
      : 1;
  const selected = params.get("sessionId");
  const desktop = useMediaQuery("(min-width: 1280px)");
  const opener = useRef<HTMLAnchorElement | null>(null);
  useEffect(() => {
    if (
      params.get("start") === start &&
      params.get("end") === end &&
      params.get("page") === String(page)
    )
      return;
    const next = new URLSearchParams(params);
    next.set("start", start);
    next.set("end", end);
    next.set("page", String(page));
    setParams(next, { replace: true });
  }, [params, start, end, page, setParams]);
  const query = useQuery({
    queryKey: [...queryKeys.cash, "history", start, end, page],
    queryFn: ({ signal }) => cashHistoryService.list(start, end, page, signal),
  });
  const changePage = (nextPage: number) => {
    const next = new URLSearchParams(params);
    next.set("page", String(nextPage));
    next.delete("sessionId");
    setParams(next);
  };
  const closeDetail = () => {
    const next = new URLSearchParams(params);
    next.delete("sessionId");
    setParams(next);
    if (desktop)
      requestAnimationFrame(
        () => opener.current?.isConnected && opener.current.focus(),
      );
  };
  const summary = query.data?.data.summary;
  return (
    <div className="flex min-w-0 flex-col">
      <PageHeader
        title="Histórico"
        ancestors={[{ label: "Caixa", href: "/cash" }]}
        actions={
          <>
            <HistoryPeriod
              key={`${start}:${end}`}
              start={start}
              end={end}
              onChange={(from, to) => {
                const next = new URLSearchParams(params);
                next.set("start", from);
                next.set("end", to);
                next.set("page", "1");
                next.delete("sessionId");
                setParams(next);
              }}
            />
            <Button variant="outline" render={<Link to="/cash" />}>
              Turno atual
            </Button>
            <LoadingOverlay
              isFetching={query.isFetching && !query.isPending}
              position="inline"
            />
          </>
        }
      />
      <div className="flex min-w-0 flex-col gap-4 p-4 md:gap-6 md:p-6">
        <QueryState
          isPending={query.isPending}
          error={query.error}
          hasData={query.data !== undefined}
          refetch={query.refetch}
        >
          {summary && (
            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
              <StatTile
                label="Turnos fechados"
                value={String(summary.closedCount)}
                foot={`${summary.openCount} ${summary.openCount === 1 ? "turno aberto" : "turnos abertos"} no período`}
              />
              <StatTile
                label="Recebido"
                value={formatMoney(summary.totalRevenue)}
                foot={`${summary.totalOrders} ${summary.totalOrders === 1 ? "pedido" : "pedidos"} com recebimento`}
                emphasis
              />
              <StatTile
                label="Diferença acumulada"
                value={formatMoney(summary.totalDifference)}
                foot="Contado menos esperado"
              />
              <StatTile
                label="Turnos com diferença"
                value={String(summary.sessionsWithDifference)}
                foot="Entre turnos fechados"
              />
            </div>
          )}
          <div
            className={
              selected && desktop
                ? "grid min-w-0 items-start gap-4 xl:grid-cols-3"
                : "min-w-0"
            }
          >
            <Card
              size="sm"
              className={
                selected && desktop ? "min-w-0 xl:col-span-2" : "min-w-0"
              }
            >
              <CardHeader>
                <CardTitle>
                  <h2>Turnos do caixa</h2>
                </CardTitle>
              </CardHeader>
              <CardContent className="min-w-0 gap-4">
                <div className="min-w-0 overflow-hidden rounded-md border">
                  <Table aria-label="Turnos do caixa">
                    <TableHeader>
                      <TableRow>
                        <TableHead>Turno</TableHead>
                        <TableHead>Aberto por</TableHead>
                        <TableHead className="text-right">Pedidos</TableHead>
                        <TableHead className="text-right">Recebido</TableHead>
                        <TableHead className="text-right">Contado</TableHead>
                        <TableHead className="text-right">Diferença</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {query.data?.data.sessions.map((session) => {
                        const next = new URLSearchParams(params);
                        next.set("sessionId", session.id);
                        return (
                          <TableRow
                            key={session.id}
                            data-state={
                              selected === session.id ? "selected" : undefined
                            }
                          >
                            <TableCell>
                              <Link
                                to={`?${next}`}
                                aria-label={`Ver turno de ${formatCashHistoryDateTime(session.startAt)}, ${session.attendant}`}
                                aria-current={
                                  selected === session.id ? "true" : undefined
                                }
                                className="font-medium underline-offset-4 hover:underline"
                                onClick={(event) => {
                                  opener.current = event.currentTarget;
                                }}
                              >
                                {formatCashHistoryDateTime(session.startAt)}
                              </Link>
                              <span className="block text-muted-foreground">
                                {session.endAt
                                  ? `até ${formatCashHistoryDateTime(session.endAt)}`
                                  : "Aberto · até agora"}
                              </span>
                            </TableCell>
                            <TableCell>{session.attendant}</TableCell>
                            <TableCell className="text-right tabular-nums">
                              {session.totalOrders}
                            </TableCell>
                            <TableCell className="text-right tabular-nums">
                              {formatMoney(session.totalRevenue)}
                            </TableCell>
                            <TableCell className="text-right tabular-nums">
                              {session.closingAmount === null
                                ? "—"
                                : formatMoney(session.closingAmount)}
                            </TableCell>
                            <TableCell
                              className={
                                session.difference !== null &&
                                session.difference < 0
                                  ? "text-right tabular-nums text-destructive"
                                  : "text-right tabular-nums"
                              }
                            >
                              {session.status === "Open"
                                ? "Aberto"
                                : session.difference === null
                                  ? "—"
                                  : session.difference === 0
                                    ? "Sem diferença"
                                    : `${session.difference > 0 ? "+ " : ""}${formatMoney(session.difference)}`}
                            </TableCell>
                          </TableRow>
                        );
                      })}
                      {query.data?.data.sessions.length === 0 && (
                        <TableRow>
                          <TableCell colSpan={6}>
                            <Empty>
                              <EmptyHeader>
                                <EmptyTitle>Nenhum turno no período</EmptyTitle>
                                <EmptyDescription>
                                  Selecione outro período para consultar o
                                  histórico.
                                </EmptyDescription>
                              </EmptyHeader>
                            </Empty>
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </div>
                {query.data && query.data.meta.totalPages > 1 && (
                  <div className="flex flex-wrap items-center justify-end gap-3">
                    <span className="text-sm text-muted-foreground">
                      Página {page} de {query.data.meta.totalPages}
                    </span>
                    <Button
                      size="sm"
                      variant="outline"
                      disabled={page === 1 || query.isFetching}
                      onClick={() => changePage(page - 1)}
                    >
                      Anterior
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      disabled={
                        page >= query.data.meta.totalPages || query.isFetching
                      }
                      onClick={() => changePage(page + 1)}
                    >
                      Próxima
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>
            {desktop && selected && (
              <Card
                size="sm"
                className="min-w-0"
                role="region"
                aria-label="Detalhes do turno"
              >
                <CardHeader>
                  <CardTitle>
                    <h2>Detalhes do turno</h2>
                  </CardTitle>
                  <CardAction>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Fechar detalhes"
                      onClick={closeDetail}
                    >
                      <X />
                    </Button>
                  </CardAction>
                </CardHeader>
                <CardContent className="min-w-0">
                  <CashHistoryDetail key={selected} sessionId={selected} />
                </CardContent>
              </Card>
            )}
          </div>
        </QueryState>
      </div>
      {!desktop && (
        <Drawer
          open={!!selected}
          onOpenChange={(open) => {
            if (!open) closeDetail();
          }}
          showSwipeHandle
        >
          <DrawerContent finalFocus={opener}>
            <DrawerHeader>
              <DrawerTitle>Detalhes do turno</DrawerTitle>
              <Button
                variant="ghost"
                size="icon-sm"
                aria-label="Fechar detalhes"
                onClick={closeDetail}
              >
                <X />
              </Button>
            </DrawerHeader>
            <div className="min-h-0 flex-1 overflow-y-auto p-4">
              {selected && (
                <CashHistoryDetail key={selected} sessionId={selected} />
              )}
            </div>
          </DrawerContent>
        </Drawer>
      )}
    </div>
  );
}
