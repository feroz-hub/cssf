"use client";

import Link from "next/link";

import { Button } from "@/components/ui/button";

export default function RootError({ error, reset }: { error: Error; reset: () => void }) {
  return (
    <main className="admin-content" style={{ padding: "2rem", maxWidth: 560 }}>
      <section className="card">
        <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
          <h2>Something went wrong</h2>
          <p className="inline-message error">{error.message}</p>
          <div className="toolbar" style={{ gap: "0.5rem" }}>
            <Button type="button" variant="secondary" onClick={reset}>
              Retry
            </Button>
            <Link href="/admin">
              <Button type="button" variant="ghost">
                Back to Admin
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
