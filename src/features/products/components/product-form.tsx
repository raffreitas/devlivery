import { useState } from "react";
import { AutocompleteSelect } from "@/shared/components/autocomplete-select";
import { Button } from "@/shared/components/ui/button";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Textarea } from "@/shared/components/ui/textarea";
import type { ProductFormData } from "../types";

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
  const [formData, setFormData] = useState<ProductFormData>({
    name: initialData?.name || "",
    description: initialData?.description || "",
    price: initialData?.price || 0,
    category: initialData?.category || "",
    available: initialData?.available ?? true,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <Label htmlFor="name">Nome do Produto</Label>
      <Input
        type="text"
        value={formData.name}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          setFormData({ ...formData, name: e.target.value })
        }
        required
      />

      <Label htmlFor="description">Descrição</Label>
      <Textarea
        id="description"
        value={formData.description}
        onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
          setFormData({ ...formData, description: e.target.value })
        }
        rows={3}
        required
      />

      <Label htmlFor="price">Preço (R$)</Label>
      <Input
        id="price"
        type="number"
        step="0.01"
        value={formData.price}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          setFormData({ ...formData, price: Number.parseFloat(e.target.value) })
        }
        required
      />

      <AutocompleteSelect
        id="category"
        name="category"
        label="Categoria"
        placeholder="Ex: Pizza, Bebida, Sobremesa"
        options={categoryOptions ?? []}
        value={formData.category || null}
        allowCustomValue={true}
        onChange={(v) => setFormData({ ...formData, category: v ?? "" })}
        required
      />

      <div className="flex items-center gap-3">
        <Checkbox
          id="available"
          onCheckedChange={(e) =>
            setFormData({ ...formData, available: e === true })
          }
        />
        <Label>Disponível</Label>
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
