"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { useAuth } from "@/lib/useAuth";

/** Redirects logged-in users away from login/register pages. */
export function GuestGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { loggedIn } = useAuth();

  useEffect(() => {
    if (loggedIn) {
      router.replace("/dashboard");
    }
  }, [loggedIn, router]);

  if (loggedIn) {
    return (
      <div className="container main-pad">
        <p>Already signed in. Redirecting…</p>
      </div>
    );
  }

  return <>{children}</>;
}
