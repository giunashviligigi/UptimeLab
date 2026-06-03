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
}

export interface DashboardStats {
  totalSites: number;
  onlineSites: number;
  offlineSites: number;
  averageResponseTimeMs: number | null;
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
  history: CheckResult[];
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
  }[];
}
