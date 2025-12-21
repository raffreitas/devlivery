import { Plus } from "lucide-react";
import { useState } from "react";
import type { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { Modal } from "@/shared/components/modal";
import { Button } from "@/shared/components/ui/button";
import { ExpenseFiltersComponent } from "../components/expense-filters";
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
      toast.error("Erro ao salvar despesa");
      console.error(error);
    }
  };

  const handleEdit = (expense: Expense) => {
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
      toast.error("Erro ao marcar despesa como paga");
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

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex items-center justify-center h-64">
          <p className="text-muted-foreground">Carregando despesas...</p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Despesas</h1>
            <p className="text-muted-foreground">
              Gerencie as despesas do estabelecimento
            </p>
          </div>
          <Button onClick={handleOpenCreateModal}>
            <Plus className="mr-2 h-4 w-4" />
            Nova Despesa
          </Button>
        </div>

        {/* Summary Cards */}
        <ExpenseSummaryCard summary={summary} />

        {/* Filters */}
        <ExpenseFiltersComponent
          period={duePeriod}
          categoryId={categoryId}
          status={status}
          onDuePeriodChange={setDuePeriod}
          onCategoryChange={setCategoryId}
          onStatusChange={setStatus}
        />

        {/* List */}
        <ExpenseList
          expenses={expenses}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onMarkAsPaid={handleMarkAsPaid}
        />
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
    </>
  );
}
