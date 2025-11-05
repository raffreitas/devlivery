import { env } from "@/env";
import { AUTH_STORAGE_KEY } from "@/features/auth/services/auth-service";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message?: string | null;
  timestamp?: string;
}

export class ApiError extends Error {
  status: number;
  details?: unknown;
  constructor(message: string, status: number, details?: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.details = details;
  }
}

function getAuthToken(): string | null {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as { token: string | null };
    return parsed.token ?? null;
  } catch {
    return null;
  }
}

async function request<T>(
  path: string,
  options: {
    method?: HttpMethod;
    body?: unknown;
    headers?: Record<string, string>;
  } = {},
): Promise<T> {
  const url = `${env.VITE_API_URL.replace(/\/$/, "")}${path.startsWith("/") ? "" : "/"}${path}`;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers ?? {}),
  };

  const token = getAuthToken();
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(url, {
    method: options.method ?? "GET",
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined,
  });

  // No content
  if (res.status === 204) return undefined as unknown as T;

  let json: unknown;
  try {
    json = await res.json();
  } catch {
    if (!res.ok) throw new ApiError(res.statusText, res.status);
    return undefined as unknown as T;
  }

  if (!res.ok) {
    const j = (json ?? {}) as Record<string, unknown>;
    const errMsg =
      (typeof j.title === "string" && j.title) ||
      (typeof j.message === "string" && j.message) ||
      res.statusText;
    throw new ApiError(String(errMsg), res.status, json);
  }

  return json as T;
}

export const api = {
  get: <T>(path: string) => request<T>(path, { method: "GET" }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "POST", body }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PUT", body }),
  patch: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PATCH", body }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};
