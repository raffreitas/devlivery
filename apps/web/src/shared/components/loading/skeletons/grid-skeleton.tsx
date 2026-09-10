import { useId } from "react";
import { CardSkeleton } from "./card-skeleton";

interface GridSkeletonProps {
  items?: number;
  columns?: number;
  className?: string;
  showImage?: boolean;
}

/**
 * GridSkeleton - Skeleton loader para grids de cards
 */
export function GridSkeleton({
  items = 8,
  columns = 4,
  className,
  showImage = false,
}: GridSkeletonProps) {
  const id = useId();
  const itemKeys = Array.from({ length: items }, (_, i) => `${id}-grid-${i}`);

  const gridCols = {
    1: "grid-cols-1",
    2: "grid-cols-1 md:grid-cols-2",
    3: "grid-cols-1 md:grid-cols-2 lg:grid-cols-3",
    4: "grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4",
    5: "grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5",
  };

  return (
    <div
      className={`grid ${gridCols[columns as keyof typeof gridCols] ?? gridCols[4]} gap-6 ${className ?? ""}`}
    >
      {itemKeys.map((key) => (
        <CardSkeleton key={key} showImage={showImage} />
      ))}
    </div>
  );
}
