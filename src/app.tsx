import { AppRoutes } from "./app-routes";
import { OrderProvider } from "./shared/contexts/OrderContext";
import { ProductProvider } from "./shared/contexts/product-context";

export function App() {
  return (
    <ProductProvider>
      <OrderProvider>
        <AppRoutes />
      </OrderProvider>
    </ProductProvider>
  );
}
