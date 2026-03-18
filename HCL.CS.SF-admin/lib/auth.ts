import { createHash } from "crypto";

import { getServerSession, type NextAuthOptions } from "next-auth";
import { type JWT } from "next-auth/jwt";
import { cache } from "react";

import { SESSION_ERROR_INVALIDATED, SESSION_ERROR_REFRESH_FAILED, isHardSessionError } from "@/lib/auth-errors";
import { assertAuthEnv, env } from "@/lib/env";
import { resolveTokenEndpoint } from "@/lib/oidc";
import { HCLCSSFFetch } from "@/lib/server-fetch";

/* ── Helpers ────────────────────────────────────────────────────────────── */

function encodeBasicAuth(clientId: string, clientSecret: string): string {
  const escapedClientId = encodeURIComponent(clientId);
  const escapedClientSecret = encodeURIComponent(clientSecret);
  return Buffer.from(`${escapedClientId}:${escapedClientSecret}`).toString("base64");
}

function decodeJwtPayload(token?: string): Record<string, unknown> {
  if (!token) return {};
  const sections = token.split(".");
  if (sections.length < 2) return {};
  try {
    const payload = sections[1].replace(/-/g, "+").replace(/_/g, "/");
    const json = Buffer.from(payload, "base64").toString("utf8");
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return {};
  }
}

function coerceRoleValues(value: unknown): string[] {
  if (Array.isArray(value)) return value.map((entry) => String(entry)).filter(Boolean);
  if (typeof value === "string") return value.split(/[\s,]+/).map((e) => e.trim()).filter(Boolean);
  return [];
}

export function extractRolesFromToken(accessToken?: string): string[] {
  const payload = decodeJwtPayload(accessToken);
  const roleCandidates = [
    payload.role,
    payload.roles,
    payload.userrole,
    payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
  ];
  const roles = roleCandidates.flatMap(coerceRoleValues);
  return [...new Set(roles.map((r) => r.trim()).filter(Boolean))];
}

export function hasAdminRole(roles: string[]): boolean {
  return roles.some((role) => role.toLowerCase().includes("admin"));
}

/* ── Token types ────────────────────────────────────────────────────────── */

type TokenEndpointResponse = {
  access_token?: string;
  refresh_token?: string;
  id_token?: string;
  expires_in?: number;
  scope?: string;
  error?: string;
  error_description?: string;
};

type SafeTokenMetadata = {
  sub?: string;
  exp?: number;
  expHuman?: string;
  aud?: unknown;
  iss?: unknown;
};

/* ── Token refresh infrastructure ───────────────────────────────────────── */

const ACCESS_TOKEN_REFRESH_BUFFER_MS = 60_000;
const refreshFlights = new Map<string, Promise<JWT>>();

function createTokenFingerprint(value?: string): string | undefined {
  if (!value) return undefined;
  return createHash("sha256").update(value).digest("hex").slice(0, 12);
}

function readStringClaim(value: unknown): string | undefined {
  if (typeof value !== "string") return undefined;
  const trimmed = value.trim();
  return trimmed || undefined;
}

function readNumericClaim(value: unknown): number | undefined {
  if (typeof value === "number" && Number.isFinite(value)) return value;
  if (typeof value === "string") {
    const parsed = Number(value);
    if (Number.isFinite(parsed)) return parsed;
  }
  return undefined;
}

function extractSafeTokenMetadata(accessToken?: string, fallbackSub?: string): SafeTokenMetadata {
  const payload = decodeJwtPayload(accessToken);
  const exp = readNumericClaim(payload.exp);
  return {
    sub: readStringClaim(payload.sub) ?? fallbackSub,
    exp,
    expHuman: exp ? new Date(exp * 1000).toISOString() : undefined,
    aud: payload.aud,
    iss: payload.iss
  };
}

function describeJwtToken(token: JWT): SafeTokenMetadata {
  return extractSafeTokenMetadata(token.accessToken, readStringClaim(token.sub));
}

function logRefreshEvent(
  level: "info" | "warn",
  event: string,
  token: Pick<JWT, "accessToken" | "refreshToken" | "sub">,
  extra: Record<string, unknown> = {}
): void {
  const metadata = describeJwtToken(token as JWT);
  const logger = level === "warn" ? console.warn : console.info;
  logger(`[HCL.CS.SF-auth] ${event}.`, {
    sub: metadata.sub ?? "unknown",
    exp_human: metadata.expHuman ?? "unknown",
    aud: metadata.aud,
    iss: metadata.iss,
    refresh_key: createTokenFingerprint(token.refreshToken),
    ...extra
  });
}

