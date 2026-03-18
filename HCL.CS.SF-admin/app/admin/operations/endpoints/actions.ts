"use server";

import { z } from "zod";

import { ApiRoutes } from "@/lib/api/routes";
import { env } from "@/lib/env";
import { HCLCSSFFetch } from "@/lib/server-fetch";
import { type ActionResult } from "@/lib/types/HCL.CS.SF";

const clientCredentialsSchema = z.object({
  clientId: z.string().min(1),
  clientSecret: z.string().min(1),
  scope: z.string().min(1)
});

const resourceOwnerSchema = z.object({
  userName: z.string().min(1),
  password: z.string().min(1),
  scope: z.string().min(1).default("openid profile email"),
  clientId: z.string().optional(),
  clientSecret: z.string().optional()
});

const tokenWithClientSchema = z.object({
  token: z.string().min(1),
  tokenTypeHint: z.string().optional(),
  clientId: z.string().optional(),
  clientSecret: z.string().optional()
});

const authorizeUrlSchema = z.object({
  clientId: z.string().min(1),
  redirectUri: z.string().url(),
  scope: z.string().min(1),
  state: z.string().optional(),
  nonce: z.string().optional(),
  codeChallenge: z.string().optional(),
  codeChallengeMethod: z.string().optional()
});

const authorizationCodeExchangeSchema = z.object({
  code: z.string().min(1),
  redirectUri: z.string().url(),
  clientId: z.string().min(1),
  clientSecret: z.string().min(1),
  codeVerifier: z.string().optional()
});

function encodeBasicAuth(clientId: string, clientSecret: string): string {
  const escapedClientId = encodeURIComponent(clientId);
  const escapedClientSecret = encodeURIComponent(clientSecret);
  const encoded = Buffer.from(`${escapedClientId}:${escapedClientSecret}`).toString("base64");
  return `Basic ${encoded}`;
}

async function postForm(
  path: string,
  form: Record<string, string>,
  clientId: string,
  clientSecret: string,
  options?: { authInBody?: boolean }
): Promise<{ status: number; body: unknown }> {
  const url = `${env.apiBaseUrl.replace(/\/+$/, "")}${path}`;
  const authInBody = options?.authInBody ?? false;

  const formWithClient: Record<string, string> = { ...form };
  if (authInBody) {
    formWithClient.client_id = clientId;
    formWithClient.client_secret = clientSecret;
  }

  const headers: Record<string, string> = {
    "Content-Type": "application/x-www-form-urlencoded"
  };
  if (!authInBody) {
    headers.Authorization = encodeBasicAuth(clientId, clientSecret);
  }

  const body = new URLSearchParams(formWithClient).toString();
  const response = await HCLCSSFFetch(url, {
    method: "POST",
    headers,
    body,
    cache: "no-store"
  });

  const contentType = response.headers.get("content-type") ?? "";
  const isJson = contentType.toLowerCase().includes("application/json");
  const payload = isJson ? await response.json().catch(() => null) : await response.text().catch(() => "");

  if (!response.ok) {
    const serverMessage =
      typeof payload === "object" && payload !== null && "error_description" in payload
        ? String((payload as { error_description?: string }).error_description)
        : typeof payload === "string" && payload
          ? payload
          : null;
    const message =
      response.status === 401 && serverMessage
        ? `401 Unauthorized: ${serverMessage}`
        : `HTTP ${response.status}${serverMessage ? `: ${serverMessage}` : ""}`;
    throw new Error(message);
  }

  return { status: response.status, body: payload };
}

