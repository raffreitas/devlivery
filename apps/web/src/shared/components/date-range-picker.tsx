import { cn } from "cn";
import { format } from "date-fns";
import { ptBR } from "date-fns/locale";
import { Calendar as CalendarIcon } from "lucide-react";
import type { ComponentProps } from "react";
import { useEffect, useId, useState } from "react";
import type { DateRange } from "react-day-picker";
import { AccessibleCalendar } from "@/shared/components/accessible-calendar";
import { Button } from "@/shared/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/shared/components/ui/popover";

interface DateRangePickerProps extends ComponentProps<"div"> {
  date: DateRange | undefined;
  onDateChange: (date: DateRange | undefined) => void;
  selectNewRange?: boolean;
  triggerClassName?: string;
}

const EMPTY_LABEL_TEXT = "Selecione um período";

export function DateRangePicker({
  date,
  onDateChange,
  className,
  selectNewRange = false,
  triggerClassName,
}: DateRangePickerProps) {
  const id = useId();
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState<DateRange>();
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 640);
    };
    checkMobile();
    window.addEventListener("resize", checkMobile);
    return () => window.removeEventListener("resize", checkMobile);
  }, []);

  return (
    <div className={cn("grid gap-2", className)}>
      <Popover
        open={open}
        onOpenChange={(next) => {
          if (next && selectNewRange) setDraft(date);
          setOpen(next);
        }}
      >
        <PopoverTrigger
          render={
            <Button
              id={id}
              variant={"outline"}
              className={cn(
                "w-full sm:w-auto lg:w-62.5 justify-start px-3 text-left font-normal",
                !date && "text-muted-foreground",
                triggerClassName,
              )}
            />
          }
        >
          <CalendarIcon data-icon="inline-start" />
          {date?.from ? (
            date.to ? (
              <>
                {format(date.from, "MMM dd, y", { locale: ptBR })} -{" "}
                {format(date.to, "MMM dd, y", { locale: ptBR })}
              </>
            ) : (
              format(date.from, "MMM dd, y", { locale: ptBR })
            )
          ) : (
            <span>{EMPTY_LABEL_TEXT}</span>
          )}
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0 z-10000" align="start">
          <AccessibleCalendar
            autoFocus
            mode="range"
            defaultMonth={date?.from}
            selected={selectNewRange ? draft : date}
            resetOnSelect={selectNewRange}
            required={selectNewRange || undefined}
            onSelect={(value: DateRange | undefined) => {
              if (selectNewRange) {
                setDraft(value);
                if (!value?.from || !value.to) return;
                setOpen(false);
              }
              onDateChange(value);
            }}
            numberOfMonths={isMobile ? 1 : 2}
            locale={ptBR}
          />
        </PopoverContent>
      </Popover>
    </div>
  );
}
