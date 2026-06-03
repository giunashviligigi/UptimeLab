"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { AuthGuard } from "@/components/AuthGuard";
import { api } from "@/lib/api";

export default function AddSitePage() {
  const router = useRouter();
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");
    setLoading(true);
    const form = new FormData(e.currentTarget);
    try {
      const site = await api.createSite(
        form.get("url") as string,
        (form.get("name") as string) || undefined
      );
      router.push(`/sites/${site.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to add site");
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthGuard>
      <main className="container main-pad" style={{ maxWidth: 520 }}>
        <h1 className="page-title">Add monitored site</h1>
        <p className="page-sub">
          Enter a full URL (https://…). Checks start within 60 seconds.
        </p>
        <form className="card form-stack" onSubmit={onSubmit}>
          <div>
            <label className="label" htmlFor="url">
              Website URL
            </label>
            <input
              className="input"
              id="url"
              name="url"
              type="url"
              placeholder="https://example.com"
              required
            />
          </div>
          <div>
            <label className="label" htmlFor="name">
              Friendly name (optional)
            </label>
            <input
              className="input"
              id="name"
              name="name"
              placeholder="My API"
            />
          </div>
          {error && <p style={{ color: "var(--danger)" }}>{error}</p>}
          <div style={{ display: "flex", gap: "0.75rem" }}>
            <button className="btn btn-primary" type="submit" disabled={loading}>
              {loading ? "Saving…" : "Add site"}
            </button>
            <Link href="/dashboard" className="btn btn-ghost">
              Cancel
            </Link>
          </div>
        </form>
      </main>
    </AuthGuard>
  );
}
