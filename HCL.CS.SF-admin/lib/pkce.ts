/**
 * PKCE (RFC 7636) helpers for use in the browser.
 * Generates code_verifier and code_challenge (S256, base64url) for authorization_code flow.
 */

const CODE_VERIFIER_CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
const CODE_VERIFIER_LENGTH = 64; // 43-128 per RFC; 64 is a safe default

/**
 * Generate a random code_verifier (43-128 chars from RFC 7636 allowed set).
 */
function randomCodeVerifier(): string {
  if (typeof crypto !== "undefined" && crypto.getRandomValues) {
    const array = new Uint8Array(CODE_VERIFIER_LENGTH);
    crypto.getRandomValues(array);
    return Array.from(array, (b) => CODE_VERIFIER_CHARSET[b % CODE_VERIFIER_CHARSET.length]).join("");
  }
  // Fallback for very old environments (not recommended)
  let s = "";
  for (let i = 0; i < CODE_VERIFIER_LENGTH; i++) {
    s += CODE_VERIFIER_CHARSET[Math.floor(Math.random() * CODE_VERIFIER_CHARSET.length)];
  }
  return s;
}

/**
 * Base64url encode (no padding, - and _ instead of + and /). Uses btoa (browser).
 */
function base64UrlEncode(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let binary = "";
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  const base64 = btoa(binary);
  return base64.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

/**
 * Generate code_verifier and code_challenge (S256).
 * Use the same code_verifier when exchanging the authorization code for tokens.
 */
export async function generatePkce(): Promise<{ codeVerifier: string; codeChallenge: string }> {
  const codeVerifier = randomCodeVerifier();
  if (typeof crypto !== "undefined" && crypto.subtle) {
    const encoder = new TextEncoder();
    const data = encoder.encode(codeVerifier);
    const hash = await crypto.subtle.digest("SHA-256", data);
    const codeChallenge = base64UrlEncode(hash);
    return { codeVerifier, codeChallenge };
  }
  // Server-side or old browser: we cannot compute SHA-256 in a standard way without Node crypto.
  // Caller should only use this in the browser for the Endpoints page.
  throw new Error("PKCE generation requires crypto.subtle (browser).");
}
