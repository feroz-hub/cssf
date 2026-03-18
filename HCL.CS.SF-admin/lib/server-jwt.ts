import "server-only";

import { cookies } from "next/headers";
import { decode } from "next-auth/jwt";

import { env } from "@/lib/env";

/**
 * Reads the raw NextAuth JWT from the encrypted httpOnly session cookie.
 *
 * This runs ONLY on the server (enforced by "server-only" import).
 * It gives access to the full JWT including access_token and refresh_token,
 * which are intentionally stripped from the client-visible session.
 *
 * Use this for server-side API calls that need the access token.
 */
export async function getServerJwt(): Promise<Record<string, unknown> | null> {
  const cookieStore = await cookies();

  // NextAuth v4 cookie names: __Secure-next-auth.session-token (HTTPS)
  // or next-auth.session-token (HTTP/dev).
  // When the JWT exceeds ~4 KB NextAuth splits it into numbered chunks:
  //   __Secure-next-auth.session-token.0, .1, .2, …
  const secureCookie = cookieStore.get("__Secure-next-auth.session-token");
  const devCookie = cookieStore.get("next-auth.session-token");
  let tokenValue = secureCookie?.value ?? devCookie?.value;

  // Handle chunked cookies (large JWTs)
  if (!tokenValue) {
    const chunks: string[] = [];
    for (let i = 0; ; i++) {
      const chunk =
        cookieStore.get(`__Secure-next-auth.session-token.${i}`)?.value ??
        cookieStore.get(`next-auth.session-token.${i}`)?.value;
      if (!chunk) break;
      chunks.push(chunk);
    }
    if (chunks.length > 0) {
      tokenValue = chunks.join("");
    }
  }

  if (!tokenValue) {
    return null;
  }

  try {
    const decoded = await decode({
      token: tokenValue,
      secret: env.nextAuthSecret
    });
    return decoded as Record<string, unknown> | null;
  } catch {
    return null;
  }
}

/**
 * Server-only: retrieve the access token from the JWT cookie for API calls.
 * Returns undefined if no valid session exists.
 */
export async function getServerAccessToken(): Promise<string | undefined> {
  const jwt = await getServerJwt();
  if (!jwt) return undefined;

  const accessToken = jwt.accessToken;
  return typeof accessToken === "string" ? accessToken : undefined;
}
