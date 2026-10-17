import type { ReactNode } from "react";

export function ChartTooltipValue({
  label,
  value,
  detail,
}: {
  label: ReactNode;
  value: ReactNode;
  detail?: ReactNode;
}) {
  return (
    <div className="flex w-full flex-col gap-1">
      <div className="flex items-center justify-between gap-4">
        <span className="text-muted-foreground">{label}</span>
        <span className="font-medium tabular-nums">{value}</span>
      </div>
      {detail && <span className="text-muted-foreground">{detail}</span>}
    </div>
  );
}
