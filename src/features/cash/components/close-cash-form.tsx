import { zodResolver } from "@hookform/resolvers/zod";
import { AlertCircle, Banknote, PlusIcon, TrendingUp } from "lucide-react";
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
import { formatMoney } from "@/shared/utils/formatters";
import {
  type CloseCashSessionFormData,
  closeCashSessionFormSchema,
} from "../types";

interface CloseCashFormProps {
  expectedCashAmount: number;
  openingAmount: number;
  depositsTotal: number;
  cashSales: number;
  onSubmit: (dto: CloseCashSessionFormData) => Promise<void>;
  isSubmitting: boolean;
  onCancel: () => void;
}

export function CloseCashForm({
  expectedCashAmount,
  openingAmount,
  depositsTotal,
  cashSales,
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
  const hasDifference = closingAmount > 0 && difference !== 0;

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-5">
        {/* Breakdown do valor esperado */}
        <div className="p-4 rounded-lg bg-orange-50 border border-orange-200">
          <div className="flex items-center gap-2 mb-3">
            <Banknote className="w-5 h-5 text-orange-700" />
            <p className="text-sm font-semibold text-orange-900">
              Composição do Valor Esperado
            </p>
          </div>

          <div className="space-y-2 text-sm mb-3">
            <div className="flex items-center justify-between text-orange-800">
              <span>Abertura</span>
              <span className="font-medium">{formatMoney(openingAmount)}</span>
            </div>
            {depositsTotal > 0 && (
              <div className="flex items-center justify-between text-orange-800">
                <span className="flex items-center gap-1">
                  <PlusIcon className="w-3 h-3" />
                  Aportes
                </span>
                <span className="font-medium">
                  {formatMoney(depositsTotal)}
                </span>
              </div>
            )}
            <div className="flex items-center justify-between text-orange-800">
              <span className="flex items-center gap-1">
                <TrendingUp className="w-3 h-3" />
                Vendas em Dinheiro
              </span>
              <span className="font-medium">{formatMoney(cashSales)}</span>
            </div>
          </div>

          <div className="pt-3 border-t border-orange-300">
            <div className="flex items-center justify-between">
              <p className="text-xs font-medium text-orange-700">
                Total Esperado
              </p>
              <p className="text-2xl font-bold text-orange-700">
                {formatMoney(expectedCashAmount)}
              </p>
            </div>
          </div>
        </div>

        {/* Valor real contado */}
        <FormField
          control={form.control}
          name="closingAmount"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Valor Real no Caixa</FormLabel>
              <FormControl>
                <InputMoney {...field} placeholder="0,00" />
              </FormControl>
              <p className="text-xs text-gray-500 flex items-start gap-1">
                <AlertCircle className="w-3 h-3 mt-0.5 shrink-0" />
                Conte o dinheiro físico no caixa e informe o valor total
              </p>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Diferença - sempre visível quando há valor digitado */}
        {closingAmount > 0 && (
          <div
            className={`p-4 rounded-lg border-2 transition-all ${
              difference === 0
                ? "bg-green-50 border-green-300"
                : difference > 0
                  ? "bg-yellow-50 border-yellow-300"
                  : "bg-red-50 border-red-300"
            }`}
          >
            <div className="flex items-center justify-between mb-2">
              <p className="text-sm font-semibold text-gray-700">
                {difference === 0
                  ? "✓ Caixa Conferido"
                  : difference > 0
                    ? "⚠️ Sobra no Caixa"
                    : "⚠️ Falta no Caixa"}
              </p>
              <p
                className={`text-2xl font-bold ${
                  difference === 0
                    ? "text-green-700"
                    : difference > 0
                      ? "text-yellow-700"
                      : "text-red-700"
                }`}
              >
                {difference >= 0 ? "+" : ""}
                {formatMoney(Math.abs(difference))}
              </p>
            </div>
            {hasDifference && (
              <p className="text-xs text-gray-600">
                {difference > 0
                  ? "Você pode registrar uma observação sobre a sobra no campo abaixo."
                  : "Por favor, verifique a contagem ou registre uma observação sobre a diferença."}
              </p>
            )}
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
