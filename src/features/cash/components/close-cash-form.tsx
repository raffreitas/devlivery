import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { Button } from "@/shared/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/components/ui/form";
import { InputMoney } from "@/shared/components/ui/input-money";
import { Textarea } from "@/shared/components/ui/textarea";
import {
  type CloseCashSessionFormData,
  closeCashSessionFormSchema,
} from "../types";

interface CloseCashFormProps {
  expectedCashAmount: number;
  onSubmit: (dto: CloseCashSessionFormData) => Promise<void>;
  isSubmitting: boolean;
  onCancel: () => void;
}

export function CloseCashForm({
  expectedCashAmount,
  onSubmit,
  isSubmitting,
  onCancel,
}: CloseCashFormProps) {
  const form = useForm<CloseCashSessionFormData>({
    resolver: zodResolver(closeCashSessionFormSchema),
    defaultValues: {
      closingAmount: 0,
      notes: "",
    },
  });

  const handleSubmit = async (data: CloseCashSessionFormData) => {
    try {
      await onSubmit({
        closingAmount: data.closingAmount,
        notes: data.notes?.trim() || undefined,
      });
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Erro ao fechar caixa");
      throw err;
    }
  };

  const closingAmount = form.watch("closingAmount");
  const difference = closingAmount ? closingAmount - expectedCashAmount : 0;

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
        <div className="p-4 rounded-lg bg-blue-50 border border-blue-200">
          <p className="text-sm font-medium text-blue-900">
            Dinheiro Esperado no Caixa
          </p>
          <p className="text-2xl font-bold text-blue-600">
            R$ {expectedCashAmount.toFixed(2)}
          </p>
          <p className="text-xs text-blue-700 mt-1">
            Abertura + vendas em dinheiro apenas
          </p>
        </div>

        <FormField
          control={form.control}
          name="closingAmount"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Valor Real no Caixa</FormLabel>
              <FormControl>
                <InputMoney
                  placeholder="seu@email.com"
                  autoComplete="email"
                  {...field}
                />
              </FormControl>
              <p className="text-xs text-gray-500">
                Conte o dinheiro físico no caixa e informe o valor total
              </p>
              <FormMessage />
            </FormItem>
          )}
        />

        {closingAmount && (
          <div
            className={`p-3 rounded-lg border ${
              difference === 0
                ? "bg-green-50 border-green-200"
                : difference > 0
                  ? "bg-yellow-50 border-yellow-200"
                  : "bg-red-50 border-red-200"
            }`}
          >
            <p className="text-sm font-medium">
              {difference === 0
                ? "✓ Caixa confere"
                : difference > 0
                  ? "⚠ Sobra no caixa"
                  : "⚠ Falta no caixa"}
            </p>
            <p
              className={`text-lg font-bold ${
                difference === 0
                  ? "text-green-600"
                  : difference > 0
                    ? "text-yellow-600"
                    : "text-red-600"
              }`}
            >
              {difference >= 0 ? "+" : ""}R$ {difference.toFixed(2)}
            </p>
          </div>
        )}

        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Observações (opcional)</FormLabel>
              <FormControl>
                <Textarea
                  id="notes"
                  placeholder="Observações sobre o fechamento do caixa..."
                  rows={3}
                  disabled={isSubmitting}
                  className="resize-none"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex gap-2 pt-2">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={isSubmitting}
            className="flex-1"
          >
            Cancelar
          </Button>
          <Button
            type="submit"
            disabled={isSubmitting}
            className="flex-1 bg-red-600 hover:bg-red-700"
          >
            {isSubmitting ? "Fechando..." : "Fechar Caixa"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
