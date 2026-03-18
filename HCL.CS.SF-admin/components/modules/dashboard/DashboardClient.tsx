"use client";

import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import {
  Users,
  ShieldCheck,
  KeyRound,
  Layers,
  Fingerprint,
  Lock,
  Activity,
  Zap,
  type LucideIcon,
} from "lucide-react";
import { type AuditTrailModel } from "@/lib/types/HCL.CS.SF";
import "./dashboard.css";

/* ── Types ───────────────────────────────────────────────────────────────── */

type DashboardStats = {
  users: number;
  roles: number;
  clients: number;
  apiResources: number;
  identityResources: number;
  scopes: number;
  claims: number;
  enabledResources: number;
  mfaUsers: number;
  lockedUsers: number;
};

type RoleCount = { name: string; claims: number };

type Props = {
  stats: DashboardStats;
  auditEntries: AuditTrailModel[];
  auditByAction: Record<string, number>;
  auditByTable: Record<string, number>;
  roleCounts: RoleCount[];
};

/* ── Animated counter ────────────────────────────────────────────────────── */

function AnimatedNumber({ value, duration = 1200 }: { value: number; duration?: number }) {
  const [display, setDisplay] = useState(0);
  const ref = useRef<number | null>(null);

  useEffect(() => {
    const start = performance.now();
    const from = 0;
    const to = value;

    function tick(now: number) {
      const elapsed = now - start;
      const progress = Math.min(elapsed / duration, 1);
      // Ease-out cubic
      const eased = 1 - Math.pow(1 - progress, 3);
      setDisplay(Math.round(from + (to - from) * eased));
      if (progress < 1) {
        ref.current = requestAnimationFrame(tick);
      }
    }

    ref.current = requestAnimationFrame(tick);
    return () => {
      if (ref.current) cancelAnimationFrame(ref.current);
    };
  }, [value, duration]);

  return <>{display.toLocaleString()}</>;
}

/* ── Hex Grid Node ───────────────────────────────────────────────────────── */

function HexMetric({
  icon: Icon,
  label,
  value,
  href,
  accent,
  delay = 0,
}: {
  icon: LucideIcon;
  label: string;
  value: number;
  href: string;
  accent: string;
  delay?: number;
}) {
  return (
    <Link
      href={href}
      className="hex-metric"
      style={{ "--hex-accent": accent, "--hex-delay": `${delay}ms` } as React.CSSProperties}
    >
      <div className="hex-metric-glow" />
      <div className="hex-metric-icon">
        <Icon size={22} strokeWidth={1.6} />
      </div>
      <div className="hex-metric-value">
        <AnimatedNumber value={value} />
      </div>
      <div className="hex-metric-label">{label}</div>
      <div className="hex-metric-ring" />
    </Link>
  );
}

/* ── Audit Event Pulse ───────────────────────────────────────────────────── */

