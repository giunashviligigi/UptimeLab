import type { AuthResponse } from "./types";

const TOKEN_KEY = "uptimelab_token";
const USER_KEY = "uptimelab_user";

export interface StoredUser {
  userId: string;
  email: string;
  displayName: string;
  role: string;
}

/** Save JWT and user info after login/register. */
export function saveAuth(response: AuthResponse): void {
  if (typeof window === "undefined") return;
  localStorage.setItem(TOKEN_KEY, response.token);
  localStorage.setItem(
    USER_KEY,
    JSON.stringify({
      userId: response.userId,
      email: response.email,
      displayName: response.displayName,
      role: response.role,
    } satisfies StoredUser)
  );
}

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(TOKEN_KEY);
}

export function getUser(): StoredUser | null {
  if (typeof window === "undefined") return null;
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as StoredUser;
  } catch {
    return null;
  }
}

export function clearAuth(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function isLoggedIn(): boolean {
  return !!getToken();
}