export async function clientCredentialsFlowAction(
  input: z.infer<typeof clientCredentialsSchema>
): Promise<ActionResult<unknown>> {
  const parsed = clientCredentialsSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const form = {
      grant_type: "client_credentials",
      scope: parsed.data.scope
    };
    const result = await postForm(
      ApiRoutes.endpoint.token,
      form,
      parsed.data.clientId,
      parsed.data.clientSecret,
      { authInBody: true }
    );
    return { ok: true, message: "Client credentials token request completed.", data: result.body };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function resourceOwnerPasswordFlowAction(
  input: z.infer<typeof resourceOwnerSchema>
): Promise<ActionResult<unknown>> {
  const parsed = resourceOwnerSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const { userName, password, scope, clientId, clientSecret } = parsed.data;
    if (!clientId || !clientSecret) {
      return {
        ok: false,
        message: "Client ID and Client Secret are required for resource owner password flow."
      };
    }

    const form: Record<string, string> = {
      grant_type: "password",
      username: userName,
      password,
      scope
    };

    const result = await postForm(ApiRoutes.endpoint.token, form, clientId, clientSecret, {
      authInBody: true
    });
    return { ok: true, message: "Resource owner password token request completed.", data: result.body };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function introspectTokenAction(
  input: z.infer<typeof tokenWithClientSchema>
): Promise<ActionResult<unknown>> {
  const parsed = tokenWithClientSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const { token, tokenTypeHint, clientId, clientSecret } = parsed.data;
    if (!clientId || !clientSecret) {
      return {
        ok: false,
        message: "Client ID and Client Secret are required for introspection."
      };
    }

    const form: Record<string, string> = {
      token
    };
    if (tokenTypeHint) form.token_type_hint = tokenTypeHint;

    const result = await postForm(ApiRoutes.endpoint.introspect, form, clientId, clientSecret, {
      authInBody: true
    });
    return { ok: true, message: "Introspection completed.", data: result.body };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function revokeTokenAction(
  input: z.infer<typeof tokenWithClientSchema>
): Promise<ActionResult<unknown>> {
  const parsed = tokenWithClientSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const { token, tokenTypeHint, clientId, clientSecret } = parsed.data;
    if (!clientId || !clientSecret) {
      return {
        ok: false,
        message: "Client ID and Client Secret are required for revocation."
      };
    }

    const form: Record<string, string> = {
      token
    };
    if (tokenTypeHint) form.token_type_hint = tokenTypeHint;

    const result = await postForm(ApiRoutes.endpoint.revocation, form, clientId, clientSecret, {
      authInBody: true
    });
    return { ok: true, message: "Revocation request completed.", data: result.body };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function getAuthorizeUrlAction(
  input: z.infer<typeof authorizeUrlSchema>
): Promise<ActionResult<{ url: string }>> {
  const parsed = authorizeUrlSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  const base = env.apiBaseUrl.replace(/\/+$/, "");
  const params = new URLSearchParams({
    client_id: parsed.data.clientId,
    redirect_uri: parsed.data.redirectUri,
    response_type: "code",
    scope: parsed.data.scope
  });
  if (parsed.data.state) params.set("state", parsed.data.state);
  if (parsed.data.nonce) params.set("nonce", parsed.data.nonce);
  if (parsed.data.codeChallenge) params.set("code_challenge", parsed.data.codeChallenge);
  if (parsed.data.codeChallengeMethod) params.set("code_challenge_method", parsed.data.codeChallengeMethod);

  const url = `${base}${ApiRoutes.endpoint.authorize}?${params.toString()}`;
  return { ok: true, message: "Authorize URL built.", data: { url } };
}

export async function authorizationCodeExchangeAction(
  input: z.infer<typeof authorizationCodeExchangeSchema>
): Promise<ActionResult<unknown>> {
  const parsed = authorizationCodeExchangeSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const form: Record<string, string> = {
      grant_type: "authorization_code",
      code: parsed.data.code,
      redirect_uri: parsed.data.redirectUri
    };
    if (parsed.data.codeVerifier) form.code_verifier = parsed.data.codeVerifier;

    const result = await postForm(
      ApiRoutes.endpoint.token,
      form,
      parsed.data.clientId,
      parsed.data.clientSecret,
      { authInBody: true }
    );
    return { ok: true, message: "Authorization code exchanged for tokens.", data: result.body };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}
