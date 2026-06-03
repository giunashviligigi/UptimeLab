import Link from "next/link";

export default function LandingPage() {
  return (
    <main>
      <section className="hero container">
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
          DevOps lab — feature branch demo
        </p>
        <h1>Monitor your websites with UptimeLab</h1>
        <p>
          Self-hosted uptime monitoring built for DevOps learning. Register,
          add URLs, and get checks every 60 seconds with history and a public
          status page.
        </p>
        <div style={{ display: "flex", gap: "1rem", justifyContent: "center", flexWrap: "wrap" }}>
          <Link href="/register" className="btn btn-primary">
            Get started
          </Link>
          <Link href="/login" className="btn btn-ghost">
            Sign in
          </Link>
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
            <h3 style={{ marginBottom: "0.5rem" }}>Docker ready</h3>
            <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
              Compose stack with PostgreSQL, Nginx-ready, Prometheus metrics.
            </p>
          </div>
        </div>
      </section>
    </main>
  );
}
