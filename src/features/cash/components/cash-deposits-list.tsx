import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { formatMoney } from "@/shared/utils/formatters";
import type { CashDeposit } from "../types";

interface CashDepositsListProps {
  deposits?: CashDeposit[];
  isLoading?: boolean;
}

export function CashDepositsList({
  deposits = [],
  isLoading = false,
}: CashDepositsListProps) {
  if (isLoading) {
    return (
      <div className="space-y-2 p-4">
        <div className="h-10 bg-gray-100 rounded w-full animate-pulse" />
        <div className="h-10 bg-gray-100 rounded w-full animate-pulse" />
        <div className="h-10 bg-gray-100 rounded w-full animate-pulse" />
      </div>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Histórico de Aportes</CardTitle>
      </CardHeader>
      <CardContent>
        {!deposits || deposits.length === 0 ? (
          <div className="text-center py-8 text-gray-500 border rounded-md border-dashed">
            Nenhum aporte registrado nesta sessão.
          </div>
        ) : (
          <div>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Valor</TableHead>
                  <TableHead>Horário</TableHead>
                  <TableHead>Atendente</TableHead>
                  <TableHead>Observação</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {deposits.map((deposit) => {
                  const depositDate = new Date(deposit.depositedAt);
                  const time = depositDate.toLocaleTimeString("pt-BR", {
                    hour: "2-digit",
                    minute: "2-digit",
                  });

                  return (
                    <TableRow key={deposit.id}>
                      <TableCell className="font-medium">
                        {formatMoney(deposit.amount)}
                      </TableCell>
                      <TableCell>{time}</TableCell>
                      <TableCell>{deposit.attendant}</TableCell>
                      <TableCell className="text-gray-500 italic max-w-[200px] truncate">
                        {deposit.notes || "-"}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
