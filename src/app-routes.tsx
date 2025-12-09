import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { LoginPage } from "./features/auth/pages/login-page";
import { CashPage } from "./features/cash/pages/cash-page";
import { DashboardPage } from "./features/dashboard/pages/dashboard-page";
import { OrdersPage } from "./features/orders/pages/orders-page";
import { ProductsPage } from "./features/products/pages/products-page";
import { Layout } from "./shared/components/layout";
import { RequireAuth } from "./shared/components/require-auth";

const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/",
    element: <RequireAuth />,
    children: [
      {
        element: <Layout />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: "orders", element: <OrdersPage /> },
          { path: "cash", element: <CashPage /> },
          { path: "products", element: <ProductsPage /> },
        ],
      },
    ],
  },
]);

export { router };

export function AppRoutes() {
  return <RouterProvider router={router} />;
}
