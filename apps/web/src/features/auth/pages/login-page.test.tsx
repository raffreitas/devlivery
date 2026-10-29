import { QueryClientProvider } from "@tanstack/react-query";
import { createTestClient } from "@test/render";
import { server } from "@test/server";
import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { AuthProvider } from "@/shared/contexts/auth-context";
import { authEvents } from "@/shared/services/auth-events";
import { AUTH_STORAGE_KEY } from "@/shared/services/auth-storage";
import { LoginPage } from "./login-page";

const url = "http://localhost:5000/api/v1/auth/login";

function renderLogin() {
  return render(
    <QueryClientProvider client={createTestClient()}>
      <AuthProvider>
        <MemoryRouter initialEntries={["/login"]}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/" element={<p>Painel</p>} />
          </Routes>
        </MemoryRouter>
      </AuthProvider>
    </QueryClientProvider>,
  );
}

async function submitCredentials(email: string, password: string) {
  const user = userEvent.setup();
  await user.type(await screen.findByLabelText("E-mail"), email);
  await user.type(screen.getByLabelText("Senha"), password);
  await user.click(screen.getByRole("button", { name: "Entrar" }));
}

describe("LoginPage", () => {
  it("signs in, keeps the session and opens the dashboard", async () => {
    server.use(
      http.post(url, () =>
        HttpResponse.json({
          data: { userId: "user-1", userName: "Maria", token: "jwt-token" },
        }),
      ),
    );
    renderLogin();

    await submitCredentials("maria@pizzaria.com", "segredo");

    expect(await screen.findByText("Painel")).toBeInTheDocument();
    expect(
      JSON.parse(localStorage.getItem(AUTH_STORAGE_KEY) ?? "{}"),
    ).toMatchObject({ token: "jwt-token" });
  });

  it("keeps the operator on the form and explains rejected credentials", async () => {
    server.use(
      http.post(url, () =>
        HttpResponse.json(
          {
            title: "Acesso não autorizado",
            status: 401,
            detail: "Usuário ou senha inválidos.",
          },
          { status: 401 },
        ),
      ),
    );
    renderLogin();

    await submitCredentials("maria@pizzaria.com", "senha-errada");

    expect(
      await screen.findByText("Usuário ou senha inválidos."),
    ).toBeInTheDocument();
    expect(screen.queryByText("Painel")).not.toBeInTheDocument();
    expect(localStorage.getItem(AUTH_STORAGE_KEY)).toBeNull();
  });

  it("ends the session when the API reports an expired token", async () => {
    localStorage.setItem(
      AUTH_STORAGE_KEY,
      JSON.stringify({
        user: { id: "user-1", name: "Maria", email: "maria@pizzaria.com" },
        token: "jwt-expirado",
      }),
    );
    renderLogin();
    expect(await screen.findByText("Painel")).toBeInTheDocument();

    await act(async () => authEvents.emit());

    expect(localStorage.getItem(AUTH_STORAGE_KEY)).toBeNull();
  });
});
