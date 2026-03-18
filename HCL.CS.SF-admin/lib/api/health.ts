"use server";

import { env } from "@/lib/env";
import { HCLCSSFFetch } from "@/lib/server-fetch";

export type HealthStatus = "ok" | "degraded" | "down";

export type HealthDetail = {
  status: HealthStatus;
  checks?: { name: string; status: string; durationMs?: number }[];
};

/**
 * Two-phase health check (runs server-side to avoid browser TLS/CORS issues):
 *   1. /health/live — is the server process alive?
 *   2. /health/ready — are dependencies (database, cache) healthy?
 */
export async function checkDemoHealth(): Promise<HealthDetail> {
  const base = env.demoServerBaseUrl;
  const baseUrl = base.replace(/\/+$/, "");

  // Phase 1: Is the server process alive? (/health/live only checks "self")
  let isLive = false;
  try {
    const liveRes = await HCLCSSFFetch(`${baseUrl}/health/live`, {
      cache: "no-store",
      signal: AbortSignal.timeout(5000),
    });
    isLive = liveRes.ok;
  } catch {
    // Server unreachable
  }

  if (!isLive) {
    return { status: "down" };
  }

  // Phase 2: Are dependencies (database, cache) healthy?
  try {
    const readyRes = await HCLCSSFFetch(`${baseUrl}/health/ready`, {
      cache: "no-store",
      signal: AbortSignal.timeout(5000),
    });
    if (readyRes.ok) {
      return { status: "ok" };
    }
    // Server is alive but dependencies are unhealthy — parse details
    try {
      const body = await readyRes.json();
      return {
        status: "degraded",
        checks: body.checks ?? [],
      };
    } catch {
      return { status: "degraded" };
    }
  } catch {
    return { status: "degraded" };
  }
}
