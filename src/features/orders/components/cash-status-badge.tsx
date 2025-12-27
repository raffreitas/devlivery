import { CheckCircle2, XCircle } from "lucide-react";
import { useCurrentCashSession } from "@/features/cash/hooks/use-current-cash-session";

export function CashStatusBadge() {
  const { currentSession } = useCurrentCashSession();
  const isOpen = !!currentSession;

  return (
    <div
      className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-medium w-full ${
        isOpen ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800"
      }`}
    >
      {isOpen ? (
        <>
          <CheckCircle2 className="w-4 h-4" />
          Caixa Aberto
        </>
      ) : (
        <>
          <XCircle className="w-4 h-4" />
          Caixa Fechado
        </>
      )}
    </div>
  );
}
