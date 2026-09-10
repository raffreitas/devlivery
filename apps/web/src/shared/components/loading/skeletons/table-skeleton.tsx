import { useId } from "react";
import { Skeleton } from "@/shared/components/ui/skeleton";

interface TableSkeletonProps {
  rows?: number;
  columns?: number;
  className?: string;
}

/**
 * TableSkeleton - Skeleton loader para tabelas
 */
export function TableSkeleton({
  rows = 5,
  columns = 4,
  className,
}: TableSkeletonProps) {
  const id = useId();
  const headerKeys = Array.from(
    { length: columns },
    (_, i) => `${id}-header-${i}`,
  );
  const rowKeys = Array.from({ length: rows }, (_, rowIndex) => ({
    rowKey: `${id}-row-${rowIndex}`,
    cells: Array.from(
      { length: columns },
      (_, colIndex) => `${id}-cell-${rowIndex}-${colIndex}`,
    ),
  }));

  return (
    <div className={className}>
      <div className="space-y-2">
        {/* Header */}
        <div className="flex gap-4 pb-2 border-b">
          {headerKeys.map((key) => (
            <Skeleton key={key} className="h-4 flex-1" />
          ))}
        </div>
        {/* Rows */}
        {rowKeys.map(({ rowKey, cells }) => (
          <div key={rowKey} className="flex gap-4 py-2">
            {cells.map((cellKey) => (
              <Skeleton key={cellKey} className="h-4 flex-1" />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
