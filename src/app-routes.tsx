import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { LoginPage } from "./features/auth/pages/login-page";
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
          { path: "products", element: <ProductsPage /> },
          { path: "orders", element: <OrdersPage /> },
        ],
      },
    ],
  },
]);

export { router };

export function AppRoutes() {
  return <RouterProvider router={router} />;
}
