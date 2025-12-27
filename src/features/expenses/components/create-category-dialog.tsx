import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { LoadingButton } from "@/shared/components/loading";
import { Modal } from "@/shared/components/modal";
import { Button } from "@/shared/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/components/ui/form";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
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
  const { data: categories } = useExpenseCategories();
  const { createCategory, isCreating } = useExpenseCategoriesManagement();
  // Se tem initialParentCategoryId, é subcategoria
  // Se categoryOnly é true, força ser categoria principal
  const isSubcategory = !categoryOnly && !!initialParentCategoryId;

  const form = useForm<CreateCategoryFormData>({
    resolver: zodResolver(createCategorySchema),
    defaultValues: {
      name: initialName || "",
      categoryType: isSubcategory ? "subcategory" : "main",
      parentCategoryId: initialParentCategoryId,
    },
  });

  // Atualiza o nome quando initialName muda
  useEffect(() => {
    if (initialName && open) {
      form.setValue("name", initialName);
    }
  }, [initialName, open, form]);

  // Reset form quando modal abre/fecha
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
  // Se escolheu "subcategory" no tipo, precisa selecionar categoria pai
  const needsParentSelection = !isSubcategory && categoryType === "subcategory";

  const handleSubmit = async (data: CreateCategoryFormData) => {
    try {
      // Se é subcategoria (tem initialParentCategoryId), usa ele
      // Se escolheu "subcategory" no tipo, precisa ter selecionado categoria pai
      let finalParentId: string | undefined;

      if (initialParentCategoryId) {
        // Subcategoria criada a partir do SubcategoryCombobox
        finalParentId = initialParentCategoryId;
      } else if (data.categoryType === "subcategory" && data.parentCategoryId) {
        // Subcategoria criada escolhendo tipo "Subcategoria" e selecionando categoria pai
        finalParentId = data.parentCategoryId;
      } else {
        // Categoria principal
        finalParentId = undefined;
      }

      const created = await createCategory({
        name: data.name,
        parentCategoryId: finalParentId,
      });
      toast.success(
        finalParentId
          ? "Subcategoria criada com sucesso!"
          : "Categoria criada com sucesso!",
      );
      form.reset();
      onOpenChange(false);
      onSuccess(created);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : isSubcategory
            ? "Erro ao criar subcategoria"
            : "Erro ao criar categoria",
      );
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
      <Form {...form}>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
            form.handleSubmit(handleSubmit)();
          }}
          className="space-y-5"
        >
          {/* Mostra categoria pai se for subcategoria */}
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

          {/* Se não é subcategoria e não é categoryOnly, permite escolher tipo */}
          {!isSubcategory && !categoryOnly && (
            <FormField
              control={form.control}
              name="categoryType"
              render={({ field }) => (
                <FormItem className="space-y-2">
                  <FormLabel className="text-sm font-medium">Tipo</FormLabel>
                  <FormControl>
                    <Select
                      onValueChange={(value) => {
                        field.onChange(value as "main" | "subcategory");
                        // Limpa parentCategoryId quando muda para "main"
                        if (value === "main") {
                          form.setValue("parentCategoryId", undefined);
                        }
                      }}
                      value={field.value || "main"}
                    >
                      <SelectTrigger className="h-10">
                        <SelectValue placeholder="Selecione o tipo" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="main">
                          Categoria Principal
                        </SelectItem>
                        <SelectItem value="subcategory">
                          Subcategoria
                        </SelectItem>
                      </SelectContent>
                    </Select>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

          {/* Se escolheu subcategoria mas não tem initialParentCategoryId, mostra select de categoria pai */}
          {needsParentSelection && (
            <FormField
              control={form.control}
              name="parentCategoryId"
              render={({ field }) => (
                <FormItem className="space-y-2">
                  <FormLabel className="text-sm font-medium">
                    Categoria Pai
                  </FormLabel>
                  <FormControl>
                    <Select
                      onValueChange={field.onChange}
                      value={field.value || ""}
                    >
                      <SelectTrigger className="h-10">
                        <SelectValue placeholder="Selecione a categoria pai" />
                      </SelectTrigger>
                      <SelectContent>
                        {parentCategories.map((cat) => (
                          <SelectItem key={cat.id} value={cat.id}>
                            {cat.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem className="space-y-2">
                <FormLabel className="text-sm font-medium">
                  {isSubcategory ? "Nome da Subcategoria" : "Nome da Categoria"}
                </FormLabel>
                <FormControl>
                  <Input
                    placeholder={
                      isSubcategory
                        ? "Ex: Combustível, Manutenção..."
                        : "Ex: Operacional, Marketing..."
                    }
                    {...field}
                    autoFocus
                    className="h-10"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </form>
      </Form>
    </Modal>
  );
}
