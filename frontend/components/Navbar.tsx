"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { clearAuth } from "@/lib/auth";
import { useAuth } from "@/lib/useAuth";
import { useTheme } from "./ThemeProvider";

export function Navbar() {
  const pathname = usePathname();
  const router = useRouter();
  const { toggle, theme } = useTheme();
  const { loggedIn, user } = useAuth();

  const logout = () => {
    clearAuth();
    router.push("/login");
  };

  return (
    <nav className="nav container">
      <Link href={loggedIn ? "/dashboard" : "/"} className="nav-brand">
        UptimeLab
      </Link>
      <div className="nav-links">
        <button type="button" className="btn btn-ghost" onClick={toggle}>
          {theme === "dark" ? "Light" : "Dark"} mode
        </button>
        {loggedIn ? (
          <>
            <Link href="/dashboard">Dashboard</Link>
            <Link href="/settings">Alerts</Link>
            <Link href="/sites/new">Add site</Link>
            {user && (
              <Link href={`/status/${user.userId}`}>Public status</Link>
            )}
            <button type="button" className="btn btn-ghost" onClick={logout}>
              Logout
            </button>
          </>
        ) : (
          pathname !== "/login" &&
          pathname !== "/register" && (
            <>
              <Link href="/login">Login</Link>
              <Link href="/register" className="btn btn-primary">
                Register
              </Link>
            </>
          )
        )}
      </div>
    </nav>
  );
}
