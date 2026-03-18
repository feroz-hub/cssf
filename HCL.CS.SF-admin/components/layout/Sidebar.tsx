"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import {
  Activity,
  Ban,
  Bell,
  ChevronsLeft,
  ChevronsRight,
  Fingerprint,
  Globe,
  KeyRound,
  LayoutDashboard,
  Layers,
  Network,
  ScrollText,
  ShieldCheck,
  Users,
  Wrench,
  type LucideIcon,
} from "lucide-react";

import { cn } from "@/lib/utils";
import { useUiStore } from "@/src/store/uiStore";

/* ── Icon registry ─────────────────────────────────────────────────────── */

const iconMap: Record<string, LucideIcon> = {
  "/admin": LayoutDashboard,
  "/admin/users": Users,
  "/admin/roles": ShieldCheck,
  "/admin/clients": KeyRound,
  "/admin/resources": Layers,
  "/admin/identity-resources": Fingerprint,
  "/admin/external-auth": Globe,
  "/admin/auth-tools": Wrench,
  "/admin/revocation": Ban,
  "/admin/audit": ScrollText,
  "/admin/notifications": Bell,
  "/admin/operations": Activity,
  "/admin/operations/endpoints": Network,
};

/* ── Navigation structure ──────────────────────────────────────────────── */

const sections = [
  {
    label: "Overview",
    items: [{ href: "/admin", label: "Dashboard" }],
  },
  {
    label: "Identity",
    items: [
      { href: "/admin/users", label: "Users" },
      { href: "/admin/roles", label: "Roles & Claims" },
    ],
  },
  {
    label: "Security",
    items: [
      { href: "/admin/clients", label: "Clients" },
      { href: "/admin/resources", label: "Resources & Scopes" },
      { href: "/admin/identity-resources", label: "Identity Resources" },
      { href: "/admin/external-auth", label: "External Auth" },
    ],
  },
  {
    label: "Operations",
    items: [
      { href: "/admin/auth-tools", label: "Auth Tools" },
      { href: "/admin/revocation", label: "Revocation" },
      { href: "/admin/audit", label: "Audit Log" },
      { href: "/admin/notifications", label: "Notifications" },
      { href: "/admin/operations", label: "Operations" },
      { href: "/admin/operations/endpoints", label: "Endpoints" },
    ],
  },
] as const;

/* ── Mobile context (unchanged) ────────────────────────────────────────── */

type SidebarMobileContextValue = {
  open: boolean;
  toggleMobile: () => void;
  closeMobile: () => void;
};

const SidebarMobileContext = createContext<SidebarMobileContextValue | null>(
  null
);

export function SidebarMobileProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const [open, setOpen] = useState(false);

  const value = useMemo(
    () => ({
      open,
      toggleMobile: () => setOpen((v) => !v),
      closeMobile: () => setOpen(false),
    }),
    [open]
  );

  return (
    <SidebarMobileContext.Provider value={value}>
      {children}
    </SidebarMobileContext.Provider>
  );
}

export function useSidebarMobile(): SidebarMobileContextValue {
  const ctx = useContext(SidebarMobileContext);
  if (!ctx) {
    throw new Error("useSidebarMobile must be used within SidebarMobileProvider");
  }
  return ctx;
}

/* ── Sidebar component ─────────────────────────────────────────────────── */

export function Sidebar() {
  const pathname = usePathname();
  const { open, closeMobile } = useSidebarMobile();
  const { navCollapsed, toggleNavCollapsed } = useUiStore();

  // Hydration guard: defer collapsed state until client
  const [hydrated, setHydrated] = useState(false);
  useEffect(() => setHydrated(true), []);

  const collapsed = hydrated && navCollapsed;

  // Close overlay on route change
  useEffect(() => {
    closeMobile();
  }, [pathname, closeMobile]);

  const sidebarContent = (
    <aside
      className="sidebar"
      aria-label="HCL.CS.SF navigation"
      data-collapsed={collapsed ? "true" : "false"}
    >
      <div>
        {/* Header: logo + collapse toggle */}
        <div className="sidebar-header">
          <div className="sidebar-logo-mark" />
          <span className="sidebar-logo-wordmark">HCL.CS.SF Admin</span>
          <button
            type="button"
            className="sidebar-collapse-toggle"
            onClick={toggleNavCollapsed}
            aria-expanded={!collapsed}
            aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
          >
            {collapsed ? (
              <ChevronsRight size={16} />
            ) : (
              <ChevronsLeft size={16} />
            )}
          </button>
        </div>

        {/* Navigation */}
        <nav className="sidebar-nav" aria-label="Main navigation">
          {sections.map((section) => (
            <div key={section.label} className="sidebar-section">
              <span className="sidebar-section-label" aria-hidden="true">
                {section.label}
              </span>
              {section.items.map((item) => {
                const active =
                  item.href === "/admin"
                    ? pathname === "/admin"
                    : pathname === item.href ||
                      pathname.startsWith(`${item.href}/`);
                const Icon = iconMap[item.href] ?? LayoutDashboard;
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    className={cn(
                      "sidebar-link",
                      active && "sidebar-link-active"
                    )}
                    aria-current={active ? "page" : undefined}
                    data-tooltip={item.label}
                  >
                    <span className="sidebar-link-icon" aria-hidden="true">
                      <Icon size={18} strokeWidth={1.8} />
                    </span>
                    <span className="sidebar-link-label">{item.label}</span>
                  </Link>
                );
              })}
            </div>
          ))}
        </nav>
      </div>

      {/* Footer */}
      <div className="sidebar-footer">
        <div className="sidebar-footer-row">
          <div className="sidebar-version">v0.2.0</div>
          <button
            type="button"
            className="sidebar-theme-toggle"
            aria-label="Toggle dark and light theme"
            onClick={toggleTheme}
            data-tooltip="Theme"
          >
            <span className="sidebar-theme-toggle-icon">◐</span>
            <span className="sidebar-theme-label">Theme</span>
          </button>
        </div>
      </div>
    </aside>
  );

  return (
    <>
      {/* Desktop / tablet sidebar (always visible) */}
      <div className="sidebar-desktop">{sidebarContent}</div>

      {/* Mobile overlay sidebar */}
      {open ? (
        <div className="sidebar-overlay" role="dialog" aria-modal="true">
          <div className="sidebar-overlay-panel">{sidebarContent}</div>
          <button
            type="button"
            aria-label="Close navigation"
            className="sidebar-overlay-backdrop"
            onClick={closeMobile}
          />
        </div>
      ) : null}
    </>
  );
}

/* ── Theme toggle helper ───────────────────────────────────────────────── */

function toggleTheme() {
  if (typeof document === "undefined") return;

  const root = document.documentElement;
  const current =
    root.getAttribute("data-theme") === "light" ? "light" : "dark";
  const next = current === "light" ? "dark" : "light";
  root.setAttribute("data-theme", next);
  try {
    window.localStorage.setItem("HCL.CS.SF-theme", next);
  } catch {
    // ignore
  }
}