function buildSessionInvalidatedToken(token: JWT, errorCode: string): JWT {
  return {
    ...token,
    name: undefined,
    email: undefined,
    picture: undefined,
    sub: undefined,
    accessToken: undefined,
    refreshToken: undefined,
    idToken: undefined,
    accessTokenExpires: undefined,
    scopes: undefined,
    roles: [],
    isAdmin: false,
    error: errorCode
  };
}

function buildRefreshedToken(token: JWT, refreshed: TokenEndpointResponse): JWT {
  if (!refreshed.access_token || !refreshed.refresh_token) {
    return buildSessionInvalidatedToken(token, SESSION_ERROR_REFRESH_FAILED);
  }
  const expiresInSeconds = Number(refreshed.expires_in ?? 300);
  const roles = extractRolesFromToken(refreshed.access_token);
  return {
    ...token,
    accessToken: refreshed.access_token,
    refreshToken: refreshed.refresh_token,
    idToken: refreshed.id_token ?? token.idToken,
    accessTokenExpires: Date.now() + expiresInSeconds * 1000,
    scopes: refreshed.scope ?? token.scopes,
    roles,
    isAdmin: hasAdminRole(roles),
    error: undefined
  };
}

function shouldUseCurrentAccessToken(token: JWT): boolean {
  if (!token.accessToken || !token.accessTokenExpires) return false;
  return Date.now() < token.accessTokenExpires - ACCESS_TOKEN_REFRESH_BUFFER_MS;
}

function shouldInvalidateSession(errorCode?: string): boolean {
  return isHardSessionError(errorCode);
}

/* ── Refresh token grant ────────────────────────────────────────────────── */

async function refreshAccessToken(token: JWT): Promise<JWT> {
  if (!token.refreshToken) {
    logRefreshEvent("warn", "refresh failed", token, { reason: "missing_refresh_token" });
    return buildSessionInvalidatedToken(token, SESSION_ERROR_INVALIDATED);
  }

  const refreshKey = createTokenFingerprint(token.refreshToken);
  if (!refreshKey) {
    logRefreshEvent("warn", "refresh failed", token, { reason: "refresh_key_unavailable" });
    return buildSessionInvalidatedToken(token, SESSION_ERROR_INVALIDATED);
  }

  const existingRefresh = refreshFlights.get(refreshKey);
  if (existingRefresh) {
    logRefreshEvent("info", "refresh joined existing in-flight promise", token);
    return existingRefresh;
  }

  logRefreshEvent("info", "refresh started", token);

  const refreshPromise = (async () => {
    try {
      const tokenEndpoint = await resolveTokenEndpoint();
      const payload = new URLSearchParams({
        grant_type: "refresh_token",
        refresh_token: token.refreshToken!
      });

      const response = await HCLCSSFFetch(tokenEndpoint, {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          Authorization: `Basic ${encodeBasicAuth(env.clientId, env.clientSecret)}`
        },
        body: payload.toString(),
        cache: "no-store"
      });

      let refreshed: TokenEndpointResponse = {};
      try {
        refreshed = (await response.json()) as TokenEndpointResponse;
      } catch {
        refreshed = {};
      }

      if (!response.ok || !refreshed.access_token || !refreshed.refresh_token) {
        const reason =
          typeof refreshed.error_description === "string" && refreshed.error_description.trim()
            ? refreshed.error_description.trim()
            : refreshed.error ?? "RefreshAccessTokenError";
        logRefreshEvent("warn", "refresh failed", token, { reason, httpStatus: response.status });
        return buildSessionInvalidatedToken(token, SESSION_ERROR_REFRESH_FAILED);
      }

      const updatedToken = buildRefreshedToken(token, refreshed);
      logRefreshEvent("info", "refresh succeeded", updatedToken, { previous_refresh_key: refreshKey });
      return updatedToken;
    } catch {
      logRefreshEvent("warn", "refresh failed", token, { reason: "network_or_runtime_error" });
      return buildSessionInvalidatedToken(token, SESSION_ERROR_REFRESH_FAILED);
    }
  })().finally(() => {
    refreshFlights.delete(refreshKey);
  });

  refreshFlights.set(refreshKey, refreshPromise);
  return refreshPromise;
}

/* ══════════════════════════════════════════════════════════════════════════
   NextAuth Configuration — Authorization Code Flow with PKCE
   ══════════════════════════════════════════════════════════════════════════ */

assertAuthEnv();

