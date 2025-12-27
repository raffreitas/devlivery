import { Skeleton } from "@/shared/components/ui/skeleton";

interface CardSkeletonProps {
  className?: string;
  showImage?: boolean;
  lines?: number;
}

/**
 * CardSkeleton - Skeleton loader para cards
 */
export function CardSkeleton({
  className,
  showImage = false,
  lines = 3,
}: CardSkeletonProps) {
  return (
    <div className={className}>
      <div className="border rounded-lg p-4 space-y-3">
        {showImage && <Skeleton className="h-32 w-full rounded-md" />}
        <Skeleton className="h-5 w-3/4" />
        {Array.from({ length: lines }).map((_, i) => (
          <Skeleton
            key={`line-${Math.random() * i * 10000}`}
            className={i === lines - 1 ? "h-4 w-1/2" : "h-4 w-full"}
          />
        ))}
      </div>
    </div>
  );
}
