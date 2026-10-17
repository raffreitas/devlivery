import type { ComponentProps } from "react";
import { OptionCombobox } from "@/shared/components/option-combobox";
import { toast } from "@/shared/components/ui/toast";
import {
  useExpenseCategories,
  useExpenseCategoriesManagement,
} from "../hooks/use-expenses";

type CategoryComboboxProps = Omit<
  ComponentProps<typeof OptionCombobox>,
  "options" | "onCreate" | "createLabel" | "isCreating"
> & {
  allowCreate?: boolean;
};

export function CategoryCombobox({
  placeholder = "Selecione uma categoria",
  allowCreate = true,
  ...props
}: CategoryComboboxProps) {
  const { data: categories } = useExpenseCategories();
  const { createCategory, isCreating } = useExpenseCategoriesManagement();

  const options =
    categories
      ?.filter((cat) => cat.isActive)
      .map((cat) => ({ value: cat.id, label: cat.name })) ?? [];

  const handleCreate = async (name: string) => {
    try {
      const created = await createCategory({
        name,
        parentCategoryId: undefined, // Sempre categoria principal
      });
      toast.add({ type: "success", title: "Categoria criada com sucesso!" });
      return { value: created.id, label: created.name };
    } catch {
      // Reported by the global MutationCache.
    }
  };

  return (
    <OptionCombobox
      {...props}
      options={options}
      placeholder={placeholder}
      emptyText="Nenhuma categoria encontrada."
      onCreate={allowCreate ? handleCreate : undefined}
      createLabel={(name) => `Criar categoria "${name}"`}
      isCreating={isCreating}
    />
  );
}
