import { CheckIcon, CircleAlertIcon, TriangleAlertIcon } from "lucide-react";
import { cn } from "@/shared/lib/utils";
import { formatMoney } from "@/shared/utils/formatters";

export function CashDiffAlertTitle({ difference }: { difference: number }) {
  function getColorsClasses() {
    if (difference === 0) {
      return "text-green-600 dark:text-green-400";
    }
    if (difference > 0) {
      return "text-amber-600 dark:text-amber-400";
    }
    return "text-red-600 dark:text-red-400";
  }

  function getAlertTitle() {
    if (difference === 0) {
      return (
        <p className="flex items-center gap-2">
          <CheckIcon className={getColorsClasses()} /> Caixa Conferido
        </p>
      );
    }
    if (difference > 0) {
      return (
        <p className="flex items-center gap-2">
          <CircleAlertIcon className={getColorsClasses()} /> Sobra no Caixa
        </p>
      );
    }
    return (
      <p className="flex items-center gap-2">
        <TriangleAlertIcon className={getColorsClasses()} /> Falta no Caixa
      </p>
    );
  }

  return (
    <div className="flex justify-between">
      {getAlertTitle()}
      <span className={cn("text-xl font-bold", getColorsClasses())}>
        {difference >= 0 ? "+" : ""}
        {formatMoney(Math.abs(difference))}
      </span>
    </div>
  );
}
