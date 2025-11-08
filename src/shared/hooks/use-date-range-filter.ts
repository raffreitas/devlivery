import { useState } from "react";
import { formatDate } from "../utils/formaters";

interface UseDateRangeFilterOptions {
  defaultDaysBack?: number;
}

export function useDateRangeFilter(options?: UseDateRangeFilterOptions) {
  const { defaultDaysBack = 0 } = options ?? {};

  const today = new Date();
  const startDay = new Date();
  startDay.setDate(startDay.getDate() - defaultDaysBack);

  const [inputStart, setInputStart] = useState<string>(formatDate(startDay));
  const [inputEnd, setInputEnd] = useState<string>(formatDate(today));
  const [startDate, setStartDate] = useState<string>(formatDate(startDay));
  const [endDate, setEndDate] = useState<string>(formatDate(today));

  const applyRange = () => {
    if (inputStart && inputEnd && inputStart > inputEnd) return;
    setStartDate(inputStart);
    setEndDate(inputEnd);
  };

  const resetToToday = () => {
    const d = formatDate(new Date());
    setInputStart(d);
    setInputEnd(d);
    setStartDate(d);
    setEndDate(d);
  };

  const isInvalid = !!(inputStart && inputEnd && inputStart > inputEnd);

  return {
    inputStart,
    setInputStart,
    inputEnd,
    setInputEnd,
    startDate,
    endDate,
    applyRange,
    resetToToday,
    isInvalid,
  };
}
