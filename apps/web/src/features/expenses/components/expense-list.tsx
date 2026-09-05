import { format } from "date-fns";
import {
  AlertCircle,
  Calendar,
  CheckCircle2,
  Clock,
  MoreVertical,
  Pencil,
  Trash2,
} from "lucide-react";
import { useState } from "react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import { Separator } from "@/shared/components/ui/separator";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { formatMoney } from "@/shared/utils/formatters";
import type { Expense } from "../types";
import { ExpenseStatus } from "../types";

interface ExpenseListProps {
  expenses: Expense[];
  onEdit: (expense: Expense) => void;
  onDelete: (id: string) => void;
  onMarkAsPaid: (id: string, paymentDate: string) => void;
}

const statusConfig = {
  [ExpenseStatus.PAID]: {
    label: "Pago",
    variant: "default" as const,
    icon: CheckCircle2,
    className:
      "bg-green-100 dark:bg-green-950 text-green-800 dark:text-green-200 border border-green-300 dark:border-green-700",
  },
  [ExpenseStatus.PENDING]: {
    label: "Pendente",
    variant: "secondary" as const,
    icon: Clock,
    className:
      "bg-blue-100 dark:bg-blue-950 text-blue-800 dark:text-blue-200 border border-blue-300 dark:border-blue-700",
  },
  [ExpenseStatus.OVERDUE]: {
    label: "Vencido",
    variant: "destructive" as const,
    icon: AlertCircle,
    className:
      "bg-red-100 dark:bg-red-950 text-red-800 dark:text-red-200 border border-red-300 dark:border-red-700",
  },
  [ExpenseStatus.DUE_TODAY]: {
    label: "Vence Hoje",
    variant: "outline" as const,
    icon: Calendar,
    className:
      "bg-amber-100 dark:bg-amber-950 text-amber-800 dark:text-amber-200 border border-amber-300 dark:border-amber-700",
  },
  [ExpenseStatus.CANCELLED]: {
    label: "Cancelado",
    variant: "secondary" as const,
    icon: AlertCircle,
    className:
      "bg-gray-100 dark:bg-gray-950 text-gray-800 dark:text-gray-200 border border-gray-300 dark:border-gray-700",
  },
};

