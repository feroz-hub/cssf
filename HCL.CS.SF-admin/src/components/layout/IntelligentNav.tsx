"use client";

import { motion } from "framer-motion";
import { Home, Shield, Activity, Users, Cpu } from "lucide-react";
import { useUiStore } from "@/src/store/uiStore";
import { cn } from "@/lib/utils";

type NavItem = {
  id: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  route: string;
  section: "operations" | "intelligence" | "security" | "system";
};

const NAV_ITEMS: NavItem[] = [
  { id: "dashboard", label: "Dashboard", icon: Home, route: "/future-dashboard", section: "operations" },
  { id: "threats", label: "Threat Matrix", icon: Shield, route: "/threats", section: "security" },
  { id: "users", label: "Identities", icon: Users, route: "/users", section: "operations" },
  { id: "signals", label: "AI Signals", icon: Activity, route: "/signals", section: "intelligence" },
  { id: "system", label: "System Mesh", icon: Cpu, route: "/system", section: "system" }
];

export function IntelligentNav() {
  const { navCollapsed, setNavCollapsed, activeRoute, setActiveRoute } = useUiStore();

  return (
    <motion.aside
      initial={false}
      animate={{ width: navCollapsed ? 72 : 260 }}
      className="relative z-30 flex h-screen flex-col border-r border-slate-800/70 bg-slate-950/80 shadow-[16px_0_60px_rgba(15,23,42,0.85)] backdrop-blur-xl"
    >
      <div className="flex items-center gap-3 px-4 py-4">
        <div className="h-8 w-8 rounded-full bg-gradient-to-br from-cyan-400 to-violet-500 shadow-[0_0_18px_rgba(34,211,238,0.7)]" />
        {!navCollapsed && (
          <div className="text-[10px] font-semibold uppercase tracking-[0.28em] text-slate-400">
            HCL.CS.SF Command
          </div>
        )}
      </div>

      <div className="flex-1 space-y-4 px-2">
        {(["operations", "intelligence", "security", "system"] as const).map((section) => (
          <div key={section} className="space-y-1">
            {!navCollapsed && (
              <div className="px-2 text-[10px] font-semibold uppercase tracking-[0.24em] text-slate-600">
                {section}
              </div>
            )}
            {NAV_ITEMS.filter((item) => item.section === section).map((item) => {
              const Icon = item.icon;
              const active = activeRoute === item.route;
              return (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => setActiveRoute(item.route)}
                  className={cn(
                    "group flex w-full items-center rounded-xl px-2 py-2 text-xs transition-colors",
                    active
                      ? "bg-cyan-500/15 text-cyan-300 shadow-[0_0_16px_rgba(34,211,238,0.35)]"
                      : "text-slate-400 hover:bg-slate-800/60 hover:text-slate-50"
                  )}
                >
                  <Icon className="h-4 w-4 flex-shrink-0" />
                  {!navCollapsed && <span className="ml-3 flex-1 text-left">{item.label}</span>}
                </button>
              );
            })}
          </div>
        ))}
      </div>

      <div className="border-t border-slate-800/70 px-2 py-3">
        <button
          type="button"
          onClick={() => setNavCollapsed(!navCollapsed)}
          className="flex w-full items-center justify-center rounded-full border border-slate-700 bg-slate-900/70 px-3 py-1 text-[11px] text-slate-300 hover:border-cyan-400 hover:text-cyan-300"
        >
          {navCollapsed ? "Expand" : "Collapse"}
        </button>
      </div>
    </motion.aside>
  );
}

