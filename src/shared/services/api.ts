import axios, { type AxiosError } from "axios";
import { env } from "@/env";
import { AUTH_STORAGE_KEY } from "@/features/auth/services/auth-service";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export type ApiProblem = {
  title: string;
  detail: string;
  status: number;
  errors: Record<string, string>;
};

export type ApiResponse<T> = {
  data: T;
  meta?: Metadata;
};

export interface Metadata {
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
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

export class UnauthorizedError extends ApiError {
  constructor(message = "Não autorizado", details?: unknown) {
    super(message, 401, details);
    this.name = "UnauthorizedError";
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

const axiosInstance = axios.create({
  baseURL: env.VITE_API_URL.replace(/\/$/, ""),
  headers: { "Content-Type": "application/json" },
});

async function request<T>(
  path: string,
  options: {
    method?: HttpMethod;
    body?: unknown;
    headers?: Record<string, string>;
  } = {},
): Promise<T> {
  const url = path.startsWith("/") ? path : `/${path}`;

  const token = getAuthToken();
  const headers: Record<string, string> = {
    ...(options.headers ?? {}),
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  try {
    const res = await axiosInstance.request({
      url,
      method: options.method ?? "GET",
      data: options.body ?? undefined,
      headers,
    });

    if (res.status === 204) return undefined as unknown as T;

    return res.data as T;
  } catch (err) {
    const axiosErr = err as AxiosError;

    if (axiosErr.response) {
      const status = axiosErr.response.status;
      const data = axiosErr.response.data as ApiProblem | null;
      const message =
        data?.detail ||
        data?.title ||
        "Houve um erro ao processar sua requisição.";

      if (status === 401) {
        throw new UnauthorizedError(
          message || "Sua sessão expirou. Por favor, faça login novamente.",
          data,
        );
      }

      return Promise.reject({
        message,
        status,
        data,
      });
    }

    return Promise.reject({
      message: axiosErr.message,
    });
  }
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
