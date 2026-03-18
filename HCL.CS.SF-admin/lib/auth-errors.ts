export const SESSION_ERROR_INVALIDATED = "SessionInvalidated";
export const SESSION_ERROR_REFRESH_FAILED = "SessionRefreshFailed";

const hardSessionErrors = new Set([
  SESSION_ERROR_INVALIDATED,
  SESSION_ERROR_REFRESH_FAILED,
  "RefreshAccessTokenError",
  "Refresh token validation failed.",
  "MissingRefreshToken"
]);

export function isHardSessionError(error: unknown): boolean {
  return typeof error === "string" && hardSessionErrors.has(error);
}
