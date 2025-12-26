import { PlusIcon } from "lucide-react";
import { useState } from "react";
import { BottomSheet } from "@/shared/components/bottom-sheet";
import {
  GridSkeleton,
  LoadingOverlay,
  LoadingState,
} from "@/shared/components/loading";
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
import { Separator } from "@/shared/components/ui/separator";
import { ProductCard } from "../components/product-card";
import { ProductForm } from "../components/product-form";
import { ProductsFilters } from "../components/products-filters";
import { ProductsFiltersContent } from "../components/products-filters-content";
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
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);

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
      <LoadingOverlay isFetching={isFetching} position="top-bar" />

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 tracking-tight">
            Produtos
          </h1>
          <p className="text-muted-foreground">
            Gerencie seu catálogo de produtos
          </p>
        </div>
        <div className="flex items-center gap-2 w-full sm:w-auto">
          <Button
            onClick={() => setIsModalOpen(true)}
            className="w-full sm:w-auto"
          >
            <PlusIcon className="w-4 h-4 mr-2" />
            Novo Produto
          </Button>
        </div>
      </div>

      <ProductsFilters
        searchTerm={searchTerm}
        filterCategory={filterCategory}
        categories={categories}
        onSearchChange={setSearchTerm}
        onCategoryChange={setFilterCategory}
        onOpenFilters={() => setIsFiltersOpen(true)}
      />

      <LoadingState
        isLoading={loading && products.length === 0}
        skeleton={<GridSkeleton items={10} columns={5} />}
      >
        {filteredProducts.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-500 text-lg">
              {products.length === 0
                ? "Nenhum produto cadastrado. Comece criando um novo produto!"
                : "Nenhum produto encontrado com os filtros aplicados."}
            </p>
          </div>
        ) : (
          <div
            className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-6 transition-opacity duration-200 ${
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
      </LoadingState>

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
            <AlertDialogAction onClick={() => handleDelete()}>
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog open={isModalOpen} onOpenChange={handleCloseModal}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingProduct ? "Editar Produto" : "Novo Produto"}
            </DialogTitle>
          </DialogHeader>

          <Separator />

          <ProductForm
            initialData={editingProduct}
            onSubmit={handleCreateOrUpdate}
            onCancel={handleCloseModal}
            categoryOptions={categories.map((c) => ({ value: c, label: c }))}
          />
        </DialogContent>
      </Dialog>

      <BottomSheet
        isOpen={isFiltersOpen}
        onClose={() => setIsFiltersOpen(false)}
        title="Filtros"
      >
        <div className="space-y-4">
          <ProductsFiltersContent
            searchTerm={searchTerm}
            filterCategory={filterCategory}
            categories={categories}
            onSearchChange={setSearchTerm}
            onCategoryChange={setFilterCategory}
          />

          <div className="pt-4 pb-2 border-t border-gray-200">
            <Button onClick={() => setIsFiltersOpen(false)} className="w-full">
              Aplicar Filtros
            </Button>
          </div>
        </div>
      </BottomSheet>
    </div>
  );
}
