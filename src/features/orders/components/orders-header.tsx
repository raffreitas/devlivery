import { PlusIcon } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { CashStatusBadge } from "./cash-status-badge";

interface OrdersHeaderProps {
  isFetching: boolean;
  onNewOrder: () => void;
}

export function OrdersHeader({ isFetching, onNewOrder }: OrdersHeaderProps) {
  return (
    <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 w-full">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 tracking-tight">
          Pedidos
        </h1>
        <p className="text-muted-foreground">Gerencie e acompanhe os pedidos</p>
      </div>

      <div className="flex flex-col items-center gap-2 w-full sm:w-auto sm:flex-row">
        {isFetching && (
          <div className="hidden sm:flex items-center gap-2 text-sm text-muted-foreground mr-2">
            <Spinner className="w-4 h-4" />
            <span>Sincronizando...</span>
          </div>
        )}
        <CashStatusBadge />
        <Button onClick={onNewOrder} className="w-full sm:w-auto">
          <PlusIcon className="w-4 h-4 mr-2" />
          Novo Pedido
        </Button>
      </div>
    </div>
  );
}
