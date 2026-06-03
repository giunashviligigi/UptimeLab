"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { api } from "@/lib/api";
import { saveAuth } from "@/lib/auth";

export default function RegisterPage() {
  const router = useRouter();
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");
    setLoading(true);
    const form = new FormData(e.currentTarget);
    try {
      const res = await api.register(
        form.get("email") as string,
        form.get("password") as string,
        form.get("displayName") as string
      );
      saveAuth(res);
      router.push("/dashboard");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="container main-pad" style={{ maxWidth: 420 }}>
      <h1 className="page-title">Create account</h1>
      <p className="page-sub">Start monitoring your websites.</p>
      <form className="card form-stack" onSubmit={onSubmit}>
        <div>
          <label className="label" htmlFor="displayName">
            Display name
          </label>
          <input
            className="input"
            id="displayName"
            name="displayName"
            required
            minLength={2}
          />
        </div>
        <div>
          <label className="label" htmlFor="email">
            Email
          </label>
          <input className="input" id="email" name="email" type="email" required />
        </div>
        <div>
          <label className="label" htmlFor="password">
            Password (min 6 chars)
          </label>
          <input
            className="input"
            id="password"
            name="password"
            type="password"
            required
            minLength={6}
          />
        </div>
        {error && <p style={{ color: "var(--danger)" }}>{error}</p>}
        <button className="btn btn-primary" type="submit" disabled={loading}>
          {loading ? "Creating…" : "Register"}
        </button>
      </form>
      <p style={{ marginTop: "1rem", color: "var(--text-muted)" }}>
        Already have an account? <Link href="/login">Sign in</Link>
      </p>
    </main>
  );
}
