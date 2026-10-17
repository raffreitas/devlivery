import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useId } from "react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { LoadingButton } from "@/shared/components/loading";
import { Modal } from "@/shared/components/modal";
import { Button } from "@/shared/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { toast } from "@/shared/components/ui/toast";
import {
  useExpenseCategories,
  useExpenseCategoriesManagement,
} from "../hooks/use-expenses";
import type { Category } from "../types";

const createCategorySchema = z.object({
  name: z.string().min(1, "Nome é obrigatório").max(200, "Nome muito longo"),
  categoryType: z.enum(["main", "subcategory"]).optional(),
  parentCategoryId: z.string().optional(),
});

type CreateCategoryFormData = z.infer<typeof createCategorySchema>;

interface CreateCategoryDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: (category: Category) => void;
  initialParentCategoryId?: string;
  initialName?: string;
  categoryOnly?: boolean; // Se true, só permite criar categoria principal (sem opção de subcategoria)
}

export function CreateCategoryDialog({
  open,
  onOpenChange,
  onSuccess,
  initialParentCategoryId,
  initialName,
  categoryOnly = false,
}: CreateCategoryDialogProps) {
  const formId = useId();
  const { data: categories } = useExpenseCategories();
  const { createCategory, isCreating } = useExpenseCategoriesManagement();
  const isSubcategory = !categoryOnly && !!initialParentCategoryId;

  const form = useForm<CreateCategoryFormData>({
    resolver: zodResolver(createCategorySchema),
    defaultValues: {
      name: initialName || "",
      categoryType: isSubcategory ? "subcategory" : "main",
      parentCategoryId: initialParentCategoryId,
    },
  });

  useEffect(() => {
    if (initialName && open) {
      form.setValue("name", initialName);
    }
  }, [initialName, open, form]);

  useEffect(() => {
    if (open) {
      form.reset({
        name: initialName || "",
        categoryType: isSubcategory ? "subcategory" : "main",
        parentCategoryId: initialParentCategoryId,
      });
    }
  }, [open, initialName, initialParentCategoryId, isSubcategory, form]);

  const categoryType = form.watch("categoryType");
  const needsParentSelection = !isSubcategory && categoryType === "subcategory";

  const handleSubmit = async (data: CreateCategoryFormData) => {
    try {
      let finalParentId: string | undefined;

      if (initialParentCategoryId) {
        // Subcategoria criada a partir do SubcategoryCombobox
        finalParentId = initialParentCategoryId;
      } else if (data.categoryType === "subcategory" && data.parentCategoryId) {
        // Subcategoria criada escolhendo tipo "Subcategoria" e selecionando categoria pai
        finalParentId = data.parentCategoryId;
      } else {
        finalParentId = undefined;
      }

      const created = await createCategory({
        name: data.name,
        parentCategoryId: finalParentId,
      });
      toast.add({
        type: "success",
        title: finalParentId
          ? "Subcategoria criada com sucesso!"
          : "Categoria criada com sucesso!",
      });
      form.reset();
      onOpenChange(false);
      onSuccess(created);
    } catch {
      // Reported by the global MutationCache.
    }
  };

  const parentCategories = categories?.filter((cat) => cat.isActive) ?? [];
  const selectedParentCategory = parentCategories.find(
    (cat) => cat.id === initialParentCategoryId,
  );

  const title = isSubcategory
    ? "Criar Nova Subcategoria"
    : "Criar Nova Categoria";
  const description = isSubcategory
    ? selectedParentCategory
      ? `Criar subcategoria para "${selectedParentCategory.name}"`
      : "Preencha os dados para criar uma nova subcategoria."
    : "Preencha os dados para criar uma nova categoria de despesa.";

  const handleClose = () => {
    form.reset();
    onOpenChange(false);
  };

  return (
    <Modal
      isOpen={open}
      onClose={handleClose}
      title={title}
      description={description}
      footer={
        <div className="flex gap-2 justify-end">
          <Button
            type="button"
            variant="outline"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              handleClose();
            }}
            disabled={isCreating}
          >
            Cancelar
          </Button>
          <LoadingButton
            type="button"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              form.handleSubmit(handleSubmit)();
            }}
            isLoading={isCreating}
          >
            Criar
          </LoadingButton>
        </div>
      }
    >
      <form
        onSubmit={(e) => {
          e.preventDefault();
          e.stopPropagation();
          form.handleSubmit(handleSubmit)();
        }}
      >
        <FieldGroup className="gap-5 *:data-[slot=field]:gap-2">
          {isSubcategory && selectedParentCategory && (
            <div className="p-3 bg-muted rounded-md">
              <p className="text-xs text-muted-foreground mb-1">
                Categoria Pai:
              </p>
              <p className="font-medium text-sm">
                {selectedParentCategory.name}
              </p>
            </div>
          )}

          {!isSubcategory && !categoryOnly && (
            <Controller
              control={form.control}
              name="categoryType"
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor={`${formId}-category-type`}>
                    Tipo
                  </FieldLabel>
                  <Select
                    name={field.name}
                    onValueChange={(value) => {
                      if (value === null) return;
                      field.onChange(value as "main" | "subcategory");
                      if (value === "main") {
                        form.setValue("parentCategoryId", undefined);
                      }
                    }}
                    value={field.value || "main"}
                  >
                    <SelectTrigger
                      id={`${formId}-category-type`}
                      className="w-full"
                      aria-invalid={fieldState.invalid}
                      aria-describedby={
                        fieldState.invalid
                          ? `${formId}-category-type-error`
                          : undefined
                      }
                    >
                      <SelectValue placeholder="Selecione o tipo">
                        {field.value === "subcategory"
                          ? "Subcategoria"
                          : "Categoria Principal"}
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        <SelectItem value="main">
                          Categoria Principal
                        </SelectItem>
                        <SelectItem value="subcategory">
                          Subcategoria
                        </SelectItem>
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                  {fieldState.invalid && (
                    <FieldError
                      id={`${formId}-category-type-error`}
                      errors={[fieldState.error]}
                    />
                  )}
                </Field>
              )}
            />
          )}

          {needsParentSelection && (
            <Controller
              control={form.control}
              name="parentCategoryId"
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor={`${formId}-category-parent`}>
                    Categoria Pai
                  </FieldLabel>
                  <Select
                    name={field.name}
                    onValueChange={(value) =>
                      field.onChange(value ?? undefined)
                    }
                    value={field.value || null}
                  >
                    <SelectTrigger
                      id={`${formId}-category-parent`}
                      className="w-full"
                      aria-invalid={fieldState.invalid}
                      aria-describedby={
                        fieldState.invalid
                          ? `${formId}-category-parent-error`
                          : undefined
                      }
                    >
                      <SelectValue placeholder="Selecione a categoria pai">
                        {
                          parentCategories.find((cat) => cat.id === field.value)
                            ?.name
                        }
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        {parentCategories.map((cat) => (
                          <SelectItem key={cat.id} value={cat.id}>
                            {cat.name}
                          </SelectItem>
                        ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                  {fieldState.invalid && (
                    <FieldError
                      id={`${formId}-category-parent-error`}
                      errors={[fieldState.error]}
                    />
                  )}
                </Field>
              )}
            />
          )}

          <Controller
            control={form.control}
            name="name"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`${formId}-category-name`}>
                  {isSubcategory ? "Nome da Subcategoria" : "Nome da Categoria"}
                </FieldLabel>
                <Input
                  {...field}
                  id={`${formId}-category-name`}
                  placeholder={
                    isSubcategory
                      ? "Ex: Combustível, Manutenção..."
                      : "Ex: Operacional, Marketing..."
                  }
                  autoFocus
                  aria-invalid={fieldState.invalid}
                  aria-describedby={
                    fieldState.invalid
                      ? `${formId}-category-name-error`
                      : undefined
                  }
                />
                {fieldState.invalid && (
                  <FieldError
                    id={`${formId}-category-name-error`}
                    errors={[fieldState.error]}
                  />
                )}
              </Field>
            )}
          />
        </FieldGroup>
      </form>
    </Modal>
  );
}
