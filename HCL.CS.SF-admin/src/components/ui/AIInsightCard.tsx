"use client";

import { motion } from "framer-motion";
import { cn } from "@/lib/utils";

type AIInsightCardProps = {
  title: string;
  metric: string;
  trend?: "up" | "down" | "flat";
  trendLabel?: string;
  footer?: string;
  accent?: "cyan" | "violet" | "emerald";
};

const accentMap: Record<
  NonNullable<AIInsightCardProps["accent"]>,
  { ring: string; glow: string; pill: string }
> = {
  cyan: {
    ring: "ring-cyan-500/40",
    glow: "shadow-[0_0_40px_rgba(34,211,238,0.4)]",
    pill: "from-cyan-400 to-sky-500"
  },
  violet: {
    ring: "ring-violet-500/40",
    glow: "shadow-[0_0_40px_rgba(139,92,246,0.4)]",
    pill: "from-violet-400 to-fuchsia-500"
  },
  emerald: {
    ring: "ring-emerald-500/40",
    glow: "shadow-[0_0_40px_rgba(16,185,129,0.4)]",
    pill: "from-emerald-400 to-teal-500"
  }
};

export function AIInsightCard({
  title,
  metric,
  trend = "flat",
  trendLabel,
  footer,
  accent = "cyan"
}: AIInsightCardProps) {
  const accentTokens = accentMap[accent];

  return (
    <motion.article
      initial={{ y: 8, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.4, ease: "easeOut" }}
      className={cn(
        "relative overflow-hidden rounded-2xl border border-slate-800/80 bg-slate-900/75 px-4 py-3",
        "ring-1 ring-inset",
        accentTokens.ring,
        accentTokens.glow,
        "backdrop-blur-xl"
      )}
    >
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_0_0,rgba(15,23,42,0.4),transparent_55%),radial-gradient(circle_at_100%_100%,rgba(15,23,42,0.4),transparent_55%)]" />
      <div className="relative flex flex-col gap-2">
        <div className="flex items-center justify-between gap-2">
          <h3 className="text-xs font-medium text-slate-300">{title}</h3>
          <span
            className={cn(
              "inline-flex items-center rounded-full bg-gradient-to-r px-2 py-0.5 text-[10px] font-medium text-slate-950",
              accentTokens.pill
            )}
          >
            AI Signal
          </span>
        </div>
        <div className="flex items-baseline gap-2">
          <div className="text-lg font-semibold text-slate-50">{metric}</div>
          {trendLabel && (
            <div
              className={cn(
                "text-[11px]",
                trend === "up" && "text-emerald-300",
                trend === "down" && "text-rose-300",
                trend === "flat" && "text-slate-400"
              )}
            >
              {trendLabel}
            </div>
          )}
        </div>
        {footer && <p className="text-[11px] text-slate-400">{footer}</p>}
      </div>
    </motion.article>
  );
}

