"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { AuthGuard } from "@/components/AuthGuard";
import { api } from "@/lib/api";

export default function SettingsPage() {
  const [webhookUrl, setWebhookUrl] = useState("");
  const [alertsEnabled, setAlertsEnabled] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .getSettings()
      .then((s) => {
        setWebhookUrl(s.webhookUrl ?? "");
        setAlertsEnabled(s.webhookAlertsEnabled);
      })
      .catch((e) => setError(e instanceof Error ? e.message : "Failed to load"))
      .finally(() => setLoading(false));
  }, []);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError("");
    setMessage("");
    try {
      await api.updateSettings(
        webhookUrl.trim() || null,
        alertsEnabled
      );
      setMessage("Settings saved. Alerts fire when a site goes DOWN or recovers to UP.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Save failed");
    } finally {
      setSaving(false);
    }
  }

  return (
    <AuthGuard>
      <main className="container main-pad" style={{ maxWidth: 560 }}>
        <Link href="/dashboard" style={{ fontSize: "0.9rem" }}>
          ← Dashboard
        </Link>
        <h1 className="page-title" style={{ marginTop: "1rem" }}>
          Alert settings
        </h1>
        <p className="page-sub">
          Send a JSON POST to your webhook when any site goes down or comes back up
          (works with Slack incoming webhooks, Discord, or custom URLs).
        </p>

        {loading ? (
          <p>Loading…</p>
        ) : (
          <form className="card form-stack" onSubmit={onSubmit}>
            <div>
              <label className="label" htmlFor="webhook">
                Webhook URL
              </label>
              <input
                className="input"
                id="webhook"
                type="url"
                placeholder="https://hooks.slack.com/services/..."
                value={webhookUrl}
                onChange={(e) => setWebhookUrl(e.target.value)}
              />
            </div>
            <label style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
              <input
                type="checkbox"
                checked={alertsEnabled}
                onChange={(e) => setAlertsEnabled(e.target.checked)}
              />
              Enable webhook alerts
            </label>
            {error && <p style={{ color: "var(--danger)" }}>{error}</p>}
            {message && <p style={{ color: "var(--success)" }}>{message}</p>}
            <button className="btn btn-primary" type="submit" disabled={saving}>
              {saving ? "Saving…" : "Save settings"}
            </button>
          </form>
        )}

        <div className="card" style={{ marginTop: "1.5rem" }}>
          <h3 style={{ marginBottom: "0.5rem" }}>Payload example</h3>
          <pre
            style={{
              fontSize: "0.8rem",
              overflow: "auto",
              color: "var(--text-muted)",
            }}
          >
{`{
  "text": "UptimeLab: My API is DOWN",
  "event": "down",
  "status": "DOWN",
  "site": { "url": "https://..." }
}`}
          </pre>
        </div>
      </main>
    </AuthGuard>
  );
}
