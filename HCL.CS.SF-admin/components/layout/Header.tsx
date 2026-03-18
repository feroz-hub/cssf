'use client';

import { signOut, useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';

import { logoutAction } from '@/app/actions/logout';
import { Button } from '@/components/ui/button';
import { useSidebarMobile } from '@/components/layout/Sidebar';
import { useDensity } from '@/context/DensityContext';
import { checkDemoHealth } from '@/lib/api/health';

export function Header() {
  const { data: session } = useSession();
  const [busy, setBusy] = useState(false);
  const [apiHealth, setApiHealth] = useState<'unknown' | 'ok' | 'degraded' | 'down'>('unknown');
  const [healthChecks, setHealthChecks] = useState<{ name: string; status: string }[]>([]);
  const router = useRouter();
  const { toggleMobile } = useSidebarMobile();
  const { density, setDensity } = useDensity();

  useEffect(() => {
    let cancelled = false;

    const check = async () => {
      const detail = await checkDemoHealth();
      if (!cancelled) {
        setApiHealth(detail.status);
        setHealthChecks(detail.checks ?? []);
      }
    };

    check();
    const id = setInterval(check, 30000);
    return () => {
      cancelled = true;
      clearInterval(id);
    };
  }, []);

  const doLogout = async () => {
    setBusy(true);

    // Get federated logout URL while still signed in (needs session for id_token_hint).
    let federatedLogoutUrl: string | null = null;
    try {
      const res = await fetch('/api/auth/federated-logout-url');
      const data = (await res.json()) as { url: string | null };
      if (data?.url) {
        federatedLogoutUrl = data.url;
      }
    } catch {
      // Ignore; we will redirect to /login.
    }

    try {
      // Server-side: HCL.CS.SF SignOut API and revoke access/refresh tokens.
      await logoutAction();
    } catch {
      // Still clear client session and redirect.
    } finally {
      // Clear NextAuth session and redirect (always run).
      await signOut({ redirect: false });
      if (federatedLogoutUrl) {
        window.location.href = federatedLogoutUrl;
      } else {
        router.push('/login');
      }
    }
  };

  const userName = session?.user?.name ?? 'Administrator';
  const apiLabel =
    apiHealth === 'ok' ? 'Demo: OK'
    : apiHealth === 'degraded' ? 'Demo: Degraded'
    : apiHealth === 'down' ? 'Demo: Down'
    : 'Demo: Checking…';

  const healthTooltip =
    apiHealth === 'degraded' && healthChecks.length > 0
      ? `Server is running but dependencies are unhealthy:\n${healthChecks.map((c) => `${c.name}: ${c.status}`).join('\n')}`
      : apiLabel;

  return (
    <header className="admin-header">
      <div className="admin-header-left">
        <button
          type="button"
          className="sidebar-mobile-toggle"
          aria-label="Open navigation"
          onClick={toggleMobile}
        >
          ☰
        </button>
        <div className="admin-header-user">
          <span className="admin-header-name">{userName}</span>
          <span className="admin-header-email">{session?.user?.email ?? 'HCL.CS.SF-admin'}</span>
        </div>
      </div>
      <div className="admin-header-actions">
        {/* Secondary toolbar: hidden on mobile, shown on tablet+ */}
        <div className="admin-header-toolbar-secondary">
          {/* SECURITY: Bearer token copy button removed.
              Tokens are now stored exclusively in server-side encrypted JWT.
              Browser only has opaque httpOnly session cookies. */}
          <div
            className="sidebar-theme-toggle"
            aria-label="API health status"
            title={healthTooltip}
          >
            <span
              className={`status-dot ${
                apiHealth === 'ok'
                  ? 'status-dot-active status-dot-pulse'
                  : apiHealth === 'degraded'
                    ? 'status-dot-warning'
                    : 'status-dot-inactive'
              }`}
              aria-hidden="true"
            />
            <span className="text-caption">{apiLabel}</span>
          </div>
          <select
            aria-label="Display density"
            value={density}
            onChange={(event) => setDensity(event.target.value as typeof density)}
            className="input"
            style={{ width: 130, paddingInline: '0.5rem' }}
          >
            <option value="compact">Compact</option>
            <option value="default">Default</option>
            <option value="comfortable">Comfortable</option>
          </select>
        </div>
        <Button type="button" variant="ghost" onClick={doLogout} disabled={busy}>
          {busy ? 'Signing out...' : 'Logout'}
        </Button>
      </div>
    </header>
  );
}
