import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { LoginPage } from "./features/auth/pages/login-page";
import { CashPage } from "./features/cash/pages/cash-page";
import { DashboardPage } from "./features/dashboard/pages/dashboard-page";
import { ExpensesPage } from "./features/expenses/pages/expenses-page";
import { LandingPage } from "./features/landing/pages/landing-page";
import { OrdersPage } from "./features/orders/pages/orders-page";
import { ProductsPage } from "./features/products/pages/products-page";
import { Layout } from "./shared/components/layout";
import { RequireAuth } from "./shared/components/require-auth";
import { NotFoundPage } from "./shared/pages/not-found-page";

const router = createBrowserRouter([
  {
    path: "/lp",
    element: <LandingPage />,
  },
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
          {
            index: true,
            element: <DashboardPage />,
          },
          {
            path: "orders",
            element: <OrdersPage />,
          },
          {
            path: "cash",
            element: <CashPage />,
          },
          {
            path: "products",
            element: <ProductsPage />,
          },
          {
            path: "expenses",
            element: <ExpensesPage />,
          },
        ],
      },
    ],
  },
  {
    path: "*",
    element: <NotFoundPage />,
  },
]);

export { router };

export function AppRoutes() {
  return <RouterProvider router={router} />;
}
