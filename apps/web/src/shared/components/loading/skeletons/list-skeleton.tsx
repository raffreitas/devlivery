import { useId } from "react";
import { Skeleton } from "@/shared/components/ui/skeleton";

interface ListSkeletonProps {
  items?: number;
  className?: string;
  showAvatar?: boolean;
}

/**
 * ListSkeleton - Skeleton loader para listas
 */
export function ListSkeleton({
  items = 3,
  className,
  showAvatar = false,
}: ListSkeletonProps) {
  const id = useId();
  const itemKeys = Array.from({ length: items }, (_, i) => `${id}-list-${i}`);

  return (
    <div className={className}>
      <div className="space-y-3">
        {itemKeys.map((key) => (
          <div key={key} className="flex items-center gap-3">
            {showAvatar && <Skeleton className="h-10 w-10 rounded-full" />}
            <div className="flex-1 space-y-2">
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-3 w-1/2" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
