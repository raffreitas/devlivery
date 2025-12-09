import { format } from "date-fns";
import { ptBR } from "date-fns/locale";
import {
  AlertCircle,
  Calendar,
  Clock,
  Lock,
  PlusIcon,
  TrendingUp,
  Unlock,
  User,
} from "lucide-react";
import { getPaymentOptionLabel } from "@/features/orders/constants/payment-methods";
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
  const isOpen = session.status === "open";
  const expectedCashAmount = session.expectedCashAmount;
  const difference = session.closingAmount
    ? session.closingAmount - expectedCashAmount
    : 0;

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

  // Warn if session is open for more than 24 hours
  const isLongSession = () => {
    if (!isOpen) return false;
    const durationMs = Date.now() - startDate.getTime();
    return durationMs > 24 * 60 * 60 * 1000;
  };

  const totalDeposits = deposits?.reduce(
    (sum, deposit) => sum + deposit.amount,
    0,
  );

  return (
    <Card className="p-4 sm:p-6 gap-1">
      <div className="flex items-start justify-between mb-4">
        <div>
          <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
            {isOpen ? (
              <>
                <Unlock className="w-5 h-5 text-green-600" />
                Caixa Aberto
              </>
            ) : (
              <>
                <Lock className="w-5 h-5 text-gray-600" />
                Caixa Fechado
              </>
            )}
          </h3>
          <p className="text-sm text-gray-500 flex items-center gap-1 mt-1">
            <User className="w-3 h-3" />
            {session.attendant}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {isOpen && onAddDeposit && (
            <Button
              type="button"
              onClick={onAddDeposit}
              className="px-3 py-1 text-sm font-medium text-blue-700 bg-blue-50 hover:bg-blue-100 rounded-lg transition-colors"
            >
              Adicionar Aporte
            </Button>
          )}
          {isOpen && onOpenClose && (
            <Button
              type="button"
              onClick={onOpenClose}
              className="px-3 py-1 text-sm font-medium text-red-700 bg-red-50 hover:bg-red-100 rounded-lg transition-colors"
            >
              Fechar
            </Button>
          )}
        </div>
      </div>

      {/* Session period */}
      <div className="flex items-center gap-4 text-sm text-gray-600 mb-4 pb-4 border-b border-gray-200">
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
        <div className="text-xs text-gray-500">({getDuration()})</div>
      </div>

      {/* Long session warning */}
      {isLongSession() && (
        <div className="mb-4 p-3 rounded-lg bg-yellow-50 border border-yellow-200 flex items-start gap-2">
          <AlertCircle className="w-4 h-4 text-yellow-600 mt-0.5 shrink-0" />
          <p className="text-sm text-yellow-800">
            Atenção: Este caixa está aberto há mais de 24 horas
          </p>
        </div>
      )}

      {/* Cash flow explanation */}
      <div className="mb-4 p-3 rounded-lg bg-orange-50 border border-orange-200">
        <p className="text-xs text-orange-800 leading-relaxed">
          <strong>Dinheiro Esperado no Caixa:</strong> Valor de abertura +
          aportes + vendas em dinheiro apenas
        </p>
      </div>

      {/* Cash amounts */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-600">Abertura</span>
          <span className="text-base font-medium text-gray-900">
            {formatMoney(session.openingAmount)}
          </span>
        </div>

        {deposits && deposits.length > 0 && (
          <div className="flex items-center justify-between text-orange-600">
            <span className="text-sm flex items-center gap-1">
              <PlusIcon className="w-4 h-4" />
              Aportes
            </span>
            <span className="text-base font-medium">
              {formatMoney(totalDeposits)}
            </span>
          </div>
        )}

        <div className="flex items-center justify-between text-green-600">
          <span className="text-sm flex items-center gap-1">
            <TrendingUp className="w-4 h-4" />
            Vendas em Dinheiro
          </span>
          <span className="text-base font-medium">
            {formatMoney(
              expectedCashAmount - session.openingAmount - totalDeposits,
            )}
          </span>
        </div>

        <div className="pt-3 mt-3 border-t-2 border-gray-300">
          {isOpen ? (
            <div className="flex items-center justify-between p-3 rounded-lg bg-orange-50">
              <div>
                <div className="text-xs text-orange-700 font-medium mb-0.5">
                  Dinheiro Esperado no Caixa
                </div>
              </div>
              <span className="text-xl font-bold text-orange-700">
                {formatMoney(expectedCashAmount)}
              </span>
            </div>
          ) : (
            <>
              <div className="flex items-center justify-between mb-3 p-3 rounded-lg bg-gray-50">
                <div>
                  <div className="text-xs text-gray-600 mb-0.5">
                    Dinheiro Esperado
                  </div>
                  <div className="text-base font-semibold text-gray-900">
                    {formatMoney(expectedCashAmount)}
                  </div>
                </div>
                <div>
                  <div className="text-xs text-gray-600 mb-0.5 text-right">
                    Dinheiro Contado
                  </div>
                  <div className="text-base font-semibold text-gray-900">
                    {formatMoney(session.closingAmount ?? 0)}
                  </div>
                </div>
              </div>
              {session.closingAmount !== undefined && (
                <div
                  className={`flex items-center justify-between p-3 rounded-lg ${
                    difference === 0
                      ? "bg-green-50 border border-green-200"
                      : difference > 0
                        ? "bg-yellow-50 border border-yellow-200"
                        : "bg-red-50 border border-red-200"
                  }`}
                >
                  <span className="text-sm font-semibold text-gray-700">
                    {difference === 0
                      ? "✓ Caixa Conferido"
                      : difference > 0
                        ? "Sobra de Caixa"
                        : "Falta no Caixa"}
                  </span>
                  <span
                    className={`text-lg font-bold ${
                      difference === 0
                        ? "text-green-600"
                        : difference > 0
                          ? "text-yellow-600"
                          : "text-red-600"
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
          <div className="mt-4 pt-4 border-t border-gray-200">
            <details className="group">
              <summary className="flex items-center justify-between cursor-pointer text-sm text-gray-600 hover:text-gray-900 transition-colors">
                <span className="flex items-center gap-2">
                  <PlusIcon className="w-4 h-4" />
                  Ver aportes realizados
                </span>
                <span className="text-xs text-gray-500 group-open:hidden">
                  {deposits.length}{" "}
                  {deposits.length === 1 ? "aporte" : "aportes"}
                </span>
              </summary>
              <div className="mt-3 space-y-2">
                {deposits.map((deposit) => {
                  const depositDate = new Date(deposit.depositedAt);
                  const time = depositDate.toLocaleTimeString("pt-BR", {
                    hour: "2-digit",
                    minute: "2-digit",
                  });
                  return (
                    <div
                      key={deposit.id}
                      className="flex items-start justify-between p-2 rounded-lg bg-orange-50 border border-orange-100"
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-2 text-sm">
                          <span className="font-semibold text-orange-900">
                            {formatMoney(deposit.amount)}
                          </span>
                          <span className="text-xs text-orange-600">
                            • {time}
                          </span>
                        </div>
                        <div className="text-xs text-orange-700 mt-0.5">
                          {deposit.attendant}
                        </div>
                        {deposit.notes && (
                          <div className="text-xs text-orange-600 mt-1 italic">
                            {deposit.notes}
                          </div>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </details>
          </div>
        )}

        {/* Sales summary - collapsed, secondary info */}
        <div className="mt-4 pt-4 border-t border-gray-200">
          <details className="group">
            <summary className="flex items-center justify-between cursor-pointer text-sm text-gray-600 hover:text-gray-900 transition-colors">
              <span className="flex items-center gap-2">
                <TrendingUp className="w-4 h-4" />
                Ver resumo completo de vendas
              </span>
              <span className="text-xs text-gray-500 group-open:hidden">
                {session.salesTotals.totalOrders} pedidos •{" "}
                {formatMoney(session.salesTotals.totalRevenue)}
              </span>
            </summary>
            <div className="mt-3 space-y-2 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-gray-600">Total de Pedidos</span>
                <span className="font-medium text-gray-900">
                  {session.salesTotals.totalOrders}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-gray-600">Faturamento Total</span>
                <span className="font-medium text-gray-900">
                  {formatMoney(session.salesTotals.totalRevenue)}
                </span>
              </div>
              {session.paymentBreakdown &&
                session.paymentBreakdown.length > 0 && (
                  <div className="pt-2 mt-2 border-t border-gray-100">
                    <div className="text-xs text-gray-500 mb-2">
                      Por forma de pagamento:
                    </div>
                    {session.paymentBreakdown.map((payment) => (
                      <div
                        key={payment.method}
                        className="flex items-center justify-between py-1"
                      >
                        <span className="text-gray-600">
                          {getPaymentOptionLabel(payment.method)}
                        </span>
                        <span className="font-medium text-gray-900">
                          {formatMoney(payment.amount)}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
            </div>
          </details>
        </div>
      </div>

      {/* Notes */}
      {session.notes && (
        <div className="mt-4 pt-4 border-t border-gray-200">
          <p className="text-xs text-gray-500 mb-1">Observações:</p>
          <p className="text-sm text-gray-700">{session.notes}</p>
        </div>
      )}
    </Card>
  );
}
