import { format } from "date-fns";
import { ptBR } from "date-fns/locale";
import {
  Calendar,
  Clock,
  Lock,
  PlusIcon,
  TrendingUp,
  Unlock,
  User,
} from "lucide-react";
import { getPaymentOptionLabel } from "@/features/orders/constants/payment-methods";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/shared/components/ui/accordion";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import { Card } from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";
import type { CashDeposit, CashSession } from "../types";

interface CashSummaryCardProps {
  session: CashSession;
  deposits: CashDeposit[];
  onOpenClose?: () => void;
  onAddDeposit?: () => void;
}

export function CashSummaryCard({
  session,
  deposits,
  onOpenClose,
  onAddDeposit,
}: CashSummaryCardProps) {
  const isOpen = session.status === "Open";
  const expectedCashAmount = session.expectedCashAmount;
  const difference = session.closingAmount
    ? session.closingAmount - expectedCashAmount
    : 0;

  const cashSalesAmount = session.paymentBreakdown.find(
    (payment) => payment.method === "Cash",
  )?.amount;

  const startDate = new Date(session.startAt);
  const endDate = session.endAt ? new Date(session.endAt) : null;

  // Calculate session duration
  const getDuration = () => {
    const end = endDate || new Date();
    const durationMs = end.getTime() - startDate.getTime();
    const hours = Math.floor(durationMs / (1000 * 60 * 60));
    const minutes = Math.floor((durationMs % (1000 * 60 * 60)) / (1000 * 60));
    return `${hours}h ${minutes}m`;
  };

  const totalDeposits = deposits?.reduce(
    (sum, deposit) => sum + deposit.amount,
    0,
  );

  return (
    <Card className="p-4 sm:p-6 gap-1">
      <div className="flex items-start justify-between mb-4">
        <div>
          <h3 className="text-lg font-semibold text-foreground flex items-center gap-2">
            {isOpen ? (
              <>
                <Unlock className="w-5 h-5 text-green-600 dark:text-green-400" />
                Caixa Aberto
              </>
            ) : (
              <>
                <Lock className="w-5 h-5 text-muted-foreground" />
                Caixa Fechado
              </>
            )}
          </h3>
          <p className="text-sm text-muted-foreground flex items-center gap-1 mt-1">
            <User className="w-3 h-3" />
            {session.attendant}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {isOpen && onAddDeposit && (
            <Button
              type="button"
              onClick={onAddDeposit}
              variant="outline"
              size="sm"
            >
              Adicionar Aporte
            </Button>
          )}
          {isOpen && onOpenClose && (
            <Button
              type="button"
              onClick={onOpenClose}
              variant="destructive"
              size="sm"
            >
              Fechar
            </Button>
          )}
        </div>
      </div>

      {/* Session period */}
      <div className="flex items-center gap-4 text-sm text-muted-foreground mb-4 pb-4 border-b border-border">
        <div className="flex items-center gap-1">
          <Calendar className="w-4 h-4" />
          <span>{format(startDate, "dd/MM/yyyy", { locale: ptBR })}</span>
        </div>
        <div className="flex items-center gap-1">
          <Clock className="w-4 h-4" />
          <span>
            {format(startDate, "HH:mm", { locale: ptBR })}
            {endDate && ` - ${format(endDate, "HH:mm", { locale: ptBR })}`}
          </span>
        </div>
        <div className="text-xs text-muted-foreground">({getDuration()})</div>
      </div>

      {/* Cash flow explanation */}
      <Alert>
        <AlertTitle>Dinheiro Esperado no Caixa:</AlertTitle>
        <AlertDescription>
          Valor de abertura + aportes + vendas em dinheiro apenas
        </AlertDescription>
      </Alert>

      {/* Cash amounts */}
      <div className="space-y-3 border-t border-border pt-4 mt-4">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Abertura</span>
          <span className="text-base font-medium text-foreground">
            {formatMoney(session.openingAmount)}
          </span>
        </div>

        {deposits && deposits.length > 0 && (
          <div className="flex items-center justify-between text-orange-600 dark:text-orange-400">
            <span className="text-sm flex items-center gap-1">
              <PlusIcon className="w-4 h-4" />
              Aportes
            </span>
            <span className="text-base font-medium">
              {formatMoney(totalDeposits)}
            </span>
          </div>
        )}

        <div className="flex items-center justify-between text-green-600 dark:text-green-400">
          <span className="text-sm flex items-center gap-1">
            <TrendingUp className="w-4 h-4" />
            Vendas em Dinheiro
          </span>
          <span className="text-base font-medium">
            {formatMoney(cashSalesAmount ?? 0)}
          </span>
        </div>

        <div className="pt-3 mt-3 border-t border-border">
          {isOpen ? (
            <Alert>
              <AlertTitle>Dinheiro Esperado no Caixa</AlertTitle>
              <AlertDescription>
                {formatMoney(expectedCashAmount)}
              </AlertDescription>
            </Alert>
          ) : (
            <>
              <div className="flex items-center justify-between mb-3 p-3 rounded-lg bg-muted/50 dark:bg-muted/20">
                <div>
                  <div className="text-xs text-muted-foreground mb-0.5">
                    Dinheiro Esperado
                  </div>
                  <div className="text-base font-semibold text-foreground">
                    {formatMoney(expectedCashAmount)}
                  </div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground mb-0.5 text-right">
                    Dinheiro Contado
                  </div>
                  <div className="text-base font-semibold text-foreground">
                    {formatMoney(session.closingAmount ?? 0)}
                  </div>
                </div>
              </div>
              {session.closingAmount !== undefined && (
                <div
                  className={`flex items-center justify-between p-3 rounded-lg ${
                    difference === 0
                      ? "bg-green-50 dark:bg-green-950/30 border border-green-200 dark:border-green-700"
                      : difference > 0
                        ? "bg-yellow-50 dark:bg-yellow-950/30 border border-yellow-200 dark:border-yellow-700"
                        : "bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-700"
                  }`}
                >
                  <span className="text-sm font-semibold text-foreground">
                    {difference === 0
                      ? "✓ Caixa Conferido"
                      : difference > 0
                        ? "Sobra de Caixa"
                        : "Falta no Caixa"}
                  </span>
                  <span
                    className={`text-lg font-bold ${
                      difference === 0
                        ? "text-green-600 dark:text-green-400"
                        : difference > 0
                          ? "text-yellow-600 dark:text-yellow-400"
                          : "text-red-600 dark:text-red-400"
                    }`}
                  >
                    {difference >= 0 ? "+" : ""}
                    {formatMoney(Math.abs(difference))}
                  </span>
                </div>
              )}
            </>
          )}
        </div>

        {/* Deposits list - if any */}
        {deposits && deposits.length > 0 && (
          <div className="border-t border-border">
            <Accordion type="single" collapsible>
              <AccordionItem value="deposits-list">
                <AccordionTrigger>
                  <span className="flex items-center gap-2">
                    <PlusIcon className="w-4 h-4" />
                    Ver aportes realizados
                  </span>
                  <span className="text-xs text-muted-foreground ml-auto">
                    {deposits.length}{" "}
                    {deposits.length === 1 ? "aporte" : "aportes"}
                  </span>
                </AccordionTrigger>
                <AccordionContent>
                  <div className="space-y-2">
                    {deposits.map((deposit) => {
                      const depositDate = new Date(deposit.depositedAt);
                      const time = depositDate.toLocaleTimeString("pt-BR", {
                        hour: "2-digit",
                        minute: "2-digit",
                      });
                      return (
                        <div
                          key={deposit.id}
                          className="flex items-start justify-between p-2 rounded-lg bg-accent/10 dark:bg-accent/5 border border-accent/30 dark:border-accent/20"
                        >
                          <div className="flex-1">
                            <div className="flex items-center gap-2 text-sm">
                              <span className="font-semibold text-accent-foreground">
                                {formatMoney(deposit.amount)}
                              </span>
                              <span className="text-xs text-accent-foreground/70">
                                • {time}
                              </span>
                            </div>
                            <div className="text-xs text-accent-foreground/60 mt-0.5">
                              {deposit.attendant}
                            </div>
                            {deposit.notes && (
                              <div className="text-xs text-muted-foreground mt-1 italic">
                                {deposit.notes}
                              </div>
                            )}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </AccordionContent>
              </AccordionItem>
            </Accordion>
          </div>
        )}

        {/* Sales summary - collapsed, secondary info */}
        <div className="border-t border-border">
          <Accordion type="single" collapsible>
            <AccordionItem value="sales-summary">
              <AccordionTrigger>
                <span className="flex items-center gap-2">
                  <TrendingUp className="w-4 h-4" />
                  Ver resumo completo de vendas
                </span>
                <span className="text-xs text-muted-foreground ml-auto">
                  {session.salesTotals.totalOrders} pedidos •{" "}
                  {formatMoney(session.salesTotals.totalRevenue)}
                </span>
              </AccordionTrigger>
              <AccordionContent>
                <div className="space-y-2 text-sm">
                  <div className="flex items-center justify-between">
                    <span className="text-muted-foreground">
                      Total de Pedidos
                    </span>
                    <span className="font-medium text-foreground">
                      {session.salesTotals.totalOrders}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-muted-foreground">
                      Faturamento Total
                    </span>
                    <span className="font-medium text-foreground">
                      {formatMoney(session.salesTotals.totalRevenue)}
                    </span>
                  </div>
                  {session.paymentBreakdown &&
                    session.paymentBreakdown.length > 0 && (
                      <div className="pt-2 mt-2 border-t border-border">
                        <div className="text-xs text-muted-foreground mb-2">
                          Por forma de pagamento:
                        </div>
                        {session.paymentBreakdown.map((payment) => (
                          <div
                            key={payment.method}
                            className="flex items-center justify-between py-1"
                          >
                            <span className="text-muted-foreground">
                              {getPaymentOptionLabel(payment.method)}
                            </span>
                            <span className="font-medium text-foreground">
                              {formatMoney(payment.amount)}
                            </span>
                          </div>
                        ))}
                      </div>
                    )}
                </div>
              </AccordionContent>
            </AccordionItem>
          </Accordion>
        </div>
      </div>

      {/* Notes */}
      {session.notes && (
        <div className="mt-4 pt-4 border-t border-border">
          <p className="text-xs text-muted-foreground mb-1">Observações:</p>
          <p className="text-sm text-foreground">{session.notes}</p>
        </div>
      )}
    </Card>
  );
}
