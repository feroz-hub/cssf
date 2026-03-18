import { env } from "@/lib/env";
import { HCLCSSFFetch } from "@/lib/server-fetch";

type DiscoveryDocument = {
  authorization_endpoint?: string;
  token_endpoint?: string;
  end_session_endpoint?: string;
  revocation_endpoint?: string;
};

let discoveryPromise: Promise<DiscoveryDocument | null> | null = null;

export function normalizeIssuer(issuer: string): string {
  return issuer.replace(/\/+$/, "");
}

export function getWellKnownUrl(): string {
  return env.metadataAddress;
}

export async function getDiscoveryDocument(): Promise<DiscoveryDocument | null> {
  if (!discoveryPromise) {
    discoveryPromise = HCLCSSFFetch(getWellKnownUrl(), {
      method: "GET",
      cache: "force-cache"
    })
      .then(async (response) => {
        if (!response.ok) {
          return null;
        }

        return (await response.json()) as DiscoveryDocument;
      })
      .catch(() => null);
  }

  return discoveryPromise;
}

export async function resolveAuthorizationEndpoint(): Promise<string> {
  const discovery = await getDiscoveryDocument();
  if (discovery?.authorization_endpoint) {
    return discovery.authorization_endpoint;
  }

  return `${normalizeIssuer(env.issuer)}/security/authorize`;
}

export async function resolveTokenEndpoint(): Promise<string> {
  if (env.tokenEndpoint) {
    return env.tokenEndpoint;
  }

  const discovery = await getDiscoveryDocument();
  if (discovery?.token_endpoint) {
    return discovery.token_endpoint;
  }

  return `${normalizeIssuer(env.issuer)}/security/token`;
}

export async function resolveEndSessionEndpoint(): Promise<string> {
  const discovery = await getDiscoveryDocument();
  if (discovery?.end_session_endpoint) {
    return discovery.end_session_endpoint;
  }

  return `${normalizeIssuer(env.issuer)}/security/endsession`;
}

export async function resolveRevocationEndpoint(): Promise<string> {
  if (env.revocationEndpoint) {
    return env.revocationEndpoint;
  }

  const discovery = await getDiscoveryDocument();
  if (discovery?.revocation_endpoint) {
    return discovery.revocation_endpoint;
  }

  return `${normalizeIssuer(env.issuer)}/security/revocation`;
}
