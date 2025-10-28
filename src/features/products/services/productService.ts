import type { Product, ProductFormData } from '../types';

const STORAGE_KEY = 'products';

export const productService = {
  getAll: (): Product[] => {
    const data = localStorage.getItem(STORAGE_KEY);
    return data ? JSON.parse(data) : [];
  },

  getById: (id: string): Product | null => {
    const products = productService.getAll();
    return products.find(p => p.id === id) || null;
  },

  create: (data: ProductFormData): Product => {
    const products = productService.getAll();
    const newProduct: Product = {
      ...data,
      id: crypto.randomUUID(),
      createdAt: new Date(),
      updatedAt: new Date(),
    };
    products.push(newProduct);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(products));
    return newProduct;
  },

  update: (id: string, data: Partial<ProductFormData>): Product => {
    const products = productService.getAll();
    const index = products.findIndex(p => p.id === id);
    if (index === -1) throw new Error('Product not found');

    products[index] = {
      ...products[index],
      ...data,
      updatedAt: new Date(),
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(products));
    return products[index];
  },

  delete: (id: string): void => {
    const products = productService.getAll();
    const filtered = products.filter(p => p.id !== id);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
  },
};
