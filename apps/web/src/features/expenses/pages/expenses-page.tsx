import { addMonths, endOfMonth, startOfMonth } from "date-fns";
import { Plus } from "lucide-react";
import { useState } from "react";
import type { DateRange } from "react-day-picker";
import { LoadingState, TableSkeleton } from "@/shared/components/loading";
import { Modal } from "@/shared/components/modal";
import { QueryState } from "@/shared/components/query-state";
import { Button } from "@/shared/components/ui/button";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
} from "@/shared/components/ui/drawer";
import { toast } from "@/shared/components/ui/toast";
import { ExpenseForm } from "../components/expense-form";
import { ExpenseList } from "../components/expense-list";
import { ExpenseSummaryCard } from "../components/expense-summary-card";
import { ExpensesFilters } from "../components/expenses-filters";
import { ExpensesFiltersContent } from "../components/expenses-filters-content";
import { useExpenses } from "../hooks/use-expenses";
import type { Expense, ExpenseFormData, ExpenseStatus } from "../types";

function getDefaultDuePeriod(): DateRange {
  const today = new Date();

  return {
    from: startOfMonth(addMonths(today, -1)),
    to: endOfMonth(addMonths(today, 1)),
  };
}

export function ExpensesPage() {
  const [duePeriod, setDuePeriod] = useState<DateRange | undefined>(() =>
    getDefaultDuePeriod(),
  );
  const [categoryId, setCategoryId] = useState<string | undefined>();
  const [status, setStatus] = useState<ExpenseStatus | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingExpense, setEditingExpense] = useState<Expense | undefined>();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

  const {
    expenses,
    summary,
    isSaving,
    loading,
    error,
    hasData,
    refetch,
    createExpense,
    updateExpense,
    deleteExpense,
    markAsPaid,
  } = useExpenses({ duePeriod, categoryId, status });

  const handleSubmit = async (data: ExpenseFormData) => {
    if (editingExpense) {
      await updateExpense(editingExpense.id, data);
      toast.add({
        type: "success",
        title: "Despesa atualizada com sucesso!",
      });
    } else {
      await createExpense(data);
      toast.add({ type: "success", title: "Despesa criada com sucesso!" });
    }
    handleCloseModal();
  };

  const handleEdit = (expense: Expense) => {
    // Despesas canceladas não podem ser editadas.
    if (expense.status === "Cancelled") {
      toast.add({
        type: "error",
        title: "Não é permitido editar uma despesa cancelada.",
      });
      return;
    }
    setEditingExpense(expense);
    setIsModalOpen(true);
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteExpense(id);
      toast.add({ type: "success", title: "Despesa excluída com sucesso!" });
    } catch {
      // Reported by the global MutationCache.
    }
  };

  const handleMarkAsPaid = async (id: string, paymentDate: string) => {
    try {
      await markAsPaid(id, paymentDate);
      toast.add({ type: "success", title: "Despesa marcada como paga!" });
    } catch {
      // Reported by the global MutationCache.
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
      <div className="flex flex-col gap-4 sm:gap-6">
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
            <Plus data-icon="inline-start" />
            Nova Despesa
          </Button>
        </div>

        <QueryState
          isPending={loading}
          error={error}
          hasData={hasData}
          refetch={refetch}
        >
          {" "}
          <LoadingState
            isLoading={loading}
            skeleton={
              <div className="flex flex-col gap-4 sm:gap-6">
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
            <ExpenseSummaryCard summary={summary} />

            <ExpensesFilters
              period={duePeriod}
              categoryId={categoryId}
              status={status}
              onDuePeriodChange={setDuePeriod}
              onCategoryChange={setCategoryId}
              onStatusChange={setStatus}
              onOpenFilters={() => setIsFiltersOpen(true)}
            />

            <ExpenseList
              expenses={expenses}
              onEdit={handleEdit}
              onDelete={handleDelete}
              onMarkAsPaid={handleMarkAsPaid}
            />
          </LoadingState>
        </QueryState>
      </div>

      <Modal
        dismissible={!isSaving}
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={editingExpense ? "Editar Despesa" : "Nova Despesa"}
      >
        <ExpenseForm
          isSubmitting={isSaving}
          expense={editingExpense}
          onSubmit={handleSubmit}
          onCancel={handleCloseModal}
        />
      </Modal>

      <Drawer
        open={isFiltersOpen}
        onOpenChange={setIsFiltersOpen}
        showSwipeHandle
      >
        <DrawerContent>
          <DrawerHeader>
            <DrawerTitle>Filtros</DrawerTitle>
          </DrawerHeader>
          <div className="min-h-0 flex-1 overflow-y-auto p-4">
            <ExpensesFiltersContent
              period={duePeriod}
              categoryId={categoryId}
              status={status}
              onDuePeriodChange={setDuePeriod}
              onCategoryChange={setCategoryId}
              onStatusChange={setStatus}
            />
          </div>
          <DrawerFooter>
            <DrawerClose render={<Button />}>Aplicar Filtros</DrawerClose>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>
    </>
  );
}
