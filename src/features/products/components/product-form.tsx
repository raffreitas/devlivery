import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button } from "@/shared/components/ui/button";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Combobox } from "@/shared/components/ui/combobox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/components/ui/form";
import { Input } from "@/shared/components/ui/input";
import { InputMoney } from "@/shared/components/ui/input-money";
import { Spinner } from "@/shared/components/ui/spinner";
import { Textarea } from "@/shared/components/ui/textarea";
import { type ProductFormData, productFormSchema } from "../types";

interface ProductFormProps {
  initialData: (ProductFormData & { id?: string }) | null;
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  categoryOptions?: { value: string; label: string }[];
}

export function ProductForm({
  initialData,
  onSubmit,
  onCancel,
  categoryOptions,
}: ProductFormProps) {
  const form = useForm<ProductFormData>({
    resolver: zodResolver(productFormSchema),
    defaultValues: {
      available: initialData?.available ?? true,
      name: initialData?.name ?? "",
      description: initialData?.description ?? "",
      price: initialData?.price ?? 0,
      category: initialData?.category ?? "",
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Nome do Produto</FormLabel>
              <FormControl>
                <Input type="text" required {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Descrição</FormLabel>
              <FormControl>
                <Textarea rows={3} required {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="price"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Preço (R$)</FormLabel>
              <FormControl>
                <InputMoney {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="category"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Categoria</FormLabel>
              <FormControl>
                <Combobox
                  placeholder="Pesquisar ou criar uma categoria"
                  options={categoryOptions ?? []}
                  value={field.value}
                  onChange={field.onChange}
                  allowCustomValue
                  className="max-w-full w-full"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="available"
          render={({ field }) => (
            <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
              <FormControl>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <div className="space-y-1 leading-none">
                <FormLabel>Disponível</FormLabel>
                <p className="text-sm text-muted-foreground">
                  Este produto estará visível para registro de pedidos.
                </p>
              </div>
            </FormItem>
          )}
        />

        <div className="flex justify-end space-x-3">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancelar
          </Button>
          <Button type="submit">
            {form.formState.isSubmitting && <Spinner />}
            {initialData?.id ? "Atualizar" : "Criar"} Produto
          </Button>
        </div>
      </form>
    </Form>
  );
}
