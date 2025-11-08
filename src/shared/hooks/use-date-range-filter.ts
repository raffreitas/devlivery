import { useEffect, useRef, useState } from "react";
import { formatDate } from "../utils/formatters";

interface UseDateRangeFilterOptions {
  defaultDaysBack?: number;
  debounceMs?: number;
}

export function useDateRangeFilter(options?: UseDateRangeFilterOptions) {
  const { defaultDaysBack = 0, debounceMs = 500 } = options ?? {};

  const today = new Date();
  const startDay = new Date();
  startDay.setDate(startDay.getDate() - defaultDaysBack);

  // Estado local (input) - atualiza instantaneamente
  const [inputStartDate, setInputStartDate] = useState<string>(
    formatDate(startDay),
  );
  const [inputEndDate, setInputEndDate] = useState<string>(formatDate(today));

  // Estado aplicado (usado para fetch) - com debounce
  const [startDate, setStartDate] = useState<string>(formatDate(startDay));
  const [endDate, setEndDate] = useState<string>(formatDate(today));

  // Ref estável para o debounceMs para evitar recriar o efeito
  const debounceRef = useRef(debounceMs);

  // Debounce: aplica mudanças após delay
  useEffect(() => {
    const timer = setTimeout(() => {
      // Validação: apenas aplica se as datas forem válidas
      if (inputStartDate && inputEndDate && inputStartDate <= inputEndDate) {
        setStartDate(inputStartDate);
        setEndDate(inputEndDate);
      }
    }, debounceRef.current);

    return () => clearTimeout(timer);
  }, [inputStartDate, inputEndDate]);

  const handleStartDateChange = (value: string) => {
    setInputStartDate(value);
  };

  const handleEndDateChange = (value: string) => {
    setInputEndDate(value);
  };

  const resetToToday = () => {
    const d = formatDate(new Date());
    setInputStartDate(d);
    setInputEndDate(d);
    setStartDate(d);
    setEndDate(d);
  };

  const isInvalid = !!(
    inputStartDate &&
    inputEndDate &&
    inputStartDate > inputEndDate
  );

  return {
    inputStartDate,
    inputEndDate,
    startDate,
    endDate,
    setStartDate: handleStartDateChange,
    setEndDate: handleEndDateChange,
    resetToToday,
    isInvalid,
  };
}
