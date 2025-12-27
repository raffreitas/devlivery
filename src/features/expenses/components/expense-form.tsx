import { zodResolver } from "@hookform/resolvers/zod";
import { format } from "date-fns";
import { useEffect, useMemo } from "react";
import { useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Textarea } from "@/shared/components/ui/textarea";
import { LoadingButton, LoadingState } from "@/shared/components/loading";
import { useExpenseCategories } from "../hooks/use-expenses";
import type { Expense, ExpenseFormData } from "../types";
import { expenseFormSchema } from "../types";

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
    // Validação dinâmica: se a categoria selecionada tem subcategorias ativas,
    // então a subcategoria é obrigatória.
    const selected = categories?.find((c) => c.id === data.categoryId);
    const activeSubcategories =
      selected?.subcategories?.filter((s) => s.isActive) ?? [];
    if (activeSubcategories.length > 0 && !data.subcategoryId) {
      form.setError("subcategoryId", {
        type: "required",
        message: "Subcategoria é obrigatória para a categoria selecionada",
      });
      form.setFocus("subcategoryId");
      return;
    }

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
                <Select onValueChange={field.onChange} value={field.value}>
                  <FormControl>
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Selecione a categoria" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {categories
                      ?.filter((cat) => cat.isActive)
                      .map((cat) => (
                        <SelectItem key={cat.id} value={cat.id}>
                          {cat.name}
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
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
                <FormLabel>Subcategoria</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={!selectedCategoryId || subcategories.length === 0}
                >
                  <FormControl>
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Selecione a subcategoria" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {subcategories
                      .filter((sub) => sub.isActive)
                      .map((sub) => (
                        <SelectItem key={sub.id} value={sub.id}>
                          {sub.name}
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
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
          <div className="flex gap-2 justify-end pt-2">
            {onCancel && (
              <Button
                type="button"
                variant="outline"
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
            >
              {expense ? "Atualizar" : "Criar Despesa"}
            </LoadingButton>
          </div>
        </form>
      </Form>
    </LoadingState>
  );
}
