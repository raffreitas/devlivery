import { Plus } from "lucide-react";
import { useState } from "react";
import type { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import { Modal } from "@/shared/components/modal";
import { Button } from "@/shared/components/ui/button";
import { LoadingState, TableSkeleton } from "@/shared/components/loading";
import { ExpensesFilters } from "../components/expenses-filters";
import { ExpensesFiltersContent } from "../components/expenses-filters-content";
import { ExpenseForm } from "../components/expense-form";
import { ExpenseList } from "../components/expense-list";
import { ExpenseSummaryCard } from "../components/expense-summary-card";
import { useExpenses } from "../hooks/use-expenses";
import type { Expense, ExpenseFormData, ExpenseStatus } from "../types";

export function ExpensesPage() {
  const [duePeriod, setDuePeriod] = useState<DateRange | undefined>();
  const [categoryId, setCategoryId] = useState<string | undefined>();
  const [status, setStatus] = useState<ExpenseStatus | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingExpense, setEditingExpense] = useState<Expense | undefined>();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

  const {
    expenses,
    summary,
    loading,
    createExpense,
    updateExpense,
    deleteExpense,
    markAsPaid,
  } = useExpenses({ duePeriod, categoryId, status });

  const handleSubmit = async (data: ExpenseFormData) => {
    try {
      if (editingExpense) {
        await updateExpense(editingExpense.id, data);
        toast.success("Despesa atualizada com sucesso!");
      } else {
        await createExpense(data);
        toast.success("Despesa criada com sucesso!");
      }
      handleCloseModal();
    } catch (error) {
      // Tratamento de erros específicos do backend
      const errorMessage =
        error instanceof Error && error.message.includes("Paga ou Cancelada")
          ? "Não é permitido alterar uma despesa paga ou cancelada. Estorne o pagamento primeiro."
          : "Erro ao salvar despesa";
      toast.error(errorMessage);
      console.error(error);
    }
  };

  const handleEdit = (expense: Expense) => {
    // Validação de regra de negócio: não permite editar despesas pagas ou canceladas
    if (expense.status === "Paid" || expense.status === "Cancelled") {
      toast.error(
        expense.status === "Paid"
          ? "Não é permitido editar uma despesa paga. Estorne o pagamento primeiro."
          : "Não é permitido editar uma despesa cancelada.",
      );
      return;
    }
    setEditingExpense(expense);
    setIsModalOpen(true);
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteExpense(id);
      toast.success("Despesa excluída com sucesso!");
    } catch (error) {
      toast.error("Erro ao excluir despesa");
      console.error(error);
    }
  };

  const handleMarkAsPaid = async (id: string, paymentDate: string) => {
    try {
      await markAsPaid(id, paymentDate);
      toast.success("Despesa marcada como paga!");
    } catch (error) {
      // O backend retorna erro específico se tentar pagar despesa cancelada
      const errorMessage =
        error instanceof Error && error.message.includes("cancelada")
          ? "Não é possível pagar uma despesa cancelada."
          : "Erro ao marcar despesa como paga";
      toast.error(errorMessage);
      console.error(error);
    }
  };

  const handleOpenCreateModal = () => {
    setEditingExpense(undefined);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingExpense(undefined);
  };

  return (
    <>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold tracking-tight">
              Despesas
            </h1>
            <p className="text-muted-foreground">
              Gerencie as despesas do estabelecimento
            </p>
          </div>
          <Button onClick={handleOpenCreateModal}>
            <Plus className="mr-2 h-4 w-4" />
            Nova Despesa
          </Button>
        </div>

        <LoadingState
          isLoading={loading}
          skeleton={
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="h-24 bg-accent animate-pulse rounded-lg" />
                <div className="h-24 bg-accent animate-pulse rounded-lg" />
                <div className="h-24 bg-accent animate-pulse rounded-lg" />
              </div>
              <div className="bg-card p-4 rounded-lg border">
                <TableSkeleton rows={5} columns={9} />
              </div>
            </div>
          }
        >
          {/* Summary Cards */}
          <ExpenseSummaryCard summary={summary} />

          {/* Filters */}
          <ExpensesFilters
            period={duePeriod}
            categoryId={categoryId}
            status={status}
            onDuePeriodChange={setDuePeriod}
            onCategoryChange={setCategoryId}
            onStatusChange={setStatus}
            onOpenFilters={() => setIsFiltersOpen(true)}
          />

          {/* List */}
          <ExpenseList
            expenses={expenses}
            onEdit={handleEdit}
            onDelete={handleDelete}
            onMarkAsPaid={handleMarkAsPaid}
          />
        </LoadingState>
      </div>

      {/* Modal Create/Edit */}
      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={editingExpense ? "Editar Despesa" : "Nova Despesa"}
      >
        <ExpenseForm
          expense={editingExpense}
          onSubmit={handleSubmit}
          onCancel={handleCloseModal}
        />
      </Modal>

      <BottomSheet
        isOpen={isFiltersOpen}
        onClose={() => setIsFiltersOpen(false)}
        title="Filtros"
      >
        <div className="space-y-4">
          <ExpensesFiltersContent
            period={duePeriod}
            categoryId={categoryId}
            status={status}
            onDuePeriodChange={setDuePeriod}
            onCategoryChange={setCategoryId}
            onStatusChange={setStatus}
          />

          <div className="pt-4 pb-2 border-t border-gray-200">
            <Button onClick={() => setIsFiltersOpen(false)} className="w-full">
              Aplicar Filtros
            </Button>
          </div>
        </div>
      </BottomSheet>
    </>
  );
}
