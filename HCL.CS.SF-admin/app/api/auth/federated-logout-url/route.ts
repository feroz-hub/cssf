import { NextRequest, NextResponse } from "next/server";
import { getToken } from "next-auth/jwt";

import { env } from "@/lib/env";
import { resolveEndSessionEndpoint } from "@/lib/oidc";

export async function GET(request: NextRequest) {
  if (!env.enableFederatedLogout) {
    return NextResponse.json({ url: null });
  }

  const jwt = await getToken({ req: request, secret: process.env.NEXTAUTH_SECRET });

  let endSessionEndpoint: string;
  try {
    endSessionEndpoint = await resolveEndSessionEndpoint();
  } catch {
    return NextResponse.json({ url: null });
  }

  const origin = new URL(request.url).origin;
  const postLogoutRedirectUri = env.postLogoutRedirectUri ?? `${origin}/login`;

  const logoutUrl = new URL(endSessionEndpoint);
  logoutUrl.searchParams.set("client_id", env.clientId);
  logoutUrl.searchParams.set("post_logout_redirect_uri", postLogoutRedirectUri);

  if (jwt?.idToken) {
    logoutUrl.searchParams.set("id_token_hint", String(jwt.idToken));
  }

  return NextResponse.json({ url: logoutUrl.toString() });
}
