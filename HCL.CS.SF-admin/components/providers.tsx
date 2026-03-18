"use client";

import { type Session } from "next-auth";
import { SessionProvider } from "next-auth/react";
import { type ReactNode } from "react";

import { ToasterProvider } from "@/components/ui/toaster";
import { DensityProvider } from "@/context/DensityContext";
import { CommandPaletteProvider } from "@/context/CommandPaletteContext";

type ProvidersProps = {
  children: ReactNode;
  session: Session | null;
};

export function Providers({ children, session }: ProvidersProps) {
  return (
    <SessionProvider session={session}>
      <DensityProvider>
        <CommandPaletteProvider>
          <ToasterProvider>{children}</ToasterProvider>
        </CommandPaletteProvider>
      </DensityProvider>
    </SessionProvider>
  );
}
