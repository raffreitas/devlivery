import { formatMoney, removeChars } from "@/shared/utils/formatters";
import { Input } from "./input";

type BaseInputProps = Omit<
  React.ComponentProps<typeof Input>,
  "onChange" | "onBlur" | "value"
>;

type InputMoneyProps = BaseInputProps & {
  value?: number | string | undefined;
  // onChange receives the numeric value in reais (e.g. 2 -> R$ 2,00)
  onChange?: (value: number) => void;
  // onBlur receives the numeric value in reais
  onBlur?: (value: number) => void;
};

export function InputMoney({
  value,
  onChange,
  onBlur,
  ...rest
}: InputMoneyProps) {
  const handleChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const numeric = Number(removeChars(e.target.value)) / 100;
    onChange?.(Number.isFinite(numeric) ? numeric : 0);
  };

  const handleBlur: React.FocusEventHandler<HTMLInputElement> = (e) => {
    const numeric = Number(removeChars(e.currentTarget.value)) / 100;
    onBlur?.(Number.isFinite(numeric) ? numeric : 0);
  };

  return (
    <Input
      type="text"
      inputMode="numeric"
      {...rest}
      onChange={handleChange}
      onBlur={handleBlur}
      value={formatMoney(value)}
    />
  );
}
