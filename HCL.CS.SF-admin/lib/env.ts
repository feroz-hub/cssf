const LOCALHOST_ISSUER = "https://localhost:5001";
const LOCALHOST_NEXTAUTH_URL = "https://localhost:3000";

const defaultScopes = [
  "openid",
  "profile",
  "email",
  "offline_access",
  "phone",
  "HCL.CS.SF.apiresource.manage",
  "HCL.CS.SF.client.manage",
  "HCL.CS.SF.user.read",
  "HCL.CS.SF.user.write",
  "HCL.CS.SF.role.manage",
  "HCL.CS.SF.identityresource.manage",
  "HCL.CS.SF.adminuser.manage",
  "HCL.CS.SF.securitytoken.manage",
  "HCL.CS.SF.notification.read",
  "HCL.CS.SF.notification.manage"
].join(" ");

function readEnv(name: string): string | undefined {
  const value = process.env[name]?.trim();
  return value ? value : undefined;
}

function readFirstEnv(names: string[]): string | undefined {
  for (const name of names) {
    const value = readEnv(name);
    if (value) {
      return value;
    }
  }

  return undefined;
}

const issuer = readFirstEnv(["HCL.CS.SF_ISSUER", "HCL.CS.SF_AUTHORITY"]) ?? LOCALHOST_ISSUER;
const allowInsecureTls =
  readEnv("HCL.CS.SF_ALLOW_INSECURE_TLS") === "true" && process.env.NODE_ENV !== "production";

export const env = {
  nextAuthUrl: readEnv("NEXTAUTH_URL") ?? LOCALHOST_NEXTAUTH_URL,
  nextAuthSecret: readEnv("NEXTAUTH_SECRET") ?? "",
  issuer,
  metadataAddress:
    readEnv("HCL.CS.SF_METADATA_ADDRESS") ?? `${issuer.replace(/\/+$/, "")}/.well-known/openid-configuration`,
  clientId: readEnv("HCL.CS.SF_CLIENT_ID") ?? "",
  clientSecret: readEnv("HCL.CS.SF_CLIENT_SECRET") ?? "",
  scopes: readEnv("HCL.CS.SF_SCOPES") ?? defaultScopes,
  tokenEndpoint: readEnv("HCL.CS.SF_TOKEN_ENDPOINT"),
  revocationEndpoint: readEnv("HCL.CS.SF_REVOCATION_ENDPOINT"),
  postLogoutRedirectUri:
    readEnv("HCL.CS.SF_POST_LOGOUT_REDIRECT_URI") ?? `${readEnv("NEXTAUTH_URL") ?? LOCALHOST_NEXTAUTH_URL}/login`,
  enableFederatedLogout: readEnv("HCL.CS.SF_ENABLE_FEDERATED_LOGOUT") === "true",
  apiBaseUrl: readEnv("HCL.CS.SF_API_BASE_URL") ?? issuer,
  installerBaseUrl: readEnv("HCL.CS.SF_INSTALLER_BASE_URL") ?? (readEnv("HCL.CS.SF_API_BASE_URL") ?? issuer),
  // Demo Server health + external auth host. Default to issuer (Demo Server) and do NOT fall back to apiBaseUrl,
  // so health checks always reflect Demo Server rather than the management API.
  demoServerBaseUrl: readEnv("HCL.CS.SF_DEMO_SERVER_BASE_URL") ?? issuer,
  allowInsecureTls,
  /** When true, show "Sign in with Google" on the login page (requires Demo Server Google config and admin client user_code grant). */
  googleLoginEnabled: process.env.NEXT_PUBLIC_GOOGLE_LOGIN_ENABLED === "true"
};

function assertProductionRuntimeEnv(): void {
  if (process.env.NODE_ENV !== "production") {
    return;
  }

  if (!readEnv("NEXTAUTH_URL")) {
    throw new Error("Missing NEXTAUTH_URL in production runtime configuration");
  }

  if (!readFirstEnv(["HCL.CS.SF_ISSUER", "HCL.CS.SF_AUTHORITY"])) {
    throw new Error("Missing HCL.CS.SF_ISSUER (or HCL.CS.SF_AUTHORITY) in production runtime configuration");
  }
}

export function assertAuthEnv(): void {
  assertProductionRuntimeEnv();

  if (!env.nextAuthSecret) {
    throw new Error("Missing NEXTAUTH_SECRET");
  }

  if (!env.clientId) {
    throw new Error("Missing HCL.CS.SF_CLIENT_ID");
  }

  if (!env.clientSecret) {
    throw new Error("Missing HCL.CS.SF_CLIENT_SECRET");
  }
}
