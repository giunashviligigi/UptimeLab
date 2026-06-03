import type { AuthResponse } from "./types";

const TOKEN_KEY = "uptimelab_token";
const USER_KEY = "uptimelab_user";
export const AUTH_CHANGE_EVENT = "uptimelab-auth-change";

export function notifyAuthChange() {
  if (typeof window !== "undefined") {
    window.dispatchEvent(new Event(AUTH_CHANGE_EVENT));
  }
}

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
  notifyAuthChange();
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
  notifyAuthChange();
}

export function isLoggedIn(): boolean {
  return !!getToken();
}
