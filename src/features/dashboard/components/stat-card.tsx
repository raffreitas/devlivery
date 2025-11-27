import type { ReactNode } from "react";
import { Card } from "@/shared/components/ui/card";

interface StatCardProps {
  title: string;
  value: string | number;
  icon: ReactNode;
  color?: "orange" | "blue" | "green" | "purple";
}

const colorClasses = {
  orange: "bg-orange-100 text-primary",
  blue: "bg-blue-100 text-blue-600",
  green: "bg-green-100 text-green-600",
  purple: "bg-purple-100 text-purple-600",
};

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
          <p className="text-xs sm:text-sm text-secondary-foreground truncate">{title}</p>
          <p className="text-lg sm:text-2xl font-bold text-gray-900 truncate">
            {value}
          </p>
        </div>
      </div>
    </Card>
  );
}
