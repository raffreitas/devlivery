import { CheckIcon, ChevronsUpDownIcon, PlusIcon } from "lucide-react";
import { useId, useState } from "react";
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

export interface ComboboxOption {
  value: string;
  label: string;
}

interface ComboboxProps {
  options: ComboboxOption[];
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  emptyText?: string;
  className?: string;
  allowCustomValue?: boolean;
}

export function Combobox({
  options,
  value,
  allowCustomValue,
  onChange,
  placeholder = "Selecione uma opção",
  emptyText = "Nenhuma opção encontrada.",
  className,
}: ComboboxProps) {
  const id = useId();
  const [open, setOpen] = useState<boolean>(false);
  const [search, setSearch] = useState("");
  const [customOptions, setCustomOptions] = useState<ComboboxOption[]>([]);

  const allOptions = [
    ...options,
    ...customOptions.filter(
      (custom) => !options.some((opt) => opt.value === custom.value),
    ),
  ];

  const handleAddCustomValue = (newValue: string) => {
    const trimmed = newValue.trim();
    if (!trimmed) return;

    if (!allOptions.some((opt) => opt.value === trimmed)) {
      setCustomOptions((prev) => [...prev, { value: trimmed, label: trimmed }]);
    }

    onChange?.(trimmed);
    setSearch("");
    setOpen(false);
  };

  const selectedLabel =
    allOptions.find((option) => option.value === value)?.label ?? value;

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
                {allowCustomValue ? (
                  <Button
                    className="w-full justify-start p-1.5! px-2! font-normal text-accent-foreground h-8"
                    onClick={() => handleAddCustomValue(search.trim())}
                    variant="ghost"
                  >
                    <PlusIcon className="mr-2 size-4 text-muted-foreground" />
                    Adicionar "{search.trim()}"
                  </Button>
                ) : (
                  <p className="text-muted-foreground text-center text-sm py-2">
                    {emptyText}
                  </p>
                )}
              </CommandEmpty>
              <CommandGroup>
                {allOptions.map((option) => (
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
              {allowCustomValue &&
                search.trim() &&
                !allOptions.map((o) => o.value).includes(search.trim()) && (
                  <>
                    <CommandSeparator />
                    <CommandGroup>
                      <CommandItem
                        onSelect={() => handleAddCustomValue(search)}
                      >
                        <PlusIcon className="mr-2 size-4" />
                        Adicionar "{search.trim()}"
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
