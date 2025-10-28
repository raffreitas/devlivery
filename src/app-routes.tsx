import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { DashboardPage } from "./features/dashboard/pages/DashboardPage";
import { OrdersPage } from "./features/orders/pages/OrdersPage";
import { ProductsPage } from "./features/products/pages/products-page";
import { Layout } from "./shared/components/layout";

const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    children: [
      {
        index: true,
        element: <DashboardPage />,
      },
      {
        path: "products",
        element: <ProductsPage />,
      },
      {
        path: "orders",
        element: <OrdersPage />,
      },
    ],
  },
]);

export function AppRoutes() {
  return <RouterProvider router={router} />;
}
