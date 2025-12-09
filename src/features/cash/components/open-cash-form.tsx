import { zodResolver } from "@hookform/resolvers/zod";
import { Wallet } from "lucide-react";
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
  type CreateCashSessionFormData,
  createCashSessionFormSchema,
} from "../types";

interface OpenCashFormProps {
  onSubmit: (dto: CreateCashSessionFormData) => Promise<void>;
  isSubmitting: boolean;
  onCancel: () => void;
}

export function OpenCashForm({
  onSubmit,
  isSubmitting,
  onCancel,
}: OpenCashFormProps) {
  const form = useForm<CreateCashSessionFormData>({
    resolver: zodResolver(createCashSessionFormSchema),
    defaultValues: {
      openingAmount: 0,
      notes: "",
    },
  });

  const handleSubmit = async (data: CreateCashSessionFormData) => {
    try {
      await onSubmit({
        openingAmount: data.openingAmount,
        notes: data.notes?.trim() || undefined,
      });
    } catch (err) {
      toast.error("Erro ao abrir caixa");
      throw err;
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="openingAmount"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Valor de Abertura</FormLabel>
              <FormControl>
                <InputMoney {...field} />
              </FormControl>
              <p className="text-xs text-gray-500">
                Informe o valor inicial em dinheiro no caixa
              </p>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Observações (opcional)</FormLabel>
              <FormControl>
                <Textarea
                  id="notes"
                  placeholder="Observações sobre a abertura do caixa..."
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
            className="flex-1 bg-green-600 hover:bg-green-700"
          >
            <Wallet className="w-4 h-4 mr-2" />
            {isSubmitting ? "Abrindo..." : "Abrir Caixa"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
