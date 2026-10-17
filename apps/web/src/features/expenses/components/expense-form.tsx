import { zodResolver } from "@hookform/resolvers/zod";
import { format } from "date-fns";
import { useEffect, useId, useMemo } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { InputMoney } from "@/shared/components/input-money";
import { LoadingButton, LoadingState } from "@/shared/components/loading";
import { Button } from "@/shared/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
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
  const formId = useId();
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
    if (expense?.status === "Paid" && !data.paymentDate) {
      form.setError("paymentDate", { message: "Informe a data de pagamento." });
      return;
    }
    try {
      await onSubmit(data);
      if (!expense) {
        // Reseta o form apenas em modo criação
        form.reset();
      }
    } catch {
      // Reported by the global MutationCache.
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
      <form onSubmit={form.handleSubmit(handleSubmit)}>
        <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
          <Controller
            control={form.control}
            name="categoryId"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-expense-category`}>
                  Categoria
                </FieldLabel>
                <CategoryCombobox
                  id={`${formId}-expense-category`}
                  name={field.name}
                  inputRef={field.ref}
                  value={field.value}
                  onValueChange={field.onChange}
                  onBlur={field.onBlur}
                  placeholder="Selecione ou crie uma categoria"
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-expense-category-error`
                      : undefined
                  }
                  allowCreate
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-expense-category-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />

          <Controller
            control={form.control}
            name="subcategoryId"
            render={({ field, fieldState }) => (
              <Field
                data-invalid={fieldState.invalid}
                data-disabled={!selectedCategoryId}
              >
                <FieldLabel htmlFor={`${formId}-expense-subcategory`}>
                  Subcategoria (opcional)
                </FieldLabel>
                <SubcategoryCombobox
                  id={`${formId}-expense-subcategory`}
                  name={field.name}
                  inputRef={field.ref}
                  value={field.value}
                  onValueChange={field.onChange}
                  onBlur={field.onBlur}
                  placeholder="Selecione ou crie uma subcategoria"
                  parentCategoryId={selectedCategoryId}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-expense-subcategory-error`
                      : undefined
                  }
                  allowCreate
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-expense-subcategory-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />

          <Controller
            control={form.control}
            name="supplier"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-expense-supplier`}>
                  Fornecedor (opcional)
                </FieldLabel>
                <Input
                  {...field}
                  id={`${formId}-expense-supplier`}
                  placeholder="Nome do fornecedor"
                  value={field.value ?? ""}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-expense-supplier-error`
                      : undefined
                  }
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-expense-supplier-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />

          <Controller
            control={form.control}
            name="description"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-expense-description`}>
                  Descrição (opcional)
                </FieldLabel>
                <Textarea
                  {...field}
                  id={`${formId}-expense-description`}
                  placeholder="Detalhes adicionais..."
                  className="resize-none"
                  rows={3}
                  value={field.value ?? ""}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-expense-description-error`
                      : undefined
                  }
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-expense-description-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />

          <Controller
            control={form.control}
            name="amount"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-expense-amount`}>
                  Valor
                </FieldLabel>
                <InputMoney
                  {...field}
                  id={`${formId}-expense-amount`}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-expense-amount-error`
                      : undefined
                  }
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-expense-amount-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />

          <Controller
            control={form.control}
            name="dueDate"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-expense-due-date`}>
                  Data de Vencimento
                </FieldLabel>
                <Input
                  {...field}
                  id={`${formId}-expense-due-date`}
                  type="date"
                  value={field.value ?? ""}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-expense-due-date-error`
                      : undefined
                  }
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-expense-due-date-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />

          <Controller
            control={form.control}
            name="paymentDate"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-expense-payment-date`}>
                  Data de Pagamento{" "}
                  {expense?.status === "Paid"
                    ? "(obrigatória)"
                    : expense
                      ? "(somente leitura)"
                      : "(opcional)"}
                </FieldLabel>
                <Input
                  {...field}
                  id={`${formId}-expense-payment-date`}
                  type="date"
                  value={field.value ?? ""}
                  readOnly={!!expense && expense.status !== "Paid"}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-expense-payment-date-error`
                      : undefined
                  }
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-expense-payment-date-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />

          <Field
            orientation="horizontal"
            className="justify-center sm:justify-end"
          >
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
          </Field>
        </FieldGroup>
      </form>
    </LoadingState>
  );
}
