export const AUTH_STORAGE_KEY = "devlivery@auth";

export interface SessionUser {
  id: string;
  name: string;
  email: string;
}

export interface StoredAuth {
  user: SessionUser | null;
  token: string | null;
}

export function readAuth(): StoredAuth {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) return { user: null, token: null };
    const parsed = JSON.parse(raw) as Partial<StoredAuth>;
    return { user: parsed.user ?? null, token: parsed.token ?? null };
  } catch {
    return { user: null, token: null };
  }
}

export function writeAuth(auth: StoredAuth): void {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
}

export function clearAuth(): void {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}
