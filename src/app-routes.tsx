import {
  createBrowserRouter,
  RouterProvider,
  redirect,
} from "react-router-dom";
import { LoginPage } from "./features/auth/pages/login-page";
import { authService } from "./features/auth/services/auth-service";
import { DashboardPage } from "./features/dashboard/pages/dashboard-page";
import { OrdersPage } from "./features/orders/pages/orders-page";
import { ProductsPage } from "./features/products/pages/products-page";
import { Layout } from "./shared/components/layout";
import { RequireAuth } from "./shared/components/require-auth";

async function requireAuthLoader() {
  if (!authService.isAuthenticated()) {
    return redirect("/login");
  }
  return null;
}

const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/",
    element: <RequireAuth />,
    loader: requireAuthLoader,
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
