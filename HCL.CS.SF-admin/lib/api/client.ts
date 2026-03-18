import { randomUUID } from "crypto";

import { isHardSessionError } from "@/lib/auth-errors";
import { auth } from "@/lib/auth";
import { getServerAccessToken } from "@/lib/server-jwt";
import { env } from "@/lib/env";
import { HCLCSSFFetch } from "@/lib/server-fetch";
import { type FrameworkResult, type ResultStatus } from "@/lib/types/HCL.CS.SF";

export class HCLCSSFApiError extends Error {
  statusCode: number;
  correlationId: string;
  details: unknown;

  constructor(message: string, statusCode: number, correlationId: string, details: unknown) {
    super(message);
    this.name = "HCLCSSFApiError";
    this.statusCode = statusCode;
    this.correlationId = correlationId;
    this.details = details;
  }
}

export function apiErrorMessage(error: unknown): string {
  if (error instanceof HCLCSSFApiError) {
    if (error.statusCode === 401) {
      return "Session expired. Please sign in again.";
    }
    if (error.statusCode === 403) {
      return `Access denied (HTTP 403). ${error.message}`;
    }
    return `${error.message} (HTTP ${error.statusCode}, correlation=${error.correlationId})`;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return "Failed to load data.";
}

/** Use when building page load error state; provides message and whether to offer sign-in. */
export function getLoadErrorInfo(error: unknown): { message: string; isUnauthorized: boolean } {
  const isUnauthorized = isUnauthorizedError(error);
  return {
    message: apiErrorMessage(error),
    isUnauthorized
  };
}

export function isUnauthorizedError(error: unknown): error is HCLCSSFApiError {
  return error instanceof HCLCSSFApiError && error.statusCode === 401;
}

/** Extract safe-to-log metadata from a JWT without exposing the full token. */
function safeTokenSummary(token: string): Record<string, unknown> {
  try {
    const parts = token.split(".");
    if (parts.length < 2) return { error: "not_a_jwt" };
    const payload = JSON.parse(Buffer.from(parts[1].replace(/-/g, "+").replace(/_/g, "/"), "base64").toString("utf8"));
    return {
      sub: payload.sub,
      aud: payload.aud,
      iss: payload.iss,
      scope: payload.scope,
      client_id: payload.client_id,
      exp: payload.exp,
      exp_human: payload.exp ? new Date(payload.exp * 1000).toISOString() : undefined,
      role: payload.role ?? payload.roles ?? payload.userrole,
      nbf: payload.nbf
    };
  } catch {
    return { error: "decode_failed" };
  }
}

/** Log API call failures with safe diagnostics for debugging. */
function logApiFailure(
  method: string,
  path: string,
  correlationId: string,
  status: number,
  responseExcerpt: unknown,
  tokenSummary?: Record<string, unknown>,
  wwwAuthenticate?: string | null
): void {
  console.error(
    `[HCL.CS.SF-api] FAILED ${method} ${path}`,
    JSON.stringify({
      correlationId,
      httpStatus: status,
      response: typeof responseExcerpt === "string" ? responseExcerpt.slice(0, 500) : responseExcerpt,
      ...(tokenSummary ? { token: tokenSummary } : {}),
      ...(wwwAuthenticate ? { wwwAuthenticate } : {})
    })
  );
}

function isFailedStatus(value: ResultStatus | undefined): boolean {
  if (value === undefined) {
    return false;
  }

  if (typeof value === "number") {
    return value !== 0;
  }

  return value.toLowerCase() !== "succeeded";
}

function normalizeMessage(payload: unknown, fallback: string): string {
  if (!payload || typeof payload !== "object") {
    return fallback;
  }

  const frameworkResult = payload as Partial<FrameworkResult>;
  if (frameworkResult.Errors && frameworkResult.Errors.length > 0) {
    return frameworkResult.Errors.map((error) => error.Description).join(", ");
  }

  if ("message" in payload && typeof payload.message === "string") {
    return payload.message;
  }

  if ("error_description" in payload && typeof payload.error_description === "string") {
    return payload.error_description;
  }

  if ("error" in payload && typeof payload.error === "string") {
    return payload.error;
  }

  return fallback;
}

function toApiUrl(path: string, baseUrl: string = env.apiBaseUrl): string {
  const base = baseUrl.replace(/\/+$/, "");
  return `${base}${path}`;
}

export async function requireAccessToken(): Promise<string> {
  // First check if we have a valid session (triggers JWT refresh if needed).
  const session = await auth();
  if (!session) {
    console.warn("[HCL.CS.SF-api] requireAccessToken: blocked request because no valid session is available.");
    throw new HCLCSSFApiError("Session expired. Please sign in again.", 401, "none", null);
  }

  if (isHardSessionError(session.error)) {
    console.warn("[HCL.CS.SF-api] requireAccessToken: blocked stale token after refresh failure.", {
      sessionError: session.error
    });
    throw new HCLCSSFApiError("Session expired. Please sign in again.", 401, "none", null);
  }

  // Read the access token directly from the encrypted JWT cookie (server-only).
  // The session callback intentionally strips tokens to keep them out of the browser.
  const accessToken = await getServerAccessToken();

  if (!accessToken) {
    console.error("[HCL.CS.SF-api] requireAccessToken: no accessToken in JWT cookie.", {
      hasSession: !!session,
      hasError: session?.error ?? "none",
      roles: session?.roles ?? [],
      isAdmin: session?.isAdmin ?? false
    });
    throw new HCLCSSFApiError("Authenticated session is missing an access token.", 401, "none", null);
  }

  if (process.env.NODE_ENV !== "production") {
    const summary = safeTokenSummary(accessToken);
    console.info("[HCL.CS.SF-api] requireAccessToken: token acquired from JWT cookie.", {
      sub: summary.sub,
      aud: summary.aud,
      scope: summary.scope,
      exp_human: summary.exp_human,
      sessionError: session.error ?? "none"
    });
  }

  return accessToken;
}

export async function HCLCSSFPost<TResponse, TRequest = string>(
  path: string,
  payload: TRequest,
  accessToken: string,
  baseUrl?: string
): Promise<TResponse> {
  const correlationId = randomUUID();
  const body = JSON.stringify(payload);

  if (process.env.NODE_ENV !== "production") {
    console.info(`[HCL.CS.SF-api] POST ${path} correlation=${correlationId}`);
  }

  const response = await HCLCSSFFetch(toApiUrl(path, baseUrl), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
      "X-Correlation-ID": correlationId
    },
    body,
    cache: "no-store"
  });

  let parsedPayload: unknown = null;
  try {
    parsedPayload = await response.json();
  } catch {
    parsedPayload = null;
  }

  if (!response.ok) {
    const wwwAuth = response.headers.get("www-authenticate");
    logApiFailure("POST", path, correlationId, response.status, parsedPayload, safeTokenSummary(accessToken), wwwAuth);
    throw new HCLCSSFApiError(
      normalizeMessage(parsedPayload, wwwAuth ? `HTTP ${response.status}: ${wwwAuth}` : `HTTP ${response.status}`),
      response.status,
      correlationId,
      parsedPayload
    );
  }

  if (parsedPayload && typeof parsedPayload === "object") {
    const frameworkResult = parsedPayload as Partial<FrameworkResult>;
    if (isFailedStatus(frameworkResult.Status)) {
      throw new HCLCSSFApiError(
        normalizeMessage(parsedPayload, "HCL.CS.SF API request failed."),
        response.status,
        correlationId,
        parsedPayload
      );
    }
  }

  return parsedPayload as TResponse;
}

