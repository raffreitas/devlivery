import type { ReactNode } from 'react';
import { Card } from '../../../shared/components/Card';

interface StatCardProps {
  title: string;
  value: string | number;
  icon: ReactNode;
  color?: 'orange' | 'blue' | 'green' | 'purple';
}

const colorClasses = {
  orange: 'bg-orange-100 text-orange-600',
  blue: 'bg-blue-100 text-blue-600',
  green: 'bg-green-100 text-green-600',
  purple: 'bg-purple-100 text-purple-600',
};

export function StatCard({ title, value, icon, color = 'orange' }: StatCardProps) {
  return (
    <Card>
      <div className="flex items-center">
        <div className={`p-3 rounded-full ${colorClasses[color]}`}>
          {icon}
        </div>
        <div className="ml-4">
          <p className="text-sm text-gray-600">{title}</p>
          <p className="text-2xl font-bold text-gray-900">{value}</p>
        </div>
      </div>
    </Card>
  );
}
