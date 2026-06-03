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

  return (
    <AuthGuard>
      <main className="container main-pad">
        <h1 className="page-title">Dashboard</h1>
        <p className="page-sub">
          Welcome{user ? `, ${user.displayName}` : ""}. Sites refresh every 30s;
          backend checks every 60s.
        </p>

        {stats && (
          <div className="grid-stats">
            <StatCard label="Total sites" value={stats.totalSites} />
            <StatCard label="Online" value={stats.onlineSites} />
            <StatCard label="Offline" value={stats.offlineSites} />
            <StatCard
              label="Avg response time"
              value={
                stats.averageResponseTimeMs != null
                  ? Math.round(stats.averageResponseTimeMs)
                  : "—"
              }
              suffix={stats.averageResponseTimeMs != null ? "ms" : undefined}
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
          <Link href="/sites/new" className="btn btn-primary">
            Add site
          </Link>
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
                  <th>Name / URL</th>
                  <th>Status</th>
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
                    </td>
                    <td>
                      <StatusBadge status={site.status} />
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
                      <button
                        type="button"
                        className="btn btn-danger"
                        style={{ padding: "0.35rem 0.75rem", fontSize: "0.8rem" }}
                        onClick={() => remove(site.id)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {user && (
          <p style={{ marginTop: "1.5rem", color: "var(--text-muted)" }}>
            Public status page:{" "}
            <Link href={`/status/${user.userId}`}>
              /status/{user.userId}
            </Link>
          </p>
        )}
      </main>
    </AuthGuard>
  );
}
