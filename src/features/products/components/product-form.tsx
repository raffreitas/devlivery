import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { Button } from "@/shared/components/ui/button";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Combobox } from "@/shared/components/ui/combobox";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Textarea } from "@/shared/components/ui/textarea";
import { type ProductFormData, productFormSchema } from "../types";

interface ProductFormProps {
  initialData?: ProductFormData & { id?: string };
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
  const { register, handleSubmit, control } = useForm<ProductFormData>({
    resolver: zodResolver(productFormSchema),
    defaultValues: {
      available: initialData?.available ?? true,
      name: initialData?.name || "",
      description: initialData?.description || "",
      price: initialData?.price || 0,
      category: initialData?.category || "",
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Label htmlFor="name">Nome do Produto</Label>
      <Input type="text" id="name" required {...register("name")} />

      <Label htmlFor="description">Descrição</Label>
      <Textarea
        id="description"
        rows={3}
        required
        {...register("description")}
      />

      <Label htmlFor="price">Preço (R$)</Label>
      <Input
        id="price"
        type="number"
        step="0.01"
        required
        {...register("price", { valueAsNumber: true })}
      />

      <Label htmlFor="category">Categoria</Label>
      <Controller
        control={control}
        name="category"
        render={({ field }) => (
          <Combobox
            placeholder="Ex: Pizza, Bebida, Sobremesa"
            options={categoryOptions ?? []}
            value={field.value}
            onChange={field.onChange}
            allowCustomValue={true}
            className="max-w-full w-full"
          />
        )}
      />

      <div className="flex items-center gap-3">
        <Controller
          control={control}
          name="available"
          render={({ field }) => (
            <Checkbox
              id="available"
              checked={!!field.value}
              onCheckedChange={(v) => field.onChange(v === true)}
            />
          )}
        />
        <Label htmlFor="available">Disponível</Label>
      </div>

      <div className="flex justify-end space-x-3 pt-4">
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancelar
        </Button>
        <Button type="submit">
          {initialData?.id ? "Atualizar" : "Criar"} Produto
        </Button>
      </div>
    </form>
  );
}
