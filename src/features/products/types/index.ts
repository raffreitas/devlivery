import z from "zod";

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  category: string;
  available: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export const productFormSchema = z.object({
  name: z.string().min(1, "O nome é obrigatório"),
  description: z.string().min(1, "A descrição é obrigatória"),
  price: z.number().min(0, "O preço deve ser maior ou igual a zero"),
  category: z.string().min(1, "A categoria é obrigatória"),
  available: z.boolean(),
});

export interface ProductFormData extends z.infer<typeof productFormSchema> {}
