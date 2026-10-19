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

export type ApiErrorKind =
  | "http"
  | "network"
  | "offline"
  | "timeout"
  | "unknown-outcome";

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
  constructor(message = "HTTP 401", details?: ApiProblem) {
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
    error.kind === "network" ||
    error.kind === "offline" ||
    error.kind === "timeout" ||
    (error.kind === "http" &&
      [408, 500, 502, 503, 504].includes(error.status ?? 0))
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
  transitional: { clarifyTimeoutError: true },
  headers: { "Content-Type": "application/json" },
});

export type RequestOptions = {
  /** The write has no lasting side effect (e.g. login), so a lost response is safe to retry. */
  idempotent?: boolean;
};

async function request<T>(
  path: string,
  options: {
    method?: HttpMethod;
    body?: unknown;
    headers?: Record<string, string>;
    signal?: AbortSignal;
  } & RequestOptions = {},
): Promise<T> {
  const url = path.startsWith("/") ? path : `/${path}`;
  const isSafe =
    !options.method || options.method === "GET" || options.idempotent === true;

  const token = getAuthToken();
  const headers: Record<string, string> = {
    ...(options.headers ?? {}),
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  try {
    const res = await axiosInstance.request({
      url,
      method: options.method ?? "GET",
      // A read can be retried. A write may have committed even if its response is lost.
      timeout: isSafe ? 30_000 : 0,
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
      // Proxies and gateways may answer with HTML or an empty body instead of problem details.
      const raw = axiosErr.response.data;
      const data =
        raw && typeof raw === "object" ? (raw as ApiProblem) : undefined;
      // Messages here describe the failure for logs; user-facing text lives in error-feedback.
      const message = data?.detail || data?.title || `HTTP ${status}`;

      if (status === 401) throw new UnauthorizedError(message, data);
      throw new ApiError(message, status, data);
    }

    // Without a response a write may still have committed, unless the browser never sent it.
    const offline = typeof navigator !== "undefined" && !navigator.onLine;
    if (!isSafe && !offline) {
      throw new ApiError(
        "Response lost after the write was sent",
        undefined,
        undefined,
        "unknown-outcome",
      );
    }
    if (axiosErr.code === "ECONNABORTED") {
      throw new axios.CanceledError("Requisição interrompida", axiosErr.config);
    }
    if (offline) {
      throw new ApiError("Browser is offline", undefined, undefined, "offline");
    }
    const timeout = axiosErr.code === "ETIMEDOUT";
    throw new ApiError(
      timeout ? "Request timed out" : "Server unreachable",
      undefined,
      undefined,
      timeout ? "timeout" : "network",
    );
  }
}

export const api = {
  get: <T>(path: string, signal?: AbortSignal) =>
    request<T>(path, { method: "GET", signal }),
  post: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { method: "POST", body, ...options }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PUT", body }),
  patch: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PATCH", body }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};
