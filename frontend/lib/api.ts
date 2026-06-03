/**
 * Central API client for UptimeLab backend.
 */
import { getToken, clearAuth } from "./auth";
import type {
  AuthResponse,
  DashboardStats,
  PublicStatus,
  Site,
  SiteHistory,
  UserSettings,
} from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5001";

async function request<T>(
  path: string,
  options: RequestInit = {},
  auth = false
): Promise<T> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (auth) {
    const token = getToken();
    if (!token) throw new Error("Not authenticated");
    headers.Authorization = `Bearer ${token}`;
  }

  const res = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (res.status === 401 && auth) {
    clearAuth();
    if (typeof window !== "undefined") window.location.href = "/login";
    throw new Error("Session expired");
  }

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error((body as { message?: string }).message ?? res.statusText);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

export const api = {
  register: (email: string, password: string, displayName: string) =>
    request<AuthResponse>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ email, password, displayName }),
    }),

  login: (email: string, password: string) =>
    request<AuthResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    }),

  getSites: () => request<Site[]>("/api/sites", {}, true),

  getStats: () => request<DashboardStats>("/api/sites/stats", {}, true),

  createSite: (url: string, name?: string) =>
    request<Site>(
      "/api/sites",
      { method: "POST", body: JSON.stringify({ url, name }) },
      true
    ),

  deleteSite: (id: string) =>
    request<void>(`/api/sites/${id}`, { method: "DELETE" }, true),

  setPause: (id: string, isPaused: boolean) =>
    request<Site>(
      `/api/sites/${id}/pause`,
      { method: "PATCH", body: JSON.stringify({ isPaused }) },
      true
    ),

  getHistory: (id: string, limit = 100) =>
    request<SiteHistory>(`/api/sites/${id}/history?limit=${limit}`, {}, true),

  getSettings: () => request<UserSettings>("/api/settings", {}, true),

  updateSettings: (webhookUrl: string | null, webhookAlertsEnabled: boolean) =>
    request<UserSettings>(
      "/api/settings",
      {
        method: "PUT",
        body: JSON.stringify({ webhookUrl, webhookAlertsEnabled }),
      },
      true
    ),

  getPublicStatus: (userId: string) =>
    request<PublicStatus>(`/api/public/status/${userId}`),
};
