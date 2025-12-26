import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button } from "@/shared/components/ui/button";
import { LoadingButton } from "@/shared/components/loading";
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
  type CreateCashDepositFormData,
  createCashDepositFormSchema,
} from "../types";

interface CashDepositFormProps {
  onSubmit: (data: CreateCashDepositFormData) => Promise<void>;
  isLoading?: boolean;
}

export function CashDepositForm({
  onSubmit,
  isLoading = false,
}: CashDepositFormProps) {
  const form = useForm<CreateCashDepositFormData>({
    resolver: zodResolver(createCashDepositFormSchema),
    defaultValues: {
      amount: 0,
      notes: "",
    },
  });

  const handleFormSubmit = async (data: CreateCashDepositFormData) => {
    await onSubmit(data);
    form.reset();
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(handleFormSubmit)}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="amount"
          render={({ field }) => (
            <FormItem>
              <FormLabel>
                Valor do Aporte (R$) <span className="text-red-500">*</span>
              </FormLabel>
              <FormControl>
                <InputMoney
                  disabled={isLoading}
                  value={field.value}
                  onChange={field.onChange}
                  placeholder="0.00"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Motivo ou Observações (Opcional)</FormLabel>
              <FormControl>
                <Textarea
                  disabled={isLoading}
                  placeholder="Motivo do aporte (ex: Troco, Falta de dinheiro...)"
                  className="resize-none"
                  rows={3}
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
            disabled={isLoading}
            onClick={() => form.reset()}
            className="flex-1"
          >
            Limpar
          </Button>
          <LoadingButton
            type="submit"
            isLoading={isLoading}
            loadingText="Adicionando..."
            className="flex-1 bg-green-600 hover:bg-green-700"
          >
            Adicionar Aporte
          </LoadingButton>
        </div>
      </form>
    </Form>
  );
}
