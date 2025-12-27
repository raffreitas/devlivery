import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/shared/lib/utils";

const skeletonVariants = cva("bg-accent animate-pulse", {
  variants: {
    variant: {
      default: "rounded-md",
      text: "rounded",
      circle: "rounded-full",
      rectangle: "rounded-md",
    },
  },
  defaultVariants: {
    variant: "default",
  },
});

interface SkeletonProps
  extends React.ComponentProps<"div">,
    VariantProps<typeof skeletonVariants> {}

function Skeleton({ className, variant, ...props }: SkeletonProps) {
  return (
    <div
      data-slot="skeleton"
      className={cn(skeletonVariants({ variant }), className)}
      {...props}
    />
  );
}

export { Skeleton, skeletonVariants };
