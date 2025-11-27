import { Plus } from "lucide-react";
import { useState } from "react";
import { LoadingSpinner } from "@/shared/components/loading-spinner";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/shared/components/ui/select";
import { Separator } from "@/shared/components/ui/separator";
import { ProductCard } from "../components/product-card";
import { ProductForm } from "../components/product-form";
import { useProducts } from "../hooks/use-products";
import type { Product, ProductFormData } from "../types";

export function ProductsPage() {
  const {
    products,
    loading,
    isFetching,
    createProduct,
    updateProduct,
    deleteProduct,
  } = useProducts();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [alert, setAlert] = useState({
    open: false,
    productId: null as string | null,
  });
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterCategory, setFilterCategory] = useState<string>("all");

  const handleCreateOrUpdate = (data: ProductFormData) => {
    if (editingProduct) {
      updateProduct(editingProduct.id, data);
    } else {
      createProduct(data);
    }
    setIsModalOpen(false);
    setEditingProduct(null);
  };

  const handleEdit = (product: Product) => {
    setEditingProduct(product);
    setIsModalOpen(true);
  };

  const handleDelete = async () => {
    const id = alert.productId;
    if (!id) return;
    await deleteProduct(id);
    setAlert({ open: false, productId: null });
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingProduct(null);
  };

  const categories = Array.from(new Set(products.map((p) => p.category)));

  const filteredProducts = products.filter((product) => {
    const matchesSearch =
      product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.description.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory =
      filterCategory === "all" || product.category === filterCategory;
    return matchesSearch && matchesCategory;
  });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold text-gray-900">Produtos</h1>
          {isFetching && (
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <LoadingSpinner size="sm" className="text-orange-500" />
              <span>Atualizando...</span>
            </div>
          )}
        </div>
        <Button onClick={() => setIsModalOpen(true)}>
          <Plus size={4} />
          Novo Produto
        </Button>
      </div>

      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <Input
            type="text"
            placeholder="Buscar produtos..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="h-10"
          />
        </div>
        <Select onValueChange={setFilterCategory}>
          <SelectTrigger size="lg" className="w-full sm:w-1/6 cursor-pointer">
            <span>
              {filterCategory === "all"
                ? "Todas as categorias"
                : filterCategory}
            </span>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all" onSelect={() => setFilterCategory("all")}>
              Todas as categorias
            </SelectItem>
            {categories.map((category) => (
              <SelectItem
                key={category}
                value={category}
                onSelect={() => setFilterCategory(category)}
                className="cursor-pointer"
              >
                {category}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {loading && products.length === 0 ? (
        <div className="flex justify-center items-center h-64">
          <div className="text-xl text-secondary-foreground">Carregando...</div>
        </div>
      ) : filteredProducts.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">
            {products.length === 0
              ? "Nenhum produto cadastrado. Comece criando um novo produto!"
              : "Nenhum produto encontrado com os filtros aplicados."}
          </p>
        </div>
      ) : (
        <div
          className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 transition-opacity duration-200 ${
            isFetching ? "opacity-60" : "opacity-100"
          }`}
        >
          {filteredProducts.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              onEdit={handleEdit}
              onDelete={(productId) => setAlert({ open: true, productId })}
            />
          ))}
        </div>
      )}

      <AlertDialog
        open={alert.open}
        onOpenChange={() => setAlert({ open: false, productId: null })}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              Tem certeza que deseja excluir este produto?
            </AlertDialogTitle>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              variant="destructive"
              onClick={() => handleDelete()}
            >
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog
        defaultOpen={false}
        open={isModalOpen}
        onOpenChange={handleCloseModal}
        modal={true}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingProduct ? "Editar Produto" : "Novo Produto"}
            </DialogTitle>
          </DialogHeader>

          <Separator />

          <ProductForm
            initialData={editingProduct || undefined}
            onSubmit={handleCreateOrUpdate}
            onCancel={handleCloseModal}
            categoryOptions={categories.map((c) => ({ value: c, label: c }))}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