export const authOptions: NextAuthOptions = {
  debug: process.env.NODE_ENV !== "production",
  session: {
    strategy: "jwt"
  },
  logger: {
    error(code, metadata) {
      console.error(`[HCL.CS.SF-auth][error] ${code}`, metadata);
    },
    warn(code) {
      console.warn(`[HCL.CS.SF-auth][warn] ${code}`);
    },
    debug(code, metadata) {
      console.debug(`[HCL.CS.SF-auth][debug] ${code}`, metadata);
    }
  },
  providers: [
    {
      id: "hclcssf",
      name: "HCL.CS.SF Identity",
      type: "oauth",
      /**
       * OIDC Discovery — NextAuth fetches /.well-known/openid-configuration
       * to resolve authorization_endpoint, token_endpoint, userinfo_endpoint,
       * jwks_uri, etc. This replaces all manual endpoint wiring.
       */
      wellKnown: env.metadataAddress,
      clientId: env.clientId,
      clientSecret: env.clientSecret,
      authorization: {
        params: {
          scope: env.scopes,
          response_type: "code",
          prompt: "login" // Force fresh auth — don't reuse stale IdP session
        }
      },
      /**
       * PKCE (S256) + state + nonce check.
       * The code_verifier is generated by NextAuth, stored in an httpOnly cookie,
       * and sent alongside the authorization_code in the token exchange.
       * No secrets ever reach the browser.
       */
      checks: ["pkce", "state", "nonce"],
      idToken: true,
      client: {
        token_endpoint_auth_method: "client_secret_basic"
      },
      profile(profile) {
        return {
          id: profile.sub ?? profile.user_id ?? "unknown",
          name:
            profile.name ??
            profile.preferred_username ??
            profile.username ??
            profile.sub ??
            "User",
          email: profile.email ?? null
        };
      }
    }
  ],
  callbacks: {
    async redirect({ url, baseUrl }) {
      const configuredBaseUrl = env.nextAuthUrl.replace(/\/+$/, "");
      const effectiveBaseUrl = configuredBaseUrl || baseUrl.replace(/\/+$/, "");

      if (url.startsWith("/") && !url.startsWith("//")) {
        return `${effectiveBaseUrl}${url}`;
      }

      try {
        const parsed = new URL(url);
        if (parsed.origin === effectiveBaseUrl) {
          return parsed.toString();
        }
      } catch {
        // Fall back to default.
      }

      return `${effectiveBaseUrl}/admin`;
    },

    async jwt({ token, account, profile }) {
      // ── Initial sign-in: store tokens from the authorization code exchange ──
      if (account?.provider === "hclcssf" && account.access_token) {
        const accessToken = account.access_token;
        const roles = extractRolesFromToken(accessToken);
        const expiresAt = account.expires_at
          ? account.expires_at * 1000 // NextAuth provides seconds since epoch
          : Date.now() + (account.expires_in ? Number(account.expires_in) * 1000 : 300_000);

        console.info("[HCL.CS.SF-auth] OIDC authorization code exchange successful.", {
          sub: token.sub,
          provider: account.provider,
          hasRefreshToken: !!account.refresh_token,
          roles
        });

        return {
          ...token,
          name: profile?.name ?? token.name,
          email: profile?.email ?? token.email,
          accessToken,
          refreshToken: account.refresh_token,
          idToken: account.id_token,
          accessTokenExpires: expiresAt,
          scopes: account.scope ?? env.scopes,
          roles,
          isAdmin: hasAdminRole(roles),
          error: undefined
        };
      }

      // ── Session already invalidated ──
      if (shouldInvalidateSession(token.error)) {
        return buildSessionInvalidatedToken(token, token.error ?? SESSION_ERROR_INVALIDATED);
      }

      // ── Access token still valid ──
      if (shouldUseCurrentAccessToken(token)) {
        return token;
      }

      // ── Refresh ──
      return refreshAccessToken(token);
    },

    async session({ session, token }) {
      if (!token.accessToken || shouldInvalidateSession(token.error)) {
        logRefreshEvent("warn", "stale token blocked after refresh failure", token, {
          reason: token.error ?? SESSION_ERROR_INVALIDATED
        });
        return null as never;
      }

      // ╔══════════════════════════════════════════════════════════════════════╗
      // ║  SECURITY: Tokens stay server-side only.                            ║
      // ║  The client session only receives identity + role claims.            ║
      // ║  access_token, refresh_token, id_token are NEVER sent to browser.   ║
      // ╚══════════════════════════════════════════════════════════════════════╝
      session.roles = token.roles;
      session.isAdmin = Boolean(token.isAdmin);
      session.error = token.error;

      return session;
    }
  },
  pages: {
    signIn: "/login"
  }
};

// Reuse one resolved session per request/render tree so parallel loaders do not
// independently trigger the refresh path.
const getRequestScopedSession = cache(async () => getServerSession(authOptions));

export async function auth() {
  return getRequestScopedSession();
}
