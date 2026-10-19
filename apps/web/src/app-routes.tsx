import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { Layout } from "./shared/components/layout";
import { LoadingState } from "./shared/components/loading";
import { RequireAuth } from "./shared/components/require-auth";
import { NotFoundPage } from "./shared/pages/not-found-page";
import { RouteErrorPage } from "./shared/pages/route-error-page";

// Cada página vira um chunk próprio, carregado na primeira visita à rota
const pageFallback = <LoadingState isLoading>{null}</LoadingState>;
const fullScreenError = <RouteErrorPage fullScreen />;

const router = createBrowserRouter([
  {
    path: "/lp",
    hydrateFallbackElement: pageFallback,
    errorElement: fullScreenError,
    lazy: async () => ({
      Component: (await import("./features/landing/pages/landing-page"))
        .LandingPage,
    }),
  },
  {
    path: "/login",
    hydrateFallbackElement: pageFallback,
    errorElement: fullScreenError,
    lazy: async () => ({
      Component: (await import("./features/auth/pages/login-page")).LoginPage,
    }),
  },
  {
    path: "/",
    element: <RequireAuth />,
    hydrateFallbackElement: pageFallback,
    errorElement: fullScreenError,
    children: [
      {
        element: <Layout />,
        children: [
          {
            // A failing page renders its error inside the layout, keeping navigation usable.
            errorElement: <RouteErrorPage />,
            children: [
              {
                index: true,
                lazy: async () => ({
                  Component: (
                    await import("./features/dashboard/pages/dashboard-page")
                  ).DashboardPage,
                }),
              },
              {
                path: "orders",
                lazy: async () => ({
                  Component: (
                    await import("./features/orders/pages/orders-page")
                  ).OrdersPage,
                }),
              },
              {
                path: "cash",
                lazy: async () => ({
                  Component: (await import("./features/cash/pages/cash-page"))
                    .CashPage,
                }),
              },
              {
                path: "products",
                lazy: async () => ({
                  Component: (
                    await import("./features/products/pages/products-page")
                  ).ProductsPage,
                }),
              },
              {
                path: "expenses",
                lazy: async () => ({
                  Component: (
                    await import("./features/expenses/pages/expenses-page")
                  ).ExpensesPage,
                }),
              },
            ],
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
