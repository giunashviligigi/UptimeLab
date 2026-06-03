"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AuthGuard } from "@/components/AuthGuard";
import { StatusBadge } from "@/components/StatusBadge";
import { api } from "@/lib/api";
import type { SiteHistory } from "@/lib/types";

export default function SiteDetailPage() {
  const params = useParams();
  const id = params.id as string;
  const [data, setData] = useState<SiteHistory | null>(null);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    try {
      const h = await api.getHistory(id, 100);
      setData(h);
      setError("");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load history");
    }
  }, [id]);

  useEffect(() => {
    load();
    const timer = setInterval(load, 30000);
    return () => clearInterval(timer);
  }, [load]);

  async function togglePause() {
    if (!data) return;
    await api.setPause(id, !data.isPaused);
    load();
  }

  const latest = data?.history[0];
  const displayStatus = data?.isPaused ? "PAUSED" : latest?.status ?? "UNKNOWN";

  return (
    <AuthGuard>
      <main className="container main-pad">
        <Link href="/dashboard" style={{ fontSize: "0.9rem" }}>
          ← Back to dashboard
        </Link>
        <h1 className="page-title" style={{ marginTop: "1rem" }}>
          {data?.name || data?.url || "Site details"}
        </h1>
        {data?.name && <p className="page-sub">{data.url}</p>}

        {error && <p style={{ color: "var(--danger)" }}>{error}</p>}

        {data && (
          <div className="grid-stats" style={{ marginTop: "1rem" }}>
            <div className="card">
              <div className="stat-label">Status</div>
              <div style={{ marginTop: "0.5rem" }}>
                <StatusBadge status={displayStatus} />
              </div>
            </div>
            <div className="card">
              <div className="stat-label">Uptime 24h</div>
              <div className="stat-value" style={{ fontSize: "1.5rem" }}>
                {data.uptimePercent24h != null
                  ? `${data.uptimePercent24h}%`
                  : "—"}
              </div>
            </div>
            <div className="card">
              <div className="stat-label">Uptime 7d</div>
              <div className="stat-value" style={{ fontSize: "1.5rem" }}>
                {data.uptimePercent7d != null
                  ? `${data.uptimePercent7d}%`
                  : "—"}
              </div>
            </div>
            {latest && (
              <>
                <div className="card">
                  <div className="stat-label">HTTP</div>
                  <div className="stat-value" style={{ fontSize: "1.5rem" }}>
                    {latest.httpStatusCode ?? "—"}
                  </div>
                </div>
                <div className="card">
                  <div className="stat-label">Response</div>
                  <div className="stat-value" style={{ fontSize: "1.5rem" }}>
                    {latest.responseTimeMs} ms
                  </div>
                </div>
              </>
            )}
          </div>
        )}

        {data && (
          <button
            type="button"
            className="btn btn-ghost"
            style={{ marginTop: "1rem" }}
            onClick={togglePause}
          >
            {data.isPaused ? "Resume monitoring" : "Pause monitoring"}
          </button>
        )}

        <h2 style={{ margin: "1.5rem 0 0.75rem" }}>Check history</h2>
        {!data?.history.length ? (
          <div className="card">
            <p style={{ color: "var(--text-muted)" }}>
              {data?.isPaused
                ? "Monitoring is paused."
                : "No checks yet. Wait up to 60 seconds."}
            </p>
          </div>
        ) : (
          <div className="card table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Time</th>
                  <th>Status</th>
                  <th>HTTP</th>
                  <th>Response</th>
                  <th>Error</th>
                </tr>
              </thead>
              <tbody>
                {data.history.map((row) => (
                  <tr key={row.id}>
                    <td>{new Date(row.checkedAt).toLocaleString()}</td>
                    <td>
                      <StatusBadge status={row.status} />
                    </td>
                    <td>{row.httpStatusCode ?? "—"}</td>
                    <td>{row.responseTimeMs} ms</td>
                    <td
                      style={{
                        fontSize: "0.85rem",
                        color: "var(--text-muted)",
                        maxWidth: 240,
                      }}
                    >
                      {row.errorMessage ?? "—"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>
    </AuthGuard>
  );
}
