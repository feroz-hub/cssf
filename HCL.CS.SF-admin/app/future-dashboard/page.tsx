import { AppShell } from "@/src/components/layout/AppShell";
import { FutureDashboardScreen } from "@/src/modules/dashboard/FutureDashboardScreen";

export default function FutureDashboardPage() {
  return (
    <AppShell
      title="AI Command Center"
      subtitle="A futuristic, read‑only canvas wired for your identity telemetry."
    >
      <FutureDashboardScreen />
    </AppShell>
  );
}

