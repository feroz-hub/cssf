type Notify = (message: string, kind?: "success" | "error" | "info") => void;

const STALE_SERVER_ACTION_MARKER = "Failed to find Server Action";

export function isStaleServerActionError(error: unknown): error is Error {
  return error instanceof Error && error.message.includes(STALE_SERVER_ACTION_MARKER);
}

export function notifyClientActionError(
  error: unknown,
  notify: Notify,
  fallbackMessage: string
): void {
  if (isStaleServerActionError(error)) {
    notify("A newer admin deployment is live. Reloading this page to sync server actions.", "error");
    if (typeof window !== "undefined") {
      window.setTimeout(() => window.location.reload(), 250);
    }
    return;
  }

  notify(error instanceof Error ? error.message : fallbackMessage, "error");
}
