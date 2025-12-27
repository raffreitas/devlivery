import { CheckIcon, ChevronsUpDownIcon, PlusIcon } from "lucide-react";
import { useId, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/shared/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/shared/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/shared/components/ui/popover";
import { cn } from "@/shared/lib/utils";
import { useExpenseCategories, useExpenseCategoriesManagement } from "../hooks/use-expenses";

interface SubcategoryComboboxProps {
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  className?: string;
  parentCategoryId?: string;
  allowCreate?: boolean;
  disabled?: boolean;
}

export function SubcategoryCombobox({
  value,
  onChange,
  placeholder = "Selecione uma subcategoria",
  className,
  parentCategoryId,
  allowCreate = true,
  disabled = false,
}: SubcategoryComboboxProps) {
  const { data: categories } = useExpenseCategories();
  const { createCategory, isCreating } = useExpenseCategoriesManagement();
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");

  const id = useId();

  // Encontra a categoria pai e suas subcategorias
  const parentCategory = categories?.find((cat) => cat.id === parentCategoryId);
  const subcategories = parentCategory?.subcategories?.filter((sub) => sub.isActive) ?? [];

  // Transforma subcategorias em opções do Combobox
  const options = subcategories.map((sub) => ({
    value: sub.id,
    label: sub.name,
  }));

  const selectedLabel = options.find((option) => option.value === value)?.label ?? value;

  const handleCreateClick = async (subcategoryName: string) => {
    if (!subcategoryName.trim() || !parentCategoryId) return;

    try {
      const created = await createCategory({
        name: subcategoryName.trim(),
        parentCategoryId, // Usa a categoria pai já selecionada
      });
      onChange?.(created.id);
      setSearch("");
      setOpen(false);
      toast.success("Subcategoria criada com sucesso!");
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Erro ao criar subcategoria",
      );
    }
  };

  const hasExactMatch = options.some(
    (opt) => opt.label.toLowerCase() === search.toLowerCase(),
  );
  const showCreateOption =
    allowCreate &&
    parentCategoryId &&
    search.trim() &&
    !hasExactMatch &&
    search.trim().length > 0;

  // Se não tem categoria pai selecionada, mostra mensagem
  if (!parentCategoryId) {
    return (
      <div className={cn("w-full", className)}>
        <Button
          variant="outline"
          disabled
          className="bg-background hover:bg-background border-input w-full justify-between px-3 font-normal text-muted-foreground"
        >
          <span>Selecione uma categoria primeiro</span>
          <ChevronsUpDownIcon
            className="text-muted-foreground/80 shrink-0"
            aria-hidden="true"
          />
        </Button>
      </div>
    );
  }

  return (
    <>
      <div className={cn("w-full space-y-2", className)}>
        <Popover open={open && !disabled} onOpenChange={setOpen}>
          <PopoverTrigger asChild>
            <Button
              id={id}
              variant="outline"
              role="combobox"
              aria-expanded={open}
              disabled={disabled}
              className="bg-background hover:bg-background border-input w-full justify-between px-3 font-normal outline-offset-0 outline-none focus-visible:outline-[3px] disabled:opacity-50"
            >
              <span className={cn("truncate", !value && "text-muted-foreground")}>
                {value ? (
                  selectedLabel
                ) : (
                  <span className="text-muted-foreground">{placeholder}</span>
                )}
              </span>
              <ChevronsUpDownIcon
                className="text-muted-foreground/80 shrink-0"
                aria-hidden="true"
              />
            </Button>
          </PopoverTrigger>
          <PopoverContent
            className="border-input w-full min-w-(--radix-popper-anchor-width) p-0"
            align="start"
          >
            <Command>
              <CommandInput
                placeholder={placeholder}
                onValueChange={setSearch}
                value={search}
              />
              <CommandList>
                <CommandEmpty className="p-1">
                  {allowCreate && search.trim() && parentCategoryId ? (
                    <Button
                      type="button"
                      className="w-full justify-start p-1.5! px-2! font-normal text-accent-foreground h-8"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        handleCreateClick(search.trim());
                      }}
                      variant="ghost"
                      disabled={isCreating}
                    >
                      <PlusIcon className="mr-2 size-4 text-muted-foreground" />
                      {isCreating ? "Criando..." : `Criar subcategoria "${search.trim()}"`}
                    </Button>
                  ) : (
                    <p className="text-muted-foreground text-center text-sm py-2">
                      {parentCategoryId
                        ? "Nenhuma subcategoria encontrada."
                        : "Selecione uma categoria primeiro."}
                    </p>
                  )}
                </CommandEmpty>
                <CommandGroup>
                  {options.map((option) => (
                    <CommandItem
                      key={option.value}
                      value={option.label}
                      onSelect={() => {
                        onChange?.(option.value);
                        setSearch("");
                        setOpen(false);
                      }}
                    >
                      {option.label}
                      {value === option.value && (
                        <CheckIcon size={16} className="ml-auto" />
                      )}
                    </CommandItem>
                  ))}
                </CommandGroup>
                {showCreateOption && (
                  <>
                    <CommandSeparator />
                    <CommandGroup>
                      <CommandItem
                        onSelect={() => handleCreateClick(search.trim())}
                        disabled={isCreating}
                      >
                        <PlusIcon className="mr-2 size-4" />
                        {isCreating ? "Criando..." : `Criar subcategoria "${search.trim()}"`}
                      </CommandItem>
                    </CommandGroup>
                  </>
                )}
              </CommandList>
            </Command>
          </PopoverContent>
        </Popover>
      </div>
    </>
  );
}