export async function HCLCSSFPostWithSession<TResponse, TRequest = string>(
  path: string,
  payload: TRequest,
  baseUrl?: string
): Promise<TResponse> {
  const accessToken = await requireAccessToken();
  return HCLCSSFPost<TResponse, TRequest>(path, payload, accessToken, baseUrl);
}

/** POST to HCL.CS.SF API without Authorization. Use for anonymous endpoints (e.g. Forgot Password, Reset Password). */
export async function HCLCSSFPostAnonymous<TResponse, TRequest = string>(
  path: string,
  payload: TRequest,
  baseUrl?: string
): Promise<TResponse> {
  const correlationId = randomUUID();
  const body = JSON.stringify(payload);

  if (process.env.NODE_ENV !== "production") {
    console.info(`[HCL.CS.SF-api] POST (anonymous) ${path} correlation=${correlationId}`);
  }

  const response = await HCLCSSFFetch(toApiUrl(path, baseUrl), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Correlation-ID": correlationId
    },
    body,
    cache: "no-store"
  });

  let parsedPayload: unknown = null;
  try {
    parsedPayload = await response.json();
  } catch {
    parsedPayload = null;
  }

  if (!response.ok) {
    throw new HCLCSSFApiError(
      normalizeMessage(parsedPayload, `HTTP ${response.status}`),
      response.status,
      correlationId,
      parsedPayload
    );
  }

  if (parsedPayload && typeof parsedPayload === "object") {
    const frameworkResult = parsedPayload as Partial<FrameworkResult>;
    if (isFailedStatus(frameworkResult.Status)) {
      throw new HCLCSSFApiError(
        normalizeMessage(parsedPayload, "Request failed."),
        response.status,
        correlationId,
        parsedPayload
      );
    }
  }

  return parsedPayload as TResponse;
}

export async function HCLCSSFGet<TResponse>(path: string, accessToken: string, baseUrl?: string): Promise<TResponse> {
  const correlationId = randomUUID();

  if (process.env.NODE_ENV !== "production") {
    console.info(`[HCL.CS.SF-api] GET ${path} correlation=${correlationId}`);
  }

  const response = await HCLCSSFFetch(toApiUrl(path, baseUrl), {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "X-Correlation-ID": correlationId
    },
    cache: "no-store"
  });

  let parsedPayload: unknown = null;
  try {
    parsedPayload = await response.json();
  } catch {
    parsedPayload = null;
  }

  if (!response.ok) {
    const wwwAuth = response.headers.get("www-authenticate");
    logApiFailure("GET", path, correlationId, response.status, parsedPayload, safeTokenSummary(accessToken), wwwAuth);
    throw new HCLCSSFApiError(
      normalizeMessage(parsedPayload, wwwAuth ? `HTTP ${response.status}: ${wwwAuth}` : `HTTP ${response.status}`),
      response.status,
      correlationId,
      parsedPayload
    );
  }

  if (parsedPayload && typeof parsedPayload === "object") {
    const frameworkResult = parsedPayload as Partial<FrameworkResult>;
    if (isFailedStatus(frameworkResult.Status)) {
      throw new HCLCSSFApiError(
        normalizeMessage(parsedPayload, "HCL.CS.SF API request failed."),
        response.status,
        correlationId,
        parsedPayload
      );
    }
  }

  return parsedPayload as TResponse;
}

export async function HCLCSSFGetWithSession<TResponse>(path: string, baseUrl?: string): Promise<TResponse> {
  const accessToken = await requireAccessToken();
  return HCLCSSFGet<TResponse>(path, accessToken, baseUrl);
}

export function getClientBasicAuthHeaderValue(): string {
  const encoded = Buffer.from(`${encodeURIComponent(env.clientId)}:${encodeURIComponent(env.clientSecret)}`).toString(
    "base64"
  );
  return `Basic ${encoded}`;
}
