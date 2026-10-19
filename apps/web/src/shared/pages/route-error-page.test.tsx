import { render, screen } from "@testing-library/react";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { RouteErrorPage } from "./route-error-page";

function renderFailingRoute(lazyError: Error) {
  const router = createMemoryRouter([
    {
      path: "/",
      errorElement: <RouteErrorPage fullScreen />,
      lazy: async () => {
        throw lazyError;
      },
    },
  ]);
  return render(<RouterProvider router={router} />);
}

describe("RouteErrorPage", () => {
  const originalLocation = window.location;
  const reload = vi.fn();

  beforeEach(() => {
    sessionStorage.clear();
    reload.mockReset();
    vi.spyOn(console, "error").mockImplementation(() => {});
    Object.defineProperty(window, "location", {
      configurable: true,
      value: { ...originalLocation, reload },
    });
  });

  afterEach(() => {
    Object.defineProperty(window, "location", {
      configurable: true,
      value: originalLocation,
    });
  });

  it("GivenARenderFailure_WhenTheRouteFails_ThenShowsRecoveryActions", async () => {
    renderFailingRoute(new TypeError("Cannot read properties of undefined"));

    expect(await screen.findByText("Algo deu errado")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Recarregar/ })).toBeVisible();
    expect(
      screen.getByRole("link", { name: /Voltar ao início/ }),
    ).toBeVisible();
    expect(reload).not.toHaveBeenCalled();
  });

  it("GivenAMissingChunkAfterDeploy_WhenTheRouteLoads_ThenReloadsOnce", async () => {
    const chunkError = new TypeError(
      "Failed to fetch dynamically imported module: /assets/orders-page.js",
    );
    renderFailingRoute(chunkError);

    expect(
      await screen.findByText("Nova versão disponível"),
    ).toBeInTheDocument();
    expect(reload).toHaveBeenCalledOnce();
  });

  it("GivenAReloadThatJustFailed_WhenTheChunkIsStillMissing_ThenDoesNotLoop", async () => {
    sessionStorage.setItem("devlivery@chunk-reload-at", String(Date.now()));
    renderFailingRoute(
      new TypeError("Failed to fetch dynamically imported module"),
    );

    expect(
      await screen.findByText("Nova versão disponível"),
    ).toBeInTheDocument();
    expect(reload).not.toHaveBeenCalled();
  });
});
