"use client";

"use client";

import { motion } from "framer-motion";

export function AmbientBackground() {
  return (
    <>
      <div className="pointer-events-none fixed inset-0 bg-slate-950" />
      <div className="pointer-events-none fixed inset-0 bg-[radial-gradient(circle_at_0%_0%,rgba(34,211,238,0.32),transparent_55%),radial-gradient(circle_at_100%_100%,rgba(129,140,248,0.28),transparent_55%)]" />
      <motion.div
        aria-hidden="true"
        className="pointer-events-none fixed inset-0 bg-[radial-gradient(circle_at_50%_120%,rgba(45,212,191,0.22),transparent_60%)] opacity-60"
        animate={{ opacity: [0.4, 0.8, 0.4] }}
        transition={{ duration: 22, repeat: Infinity, repeatType: "mirror" }}
      />
      <div className="pointer-events-none fixed inset-0 bg-[linear-gradient(to_bottom,rgba(15,23,42,0.8),rgba(15,23,42,0.96))]" />
      <div className="pointer-events-none fixed inset-0 bg-[radial-gradient(circle_at_0_0,#1e293b_1px,transparent_0)] bg-[size:32px_32px] opacity-30" />
    </>
  );
}

