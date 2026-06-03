"use client";

import Link from "next/link";
import { useAuth } from "@/lib/useAuth";

export default function LandingPage() {
  const { loggedIn, user } = useAuth();

  return (
    <main>
      <section className="hero container">
        {!loggedIn && (
          <p
            style={{
              display: "inline-block",
              marginBottom: "1rem",
              padding: "0.35rem 0.85rem",
              borderRadius: "999px",
              background: "var(--bg-elevated)",
              color: "var(--accent)",
              fontSize: "0.85rem",
              fontWeight: 600,
            }}
          >
            Self-hosted uptime monitoring
          </p>
        )}
        {loggedIn && user && (
          <p
            style={{
              display: "inline-block",
              marginBottom: "1rem",
              padding: "0.35rem 0.85rem",
              borderRadius: "999px",
              background: "var(--bg-elevated)",
              color: "var(--success)",
              fontSize: "0.85rem",
              fontWeight: 600,
            }}
          >
            Signed in as {user.displayName}
          </p>
        )}
        <h1>Monitor your websites with UptimeLab</h1>
        <p>
          Self-hosted uptime monitoring built for DevOps learning. Add URLs and
          get checks every 60 seconds with history, alerts, and a public status
          page.
        </p>
        <div
          style={{
            display: "flex",
            gap: "1rem",
            justifyContent: "center",
            flexWrap: "wrap",
          }}
        >
          {loggedIn ? (
            <>
              <Link href="/dashboard" className="btn btn-primary">
                Go to dashboard
              </Link>
              <Link href="/sites/new" className="btn btn-ghost">
                Add a site
              </Link>
            </>
          ) : (
            <>
              <Link href="/register" className="btn btn-primary">
                Get started
              </Link>
              <Link href="/login" className="btn btn-ghost">
                Sign in
              </Link>
            </>
          )}
        </div>
      </section>

      <section className="container main-pad">
        <div className="grid-stats">
          <div className="card">
            <h3 style={{ marginBottom: "0.5rem" }}>JWT auth</h3>
            <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
              Secure registration and login with token-based API access.
            </p>
          </div>
          <div className="card">
            <h3 style={{ marginBottom: "0.5rem" }}>60s checks</h3>
            <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
              Background worker records status, HTTP code, and response time.
            </p>
          </div>
          <div className="card">
            <h3 style={{ marginBottom: "0.5rem" }}>Webhook alerts</h3>
            <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
              Slack-compatible notifications when sites go down or recover.
            </p>
          </div>
        </div>
      </section>
    </main>
  );
}
