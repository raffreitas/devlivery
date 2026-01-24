import { CheckCircle2, XCircle } from "lucide-react";
import { useCurrentCashSession } from "@/features/cash/hooks/use-current-cash-session";

export function CashStatusBadge() {
  const { currentSession } = useCurrentCashSession();
  const isOpen = !!currentSession;

  return (
    <div
      className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-medium w-full border ${
        isOpen
          ? "bg-green-100 dark:bg-green-950 text-green-800 dark:text-green-200 border-green-300 dark:border-green-700"
          : "bg-red-100 dark:bg-red-950 text-red-800 dark:text-red-200 border-red-300 dark:border-red-700"
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
