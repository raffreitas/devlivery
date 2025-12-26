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
    className: "bg-green-500 hover:bg-green-600",
  },
  [ExpenseStatus.PENDING]: {
    label: "Pendente",
    variant: "secondary" as const,
    icon: Clock,
    className: "bg-blue-500 hover:bg-blue-600 text-white",
  },
  [ExpenseStatus.OVERDUE]: {
    label: "Vencido",
    variant: "destructive" as const,
    icon: AlertCircle,
    className: "bg-red-500 hover:bg-red-600",
  },
  [ExpenseStatus.DUE_TODAY]: {
    label: "Vence Hoje",
    variant: "outline" as const,
    icon: Calendar,
    className: "bg-amber-500 hover:bg-amber-600 text-white",
  },
  [ExpenseStatus.CANCELLED]: {
    label: "Cancelado",
    variant: "secondary" as const,
    icon: AlertCircle,
    className: "bg-gray-500 hover:bg-gray-600 text-white",
  },
};

export function ExpenseList({
  expenses,
  onEdit,
  onDelete,
  onMarkAsPaid,
}: ExpenseListProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (id: string) => {
    if (confirm("Tem certeza que deseja excluir esta despesa?")) {
      setDeletingId(id);
      try {
        await onDelete(id);
      } finally {
        setDeletingId(null);
      }
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
    <>
      {/* Desktop Table */}
      <div className="hidden lg:block">
        <Card>
          <CardHeader>
            <CardTitle>Despesas</CardTitle>
          </CardHeader>
          <CardContent>
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
                        {expense.category.subCategories?.[0]?.name || "-"}
                      </TableCell>
                      <TableCell className="max-w-[150px] truncate">
                        {expense.supplier || "-"}
                      </TableCell>
                      <TableCell className="max-w-[200px] truncate">
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
                                expense.status === ExpenseStatus.PAID ||
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
          </CardContent>
        </Card>
      </div>

      {/* Mobile Cards */}
      <div className="lg:hidden space-y-4">
        {expenses.map((expense) => {
          const config = statusConfig[expense.status];
          const StatusIcon = config.icon;

          return (
            <Card key={expense.id}>
              <CardHeader className="pb-3">
                <div className="flex items-start justify-between">
                  <div className="space-y-1">
                    <CardTitle className="text-base">
                      {expense.category.subCategories?.[0]?.name || "-"}
                    </CardTitle>
                    <p className="text-sm text-muted-foreground">
                      {expense.category.name}
                    </p>
                  </div>
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
                          expense.status === ExpenseStatus.PAID ||
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
                </div>
              </CardHeader>
              <CardContent className="space-y-3">
                {expense.supplier && (
                  <div>
                    <p className="text-sm text-muted-foreground">Fornecedor</p>
                    <p className="text-sm font-medium">{expense.supplier}</p>
                  </div>
                )}
                {expense.description && (
                  <div>
                    <p className="text-sm text-muted-foreground">Descrição</p>
                    <p className="text-sm">{expense.description}</p>
                  </div>
                )}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Valor</p>
                    <p className="text-base font-semibold">
                      {formatMoney(expense.amount)}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Status</p>
                    <Badge className={config.className}>
                      <StatusIcon className="mr-1 h-3 w-3" />
                      {config.label}
                    </Badge>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Vencimento</p>
                    <p className="text-sm">
                      {format(expense.dueDate, "dd/MM/yyyy")}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Pagamento</p>
                    <p className="text-sm">
                      {expense.paymentDate
                        ? format(expense.paymentDate, "dd/MM/yyyy")
                        : "-"}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>
    </>
  );
}
