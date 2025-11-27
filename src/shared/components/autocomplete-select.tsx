import { X } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";

type BaseOption<T extends string = string> = {
  value: T;
  label: string;
  description?: string;
};

export type AutocompleteOption<T extends string = string> = BaseOption<T>;

interface AutocompleteSelectProps<T extends string = string> {
  id?: string;
  name?: string;
  label?: string;
  placeholder?: string;
  value?: T | null;
  disabled?: boolean;
  required?: boolean;
  className?: string;
  emptyMessage?: string;
  autocomplete?: boolean;
  /**
   * When true, the component will call `onChange` with the raw typed value
   * (allowing creation of new values not present in `options`).
   */
  allowCustomValue?: boolean;
  options: AutocompleteOption<T>[];
  onChange: (value: T | null) => void;
}

export function AutocompleteSelect<T extends string = string>({
  id,
  name,
  label,
  placeholder = "",
  value = null as T | null,
  disabled = false,
  required = false,
  className = "",
  emptyMessage = "Nenhuma opção encontrada",
  options,
  autocomplete = true,
  allowCustomValue = false,
  onChange,
}: AutocompleteSelectProps<T>) {
  const [isOpen, setIsOpen] = useState(false);
  const [inputValue, setInputValue] = useState("");
  const [highlightedIndex, setHighlightedIndex] = useState<number>(-1);
  const [dropdownPosition, setDropdownPosition] = useState<{
    top: number;
    left: number;
    width: number;
  } | null>(null);

  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const selectedOption = useMemo(
    () => options.find((option) => option.value === value) ?? null,
    [options, value],
  );

  const filteredOptions = useMemo(() => {
    const normalizedQuery = inputValue.trim().toLowerCase();

    if (!normalizedQuery) {
      return options;
    }

    return options.filter((option) =>
      option.label.trim().toLowerCase().includes(normalizedQuery),
    );
  }, [inputValue, options]);

  // Calculate display value based on mode and current state
  const displayValue = useMemo(() => {
    if (allowCustomValue && value && !selectedOption) {
      return value;
    }
    return selectedOption?.label ?? "";
  }, [allowCustomValue, value, selectedOption]);

  // Sync inputValue with displayValue when dropdown closes
  useEffect(() => {
    if (!isOpen) {
      setInputValue(displayValue);
    }
  }, [isOpen, displayValue]);

  // Reset highlighted index if it's out of bounds
  useEffect(() => {
    if (
      highlightedIndex >= filteredOptions.length &&
      filteredOptions.length > 0
    ) {
      setHighlightedIndex(filteredOptions.length - 1);
    } else if (filteredOptions.length === 0) {
      setHighlightedIndex(-1);
    }
  }, [filteredOptions.length, highlightedIndex]);

  // Update dropdown position when opened or on scroll/resize
  const updateDropdownPosition = useCallback(() => {
    if (inputRef.current) {
      const rect = inputRef.current.getBoundingClientRect();
      setDropdownPosition({
        top: rect.bottom + window.scrollY + 8,
        left: rect.left + window.scrollX,
        width: rect.width,
      });
    }
  }, []);

  useEffect(() => {
    if (isOpen) {
      updateDropdownPosition();
      window.addEventListener("scroll", updateDropdownPosition, true);
      window.addEventListener("resize", updateDropdownPosition);
    } else {
      setDropdownPosition(null);
    }

    return () => {
      window.removeEventListener("scroll", updateDropdownPosition, true);
      window.removeEventListener("resize", updateDropdownPosition);
    };
  }, [isOpen, updateDropdownPosition]);

  const handleClickOutside = useCallback((event: MouseEvent) => {
    if (
      containerRef.current &&
      event.target instanceof Node &&
      !containerRef.current.contains(event.target) &&
      dropdownRef.current &&
      !dropdownRef.current.contains(event.target)
    ) {
      setIsOpen(false);
    }
  }, []);

  useEffect(() => {
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [handleClickOutside]);

  const handleSelect = useCallback(
    (option: AutocompleteOption<T>) => {
      onChange(option.value);
      setInputValue(option.label);
      setIsOpen(false);
      setHighlightedIndex(-1);
    },
    [onChange],
  );

  const handleInputChange = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      const nextValue = event.target.value;
      setInputValue(nextValue);
      setIsOpen(true);
      setHighlightedIndex(0);

      if (allowCustomValue) {
        // forward the typed value to the parent so it can create a new entry
        // note: typed value is always a string and T extends string
        onChange(nextValue as T);
      } else {
        if (selectedOption && nextValue !== selectedOption.label) {
          onChange(null);
        }
      }
    },
    [allowCustomValue, onChange, selectedOption],
  );

  const handleInputFocus = useCallback(() => {
    setIsOpen(true);
    if (filteredOptions.length > 0) {
      setHighlightedIndex(0);
    }
  }, [filteredOptions.length]);

  const handleInputKeyDown = useCallback(
    (event: React.KeyboardEvent<HTMLInputElement>) => {
      if (!isOpen && ["ArrowDown", "ArrowUp"].includes(event.key)) {
        setIsOpen(true);
        setHighlightedIndex(0);
        return;
      }

      switch (event.key) {
        case "ArrowDown": {
          event.preventDefault();
          setHighlightedIndex((prev) => {
            const nextIndex = Math.min(prev + 1, filteredOptions.length - 1);
            return nextIndex < 0 ? 0 : nextIndex;
          });
          break;
        }
        case "ArrowUp": {
          event.preventDefault();
          setHighlightedIndex((prev) => Math.max(prev - 1, 0));
          break;
        }
        case "Enter": {
          if (isOpen && highlightedIndex >= 0) {
            event.preventDefault();
            const option = filteredOptions[highlightedIndex];
            if (option) {
              handleSelect(option);
            }
          }
          break;
        }
        case "Escape": {
          event.preventDefault();
          setIsOpen(false);
          setInputValue(displayValue);
          setHighlightedIndex(-1);
          break;
        }
        default:
          break;
      }
    },
    [
      isOpen,
      filteredOptions.length,
      highlightedIndex,
      filteredOptions,
      handleSelect,
      displayValue,
    ],
  );

  const handleClear = useCallback(() => {
    setInputValue("");
    setIsOpen(false);
    setHighlightedIndex(-1);
    onChange(null);
    inputRef.current?.focus();
  }, [onChange]);

  return (
    <div className={`w-full ${className}`} ref={containerRef}>
      {label && (
        <label
          htmlFor={id}
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          {label}
        </label>
      )}

      <div className="relative">
        <input
          id={id}
          ref={inputRef}
          name={name}
          type="text"
          readOnly={!autocomplete}
          value={inputValue}
          placeholder={placeholder}
          disabled={disabled}
          required={required && !selectedOption}
          onChange={handleInputChange}
          onFocus={handleInputFocus}
          onKeyDown={handleInputKeyDown}
          className={`w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-orange-500 focus:border-orange-500 pr-9 ${disabled ? "bg-gray-100 cursor-not-allowed" : ""}`}
          role="combobox"
          aria-expanded={isOpen}
          aria-autocomplete="list"
          aria-controls={`${id ?? "autocomplete"}-listbox`}
          aria-activedescendant={
            isOpen && highlightedIndex >= 0
              ? `${id ?? "autocomplete"}-option-${String(filteredOptions[highlightedIndex]?.value)}`
              : undefined
          }
        />

        {inputValue && !disabled && (
          <button
            type="button"
            onClick={handleClear}
            className="absolute inset-y-0 right-2 flex items-center text-gray-400 hover:text-secondary-foreground"
            aria-label="Limpar seleção"
          >
            <X className="w-4 h-4" />
          </button>
        )}
      </div>

      {isOpen &&
        dropdownPosition &&
        createPortal(
          <div
            ref={dropdownRef}
            id={`${id ?? "autocomplete"}-listbox`}
            role="listbox"
            style={{
              position: "fixed",
              top: `${dropdownPosition.top}px`,
              left: `${dropdownPosition.left}px`,
              width: `${dropdownPosition.width}px`,
            }}
            className="z-100 bg-white border border-gray-200 rounded-lg shadow-lg max-h-60 overflow-auto"
          >
            {filteredOptions.length === 0 && (
              <div className="px-3 py-2 text-sm text-gray-500">
                {emptyMessage}
              </div>
            )}

            {filteredOptions.map((option, index) => {
              const isHighlighted = index === highlightedIndex;
              const isSelected = option.value === value;

              return (
                <button
                  key={String(option.value)}
                  type="button"
                  id={`${id ?? "autocomplete"}-option-${String(option.value)}`}
                  role="option"
                  aria-selected={isSelected}
                  className={`w-full text-left px-3 py-2 cursor-pointer transition-colors ${
                    isHighlighted ? "bg-orange-50" : ""
                  } ${isSelected ? "text-primary" : "text-gray-900"}`}
                  onMouseEnter={() => setHighlightedIndex(index)}
                  onMouseDown={(event) => event.preventDefault()}
                  onClick={() => handleSelect(option)}
                >
                  <span className="block text-sm font-medium">
                    {option.label}
                  </span>
                  {option.description && (
                    <span className="block text-xs text-gray-500">
                      {option.description}
                    </span>
                  )}
                </button>
              );
            })}
          </div>,
          document.body,
        )}

      <input type="hidden" name={name} value={value ?? ""} />
    </div>
  );
}
