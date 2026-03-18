"use client";

import { useEffect, useMemo, useState } from "react";
import { signIn, useSession } from "next-auth/react";
import { useRouter, useSearchParams } from "next/navigation";

import { Button } from "@/components/ui/button";

const defaultCallbackPath = "/admin";

function sanitizeCallbackUrl(rawCallbackUrl: string | null): string {
  if (!rawCallbackUrl) return defaultCallbackPath;
  if (rawCallbackUrl.startsWith("/") && !rawCallbackUrl.startsWith("//")) return rawCallbackUrl;
  if (typeof window === "undefined") return defaultCallbackPath;
  try {
    const parsed = new URL(rawCallbackUrl);
    if (parsed.origin === window.location.origin) {
      return `${parsed.pathname}${parsed.search}${parsed.hash}`;
    }
  } catch {
    return defaultCallbackPath;
  }
  return defaultCallbackPath;
}

function mapLoginError(rawError: string | null): string | null {
  if (!rawError) return null;
  switch (rawError) {
    case "OAuthSignin":
      return "Could not start sign-in flow. Please try again.";
    case "OAuthCallback":
      return "Error during sign-in callback. Please try again.";
    case "OAuthAccountNotLinked":
      return "This account is not linked. Sign in with the original provider.";
    case "Configuration":
      return "Authentication configuration error. Contact your administrator.";
    case "AccessDenied":
      return "Access denied.";
    default:
      return rawError;
  }
}

export default function LoginPage() {
  const { status } = useSession();
  const router = useRouter();
  const searchParams = useSearchParams();
  const callbackUrl = useMemo(
    () => sanitizeCallbackUrl(searchParams.get("callbackUrl")),
    [searchParams]
  );
  const urlError = useMemo(() => mapLoginError(searchParams.get("error")), [searchParams]);
  const reasonAdminRequired = useMemo(
    () => searchParams.get("reason") === "admin_required",
    [searchParams]
  );
  const [starting, setStarting] = useState(false);

  useEffect(() => {
    if (status === "authenticated") {
      router.replace(callbackUrl);
    }
  }, [callbackUrl, router, status]);

  const startLogin = () => {
    if (starting || status === "loading") return;
    setStarting(true);
    // Initiate Authorization Code + PKCE flow via NextAuth.
    // This redirects the browser to the identity server's /security/authorize endpoint.
    // The identity server handles the login UI, then redirects back with an authorization code.
    // NextAuth exchanges the code + PKCE verifier for tokens server-side.
    // Tokens are stored in the encrypted JWT cookie — browser never sees them.
    signIn("hclcssf", { callbackUrl });
  };

  return (
    <main className="login-shell">
      <section className="login-card">
        <p className="kicker">HCL.CS.SF Administration</p>
        <h1>Admin Console</h1>
        <p>Sign in with your HCL.CS.SF administrative account.</p>

        {reasonAdminRequired ? (
          <p className="inline-message" style={{ marginBottom: "0.5rem" }}>
            Your session doesn&apos;t have administrator access. Please sign in with an account that has the admin role.
          </p>
        ) : null}
        {urlError ? <p className="inline-error">Sign-in error: {urlError}</p> : null}
        {status === "authenticated" ? <p className="inline-success">Session active. Redirecting...</p> : null}

        <Button
          type="button"
          onClick={startLogin}
          disabled={status === "loading" || starting}
          style={{ width: "100%" }}
        >
          {status === "loading" ? "Checking session..." : starting ? "Redirecting..." : "Sign in with HCL.CS.SF"}
        </Button>

        <p className="inline-message" style={{ marginTop: "0.5rem", textAlign: "center" }}>
          You will be redirected to the identity server to authenticate securely.
        </p>
      </section>
    </main>
  );
}
