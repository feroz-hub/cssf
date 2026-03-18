"use client";

import { motion } from "framer-motion";
import { Search, Sparkles, Command } from "lucide-react";
import { cn } from "@/lib/utils";

type CommandBarProps = {
  title?: string;
  subtitle?: string;
};

export function CommandBar({ title = "Command Center", subtitle }: CommandBarProps) {
  return (
    <motion.header
      initial={{ y: -12, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.4, ease: "easeOut" }}
      className="relative flex items-center justify-between gap-4 rounded-2xl border border-slate-800/80 bg-slate-900/80 px-4 py-3 shadow-[0_18px_80px_rgba(15,23,42,0.9)] backdrop-blur-2xl"
    >
      <div className="flex items-center gap-3">
        <div className="inline-flex items-center gap-2 rounded-full border border-cyan-500/40 bg-cyan-500/10 px-3 py-1 text-[10px] font-medium uppercase tracking-[0.22em] text-cyan-200">
          <Sparkles className="h-3 w-3" />
          Live AI Telemetry
        </div>
        <div className="flex flex-col">
          <div className="text-xs font-semibold uppercase tracking-[0.28em] text-slate-400">
            HCL.CS.SF
          </div>
          <div className="text-sm font-semibold text-slate-50">
            {title}
          </div>
          {subtitle && (
            <p className="text-[11px] text-slate-400">
              {subtitle}
            </p>
          )}
        </div>
      </div>

      <div className="flex items-center gap-3">
        <button
          type="button"
          className={cn(
            "hidden items-center gap-2 rounded-full border border-slate-700/90 bg-slate-900/70 px-3 py-1.5 text-[11px] text-slate-300",
            "shadow-[0_0_0_1px_rgba(15,23,42,1),0_10px_40px_rgba(15,23,42,0.9)]",
            "hover:border-cyan-400 hover:text-cyan-200 sm:flex"
          )}
        >
          <Search className="h-3 w-3" />
          <span>Ask anything</span>
          <span className="ml-2 flex items-center gap-1 rounded-full bg-slate-800/80 px-2 py-0.5 text-[9px] text-slate-400">
            <Command className="h-3 w-3" />
            <span>K</span>
          </span>
        </button>
      </div>
    </motion.header>
  );
}

