"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

function labelFor(part: string): string {
  if (part === "admin") {
    return "Dashboard";
  }

  if (part === "clients") {
    return "Clients";
  }

  if (part === "resources") {
    return "Resources";
  }

  if (part === "roles") {
    return "Roles";
  }

  if (part === "users") {
    return "Users";
  }

  if (part === "revocation") {
    return "Revocation";
  }

  if (part === "audit") {
    return "Audit";
  }

  if (part === "notifications") {
    return "Notifications";
  }

  if (part === "external-auth") {
    return "External Auth";
  }

  if (part === "sessions") {
    return "Sessions";
  }

  if (part === "secrets") {
    return "Secrets";
  }

  return part;
}

export function Breadcrumb() {
  const pathname = usePathname();
  const parts = pathname.split("/").filter(Boolean);

  const crumbs = parts.map((part, index) => {
    const href = `/${parts.slice(0, index + 1).join("/")}`;
    return { part, href, label: labelFor(part) };
  });

  return (
    <nav className="breadcrumb" aria-label="Breadcrumb">
      {crumbs.map((crumb, index) => {
        const isLast = index === crumbs.length - 1;
        return (
          <span key={crumb.href}>
            {isLast ? <span>{crumb.label}</span> : <Link href={crumb.href}>{crumb.label}</Link>}
            {!isLast ? <span className="crumb-sep">/</span> : null}
          </span>
        );
      })}
    </nav>
  );
}
