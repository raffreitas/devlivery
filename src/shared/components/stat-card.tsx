import type { ReactNode } from "react";
import { Card } from "@/shared/components/ui/card";

const colorClasses = {
  blue: "bg-blue-100 dark:bg-blue-950 text-blue-600 dark:text-blue-400",
  green: "bg-green-100 dark:bg-green-950 text-green-600 dark:text-green-400",
  amber: "bg-amber-100 dark:bg-amber-950 text-amber-600 dark:text-amber-400",
  red: "bg-red-100 dark:bg-red-950 text-red-600 dark:text-red-400",
  orange:
    "bg-orange-100 dark:bg-orange-950 text-orange-600 dark:text-orange-400",
  purple:
    "bg-purple-100 dark:bg-purple-950 text-purple-600 dark:text-purple-400",
};

interface StatCardProps {
  title: string;
  value: string | number;
  icon: ReactNode;
  color?: keyof typeof colorClasses;
}

export function StatCard({
  title,
  value,
  icon,
  color = "orange",
}: StatCardProps) {
  return (
    <Card className="p-6">
      <div className="flex items-center gap-3 sm:gap-4">
        <div className={`p-2 sm:p-3 rounded-full ${colorClasses[color]}`}>
          {icon}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-xs sm:text-sm text-secondary-foreground truncate">
            {title}
          </p>
          <p className="text-lg sm:text-2xl font-bold text-gray-900 truncate">
            {value}
          </p>
        </div>
      </div>
    </Card>
  );
}
