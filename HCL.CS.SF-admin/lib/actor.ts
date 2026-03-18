import { MAX_LENGTH_255 } from "@/lib/constants";

/** Session-like shape used for actor resolution (avoids circular dependency on auth). */
export type SessionLike = { user?: { name?: string | null; email?: string | null } } | null;

/**
 * Returns the current actor name for audit fields (CreatedBy/ModifiedBy).
 * Truncated to server limit (255) to avoid validation errors.
 */
export function getActor(session: SessionLike, maxLength: number = MAX_LENGTH_255): string {
  const raw = session?.user?.name ?? session?.user?.email ?? "HCL.CS.SF-admin";
  const s = typeof raw === "string" ? raw : "HCL.CS.SF-admin";
  return s.length > maxLength ? s.slice(0, maxLength) : s;
}
