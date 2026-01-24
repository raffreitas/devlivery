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
  return (
    <div className={className}>
      <div className="space-y-3">
        {Array.from({ length: items }).map((_, i) => (
          <div
            key={`item-${Math.random() * 10000 * i}`}
            className="flex items-center gap-3"
          >
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
