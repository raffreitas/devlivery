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
    .string({ error: "Deve ser informado um nome válido." })
    .min(1, "O nome é obrigatório"),
  description: z
    .string({ error: "Deve ser informado uma descrição válida." })
    .min(1, "A descrição é obrigatória"),
  price: z
    .number({ error: "Deve ser informado um preço válido" })
    .min(0.01, "O preço deve ser maior que a zero"),
  category: z
    .string({ error: "Deve ser informado uma categoria válida." })
    .min(1, "A categoria é obrigatória"),
  available: z.boolean(),
});

export type ProductFormData = z.infer<typeof productFormSchema>;
