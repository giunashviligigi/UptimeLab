"use client";

import { useSyncExternalStore } from "react";
import {
  AUTH_CHANGE_EVENT,
  getToken,
  getUser,
  type StoredUser,
} from "./auth";

function subscribe(onChange: () => void) {
  const handler = () => onChange();
  window.addEventListener(AUTH_CHANGE_EVENT, handler);
  window.addEventListener("storage", handler);
  return () => {
    window.removeEventListener(AUTH_CHANGE_EVENT, handler);
    window.removeEventListener("storage", handler);
  };
}

function getLoginSnapshot() {
  return !!getToken();
}

function getServerSnapshot() {
  return false;
}

export function useAuth(): { loggedIn: boolean; user: StoredUser | null } {
  const loggedIn = useSyncExternalStore(
    subscribe,
    getLoginSnapshot,
    getServerSnapshot
  );
  const user = loggedIn ? getUser() : null;
  return { loggedIn, user };
}
