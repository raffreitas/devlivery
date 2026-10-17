import { zodResolver } from "@hookform/resolvers/zod";
import { useId } from "react";
import { Controller, useForm } from "react-hook-form";
import { InputMoney } from "@/shared/components/input-money";
import { LoadingButton } from "@/shared/components/loading";
import { OptionCombobox } from "@/shared/components/option-combobox";
import { Button } from "@/shared/components/ui/button";
import { Checkbox } from "@/shared/components/ui/checkbox";
import {
  Field,
  FieldContent,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldTitle,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { Textarea } from "@/shared/components/ui/textarea";
import { type ProductFormData, productFormSchema } from "../types";

interface ProductFormProps {
  initialData: (ProductFormData & { id?: string }) | null;
  onSubmit: (data: ProductFormData) => void | Promise<void>;
  onCancel: () => void;
  categoryOptions?: { value: string; label: string }[];
  isSubmitting?: boolean;
}

export function ProductForm({
  initialData,
  onSubmit,
  onCancel,
  categoryOptions,
  isSubmitting: externalIsSubmitting,
}: ProductFormProps) {
  const formId = useId();
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
    <form onSubmit={form.handleSubmit(onSubmit)}>
      <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
        <Controller
          control={form.control}
          name="name"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-product-name`}>
                Nome do Produto
              </FieldLabel>
              <Input
                {...field}
                id={`${formId}-product-name`}
                type="text"
                placeholder="Digite o nome do produto"
                required
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid
                    ? `${formId}-product-name-error`
                    : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-product-name-error`}
                  errors={[fieldState.error]}
                />
              )}
            </Field>
          )}
        />

        <Controller
          control={form.control}
          name="description"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-product-description`}>
                Descrição
              </FieldLabel>
              <Textarea
                {...field}
                id={`${formId}-product-description`}
                placeholder="Digite a descrição do produto"
                rows={3}
                required
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid
                    ? `${formId}-product-description-error`
                    : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-product-description-error`}
                  errors={[fieldState.error]}
                />
              )}
            </Field>
          )}
        />

        <Controller
          control={form.control}
          name="price"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-product-price`}>
                Preço (R$)
              </FieldLabel>
              <InputMoney
                {...field}
                id={`${formId}-product-price`}
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid
                    ? `${formId}-product-price-error`
                    : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-product-price-error`}
                  errors={[fieldState.error]}
                />
              )}
            </Field>
          )}
        />

        <Controller
          control={form.control}
          name="category"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor={`${formId}-product-category`}>
                Categoria
              </FieldLabel>
              <OptionCombobox
                id={`${formId}-product-category`}
                name={field.name}
                inputRef={field.ref}
                placeholder="Pesquisar ou criar uma categoria"
                options={categoryOptions ?? []}
                value={field.value}
                onValueChange={field.onChange}
                onBlur={field.onBlur}
                // Categoria de produto é texto livre: o rótulo é o próprio valor
                onCreate={async (label) => ({ value: label, label })}
                aria-invalid={fieldState.invalid}
                aria-describedby={
                  fieldState.invalid
                    ? `${formId}-product-category-error`
                    : undefined
                }
              />
              {fieldState.invalid && (
                <FieldError
                  id={`${formId}-product-category-error`}
                  errors={[fieldState.error]}
                />
              )}
            </Field>
          )}
        />

        <Controller
          control={form.control}
          name="available"
          render={({ field, fieldState }) => (
            <FieldLabel htmlFor={`${formId}-product-available`}>
              <Field orientation="horizontal" data-invalid={fieldState.invalid}>
                <Checkbox
                  id={`${formId}-product-available`}
                  name={field.name}
                  checked={field.value}
                  onCheckedChange={field.onChange}
                  aria-invalid={fieldState.invalid}
                  aria-describedby={`${formId}-product-available-description`}
                />
                <FieldContent>
                  <FieldTitle>Disponível</FieldTitle>
                  <FieldDescription
                    id={`${formId}-product-available-description`}
                  >
                    Este produto estará visível para registro de pedidos.
                  </FieldDescription>
                </FieldContent>
              </Field>
            </FieldLabel>
          )}
        />

        <Field
          orientation="horizontal"
          className="justify-center sm:justify-end"
        >
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            className="flex-1 sm:flex-initial"
          >
            Cancelar
          </Button>
          <LoadingButton
            type="submit"
            className="flex-1 sm:flex-initial"
            isLoading={
              form.formState.isSubmitting || externalIsSubmitting === true
            }
            loadingText={initialData?.id ? "Atualizando..." : "Criando..."}
          >
            {initialData?.id ? "Atualizar" : "Criar"} Produto
          </LoadingButton>
        </Field>
      </FieldGroup>
    </form>
  );
}
