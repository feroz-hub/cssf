"use client";

import { signOut } from "next-auth/react";
import { useState } from "react";

import { logoutAction } from "@/app/actions/logout";
import { Button } from "@/components/ui/button";

/**
 * Button that clears the current session (including expired) and sends the user to login.
 * Use this when the session is expired or invalid so that a plain link to /login does not
 * redirect back (NextAuth still has "authenticated" with stale session).
 */
export function SignInAgainButton() {
  const [busy, setBusy] = useState(false);

  const handleClick = async () => {
    if (busy) return;
    setBusy(true);

    try {
      await logoutAction();
    } catch {
      // Session may already be invalid; still clear client session.
    } finally {
      await signOut({ redirect: false });
      // Full navigation so login page gets a clean state and no stale "authenticated" session.
      window.location.href = "/login";
    }
  };

  return (
    <Button type="button" variant="primary" onClick={handleClick} disabled={busy}>
      {busy ? "Signing out…" : "Sign in again"}
    </Button>
  );
}
