import { Combobox as ComboboxPrimitive } from "@base-ui/react/combobox";
import { PlusIcon } from "lucide-react";
import { type Ref, useEffect, useRef, useState } from "react";
import {
  Combobox,
  ComboboxContent,
  ComboboxEmpty,
  ComboboxInput,
  ComboboxItem,
  ComboboxList,
} from "@/shared/components/ui/combobox";

export interface ComboboxOption {
  value: string;
  label: string;
}

type ComboboxEntry = ComboboxOption & { create?: string };

interface OptionComboboxProps {
  id?: string;
  name?: string;
  inputRef?: Ref<HTMLInputElement>;
  options: ComboboxOption[];
  value?: string;
  onValueChange: (value: string) => void;
  onBlur?: () => void;
  placeholder?: string;
  emptyText?: string;
  disabled?: boolean;
  "aria-invalid"?: boolean;
  "aria-describedby"?: string;
  className?: string;
  // Quando informado, oferece a opção de criar um item com o texto digitado
  onCreate?: (label: string) => Promise<ComboboxOption | undefined>;
  createLabel?: (label: string) => string;
  isCreating?: boolean;
}

export function OptionCombobox({
  id,
  name,
  inputRef,
  options,
  value,
  onValueChange,
  onBlur,
  placeholder,
  emptyText = "Nenhuma opção encontrada.",
  disabled,
  "aria-invalid": ariaInvalid,
  "aria-describedby": ariaDescribedBy,
  className,
  onCreate,
  createLabel = (label) => `Adicionar "${label}"`,
  isCreating,
}: OptionComboboxProps) {
  const { contains } = ComboboxPrimitive.useFilter();
  const mounted = useRef(false);

  useEffect(() => {
    mounted.current = true;
    return () => {
      mounted.current = false;
    };
  }, []);
  const [query, setQuery] = useState("");
  // Mantém itens recém-criados visíveis até a lista de opções ser atualizada
  const [created, setCreated] = useState<ComboboxOption[]>([]);

  const allOptions = [
    ...options,
    ...created.filter((c) => !options.some((o) => o.value === c.value)),
  ];
  const selected = allOptions.find((o) => o.value === value) ?? null;

  const search = query.trim();
  const canCreate =
    !!onCreate &&
    !!search &&
    !allOptions.some(
      (o) =>
        o.label.localeCompare(search, undefined, { sensitivity: "base" }) === 0,
    );
  const items: ComboboxEntry[] = canCreate
    ? [...allOptions, { value: "", label: createLabel(search), create: search }]
    : allOptions;

  const handleValueChange = async (item: ComboboxEntry | null) => {
    if (!item) return;
    if (item.create === undefined) {
      onValueChange(item.value);
      return;
    }
    const option = await onCreate?.(item.create);
    if (!option || !mounted.current) return;
    setCreated((prev) => [...prev, option]);
    onValueChange(option.value);
  };

  return (
    <Combobox<ComboboxEntry>
      items={items}
      value={selected}
      onValueChange={handleValueChange}
      onInputValueChange={setQuery}
      isItemEqualToValue={(item, current) =>
        item.create === undefined && item.value === current.value
      }
      filter={(item, q) => item.create !== undefined || contains(item.label, q)}
      name={name}
      inputRef={inputRef}
      disabled={disabled}
    >
      <ComboboxInput
        id={id}
        placeholder={placeholder}
        disabled={disabled}
        aria-invalid={ariaInvalid}
        aria-describedby={ariaDescribedBy}
        onBlur={onBlur}
        className={className ?? "w-full"}
      />
      <ComboboxContent>
        <ComboboxEmpty>{emptyText}</ComboboxEmpty>
        <ComboboxList>
          {(item: ComboboxEntry) => (
            <ComboboxItem
              key={item.create === undefined ? item.value : "__create__"}
              value={item}
              disabled={item.create !== undefined && isCreating}
            >
              {item.create !== undefined && <PlusIcon />}
              {item.label}
            </ComboboxItem>
          )}
        </ComboboxList>
      </ComboboxContent>
    </Combobox>
  );
}
