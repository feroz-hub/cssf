"use server";

import { signOut as HCLCSSFSignOut } from "@/lib/api/authentication";
import { revokeToken } from "@/lib/api/revocation";
import { getServerJwt } from "@/lib/server-jwt";

/**
 * Performs server-side logout with HCL.CS.SF: calls SignOut API and revokes the
 * current access and refresh tokens so they cannot be reused.
 *
 * Reads tokens directly from the encrypted JWT cookie (server-side only).
 * Errors are swallowed so the client can always clear the session and redirect.
 */
export async function logoutAction(): Promise<void> {
  const jwt = await getServerJwt();

  const accessToken = typeof jwt?.accessToken === "string" ? jwt.accessToken : null;
  const refreshToken = typeof jwt?.refreshToken === "string" ? jwt.refreshToken : null;

  if (!accessToken) return;

  try {
    await HCLCSSFSignOut();
  } catch {
    // Continue; client will still clear session.
  }

  try {
    await revokeToken(accessToken, "access_token");
  } catch {
    // Continue.
  }

  if (refreshToken) {
    try {
      await revokeToken(refreshToken, "refresh_token");
    } catch {
      // Continue.
    }
  }
}
