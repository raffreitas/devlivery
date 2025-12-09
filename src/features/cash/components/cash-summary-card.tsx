import { format } from "date-fns";
import { ptBR } from "date-fns/locale";
import {
  AlertCircle,
  Calendar,
  Clock,
  Lock,
  TrendingUp,
  Unlock,
  User,
} from "lucide-react";
import { Card } from "@/shared/components/ui/card";
import type { CashSession } from "../types";

interface CashSummaryCardProps {
  session: CashSession;
  onOpenClose?: () => void;
}

export function CashSummaryCard({
  session,
  onOpenClose,
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

  return (
    <Card className="p-4 sm:p-6">
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
        {isOpen && onOpenClose && (
          <button
            type="button"
            onClick={onOpenClose}
            className="px-3 py-1 text-sm font-medium text-red-700 bg-red-50 hover:bg-red-100 rounded-lg transition-colors"
          >
            Fechar
          </button>
        )}
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

      {/* Cash amounts */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-600">Abertura</span>
          <span className="text-base font-semibold text-gray-900">
            R$ {session.openingAmount?.toFixed(2)}
          </span>
        </div>

        <div className="flex items-center justify-between text-green-600">
          <span className="text-sm flex items-center gap-1">
            <TrendingUp className="w-4 h-4" />
            Vendas Totais ({session.salesTotals.totalOrders} pedidos)
          </span>
          <span className="text-base font-semibold">
            R$ {session.salesTotals.totalRevenue?.toFixed(2)}
          </span>
        </div>

        <div className="pt-3 border-t border-gray-200">
          {isOpen ? (
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-gray-700">
                Dinheiro Esperado no Caixa
              </span>
              <span className="text-lg font-bold text-blue-600">
                R$ {expectedCashAmount.toFixed(2)}
              </span>
            </div>
          ) : (
            <>
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium text-gray-700">
                  Fechamento Real
                </span>
                <span className="text-lg font-bold text-gray-900">
                  R$ {session.closingAmount?.toFixed(2) ?? "0.00"}
                </span>
              </div>
              {session.closingAmount !== undefined && (
                <div
                  className={`flex items-center justify-between p-2 rounded-lg ${
                    difference === 0
                      ? "bg-green-50"
                      : difference > 0
                        ? "bg-yellow-50"
                        : "bg-red-50"
                  }`}
                >
                  <span className="text-xs font-medium text-gray-700">
                    {difference === 0
                      ? "✓ Confere"
                      : difference > 0
                        ? "Sobra"
                        : "Falta"}
                  </span>
                  <span
                    className={`text-sm font-bold ${
                      difference === 0
                        ? "text-green-600"
                        : difference > 0
                          ? "text-yellow-600"
                          : "text-red-600"
                    }`}
                  >
                    {difference >= 0 ? "+" : ""}R$ {difference.toFixed(2)}
                  </span>
                </div>
              )}
            </>
          )}
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
