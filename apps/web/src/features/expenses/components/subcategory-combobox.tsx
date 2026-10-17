import type { ComponentProps } from "react";
import { OptionCombobox } from "@/shared/components/option-combobox";
import { toast } from "@/shared/components/ui/toast";
import {
  useExpenseCategories,
  useExpenseCategoriesManagement,
} from "../hooks/use-expenses";

type SubcategoryComboboxProps = Omit<
  ComponentProps<typeof OptionCombobox>,
  "options" | "onCreate" | "createLabel" | "isCreating"
> & {
  parentCategoryId?: string;
  allowCreate?: boolean;
};

export function SubcategoryCombobox({
  placeholder = "Selecione uma subcategoria",
  parentCategoryId,
  allowCreate = true,
  disabled,
  ...props
}: SubcategoryComboboxProps) {
  const { data: categories } = useExpenseCategories();
  const { createCategory, isCreating } = useExpenseCategoriesManagement();

  // Encontra a categoria pai e suas subcategorias
  const parentCategory = categories?.find((cat) => cat.id === parentCategoryId);
  const options =
    parentCategory?.subcategories
      ?.filter((sub) => sub.isActive)
      .map((sub) => ({ value: sub.id, label: sub.name })) ?? [];

  const handleCreate = async (name: string) => {
    if (!parentCategoryId) return;

    try {
      const created = await createCategory({
        name,
        parentCategoryId, // Usa a categoria pai já selecionada
      });
      toast.add({ type: "success", title: "Subcategoria criada com sucesso!" });
      return { value: created.id, label: created.name };
    } catch {
      // Reported by the global MutationCache.
    }
  };

  return (
    <OptionCombobox
      key={parentCategoryId ?? "no-parent"}
      {...props}
      options={options}
      placeholder={
        parentCategoryId ? placeholder : "Selecione uma categoria primeiro"
      }
      emptyText="Nenhuma subcategoria encontrada."
      disabled={disabled || !parentCategoryId}
      onCreate={allowCreate ? handleCreate : undefined}
      createLabel={(name) => `Criar subcategoria "${name}"`}
      isCreating={isCreating}
    />
  );
}
