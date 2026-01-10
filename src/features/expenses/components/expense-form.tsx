import { zodResolver } from "@hookform/resolvers/zod";
import { format } from "date-fns";
import { useEffect, useMemo } from "react";
import { useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";
import { LoadingButton, LoadingState } from "@/shared/components/loading";
import { Button } from "@/shared/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/components/ui/form";
import { Input } from "@/shared/components/ui/input";
import { InputMoney } from "@/shared/components/ui/input-money";
import { Textarea } from "@/shared/components/ui/textarea";
import { useExpenseCategories } from "../hooks/use-expenses";
import type { Expense, ExpenseFormData } from "../types";
import { expenseFormSchema } from "../types";
import { CategoryCombobox } from "./category-combobox";
import { SubcategoryCombobox } from "./subcategory-combobox";

interface ExpenseFormProps {
  expense?: Expense;
  onSubmit: (data: ExpenseFormData) => Promise<void>;
  onCancel?: () => void;
  isSubmitting?: boolean;
}

export function ExpenseForm({
  expense,
  onSubmit,
  onCancel,
  isSubmitting = false,
}: ExpenseFormProps) {
  const { data: categories, isLoading: loadingCategories } =
    useExpenseCategories();

  const form = useForm<ExpenseFormData>({
    resolver: zodResolver(expenseFormSchema),
    defaultValues: {
      categoryId: expense?.category.id ?? "",
      subcategoryId: expense?.category.subcategories?.[0]?.id ?? "",
      supplier: expense?.supplier ?? "",
      description: expense?.description ?? "",
      amount: expense?.amount ?? 0,
      dueDate: expense?.dueDate ? format(expense.dueDate, "yyyy-MM-dd") : "",
      paymentDate: expense?.paymentDate
        ? format(expense.paymentDate, "yyyy-MM-dd")
        : "",
    },
  });

  const selectedCategoryId = useWatch({
    control: form.control,
    name: "categoryId",
    defaultValue: expense?.category.id,
  });

  // Encontra a categoria selecionada e suas subcategorias
  const selectedCategory = useMemo(() => {
    return categories?.find((cat) => cat.id === selectedCategoryId);
  }, [categories, selectedCategoryId]);

  const subcategories = useMemo(() => {
    return selectedCategory?.subcategories ?? [];
  }, [selectedCategory]);

  useEffect(() => {
    if (loadingCategories) return;

    const currentSubcategoryId = form.getValues("subcategoryId");
    if (!selectedCategoryId) {
      form.setValue("subcategoryId", "");
      return;
    }

    const belongsToSelected = subcategories.some(
      (s) => s.id === currentSubcategoryId,
    );
    if (!belongsToSelected) {
      form.setValue("subcategoryId", "");
    }
  }, [selectedCategoryId, subcategories, form, loadingCategories]);

  const handleSubmit = async (data: ExpenseFormData) => {
    try {
      await onSubmit(data);
      if (!expense) {
        // Reseta o form apenas em modo criação
        form.reset();
      }
    } catch (error) {
      toast.error("Erro ao salvar despesa");
      console.error("Failed to save expense:", error);
    }
  };

  return (
    <LoadingState
      isLoading={loadingCategories}
      skeleton={
        <div className="p-4 space-y-4">
          <div className="h-10 bg-accent animate-pulse rounded-md" />
          <div className="h-10 bg-accent animate-pulse rounded-md" />
          <div className="h-10 bg-accent animate-pulse rounded-md" />
        </div>
      }
    >
      <Form {...form}>
        <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
          {/* Categoria */}
          <FormField
            control={form.control}
            name="categoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Categoria</FormLabel>
                <FormControl>
                  <CategoryCombobox
                    value={field.value}
                    onChange={field.onChange}
                    placeholder="Selecione ou crie uma categoria"
                    allowCreate
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Subcategoria */}
          <FormField
            control={form.control}
            name="subcategoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Subcategoria (opcional)</FormLabel>
                <FormControl>
                  <SubcategoryCombobox
                    value={field.value}
                    onChange={field.onChange}
                    placeholder="Selecione ou crie uma subcategoria"
                    parentCategoryId={selectedCategoryId}
                    allowCreate
                    disabled={!selectedCategoryId}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Fornecedor */}
          <FormField
            control={form.control}
            name="supplier"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Fornecedor (opcional)</FormLabel>
                <FormControl>
                  <Input
                    placeholder="Nome do fornecedor"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Descrição */}
          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Descrição (opcional)</FormLabel>
                <FormControl>
                  <Textarea
                    placeholder="Detalhes adicionais..."
                    className="resize-none"
                    rows={3}
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Valor */}
          <FormField
            control={form.control}
            name="amount"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Valor</FormLabel>
                <FormControl>
                  <InputMoney value={field.value} onChange={field.onChange} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Data de Vencimento */}
          <FormField
            control={form.control}
            name="dueDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Data de Vencimento</FormLabel>
                <FormControl>
                  <Input type="date" {...field} value={field.value ?? ""} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Data de Pagamento */}
          <FormField
            control={form.control}
            name="paymentDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Data de Pagamento (opcional)</FormLabel>
                <FormControl>
                  <Input type="date" {...field} value={field.value ?? ""} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Botões */}
          <div className="flex gap-2 justify-center sm:justify-end pt-2">
            {onCancel && (
              <Button
                type="button"
                variant="outline"
                className="flex-1 sm:flex-initial"
                onClick={onCancel}
                disabled={isSubmitting}
              >
                Cancelar
              </Button>
            )}
            <LoadingButton
              type="submit"
              isLoading={isSubmitting}
              loadingText="Salvando..."
              className="flex-1 sm:flex-initial"
            >
              {expense ? "Atualizar" : "Criar Despesa"}
            </LoadingButton>
          </div>
        </form>
      </Form>
    </LoadingState>
  );
}
