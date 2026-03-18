"use client";

export default function GlobalError({ error, reset }: { error: Error; reset: () => void }) {
  return (
    <html lang="en">
      <body style={{ fontFamily: "system-ui, sans-serif", padding: "2rem", margin: 0 }}>
        <main style={{ maxWidth: 560 }}>
          <h1 style={{ fontSize: "1.25rem", marginBottom: "0.5rem" }}>Something went wrong</h1>
          <p style={{ color: "#b91c1c", marginBottom: "1rem" }}>{error.message}</p>
          <div style={{ display: "flex", gap: "0.5rem" }}>
            <button
              type="button"
              onClick={() => reset()}
              style={{
                padding: "0.5rem 1rem",
                backgroundColor: "#e5e7eb",
                border: "1px solid #d1d5db",
                borderRadius: 4,
                cursor: "pointer"
              }}
            >
              Retry
            </button>
            <a
              href="/login"
              style={{
                padding: "0.5rem 1rem",
                color: "#374151",
                textDecoration: "none",
                border: "1px solid #d1d5db",
                borderRadius: 4
              }}
            >
              Go to login
            </a>
          </div>
        </main>
      </body>
    </html>
  );
}
