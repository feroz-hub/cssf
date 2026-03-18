import "next-auth";
import "next-auth/jwt";

declare module "next-auth" {
  interface Session {
    /** Roles extracted from the access token. Available client-side for UI rendering. */
    roles?: string[];
    /** Whether the user has an admin role. Available client-side for UI guards. */
    isAdmin?: boolean;
    /** Session error code (e.g. refresh failure). Available client-side for error handling. */
    error?: string;
    // SECURITY: accessToken, refreshToken, idToken are intentionally NOT on Session.
    // They live exclusively in the encrypted server-side JWT and are never sent to the browser.
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    /** OAuth access token — server-side only, never exposed to client. */
    accessToken?: string;
    /** OAuth ID token — server-side only, never exposed to client. */
    idToken?: string;
    /** OAuth refresh token — server-side only, never exposed to client. */
    refreshToken?: string;
    /** Timestamp (ms) when the access token expires. */
    accessTokenExpires?: number;
    /** Space-separated scopes granted by the token endpoint. */
    scopes?: string;
    /** Role claims extracted from the access token. */
    roles?: string[];
    /** Whether the user has an admin role. */
    isAdmin?: boolean;
    /** Session error code. */
    error?: string;
  }
}
