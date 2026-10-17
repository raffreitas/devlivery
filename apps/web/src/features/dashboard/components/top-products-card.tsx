import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";

interface TopProductsCardProps {
  data: { name: string; quantity: number }[];
}

export function TopProductsCard({ data }: TopProductsCardProps) {
  const maxQuantity = Math.max(...data.map((d) => d.quantity), 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Produtos Mais Vendidos</CardTitle>
      </CardHeader>
      <CardContent>
        <div>
          {data.length > 0 ? (
            <div className="max-h-70 flex flex-col gap-3 overflow-y-auto pr-2">
              {data.map((item) => (
                <div key={item.name} className="flex items-center">
                  <div className="flex flex-1 flex-col gap-1">
                    <p className="text-sm font-medium leading-none">
                      {item.name}
                    </p>
                    <div className="relative h-2 w-full overflow-hidden rounded-full bg-secondary">
                      <div
                        className="h-full bg-chart-sales transition-all"
                        style={{
                          width: `${maxQuantity > 0 ? (item.quantity / maxQuantity) * 100 : 0}%`,
                        }}
                      />
                    </div>
                  </div>
                  <div className="ml-4 font-medium">{item.quantity} un.</div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center text-muted-foreground py-8">
              Nenhum produto vendido no período
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
