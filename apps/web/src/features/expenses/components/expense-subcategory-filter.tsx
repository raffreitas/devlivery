import { useId } from "react";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { useExpenseCategories } from "../hooks/use-expenses";

interface ExpenseSubcategoryFilterProps {
  categoryId?: string;
  subcategoryId?: string;
  onSubcategoryChange: (id: string | undefined) => void;
}

export function ExpenseSubcategoryFilter({
  categoryId,
  subcategoryId,
  onSubcategoryChange,
}: ExpenseSubcategoryFilterProps) {
  const id = useId();
  const { data: categories } = useExpenseCategories();
  const subcategories =
    categories
      ?.find((category) => category.id === categoryId)
      ?.subcategories.filter((subcategory) => subcategory.isActive) ?? [];
  const label = !categoryId
    ? "Selecione uma categoria primeiro"
    : subcategories.length === 0
      ? "Nenhuma subcategoria disponível"
      : (subcategories.find((subcategory) => subcategory.id === subcategoryId)
          ?.name ?? "Todas");

  return (
    <div className="min-w-0 flex flex-col gap-2">
      <Label htmlFor={id}>Subcategoria</Label>
      <Select
        value={subcategoryId ?? "all"}
        disabled={!categoryId || subcategories.length === 0}
        onValueChange={(value) => {
          if (value !== null)
            onSubcategoryChange(value === "all" ? undefined : value);
        }}
      >
        <SelectTrigger id={id} className="w-full">
          <SelectValue>{label}</SelectValue>
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectItem value="all">Todas</SelectItem>
            {subcategories.map((subcategory) => (
              <SelectItem key={subcategory.id} value={subcategory.id}>
                {subcategory.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
}
