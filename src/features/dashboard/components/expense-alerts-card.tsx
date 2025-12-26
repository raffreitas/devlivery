import { format } from "date-fns";
import { AlertCircle, Calendar, Clock } from "lucide-react";
import type { Expense } from "@/features/expenses/types";
import { ExpenseStatus } from "@/features/expenses/types";
import { Badge } from "@/shared/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";

interface ExpenseAlertsCardProps {
  overdueExpenses: Expense[];
  dueTodayExpenses: Expense[];
  upcomingExpenses: Expense[];
}

const statusConfig = {
  [ExpenseStatus.OVERDUE]: {
    label: "Vencido",
    icon: AlertCircle,
    className: "bg-red-500 hover:bg-red-600 text-white",
  },
  [ExpenseStatus.DUE_TODAY]: {
    label: "Vence Hoje",
    icon: Calendar,
    className: "bg-amber-500 hover:bg-amber-600 text-white",
  },
  [ExpenseStatus.PENDING]: {
    label: "Pendente",
    icon: Clock,
    className: "bg-blue-500 hover:bg-blue-600 text-white",
  },
};

export function ExpenseAlertsCard({
  overdueExpenses,
  dueTodayExpenses,
  upcomingExpenses,
}: ExpenseAlertsCardProps) {
  const hasAlerts =
    overdueExpenses.length > 0 ||
    dueTodayExpenses.length > 0 ||
    upcomingExpenses.length > 0;

  const overdueTotal = overdueExpenses.reduce(
    (sum, exp) => sum + exp.amount,
    0,
  );
  const dueTodayTotal = dueTodayExpenses.reduce(
    (sum, exp) => sum + exp.amount,
    0,
  );
  const upcomingTotal = upcomingExpenses.reduce(
    (sum, exp) => sum + exp.amount,
    0,
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Alertas de Despesas</CardTitle>
      </CardHeader>
      <CardContent>
        {!hasAlerts ? (
          <div className="text-center text-muted-foreground py-8">
            Nenhum alerta no momento
          </div>
        ) : (
          <div className="space-y-6">
            {overdueExpenses.length > 0 && (
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <AlertCircle className="h-4 w-4 text-red-500" />
                    <span className="text-sm font-semibold">
                      Despesas Vencidas ({overdueExpenses.length})
                    </span>
                  </div>
                  <span className="text-sm font-bold text-red-600">
                    {formatMoney(overdueTotal)}
                  </span>
                </div>
                <div className="space-y-1 pl-6">
                  {overdueExpenses.slice(0, 3).map((expense) => (
                    <div
                      key={expense.id}
                      className="flex items-center justify-between text-sm"
                    >
                      <div className="flex-1 truncate">
                        <span className="font-medium">
                          {expense.category.name}
                        </span>
                        {expense.supplier && (
                          <span className="text-muted-foreground ml-2">
                            - {expense.supplier}
                          </span>
                        )}
                      </div>
                      <div className="flex items-center gap-2 ml-2">
                        <span className="font-medium">
                          {formatMoney(expense.amount)}
                        </span>
                        <Badge
                          className={statusConfig[expense.status].className}
                        >
                          {statusConfig[expense.status].label}
                        </Badge>
                      </div>
                    </div>
                  ))}
                  {overdueExpenses.length > 3 && (
                    <p className="text-xs text-muted-foreground">
                      +{overdueExpenses.length - 3} despesa(s) vencida(s)
                    </p>
                  )}
                </div>
              </div>
            )}

            {dueTodayExpenses.length > 0 && (
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-amber-500" />
                    <span className="text-sm font-semibold">
                      Vencem Hoje ({dueTodayExpenses.length})
                    </span>
                  </div>
                  <span className="text-sm font-bold text-amber-600">
                    {formatMoney(dueTodayTotal)}
                  </span>
                </div>
                <div className="space-y-1 pl-6">
                  {dueTodayExpenses.slice(0, 3).map((expense) => (
                    <div
                      key={expense.id}
                      className="flex items-center justify-between text-sm"
                    >
                      <div className="flex-1 truncate">
                        <span className="font-medium">
                          {expense.category.name}
                        </span>
                        {expense.supplier && (
                          <span className="text-muted-foreground ml-2">
                            - {expense.supplier}
                          </span>
                        )}
                      </div>
                      <div className="flex items-center gap-2 ml-2">
                        <span className="font-medium">
                          {formatMoney(expense.amount)}
                        </span>
                        <Badge
                          className={statusConfig[expense.status].className}
                        >
                          {statusConfig[expense.status].label}
                        </Badge>
                      </div>
                    </div>
                  ))}
                  {dueTodayExpenses.length > 3 && (
                    <p className="text-xs text-muted-foreground">
                      +{dueTodayExpenses.length - 3} despesa(s) vence(m) hoje
                    </p>
                  )}
                </div>
              </div>
            )}

            {upcomingExpenses.length > 0 && (
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Clock className="h-4 w-4 text-blue-500" />
                    <span className="text-sm font-semibold">
                      Próximas Despesas ({upcomingExpenses.length})
                    </span>
                  </div>
                  <span className="text-sm font-bold text-blue-600">
                    {formatMoney(upcomingTotal)}
                  </span>
                </div>
                <div className="space-y-1 pl-6">
                  {upcomingExpenses.slice(0, 3).map((expense) => (
                    <div
                      key={expense.id}
                      className="flex items-center justify-between text-sm"
                    >
                      <div className="flex-1 truncate">
                        <span className="font-medium">
                          {expense.category.name}
                        </span>
                        {expense.supplier && (
                          <span className="text-muted-foreground ml-2">
                            - {expense.supplier}
                          </span>
                        )}
                        <span className="text-muted-foreground ml-2 text-xs">
                          (vence em {format(expense.dueDate, "dd/MM")})
                        </span>
                      </div>
                      <div className="flex items-center gap-2 ml-2">
                        <span className="font-medium">
                          {formatMoney(expense.amount)}
                        </span>
                        <Badge
                          className={statusConfig[expense.status].className}
                        >
                          {statusConfig[expense.status].label}
                        </Badge>
                      </div>
                    </div>
                  ))}
                  {upcomingExpenses.length > 3 && (
                    <p className="text-xs text-muted-foreground">
                      +{upcomingExpenses.length - 3} despesa(s) próxima(s)
                    </p>
                  )}
                </div>
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
