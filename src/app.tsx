import { AppRoutes } from "./app-routes";
import { AuthProvider } from "./shared/contexts/AuthContext";
import { OrderProvider } from "./shared/contexts/OrderContext";
import { ProductProvider } from "./shared/contexts/product-context";

export function App() {
  return (
    <AuthProvider>
      <ProductProvider>
        <OrderProvider>
          <AppRoutes />
        </OrderProvider>
      </ProductProvider>
    </AuthProvider>
  );
}
