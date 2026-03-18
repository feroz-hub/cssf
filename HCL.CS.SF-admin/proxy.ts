import { NextResponse, type NextRequest } from "next/server";
import { getToken } from "next-auth/jwt";

import { isHardSessionError } from "@/lib/auth-errors";

function readRolesFromToken(token: Awaited<ReturnType<typeof getToken>>): string[] {
  if (!token || typeof token === "string") {
    return [];
  }

  const rawRoles = token.roles;
  if (Array.isArray(rawRoles)) {
    return rawRoles.map((item) => String(item));
  }

  return [];
}

function isAdminRole(roles: string[]): boolean {
  return roles.some((role) => role.toLowerCase().includes("admin"));
}

function hasUsableAuthState(token: Awaited<ReturnType<typeof getToken>>): boolean {
  if (!token || typeof token === "string") {
    return false;
  }

  if (typeof token.accessToken !== "string" || token.accessToken.length === 0) {
    return false;
  }

  return !isHardSessionError(token.error);
}

export async function proxy(request: NextRequest) {
  const token = await getToken({
    req: request,
    secret: process.env.NEXTAUTH_SECRET
  });

  if (!hasUsableAuthState(token)) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("callbackUrl", request.nextUrl.pathname + request.nextUrl.search);
    return NextResponse.redirect(loginUrl);
  }

  const roles = readRolesFromToken(token);
  if (!isAdminRole(roles)) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("reason", "admin_required");
    loginUrl.searchParams.set("callbackUrl", request.nextUrl.pathname + request.nextUrl.search);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*"]
};
