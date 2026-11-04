import { server } from "@test/server";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import { UnauthorizedError } from "@/shared/services/api";
import { AUTH_STORAGE_KEY } from "@/shared/services/auth-storage";
import { authService } from "./auth-service";

const url = "http://localhost:5000/api/v1/auth/login";
const credentials = { email: "maria@pizzaria.com", password: "segredo" };

describe("authService", () => {
  it("persists the session returned by the API and reports it as authenticated", async () => {
    server.use(
      http.post(url, () =>
        HttpResponse.json({
          data: { userId: "user-1", userName: "Maria", token: "jwt-token" },
        }),
      ),
    );

    const auth = await authService.login(credentials);

    expect(auth).toEqual({
      user: { id: "user-1", name: "Maria", email: credentials.email },
      token: "jwt-token",
    });
    expect(authService.getAuth()).toEqual(auth);
    expect(authService.isAuthenticated()).toBe(true);
  });

  it("stores nothing when the API rejects the credentials", async () => {
    server.use(
      http.post(url, () =>
        HttpResponse.json(
          { title: "Acesso não autorizado", status: 401 },
          { status: 401 },
        ),
      ),
    );

    await expect(authService.login(credentials)).rejects.toBeInstanceOf(
      UnauthorizedError,
    );
    expect(localStorage.getItem(AUTH_STORAGE_KEY)).toBeNull();
    expect(authService.isAuthenticated()).toBe(false);
  });

  it("treats a corrupted stored session as signed out", () => {
    localStorage.setItem(AUTH_STORAGE_KEY, "{sessão inválida");

    expect(authService.getAuth()).toEqual({ user: null, token: null });
    expect(authService.isAuthenticated()).toBe(false);
  });

  it("clears the stored session on logout", async () => {
    localStorage.setItem(
      AUTH_STORAGE_KEY,
      JSON.stringify({ user: null, token: "jwt-token" }),
    );

    await authService.logout();

    expect(localStorage.getItem(AUTH_STORAGE_KEY)).toBeNull();
  });
});
