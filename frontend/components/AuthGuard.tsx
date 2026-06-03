"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useAuth } from "@/lib/useAuth";

/** Redirects to /login if no JWT in localStorage. */
export function AuthGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { loggedIn } = useAuth();
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (!loggedIn) {
      router.replace("/login");
    } else {
      setReady(true);
    }
  }, [loggedIn, router]);

  if (!loggedIn || !ready) {
    return (
      <div className="container main-pad">
        <p>Loading…</p>
      </div>
    );
  }

  return <>{children}</>;
}
