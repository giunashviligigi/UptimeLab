"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { AuthGuard } from "@/components/AuthGuard";
import { StatCard } from "@/components/StatCard";
import { StatusBadge } from "@/components/StatusBadge";
import { api } from "@/lib/api";
import { getUser } from "@/lib/auth";
import type { DashboardStats, Site } from "@/lib/types";

export default function DashboardPage() {
  const [sites, setSites] = useState<Site[]>([]);
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [error, setError] = useState("");
  const user = getUser();

  const load = useCallback(async () => {
    try {
      const [s, st] = await Promise.all([api.getSites(), api.getStats()]);
      setSites(s);
      setStats(st);
      setError("");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load");
    }
  }, []);

  useEffect(() => {
    load();
    const id = setInterval(load, 30000);
    return () => clearInterval(id);
  }, [load]);

  async function remove(id: string) {
    if (!confirm("Delete this monitored site?")) return;
    await api.deleteSite(id);
    load();
  }

  async function togglePause(site: Site) {
    await api.setPause(site.id, !site.isPaused);
    load();
  }

  return (
    <AuthGuard>
      <main className="container main-pad">
        <h1 className="page-title">Dashboard</h1>
        <p className="page-sub">
          Welcome{user ? `, ${user.displayName}` : ""}. Auto-refresh 30s · checks
          every 60s.
        </p>

        {stats && (
          <div className="grid-stats">
            <StatCard label="Total sites" value={stats.totalSites} />
            <StatCard label="Online" value={stats.onlineSites} />
            <StatCard label="Offline" value={stats.offlineSites} />
            <StatCard label="Paused" value={stats.pausedSites} />
            <StatCard
              label="Uptime (24h)"
              value={
                stats.overallUptimePercent24h != null
                  ? stats.overallUptimePercent24h
                  : "—"
              }
              suffix={
                stats.overallUptimePercent24h != null ? "%" : undefined
              }
            />
            <StatCard
              label="Avg response"
              value={
                stats.averageResponseTimeMs != null
                  ? Math.round(stats.averageResponseTimeMs)
                  : "—"
              }
              suffix={
                stats.averageResponseTimeMs != null ? "ms" : undefined
              }
            />
          </div>
        )}

        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            marginBottom: "1rem",
            flexWrap: "wrap",
            gap: "0.75rem",
          }}
        >
          <h2>Monitored sites</h2>
          <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap" }}>
            <Link href="/settings" className="btn btn-ghost">
              Alerts
            </Link>
            <Link href="/sites/new" className="btn btn-primary">
              Add site
            </Link>
          </div>
        </div>

        {error && <p style={{ color: "var(--danger)" }}>{error}</p>}

        {sites.length === 0 ? (
          <div className="card">
            <p style={{ color: "var(--text-muted)" }}>
              No sites yet.{" "}
              <Link href="/sites/new">Add your first URL</Link>.
            </p>
          </div>
        ) : (
          <div className="card table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Site</th>
                  <th>Status</th>
                  <th>24h uptime</th>
                  <th>HTTP</th>
                  <th>Response</th>
                  <th>Last check</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {sites.map((site) => (
                  <tr key={site.id}>
                    <td>
                      <Link href={`/sites/${site.id}`}>
                        {site.name || site.url}
                      </Link>
                      {site.name && (
                        <div
                          style={{
                            fontSize: "0.8rem",
                            color: "var(--text-muted)",
                          }}
                        >
                          {site.url}
                        </div>
                      )}
                      {site.lastErrorMessage && site.status === "DOWN" && (
                        <div
                          style={{
                            fontSize: "0.75rem",
                            color: "var(--danger)",
                            marginTop: "0.25rem",
                            maxWidth: 220,
                          }}
                          title={site.lastErrorMessage}
                        >
                          {site.lastErrorMessage.length > 60
                            ? `${site.lastErrorMessage.slice(0, 60)}…`
                            : site.lastErrorMessage}
                        </div>
                      )}
                    </td>
                    <td>
                      <StatusBadge status={site.status} />
                    </td>
                    <td>
                      {site.uptimePercent24h != null
                        ? `${site.uptimePercent24h}%`
                        : "—"}
                    </td>
                    <td>{site.httpStatusCode ?? "—"}</td>
                    <td>
                      {site.responseTimeMs != null
                        ? `${site.responseTimeMs} ms`
                        : "—"}
                    </td>
                    <td>
                      {site.lastCheckedAt
                        ? new Date(site.lastCheckedAt).toLocaleString()
                        : "Pending"}
                    </td>
                    <td>
                      <div
                        style={{
                          display: "flex",
                          gap: "0.35rem",
                          flexWrap: "wrap",
                        }}
                      >
                        <button
                          type="button"
                          className="btn btn-ghost"
                          style={{
                            padding: "0.35rem 0.6rem",
                            fontSize: "0.75rem",
                          }}
                          onClick={() => togglePause(site)}
                        >
                          {site.isPaused ? "Resume" : "Pause"}
                        </button>
                        <button
                          type="button"
                          className="btn btn-danger"
                          style={{
                            padding: "0.35rem 0.6rem",
                            fontSize: "0.75rem",
                          }}
                          onClick={() => remove(site.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <div
          className="card"
          style={{ marginTop: "1.5rem", display: "flex", flexWrap: "wrap", gap: "1rem", alignItems: "center" }}
        >
          <div>
            <strong>Observability</strong>
            <p style={{ color: "var(--text-muted)", fontSize: "0.85rem", marginTop: "0.25rem" }}>
              Prometheus/Grafana metrics + Elastic APM in Kibana (latency, throughput, errors).
            </p>
          </div>
          <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap" }}>
            <a href="http://localhost:5601/app/apm/services" target="_blank" rel="noreferrer" className="btn btn-primary">
              Kibana APM
            </a>
            <a href="http://localhost:3002" target="_blank" rel="noreferrer" className="btn btn-ghost">
              Grafana
            </a>
            <a href="http://localhost:9090" target="_blank" rel="noreferrer" className="btn btn-ghost">
              Prometheus
            </a>
          </div>
        </div>

        {user && (
          <p style={{ marginTop: "1rem", color: "var(--text-muted)" }}>
            Public status:{" "}
            <Link href={`/status/${user.userId}`}>
              /status/{user.userId}
            </Link>
          </p>
        )}
      </main>
    </AuthGuard>
  );
}
