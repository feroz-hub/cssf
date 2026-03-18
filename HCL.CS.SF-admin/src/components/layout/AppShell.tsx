"use client";

import { AmbientBackground } from "@/src/components/layout/AmbientBackground";
import { IntelligentNav } from "@/src/components/layout/IntelligentNav";
import { CommandBar } from "@/src/components/layout/CommandBar";
import { cn } from "@/lib/utils";

type AppShellProps = {
  children: React.ReactNode;
  title?: string;
  subtitle?: string;
  className?: string;
};

export function AppShell({ children, title, subtitle, className }: AppShellProps) {
  return (
    <div className="relative min-h-screen overflow-hidden">
      <AmbientBackground />
      <div className="relative z-10 flex min-h-screen">
        <IntelligentNav />
        <main className="flex-1">
          <div className="mx-auto flex h-full max-w-7xl flex-col gap-4 px-4 py-4 sm:px-6 lg:px-8">
            <CommandBar title={title} subtitle={subtitle} />
            <section
              className={cn(
                "grid flex-1 grid-cols-1 gap-4 pb-6 lg:grid-cols-[minmax(0,3fr)_minmax(0,2fr)]",
                className
              )}
            >
              {children}
            </section>
          </div>
        </main>
      </div>
    </div>
  );
}

