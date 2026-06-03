"use client";

import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { StatusBadge } from "@/components/StatusBadge";
import { api } from "@/lib/api";
import type { PublicStatus } from "@/lib/types";

/** Public status page — no login required. */
export default function PublicStatusPage() {
  const params = useParams();
  const userId = params.userId as string;
  const [data, setData] = useState<PublicStatus | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .getPublicStatus(userId)
      .then(setData)
      .catch((e) =>
        setError(e instanceof Error ? e.message : "Status page not found")
      );
  }, [userId]);

  return (
    <main className="container main-pad">
      <h1 className="page-title">Public status</h1>
      {data ? (
        <p className="page-sub">{data.displayName}&apos;s monitored services</p>
      ) : (
        <p className="page-sub">Loading…</p>
      )}

      {error && <p style={{ color: "var(--danger)" }}>{error}</p>}

      {data && data.sites.length === 0 && (
        <div className="card">
          <p style={{ color: "var(--text-muted)" }}>No public sites to show.</p>
        </div>
      )}

      {data && data.sites.length > 0 && (
        <div className="card table-wrap">
          <table>
            <thead>
              <tr>
                <th>Service</th>
                <th>Status</th>
                <th>HTTP</th>
                <th>Response</th>
                <th>Last check</th>
              </tr>
            </thead>
            <tbody>
              {data.sites.map((site) => (
                <tr key={site.url}>
                  <td>
                    {site.name || site.url}
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
                      : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <p style={{ marginTop: "2rem", color: "var(--text-muted)", fontSize: "0.85rem" }}>
        Powered by UptimeLab
      </p>
    </main>
  );
}
