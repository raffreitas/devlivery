import { PlusIcon } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";

interface OrdersHeaderProps {
  isFetching: boolean;
  onNewOrder: () => void;
}

export function OrdersHeader({ isFetching, onNewOrder }: OrdersHeaderProps) {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:justify-between sm:items-center">
      <div className="flex items-center gap-3">
        <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">
          Pedidos
        </h1>
        {isFetching && (
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <Spinner />
            <span className="hidden sm:inline">Atualizando...</span>
          </div>
        )}
      </div>
      <Button onClick={onNewOrder}>
        <PlusIcon /> Novo Pedido
      </Button>
    </div>
  );
}
