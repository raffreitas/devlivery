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
        <div className="space-y-8">
          {data.length > 0 ? (
            <div className="max-h-70 space-y-8 overflow-y-auto pr-2">
              {data.map((item) => (
                <div key={item.name} className="flex items-center">
                  <div className="space-y-1 flex-1">
                    <p className="text-sm font-medium leading-none">
                      {item.name}
                    </p>
                    <div className="relative h-2 w-full overflow-hidden rounded-full bg-secondary">
                      <div
                        className="h-full bg-primary transition-all"
                        style={{
                          width: `${(item.quantity / maxQuantity) * 100}%`,
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
