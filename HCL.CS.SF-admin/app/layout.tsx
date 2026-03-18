import type { Metadata } from "next";
import { DM_Sans, Syne, JetBrains_Mono } from "next/font/google";

import { Providers } from "@/components/providers";
import { auth } from "@/lib/auth";

import "./globals.css";

export const metadata: Metadata = {
  title: "HCL.CS.SF Admin",
  description: "Administrative console for HCL.CS.SF identity platform"
};

const fontDisplay = Syne({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-display"
});

const fontBody = DM_Sans({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-body"
});

const fontMono = JetBrains_Mono({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-mono"
});

export default async function RootLayout({ children }: { children: React.ReactNode }) {
  const session = await auth();

  return (
    <html lang="en" className={`${fontDisplay.variable} ${fontBody.variable} ${fontMono.variable}`}>
      <body>
        <Providers session={session}>{children}</Providers>
      </body>
    </html>
  );
}
