"use server";

import { randomUUID } from "crypto";
import { z } from "zod";

import { requireAccessToken, HCLCSSFApiError } from "@/lib/api/client";
import { env } from "@/lib/env";
import { HCLCSSFFetch } from "@/lib/server-fetch";
import { type ActionResult } from "@/lib/types/HCL.CS.SF";

const requestSchema = z.object({
  base: z.enum(["api", "installer", "demo"]),
  method: z.enum(["GET", "POST"]),
  path: z
    .string()
    .min(1)
    .refine((value) => value.startsWith("/"), { message: "Path must start with '/'." }),
  jsonBody: z.string().optional().default("")
});

function resolveBaseUrl(base: z.infer<typeof requestSchema>["base"]): string {
  if (base === "installer") {
    return env.installerBaseUrl;
  }
  if (base === "demo") {
    return env.demoServerBaseUrl;
  }
  return env.apiBaseUrl;
}

function join(baseUrl: string, path: string): string {
  return `${baseUrl.replace(/\/+$/, "")}${path}`;
}

export async function callEndpointAction(
  input: z.infer<typeof requestSchema>
): Promise<ActionResult<{ status: number; contentType: string | null; body: unknown }>> {
  const parsed = requestSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  const baseKey = parsed.data.base;
  const baseUrl = resolveBaseUrl(baseKey);
  const url = join(baseUrl, parsed.data.path);

  // Gateway (Security/Api) only accepts POST; other methods get 404. Use POST when base is "api".
  const method = baseKey === "api" ? "POST" : parsed.data.method;

  let accessToken: string | null = null;
  try {
    accessToken = await requireAccessToken();
  } catch {
    accessToken = null;
  }

  let body: string | undefined;
  if (method === "POST") {
    const raw = parsed.data.jsonBody?.trim() ?? "";
    if (!raw) {
      body = JSON.stringify("");
    } else {
      try {
        body = JSON.stringify(JSON.parse(raw));
      } catch {
        return { ok: false, message: "Body must be valid JSON." };
      }
    }
  }

  const headers: Record<string, string> = {};
  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`;
  }
  if (method === "POST") {
    headers["Content-Type"] = "application/json";
  }
  headers["X-Correlation-ID"] = randomUUID();

  try {
    const response = await HCLCSSFFetch(url, {
      method,
      headers,
      body,
      cache: "no-store"
    });

    const contentType = response.headers.get("content-type");
    const isJson = contentType?.toLowerCase().includes("application/json") ?? false;
    const payload = isJson ? await response.json().catch(() => null) : await response.text().catch(() => "");

    if (!response.ok) {
      let message = typeof payload === "string" && payload ? payload : `HTTP ${response.status}`;
      if (response.status === 404 && baseKey === "api") {
        message = `404 Not Found. The Gateway accepts only POST for /Security/Api routes. URL: ${url}`;
      }
      return {
        ok: false,
        message,
        data: { status: response.status, contentType, body: payload }
      };
    }

    return { ok: true, message: "Request completed.", data: { status: response.status, contentType, body: payload } };
  } catch (error) {
    if (error instanceof HCLCSSFApiError) {
      return { ok: false, message: error.message };
    }
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}