function AuditPulse({ entries }: { entries: AuditTrailModel[] }) {
  const [activeIndex, setActiveIndex] = useState(0);

  useEffect(() => {
    if (entries.length <= 1) return;
    const id = setInterval(() => {
      setActiveIndex((prev) => (prev + 1) % entries.length);
    }, 3000);
    return () => clearInterval(id);
  }, [entries.length]);

  if (entries.length === 0) {
    return <div className="audit-pulse-empty">No recent activity detected</div>;
  }

  return (
    <div className="audit-pulse">
      <div className="audit-pulse-header">
        <div className="audit-pulse-dot" />
        <span className="audit-pulse-label">Live Activity Stream</span>
        <span className="audit-pulse-count">{entries.length} events</span>
      </div>
      <div className="audit-pulse-timeline">
        {entries.map((entry, i) => (
          <div
            key={entry.Id}
            className={`audit-pulse-event ${i === activeIndex ? "audit-pulse-event-active" : ""} ${i < activeIndex ? "audit-pulse-event-past" : ""}`}
          >
            <div className="audit-pulse-event-marker" />
            <div className="audit-pulse-event-content">
              <span className="audit-pulse-event-action">{entry.ActionName}</span>
              <span className="audit-pulse-event-target">{entry.TableName}</span>
            </div>
            <div className="audit-pulse-event-meta">
              <span>{entry.CreatedBy ?? "system"}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

/* ── Distribution Bar ────────────────────────────────────────────────────── */

function DistributionBar({ data, colors }: { data: Record<string, number>; colors: string[] }) {
  const total = Object.values(data).reduce((s, v) => s + v, 0);
  if (total === 0) return <div className="dist-bar-empty">No data</div>;

  const entries = Object.entries(data).sort((a, b) => b[1] - a[1]);

  return (
    <div className="dist-bar">
      <div className="dist-bar-track">
        {entries.map(([key, value], i) => (
          <div
            key={key}
            className="dist-bar-segment"
            style={{
              width: `${(value / total) * 100}%`,
              backgroundColor: colors[i % colors.length],
            }}
            title={`${key}: ${value}`}
          />
        ))}
      </div>
      <div className="dist-bar-legend">
        {entries.slice(0, 5).map(([key, value], i) => (
          <div key={key} className="dist-bar-legend-item">
            <span className="dist-bar-legend-dot" style={{ backgroundColor: colors[i % colors.length] }} />
            <span className="dist-bar-legend-label">{key}</span>
            <span className="dist-bar-legend-value">{value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

/* ── Security Posture Ring ───────────────────────────────────────────────── */

function SecurityRing({ stats }: { stats: DashboardStats }) {
  const totalEntities = stats.users + stats.roles + stats.clients;
  const securityScore = totalEntities > 0
    ? Math.round(((stats.enabledResources + stats.mfaUsers + stats.roles) / Math.max(totalEntities, 1)) * 100)
    : 0;
  const clampedScore = Math.min(securityScore, 100);
  const circumference = 2 * Math.PI * 52;
  const dashOffset = circumference - (clampedScore / 100) * circumference;

  return (
    <div className="security-ring">
      <svg viewBox="0 0 120 120" className="security-ring-svg">
        {/* Background ring */}
        <circle cx="60" cy="60" r="52" fill="none" stroke="var(--border-subtle)" strokeWidth="6" />
        {/* Progress ring */}
        <circle
          cx="60"
          cy="60"
          r="52"
          fill="none"
          stroke="url(#ringGradient)"
          strokeWidth="6"
          strokeLinecap="round"
          strokeDasharray={circumference}
          strokeDashoffset={dashOffset}
          className="security-ring-progress"
          transform="rotate(-90 60 60)"
        />
        {/* Glow on tip */}
        <defs>
          <linearGradient id="ringGradient" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="#00d4ff" />
            <stop offset="50%" stopColor="#6366f1" />
            <stop offset="100%" stopColor="#10b981" />
          </linearGradient>
          <filter id="ringGlow">
            <feGaussianBlur stdDeviation="3" result="blur" />
            <feMerge>
              <feMergeNode in="blur" />
              <feMergeNode in="SourceGraphic" />
            </feMerge>
          </filter>
        </defs>
      </svg>
      <div className="security-ring-center">
        <div className="security-ring-score">
          <AnimatedNumber value={clampedScore} duration={1800} />
        </div>
        <div className="security-ring-unit">index</div>
      </div>
      <div className="security-ring-indicators">
        <div className="security-ring-indicator">
          <span className="security-ring-indicator-dot" style={{ background: "var(--success)" }} />
          <span>{stats.enabledResources} active</span>
        </div>
        <div className="security-ring-indicator">
          <span className="security-ring-indicator-dot" style={{ background: "var(--warning)" }} />
          <span>{stats.lockedUsers} locked</span>
        </div>
        <div className="security-ring-indicator">
          <span className="security-ring-indicator-dot" style={{ background: "var(--accent)" }} />
          <span>{stats.mfaUsers} hardened</span>
        </div>
      </div>
    </div>
  );
}

/* ── Role Topology ───────────────────────────────────────────────────────── */

function RoleTopology({ roles }: { roles: RoleCount[] }) {
  const maxClaims = Math.max(...roles.map(r => r.claims), 1);

  return (
    <div className="role-topology">
      {roles.length === 0 ? (
        <div className="dist-bar-empty">No roles configured</div>
      ) : (
        roles.slice(0, 8).map((role, i) => {
          const pct = (role.claims / maxClaims) * 100;
          return (
            <div key={role.name} className="role-topology-row" style={{ "--topo-delay": `${i * 60}ms` } as React.CSSProperties}>
              <div className="role-topology-name">{role.name}</div>
              <div className="role-topology-bar-track">
                <div className="role-topology-bar" style={{ width: `${Math.max(pct, 4)}%` }} />
              </div>
              <div className="role-topology-count">{role.claims}</div>
            </div>
          );
        })
      )}
    </div>
  );
}

/* ── Main Dashboard ──────────────────────────────────────────────────────── */

export function DashboardClient({ stats, auditEntries, auditByAction, auditByTable, roleCounts }: Props) {
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);

  return (
    <div className={`nexus-dashboard ${mounted ? "nexus-dashboard-ready" : ""}`}>
      {/* ── Top: Hero metrics ring ──────────────────────────────────── */}
      <section className="nexus-hero" aria-label="System metrics overview">
        <div className="nexus-hero-bg" />
        <div className="nexus-hero-title">
          <h2>Command Nexus</h2>
          <p>Identity infrastructure at a glance</p>
        </div>
        <div className="nexus-hex-grid">
          <HexMetric icon={Users} label="Users" value={stats.users} href="/admin/users" accent="#00d4ff" delay={0} />
          <HexMetric icon={ShieldCheck} label="Roles" value={stats.roles} href="/admin/roles" accent="#6366f1" delay={80} />
          <HexMetric icon={KeyRound} label="Clients" value={stats.clients} href="/admin/clients" accent="#10b981" delay={160} />
          <HexMetric icon={Layers} label="API Resources" value={stats.apiResources} href="/admin/resources" accent="#f59e0b" delay={240} />
          <HexMetric icon={Fingerprint} label="ID Resources" value={stats.identityResources} href="/admin/identity-resources" accent="#ec4899" delay={320} />
          <HexMetric icon={Lock} label="Scopes" value={stats.scopes} href="/admin/resources" accent="#8b5cf6" delay={400} />
        </div>
      </section>

      {/* ── Mid: Three-panel intelligence ───────────────────────────── */}
      <div className="nexus-panels">
        {/* Security posture */}
        <section className="nexus-panel nexus-panel-security" aria-label="Security posture">
          <div className="nexus-panel-head">
            <Zap size={16} strokeWidth={2} />
            <h3>Security Posture</h3>
          </div>
          <SecurityRing stats={stats} />
        </section>

        {/* Audit stream */}
        <section className="nexus-panel nexus-panel-audit" aria-label="Live audit stream">
          <div className="nexus-panel-head">
            <Activity size={16} strokeWidth={2} />
            <h3>Audit Stream</h3>
          </div>
          <AuditPulse entries={auditEntries} />
        </section>

        {/* Role topology */}
        <section className="nexus-panel nexus-panel-roles" aria-label="Role claim topology">
          <div className="nexus-panel-head">
            <ShieldCheck size={16} strokeWidth={2} />
            <h3>Role Topology</h3>
          </div>
          <RoleTopology roles={roleCounts} />
        </section>
      </div>

      {/* ── Bottom: Distribution intelligence ──────────────────────── */}
      <div className="nexus-distributions">
        <section className="nexus-dist-card" aria-label="Audit action distribution">
          <h4>Action Distribution</h4>
          <DistributionBar
            data={auditByAction}
            colors={["#00d4ff", "#6366f1", "#10b981", "#f59e0b", "#ef4444"]}
          />
        </section>
        <section className="nexus-dist-card" aria-label="Audit entity distribution">
          <h4>Entity Distribution</h4>
          <DistributionBar
            data={auditByTable}
            colors={["#ec4899", "#8b5cf6", "#06b6d4", "#f97316", "#14b8a6"]}
          />
        </section>
      </div>

      {/* ── Quick Command Grid ─────────────────────────────────────── */}
      <section className="nexus-commands" aria-label="Quick actions">
        <h4>Quick Commands</h4>
        <div className="nexus-command-grid">
          <Link href="/admin/users" className="nexus-cmd">
            <Users size={18} strokeWidth={1.6} />
            <span>Create User</span>
          </Link>
          <Link href="/admin/roles" className="nexus-cmd">
            <ShieldCheck size={18} strokeWidth={1.6} />
            <span>Create Role</span>
          </Link>
          <Link href="/admin/clients" className="nexus-cmd">
            <KeyRound size={18} strokeWidth={1.6} />
            <span>Register Client</span>
          </Link>
          <Link href="/admin/audit" className="nexus-cmd">
            <Activity size={18} strokeWidth={1.6} />
            <span>Audit Log</span>
          </Link>
          <Link href="/admin/operations/api-explorer" className="nexus-cmd">
            <Zap size={18} strokeWidth={1.6} />
            <span>API Explorer</span>
          </Link>
          <Link href="/admin/resources" className="nexus-cmd">
            <Layers size={18} strokeWidth={1.6} />
            <span>Manage Scopes</span>
          </Link>
        </div>
      </section>
    </div>
  );
}
