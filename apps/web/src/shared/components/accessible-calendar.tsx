import { type ComponentProps, useEffect, useRef } from "react";
import { type DayButtonProps, useDayPicker } from "react-day-picker";
import { Calendar, CalendarDayButton } from "@/shared/components/ui/calendar";

function FocusableDayButton(props: DayButtonProps) {
  const ref = useRef<HTMLButtonElement>(null);
  const { dayPickerProps } = useDayPicker();

  useEffect(() => {
    if (props.modifiers.focused) ref.current?.focus();
  }, [props.modifiers.focused]);

  // React 19 forwards this ref through CalendarDayButton's rest props.
  const buttonProps = { ...props, ref, locale: dayPickerProps.locale };
  return <CalendarDayButton {...buttonProps} />;
}

export function AccessibleCalendar({
  components,
  ...props
}: ComponentProps<typeof Calendar>) {
  return (
    <Calendar
      {...props}
      components={{ DayButton: FocusableDayButton, ...components }}
    />
  );
}
