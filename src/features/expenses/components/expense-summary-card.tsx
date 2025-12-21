import { AlertCircle, CheckCircle2, Clock, DollarSign } from "lucide-react";
import { StatCard } from "@/shared/components/stat-card";
import { formatMoney } from "@/shared/utils/formatters";
import type { ExpenseSummary } from "../types";

interface ExpenseSummaryCardProps {
  summary: ExpenseSummary;
}

export function ExpenseSummaryCard({ summary }: ExpenseSummaryCardProps) {
  const stats = [
    {
      label: "Total de Despesas",
      value: formatMoney(summary.total),
      icon: DollarSign,
      color: "blue",
    },
    {
      label: "Pagas",
      value: formatMoney(summary.paid),
      icon: CheckCircle2,
      color: "green",
    },
    {
      label: "Pendentes",
      value: formatMoney(summary.pending),
      icon: Clock,
      color: "amber",
    },
    {
      label: "Vencidas",
      value: formatMoney(summary.overdue),
      icon: AlertCircle,
      color: "red",
    },
  ];

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {stats.map((stat) => {
        const Icon = stat.icon;
        return (
          <StatCard
            icon={<Icon className={stat.color} />}
            title={stat.label}
            value={stat.value}
            key={stat.label}
            color={stat.color as "blue" | "green" | "amber" | "red"}
          />
        );
      })}
    </div>
  );
}
