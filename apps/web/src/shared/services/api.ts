import axios, { type AxiosError } from "axios";
import { env } from "@/env";
import { AUTH_STORAGE_KEY } from "@/features/auth/services/auth-service";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export type ApiProblem = {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string> | null;
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

export type ApiErrorKind = "http" | "network" | "timeout";

export class ApiError extends Error {
  status?: number;
  details?: ApiProblem;
  kind: ApiErrorKind;
  constructor(
    message: string,
    status?: number,
    details?: ApiProblem,
    kind: ApiErrorKind = "http",
  ) {
    super(message);
    this.status = status;
    this.details = details;
    this.kind = kind;
    this.name = "ApiError";
  }
}

export class UnauthorizedError extends ApiError {
  constructor(message = "Não autorizado", details?: ApiProblem) {
    super(message, 401, details);
    this.name = "UnauthorizedError";
  }
}

export function shouldRetryQuery(failureCount: number, error: Error): boolean {
  if (
    axios.isCancel(error) ||
    failureCount >= 2 ||
    !(error instanceof ApiError)
  )
    return false;
  return (
    error.kind !== "http" ||
    [408, 500, 502, 503, 504].includes(error.status ?? 0)
  );
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
  baseURL: `${env.VITE_API_URL.replace(/\/$/, "")}/api/v1`,
  timeout: 30_000,
  headers: { "Content-Type": "application/json" },
});

async function request<T>(
  path: string,
  options: {
    method?: HttpMethod;
    body?: unknown;
    headers?: Record<string, string>;
    signal?: AbortSignal;
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
      signal: options.signal,
    });

    if (res.status === 204) return undefined as unknown as T;

    return res.data as T;
  } catch (err) {
    if (axios.isCancel(err)) throw err;
    if (!axios.isAxiosError(err)) throw err;
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
          data ?? undefined,
        );
      }

      throw new ApiError(message, status, data ?? undefined);
    }

    const timeout =
      axiosErr.code === "ECONNABORTED" || axiosErr.code === "ETIMEDOUT";
    throw new ApiError(
      timeout
        ? "A requisição demorou demais. Tente novamente."
        : "Não foi possível conectar ao servidor. Verifique sua conexão.",
      undefined,
      undefined,
      timeout ? "timeout" : "network",
    );
  }
}

export const api = {
  get: <T>(path: string, signal?: AbortSignal) =>
    request<T>(path, { method: "GET", signal }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "POST", body }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PUT", body }),
  patch: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PATCH", body }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};
