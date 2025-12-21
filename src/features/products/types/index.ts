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
  name: z
    .string({ message: "Nome do produto é obrigatório" })
    .min(1, "O nome é obrigatório")
    .max(200, "O nome deve ter no máximo 200 caracteres")
    .trim(),
  description: z
    .string({ message: "Descrição do produto é obrigatória" })
    .min(1, "A descrição é obrigatória")
    .max(1000, "A descrição deve ter no máximo 1000 caracteres")
    .trim(),
  price: z
    .number({ message: "Deve ser informado um preço válido" })
    .min(0.01, "O preço deve ser maior que zero"),
  category: z
    .string({ message: "Categoria do produto é obrigatória" })
    .min(1, "A categoria é obrigatória")
    .max(100, "A categoria deve ter no máximo 100 caracteres")
    .trim(),
  available: z.boolean(),
});

export type ProductFormData = z.infer<typeof productFormSchema>;
