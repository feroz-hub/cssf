"use client";

import { motion } from "framer-motion";
import { AIInsightCard } from "@/src/components/ui/AIInsightCard";

export function FutureDashboardScreen() {
  return (
    <div className="grid grid-rows-[minmax(0,2fr)_minmax(0,3fr)] gap-4 lg:grid-rows-1 lg:grid-cols-[minmax(0,3fr)_minmax(0,2fr)]">
      <section className="flex flex-col gap-4">
        <div className="grid grid-cols-1 gap-3 md:grid-cols-3">
          <AIInsightCard
            title="Identity Stability"
            metric="99.97%"
            trend="up"
            trendLabel="+0.12% vs 24h"
            footer="Federated logins are stable across all tenants."
            accent="emerald"
          />
          <AIInsightCard
            title="Anomaly Surface"
            metric="Low"
            trend="flat"
            trendLabel="2 weak signals"
            footer="Minor outliers detected in a single client region."
            accent="violet"
          />
          <AIInsightCard
            title="Policy Drift"
            metric="0.3%"
            trend="down"
            trendLabel="-0.1% vs baseline"
            footer="Role and claim changes remain within safe bounds."
            accent="cyan"
          />
        </div>
        <motion.div
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.15, duration: 0.4 }}
          className="relative flex flex-1 flex-col overflow-hidden rounded-2xl border border-slate-800/80 bg-slate-900/80 p-4 shadow-[0_22px_90px_rgba(15,23,42,0.95)]"
        >
          <div className="flex items-center justify-between gap-3 pb-3">
            <div>
              <h2 className="text-xs font-medium uppercase tracking-[0.28em] text-slate-400">
                Live Mesh
              </h2>
              <p className="text-sm font-semibold text-slate-50">
                Requests & authorizations over time
              </p>
            </div>
            <span className="rounded-full border border-emerald-400/40 bg-emerald-500/10 px-2 py-0.5 text-[10px] font-medium text-emerald-200">
              Streaming from demo server
            </span>
          </div>
          <div className="relative mt-1 flex flex-1 items-center justify-center rounded-xl border border-dashed border-slate-800/80 bg-gradient-to-b from-slate-900/80 to-slate-950/90">
            <p className="max-w-sm text-center text-[11px] text-slate-400">
              This panel is wired for Framer Motion & charts. Plug in
              real-time metrics (requests, latency, authorization decisions)
              when you are ready, without changing the surrounding layout.
            </p>
          </div>
        </motion.div>
      </section>
      <motion.section
        initial={{ opacity: 0, x: 12 }}
        animate={{ opacity: 1, x: 0 }}
        transition={{ delay: 0.1, duration: 0.4 }}
        className="flex flex-col gap-3 rounded-2xl border border-slate-800/80 bg-slate-950/85 p-4 shadow-[0_22px_90px_rgba(15,23,42,0.95)]"
      >
        <header className="flex items-center justify-between gap-3">
          <div>
            <h2 className="text-xs font-medium uppercase tracking-[0.28em] text-slate-400">
              Impact Preview
            </h2>
            <p className="text-sm font-semibold text-slate-50">
              Last 5 high-sensitivity changes
            </p>
          </div>
          <span className="rounded-full border border-slate-700 bg-slate-900/80 px-2 py-0.5 text-[10px] text-slate-300">
            Read‑only demo
          </span>
        </header>
        <div className="mt-1 space-y-2 text-[11px] text-slate-300">
          <p>
            This stream will show the most recent role and policy changes with
            their blast radius — users impacted, clients affected, and
            downstream systems.
          </p>
          <p className="text-slate-400">
            Connect this panel to your existing audit API and reuse the
            identity relationships we already surfaced in the admin
            experience.
          </p>
        </div>
      </motion.section>
    </div>
  );
}

