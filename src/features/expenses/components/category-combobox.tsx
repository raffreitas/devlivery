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
import {
  useExpenseCategories,
  useExpenseCategoriesManagement,
} from "../hooks/use-expenses";

interface CategoryComboboxProps {
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  className?: string;
  allowCreate?: boolean;
}

export function CategoryCombobox({
  value,
  onChange,
  placeholder = "Selecione uma categoria",
  className,
  allowCreate = true,
}: CategoryComboboxProps) {
  const { data: categories } = useExpenseCategories();
  const { createCategory, isCreating } = useExpenseCategoriesManagement();
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");

  const id = useId();

  // Transforma categorias em opções do Combobox (apenas categorias principais, não subcategorias)
  const options =
    categories
      ?.filter((cat) => cat.isActive)
      .map((cat) => ({
        value: cat.id,
        label: cat.name,
      })) ?? [];

  const selectedLabel =
    options.find((option) => option.value === value)?.label ?? value;

  const handleCreateClick = async (categoryName: string) => {
    if (!categoryName.trim()) return;

    try {
      const created = await createCategory({
        name: categoryName.trim(),
        parentCategoryId: undefined, // Sempre categoria principal
      });
      onChange?.(created.id);
      setSearch("");
      setOpen(false);
      toast.success("Categoria criada com sucesso!");
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : "Erro ao criar categoria",
      );
    }
  };

  const hasExactMatch = options.some(
    (opt) => opt.label.toLowerCase() === search.toLowerCase(),
  );
  const showCreateOption =
    allowCreate && search.trim() && !hasExactMatch && search.trim().length > 0;

  return (
    <div className={cn("w-full space-y-2", className)}>
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            id={id}
            variant="outline"
            role="combobox"
            aria-expanded={open}
            className="bg-background hover:bg-background border-input w-full justify-between px-3 font-normal outline-offset-0 outline-none focus-visible:outline-[3px]"
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
                {allowCreate && search.trim() ? (
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
                    {isCreating
                      ? "Criando..."
                      : `Criar categoria "${search.trim()}"`}
                  </Button>
                ) : (
                  <p className="text-muted-foreground text-center text-sm py-2">
                    Nenhuma categoria encontrada.
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
                      {isCreating
                        ? "Criando..."
                        : `Criar categoria "${search.trim()}"`}
                    </CommandItem>
                  </CommandGroup>
                </>
              )}
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>
    </div>
  );
}
