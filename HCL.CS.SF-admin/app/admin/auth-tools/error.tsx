"use client";

import { Button } from "@/components/ui/button";

export default function AuthToolsError({ error, reset }: { error: Error; reset: () => void }) {
  return (
    <section className="card">
      <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
        <h2>Auth tools failed</h2>
        <p className="inline-message error">{error.message}</p>
        <div className="toolbar" style={{ gap: "0.5rem" }}>
          <Button type="button" variant="secondary" onClick={reset}>
            Retry
          </Button>
        </div>
      </div>
    </section>
  );
}

