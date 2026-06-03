export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  displayName: string;
  role: string;
}

export interface Site {
  id: string;
  url: string;
  name: string | null;
  status: string;
  httpStatusCode: number | null;
  responseTimeMs: number | null;
  lastCheckedAt: string | null;
  createdAt: string;
  isPaused: boolean;
  lastErrorMessage: string | null;
  uptimePercent24h: number | null;
}

export interface DashboardStats {
  totalSites: number;
  onlineSites: number;
  offlineSites: number;
  pausedSites: number;
  averageResponseTimeMs: number | null;
  overallUptimePercent24h: number | null;
}

export interface CheckResult {
  id: string;
  status: string;
  httpStatusCode: number | null;
  responseTimeMs: number;
  errorMessage: string | null;
  checkedAt: string;
}

export interface SiteHistory {
  siteId: string;
  url: string;
  name: string | null;
  isPaused: boolean;
  uptimePercent24h: number | null;
  uptimePercent7d: number | null;
  history: CheckResult[];
}

export interface UserSettings {
  webhookUrl: string | null;
  webhookAlertsEnabled: boolean;
}

export interface PublicStatus {
  userId: string;
  displayName: string;
  sites: {
    url: string;
    name: string | null;
    status: string;
    httpStatusCode: number | null;
    responseTimeMs: number | null;
    lastCheckedAt: string | null;
    isPaused: boolean;
  }[];
}
