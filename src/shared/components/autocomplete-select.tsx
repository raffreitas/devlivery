import { useEffect, useMemo, useRef, useState } from "react";

type BaseOption = {
  value: string;
  label: string;
  description?: string;
};

export type AutocompleteOption = BaseOption;

interface AutocompleteSelectProps {
  id?: string;
  name?: string;
  label?: string;
  placeholder?: string;
  value?: string | null;
  disabled?: boolean;
  required?: boolean;
  className?: string;
  emptyMessage?: string;
  options: AutocompleteOption[];
  onChange: (value: string | null) => void;
}

export function AutocompleteSelect({
  id,
  name,
  label,
  placeholder = "",
  value = null,
  disabled = false,
  required = false,
  className = "",
  emptyMessage = "Nenhuma opção encontrada",
  options,
  onChange,
}: AutocompleteSelectProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [inputValue, setInputValue] = useState("");
  const [highlightedIndex, setHighlightedIndex] = useState<number>(-1);

  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

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

  useEffect(() => {
    if (isOpen) {
      return;
    }

    setInputValue(selectedOption?.label ?? "");
  }, [selectedOption, isOpen]);

  useEffect(() => {
    if (highlightedIndex >= filteredOptions.length) {
      setHighlightedIndex(filteredOptions.length - 1);
    }
  }, [filteredOptions, highlightedIndex]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        containerRef.current &&
        event.target instanceof Node &&
        !containerRef.current.contains(event.target)
      ) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleSelect = (option: AutocompleteOption) => {
    onChange(option.value);
    setInputValue(option.label);
    setIsOpen(false);
    setHighlightedIndex(-1);
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const nextValue = event.target.value;
    setInputValue(nextValue);
    setIsOpen(true);
    setHighlightedIndex(0);

    if (selectedOption && nextValue !== selectedOption.label) {
      onChange(null);
    }
  };

  const handleInputFocus = () => {
    setIsOpen(true);
    if (filteredOptions.length > 0) {
      setHighlightedIndex(0);
    }
  };

  const handleInputKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
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
        setInputValue(selectedOption?.label ?? "");
        setHighlightedIndex(-1);
        break;
      }
      default:
        break;
    }
  };

  const handleClear = () => {
    setInputValue("");
    setIsOpen(false);
    setHighlightedIndex(-1);
    onChange(null);
    inputRef.current?.focus();
  };

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
              ? `${id ?? "autocomplete"}-option-${filteredOptions[highlightedIndex]?.value}`
              : undefined
          }
        />

        {selectedOption && !disabled && (
          <button
            type="button"
            onClick={handleClear}
            className="absolute inset-y-0 right-2 flex items-center text-gray-400 hover:text-gray-600"
            aria-label="Limpar seleção"
          >
            ×
          </button>
        )}

        {isOpen && (
          <div
            id={`${id ?? "autocomplete"}-listbox`}
            role="listbox"
            className="absolute z-10 mt-2 w-full bg-white border border-gray-200 rounded-lg shadow-lg max-h-60 overflow-auto"
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
                  key={option.value}
                  type="button"
                  id={`${id ?? "autocomplete"}-option-${option.value}`}
                  role="option"
                  aria-selected={isSelected}
                  className={`w-full text-left px-3 py-2 cursor-pointer transition-colors ${
                    isHighlighted ? "bg-orange-50" : ""
                  } ${isSelected ? "text-orange-600" : "text-gray-900"}`}
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
          </div>
        )}
      </div>

      <input type="hidden" name={name} value={value ?? ""} />
    </div>
  );
}
