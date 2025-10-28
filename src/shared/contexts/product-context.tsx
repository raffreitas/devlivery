import type { ReactNode } from "react";
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { productService } from "../../features/products/services/product-service";
import type { Product, ProductFormData } from "../../features/products/types";

interface ProductContextData {
  products: Product[];
  loading: boolean;
  fetchProducts: () => void;
  createProduct: (data: ProductFormData) => void;
  updateProduct: (id: string, data: Partial<ProductFormData>) => void;
  deleteProduct: (id: string) => void;
}

const ProductContext = createContext<ProductContextData | undefined>(undefined);

export const ProductProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchProducts = useCallback(() => {
    setLoading(true);
    try {
      const data = productService.getAll();
      setProducts(data);
    } catch (error) {
      console.error("Error fetching products:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  const createProduct = useCallback((data: ProductFormData) => {
    try {
      const newProduct = productService.create(data);
      setProducts((prev) => [...prev, newProduct]);
    } catch (error) {
      console.error("Error creating product:", error);
      throw error;
    }
  }, []);

  const updateProduct = useCallback(
    (id: string, data: Partial<ProductFormData>) => {
      try {
        const updatedProduct = productService.update(id, data);
        setProducts((prev) =>
          prev.map((p) => (p.id === id ? updatedProduct : p)),
        );
      } catch (error) {
        console.error("Error updating product:", error);
        throw error;
      }
    },
    [],
  );

  const deleteProduct = useCallback((id: string) => {
    try {
      productService.delete(id);
      setProducts((prev) => prev.filter((p) => p.id !== id));
    } catch (error) {
      console.error("Error deleting product:", error);
      throw error;
    }
  }, []);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  return (
    <ProductContext.Provider
      value={{
        products,
        loading,
        fetchProducts,
        createProduct,
        updateProduct,
        deleteProduct,
      }}
    >
      {children}
    </ProductContext.Provider>
  );
};

export const useProducts = () => {
  const context = useContext(ProductContext);
  if (!context) {
    throw new Error("useProducts must be used within a ProductProvider");
  }
  return context;
};
