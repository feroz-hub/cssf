/**
 * PKCE (RFC 7636) helpers for server-side use (Node.js crypto).
 * Generates code_verifier and code_challenge (S256, base64url).
 */

import { createHash, randomBytes } from "crypto";

const CODE_VERIFIER_LENGTH = 64; // 43-128 per RFC; 64 is a safe default

/**
 * Generate a random code_verifier using Node crypto (43-128 unreserved chars).
 */
function randomCodeVerifier(): string {
  return randomBytes(CODE_VERIFIER_LENGTH)
    .toString("base64url")
    .slice(0, CODE_VERIFIER_LENGTH);
}

/**
 * Compute SHA-256 code_challenge from a code_verifier (S256 method).
 */
function computeCodeChallenge(codeVerifier: string): string {
  return createHash("sha256")
    .update(codeVerifier, "utf8")
    .digest("base64url");
}

/**
 * Generate code_verifier and code_challenge (S256) for authorization_code + PKCE flow.
 */
export function generatePkceServer(): { codeVerifier: string; codeChallenge: string } {
  const codeVerifier = randomCodeVerifier();
  const codeChallenge = computeCodeChallenge(codeVerifier);
  return { codeVerifier, codeChallenge };
}