export function ExpenseList({
  expenses,
  onEdit,
  onDelete,
  onMarkAsPaid,
}: ExpenseListProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [alert, setAlert] = useState<{ open: boolean; id: string | null }>({
    open: false,
    id: null,
  });

  // Open confirmation dialog
  const handleDelete = (id: string) => {
    setAlert({ open: true, id });
  };

  // Called when user confirms deletion in the dialog
  const confirmDelete = async () => {
    const id = alert.id;
    if (!id) return;

    setDeletingId(id);
    setAlert({ open: false, id: null });

    try {
      await onDelete(id);
    } finally {
      setDeletingId(null);
    }
  };

  const handleMarkAsPaid = async (expense: Expense) => {
    const today = format(new Date(), "yyyy-MM-dd");
    await onMarkAsPaid(expense.id, today);
  };

  if (expenses.length === 0) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-12">
          <p className="text-muted-foreground text-center">
            Nenhuma despesa encontrada.
            <br />
            <span className="text-sm">
              Clique em "Nova Despesa" para começar.
            </span>
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div>
      <Card>
        <CardHeader>
          <CardTitle>Despesas</CardTitle>
        </CardHeader>
        <CardContent>
          {/* Mobile: Card View */}
          <div className="md:hidden space-y-4">
            {expenses.map((expense) => {
              const config = statusConfig[expense.status];
              const StatusIcon = config.icon;

              return (
                <Card key={expense.id} className="p-4 gap-2">
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <h3 className="font-semibold text-base truncate">
                          {expense.description || expense.category.name}
                        </h3>
                      </div>
                      <p className="text-sm text-muted-foreground">
                        {expense.category.name}
                        {expense.category.subcategories?.[0]?.name &&
                          ` • ${expense.category.subcategories[0].name}`}
                      </p>
                    </div>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button
                          variant="ghost"
                          size="sm"
                          disabled={deletingId === expense.id}
                          className="h-8 w-8 p-0"
                        >
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem
                          onClick={() => onEdit(expense)}
                          disabled={expense.status === ExpenseStatus.CANCELLED}
                        >
                          <Pencil className="mr-2 h-4 w-4" />
                          Editar
                        </DropdownMenuItem>
                        {expense.status !== ExpenseStatus.PAID &&
                          expense.status !== ExpenseStatus.CANCELLED && (
                            <DropdownMenuItem
                              onClick={() => handleMarkAsPaid(expense)}
                            >
                              <CheckCircle2 className="mr-2 h-4 w-4" />
                              Marcar como Pago
                            </DropdownMenuItem>
                          )}
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          onClick={() => handleDelete(expense.id)}
                          className="text-destructive"
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          Excluir
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>

                  <Separator className="my-3" />

                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground">
                        Fornecedor
                      </span>
                      <span className="text-sm font-medium truncate ml-2 max-w-[60%]">
                        {expense.supplier || "-"}
                      </span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground">
                        Valor
                      </span>
                      <span className="text-base font-semibold">
                        {formatMoney(expense.amount)}
                      </span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground">
                        Vencimento
                      </span>
                      <span className="text-sm">
                        {format(expense.dueDate, "dd/MM/yyyy")}
                      </span>
                    </div>
                    {expense.paymentDate && (
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">
                          Pagamento
                        </span>
                        <span className="text-sm">
                          {format(expense.paymentDate, "dd/MM/yyyy")}
                        </span>
                      </div>
                    )}
                    <div className="flex items-center justify-between pt-2">
                      <span className="text-sm text-muted-foreground">
                        Status
                      </span>
                      <Badge className={config.className}>
                        <StatusIcon className="mr-1 h-3 w-3" />
                        {config.label}
                      </Badge>
                    </div>
                  </div>
                </Card>
              );
            })}
          </div>

          {/* Desktop: Table View */}
          <div className="hidden md:block overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Categoria</TableHead>
                  <TableHead>Subcategoria</TableHead>
                  <TableHead>Fornecedor</TableHead>
                  <TableHead>Descrição</TableHead>
                  <TableHead className="text-right">Valor</TableHead>
                  <TableHead>Vencimento</TableHead>
                  <TableHead>Pagamento</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {expenses.map((expense) => {
                  const config = statusConfig[expense.status];
                  const StatusIcon = config.icon;

                  return (
                    <TableRow key={expense.id}>
                      <TableCell>{expense.category.name}</TableCell>
                      <TableCell>
                        {expense.category.subcategories?.[0]?.name || "-"}
                      </TableCell>
                      <TableCell className="max-w-37.5 truncate">
                        {expense.supplier || "-"}
                      </TableCell>
                      <TableCell className="max-w-50 truncate">
                        {expense.description || "-"}
                      </TableCell>
                      <TableCell className="text-right font-semibold">
                        {formatMoney(expense.amount)}
                      </TableCell>
                      <TableCell>
                        {format(expense.dueDate, "dd/MM/yyyy")}
                      </TableCell>
                      <TableCell>
                        {expense.paymentDate
                          ? format(expense.paymentDate, "dd/MM/yyyy")
                          : "-"}
                      </TableCell>
                      <TableCell>
                        <Badge className={config.className}>
                          <StatusIcon className="mr-1 h-3 w-3" />
                          {config.label}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button
                              variant="ghost"
                              size="sm"
                              disabled={deletingId === expense.id}
                            >
                              <MoreVertical className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => onEdit(expense)}
                              disabled={
                                expense.status === ExpenseStatus.CANCELLED
                              }
                            >
                              <Pencil className="mr-2 h-4 w-4" />
                              Editar
                            </DropdownMenuItem>
                            {expense.status !== ExpenseStatus.PAID &&
                              expense.status !== ExpenseStatus.CANCELLED && (
                                <DropdownMenuItem
                                  onClick={() => handleMarkAsPaid(expense)}
                                >
                                  <CheckCircle2 className="mr-2 h-4 w-4" />
                                  Marcar como Pago
                                </DropdownMenuItem>
                              )}
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() => handleDelete(expense.id)}
                              className="text-destructive"
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Excluir
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>

          <AlertDialog
            open={alert.open}
            onOpenChange={(open) =>
              setAlert({ open, id: open ? alert.id : null })
            }
          >
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>
                  Tem certeza que deseja excluir esta despesa?
                </AlertDialogTitle>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel disabled={deletingId === alert.id}>
                  Cancelar
                </AlertDialogCancel>
                <AlertDialogAction
                  onClick={() => confirmDelete()}
                  disabled={deletingId === alert.id}
                >
                  {deletingId && deletingId === alert.id
                    ? "Excluindo..."
                    : "Confirmar"}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </CardContent>
      </Card>
    </div>
  );
}
