import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFGetWithSession, HCLCSSFPostWithSession } from "@/lib/api/client";
import { env } from "@/lib/env";
import { HCLCSSFFetch } from "@/lib/server-fetch";

async function fetchText(url: string, init?: RequestInit): Promise<string> {
  const response = await HCLCSSFFetch(url, { ...init, cache: "no-store" });
  const body = await response.text();
  if (!response.ok) {
    throw new Error(body || `HTTP ${response.status}`);
  }
  return body;
}

export const Operations = {
  oidc: {
    jwks: async () => HCLCSSFGetWithSession<unknown>(ApiRoutes.endpoint.jwks),
    discovery: async () => HCLCSSFGetWithSession<unknown>(ApiRoutes.endpoint.discovery),
    token: async (payload: unknown) => HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.endpoint.token, payload),
    introspect: async (payload: unknown) => HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.endpoint.introspect, payload),
    userinfo: async (payload: unknown) => HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.endpoint.userinfo, payload),
    revocation: async (payload: unknown) => HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.endpoint.revocation, payload),
    authorizeUrl: () => `${env.apiBaseUrl.replace(/\/+$/, "")}${ApiRoutes.endpoint.authorize}`,
    endsessionUrl: () => `${env.apiBaseUrl.replace(/\/+$/, "")}${ApiRoutes.endpoint.endsession}`
  },
  health: {
    health: async () => HCLCSSFGetWithSession<unknown>(ApiRoutes.health.health),
    live: async () => HCLCSSFGetWithSession<unknown>(ApiRoutes.health.live),
    ready: async () => HCLCSSFGetWithSession<unknown>(ApiRoutes.health.ready)
  },
  installer: {
    rootHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.root}`),
    setupHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.setup}`),
    providerGetHtml: async () =>
      fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.providerGet}`),
    providerPostHtml: async (payload: Record<string, string>) =>
      fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.providerPost}`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams(payload).toString()
      }),
    connectionGetHtml: async () =>
      fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.connectionGet}`),
    connectionPostHtml: async (payload: Record<string, string>) =>
      fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.connectionPost}`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams(payload).toString()
      }),
    validateGetHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.validateGet}`),
    validatePostHtml: async (payload: Record<string, string>) =>
      fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.validatePost}`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams(payload).toString()
      }),
    migrateGetHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.migrateGet}`),
    migratePostHtml: async (payload: Record<string, string>) =>
      fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.migratePost}`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams(payload).toString()
      }),
    seedGetHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.seedGet}`),
    seedPostHtml: async (payload: Record<string, string>) =>
      fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.seedPost}`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams(payload).toString()
      }),
    installedHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.installed}`),
    completeHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.complete}`),
    errorHtml: async () => fetchText(`${env.installerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.installer.error}`)
  },
  demoExternalAuth: {
    googleStartUrl: () => `${env.demoServerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.demoExternalAuth.googleStart}`,
    googleCallbackUrl: () => `${env.demoServerBaseUrl.replace(/\/+$/, "")}${ApiRoutes.demoExternalAuth.googleCallback}`,
    linkGoogle: async (payload: unknown) =>
      HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.demoExternalAuth.linkGoogle, payload, env.demoServerBaseUrl),
    unlinkGoogle: async (payload: unknown) =>
      HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.demoExternalAuth.unlinkGoogle, payload, env.demoServerBaseUrl)
  }
} as const;
