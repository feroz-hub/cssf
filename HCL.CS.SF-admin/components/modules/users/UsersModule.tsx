"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState, useTransition } from "react";

import { createUserAction, lockUserAction, unlockUserAction } from "@/app/admin/users/actions";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { DataTable } from "@/components/admin/DataTable";
import { FilterBar } from "@/components/admin/FilterBar";
import { PageHeader } from "@/components/admin/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/toaster";
import { notifyClientActionError } from "@/lib/clientActionErrors";
import { formatUtcDateTime } from "@/lib/utils";

type UserRow = {
  id: string;
  userName: string;
  email: string;
  mfaType: string;
  enabled: boolean;
  lockedOut: boolean;
  createdAt: string | null;
};

type Props = {
  rows: UserRow[];
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

export function UsersModule({ rows, loadError, loadErrorIsUnauthorized }: Props) {
  const { notify } = useToast();
  const router = useRouter();

  const [search, setSearch] = useState("");
  const [enabledFilter, setEnabledFilter] = useState<"all" | "enabled" | "disabled">("all");
  const [lockFilter, setLockFilter] = useState<"all" | "locked" | "unlocked">("all");
  const [createdFrom, setCreatedFrom] = useState("");
  const [createdTo, setCreatedTo] = useState("");
  const [savedViews, setSavedViews] = useState<string[]>([]);
  const [activeView, setActiveView] = useState<string>("default");
  const [sortBy, setSortBy] = useState<"userName" | "email" | "enabled" | "lockedOut" | "createdAt">("createdAt");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("desc");
  const [page, setPage] = useState(1);
  const [lockTarget, setLockTarget] = useState<UserRow | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState({
    userName: "",
    password: "",
    firstName: "",
    lastName: "",
    email: "",
    phoneNumber: ""
  });
  const [pending, startTransition] = useTransition();
  const [helpOpen, setHelpOpen] = useState(false);

  const filteredAndSorted = useMemo(
    () => {
      const filtered = rows.filter((user) => {
        const query = search.trim().toLowerCase();

        if (query) {
          const matchesText =
            user.userName.toLowerCase().includes(query) || user.email.toLowerCase().includes(query);
          if (!matchesText) {
            return false;
          }
        }

        if (enabledFilter === "enabled" && !user.enabled) {
          return false;
        }

        if (enabledFilter === "disabled" && user.enabled) {
          return false;
        }

        if (lockFilter === "locked" && !user.lockedOut) {
          return false;
        }

        if (lockFilter === "unlocked" && user.lockedOut) {
          return false;
        }

        if ((createdFrom || createdTo) && user.createdAt) {
          const created = new Date(user.createdAt);

          if (createdFrom) {
            const from = new Date(createdFrom);
            if (Number.isFinite(from.getTime()) && created < from) {
              return false;
            }
          }

          if (createdTo) {
            const to = new Date(createdTo);
            if (Number.isFinite(to.getTime()) && created > to) {
              return false;
            }
          }
        }

        if ((createdFrom || createdTo) && !user.createdAt) {
          return false;
        }

        return true;
      });

      const sorted = [...filtered].sort((a, b) => {
        const direction = sortDirection === "asc" ? 1 : -1;

        if (sortBy === "userName") {
          return a.userName.localeCompare(b.userName) * direction;
        }

        if (sortBy === "email") {
          return a.email.localeCompare(b.email) * direction;
        }

        if (sortBy === "enabled") {
          const av = a.enabled ? 1 : 0;
          const bv = b.enabled ? 1 : 0;
          return (av - bv) * direction;
        }

        if (sortBy === "lockedOut") {
          const av = a.lockedOut ? 1 : 0;
          const bv = b.lockedOut ? 1 : 0;
          return (av - bv) * direction;
        }

        if (sortBy === "createdAt") {
          const ad = a.createdAt ? new Date(a.createdAt).getTime() : 0;
          const bd = b.createdAt ? new Date(b.createdAt).getTime() : 0;
          return (ad - bd) * direction;
        }

        return 0;
      });

      return sorted;
    },
    [rows, search, enabledFilter, lockFilter, createdFrom, createdTo, sortBy, sortDirection]
  );

  const pageSize = 10;
  const totalPages = Math.max(1, Math.ceil(filteredAndSorted.length / pageSize));
  const pagedRows = filteredAndSorted.slice((page - 1) * pageSize, page * pageSize);

  // Load saved views from localStorage on mount
  useEffect(() => {
    if (typeof window === "undefined") return;
    try {
      const raw = window.localStorage.getItem("HCL.CS.SF-users-views");
      if (!raw) return;
      const parsed = JSON.parse(raw) as Record<
        string,
        {
          search?: string;
          enabledFilter?: "all" | "enabled" | "disabled";
          lockFilter?: "all" | "locked" | "unlocked";
          createdFrom?: string;
          createdTo?: string;
        }
      >;
      setSavedViews(Object.keys(parsed));
    } catch {
      // ignore
    }
  }, []);

  useEffect(() => {
    setPage(1);
  }, [search, enabledFilter, lockFilter, createdFrom, createdTo, sortBy, sortDirection]);

  const toggleSort = (field: "userName" | "email" | "enabled" | "lockedOut" | "createdAt") => {
    setSortBy((current) => {
      if (current === field) {
        setSortDirection((dir) => (dir === "asc" ? "desc" : "asc"));
        return current;
      }
      setSortDirection("asc");
      return field;
    });
  };

  const saveCurrentView = () => {
    if (typeof window === "undefined") return;
    const name = window.prompt("Name this view", activeView === "default" ? "" : activeView)?.trim();
    if (!name) {
      return;
    }

    try {
      const raw = window.localStorage.getItem("HCL.CS.SF-users-views");
      const parsed = (raw ? JSON.parse(raw) : {}) as Record<
        string,
        {
          search?: string;
          enabledFilter?: "all" | "enabled" | "disabled";
          lockFilter?: "all" | "locked" | "unlocked";
          createdFrom?: string;
          createdTo?: string;
        }
      >;
      parsed[name] = {
        search,
        enabledFilter,
        lockFilter,
        createdFrom,
        createdTo
      };
      window.localStorage.setItem("HCL.CS.SF-users-views", JSON.stringify(parsed));
      setSavedViews(Object.keys(parsed));
      setActiveView(name);
      notify(`Saved view "${name}".`, "success");
    } catch {
      notify("Could not save view.", "error");
    }
  };

  const applyView = (name: string) => {
    setActiveView(name);
    if (name === "default") {
      setSearch("");
      setEnabledFilter("all");
      setLockFilter("all");
      setCreatedFrom("");
      setCreatedTo("");
      return;
    }
    if (typeof window === "undefined") return;
    try {
      const raw = window.localStorage.getItem("HCL.CS.SF-users-views");
      const parsed = (raw ? JSON.parse(raw) : {}) as Record<
        string,
        {
          search?: string;
          enabledFilter?: "all" | "enabled" | "disabled";
          lockFilter?: "all" | "locked" | "unlocked";
          createdFrom?: string;
          createdTo?: string;
        }
      >;
      const view = parsed[name];
      if (view) {
        setSearch(view.search ?? "");
        setEnabledFilter(view.enabledFilter ?? "all");
        setLockFilter(view.lockFilter ?? "all");
        setCreatedFrom(view.createdFrom ?? "");
        setCreatedTo(view.createdTo ?? "");
      }
    } catch {
      // ignore
    }
  };

  const lockUser = () => {
    if (!lockTarget) return;
    startTransition(async () => {
      try {
        const result = await lockUserAction({ userId: lockTarget.id });
        if (result.ok) {
          notify(result.message, "success");
          setLockTarget(null);
          router.refresh();
        } else {
          notify(result.message ?? "Lock failed", "error");
        }
      } catch (error) {
        notifyClientActionError(error, notify, "Lock failed.");
      }
    });
  };

  const unlockUser = (row: UserRow) => {
    startTransition(async () => {
      try {
        const result = await unlockUserAction({ userName: row.userName });
        if (result.ok) {
          notify(result.message, "success");
          router.refresh();
        } else {
          notify(result.message ?? "Unlock failed", "error");
        }
      } catch (error) {
        notifyClientActionError(error, notify, "Unlock failed.");
      }
    });
  };

  const onSubmitCreate = () => {
    startTransition(async () => {
      try {
        const result = await createUserAction(createForm);
        if (result.ok) {
          notify(result.message, "success");
          setCreateOpen(false);
          setCreateForm({
            userName: "",
            password: "",
            firstName: "",
            lastName: "",
            email: "",
            phoneNumber: ""
          });
          router.refresh();
        } else {
          notify(result.message ?? "Create user failed", "error");
        }
      } catch (error) {
        notifyClientActionError(error, notify, "Create user failed.");
      }
    });
  };

  return (
    <>
      {loadError ? (
        <section className="card" style={{ marginBottom: "1rem" }}>
          <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
            <p className="inline-message error">{loadError}</p>
            <div className="toolbar" style={{ gap: "0.5rem" }}>
              {loadErrorIsUnauthorized ? <SignInAgainButton /> : null}
              <Button type="button" variant="secondary" onClick={() => router.refresh()}>
                Retry
              </Button>
            </div>
          </div>
        </section>
      ) : null}
      <section className="card">
        <PageHeader
          title="User Administration"
          subtitle="Search and manage users, lockout and role assignments."
          actions={
            <FilterBar>
              <Button type="button" variant="ghost" onClick={() => setHelpOpen(true)}>
                ?
              </Button>
              <select
                aria-label="Filter by enabled state"
                className="input"
                style={{ width: 160 }}
                value={enabledFilter}
                onChange={(event) =>
                  setEnabledFilter(event.target.value as "all" | "enabled" | "disabled")
                }
              >
                <option value="all">All users</option>
                <option value="enabled">Enabled only</option>
                <option value="disabled">Disabled only</option>
              </select>
              <select
                aria-label="Filter by lockout state"
                className="input"
                style={{ width: 160 }}
                value={lockFilter}
                onChange={(event) =>
                  setLockFilter(event.target.value as "all" | "locked" | "unlocked")
                }
              >
                <option value="all">All lock states</option>
                <option value="locked">Locked only</option>
                <option value="unlocked">Not locked</option>
              </select>
              <Input
                type="date"
                aria-label="Created from date"
                value={createdFrom}
                onChange={(event) => setCreatedFrom(event.target.value)}
                style={{ width: 160 }}
              />
              <Input
                type="date"
                aria-label="Created to date"
                value={createdTo}
                onChange={(event) => setCreatedTo(event.target.value)}
                style={{ width: 160 }}
              />
              <select
                aria-label="Saved user views"
                className="input"
                style={{ width: 160 }}
                value={activeView}
                onChange={(event) => applyView(event.target.value)}
              >
                <option value="default">Default view</option>
                {savedViews.map((viewName) => (
                  <option key={viewName} value={viewName}>
                    {viewName}
                  </option>
                ))}
              </select>
              <Button type="button" variant="secondary" onClick={saveCurrentView}>
                Save view
              </Button>
              <Button type="button" variant="primary" onClick={() => setCreateOpen(true)}>
                Create User
              </Button>
              <Input
                placeholder="Search email or username"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                style={{ width: 260 }}
              />
            </FilterBar>
          }
        />

        <div className="card-body">
          <DataTable
            columns={[
              {
                key: "userName",
                label: "Username",
                sortable: true,
                sortDirection: sortBy === "userName" ? sortDirection : null,
                onSort: () => toggleSort("userName")
              },
              {
                key: "email",
                label: "Email",
                sortable: true,
                sortDirection: sortBy === "email" ? sortDirection : null,
                onSort: () => toggleSort("email")
              },
              { key: "mfaType", label: "MFA Type", sortable: false },
              {
                key: "enabled",
                label: "Enabled",
                sortable: true,
                sortDirection: sortBy === "enabled" ? sortDirection : null,
                onSort: () => toggleSort("enabled")
              },
              {
                key: "lockedOut",
                label: "Locked Out",
                sortable: true,
                sortDirection: sortBy === "lockedOut" ? sortDirection : null,
                onSort: () => toggleSort("lockedOut")
              },
              {
                key: "createdAt",
                label: "Created At",
                sortable: true,
                sortDirection: sortBy === "createdAt" ? sortDirection : null,
                onSort: () => toggleSort("createdAt")
              },
              { key: "actions", label: "Actions", sortable: false }
            ]}
            empty={
              <tr>
                <td colSpan={7}>
                  <p className="inline-message">No users found.</p>
                </td>
              </tr>
            }
          >
            {pagedRows.map((row) => (
              <tr key={row.id} className="table-row">
                <td>{row.userName}</td>
                <td>{row.email}</td>
                <td>{row.mfaType}</td>
                <td>
                  <Badge className={row.enabled ? "" : "badge-danger"}>{row.enabled ? "Yes" : "No"}</Badge>
                </td>
                <td>
                  <Badge className={row.lockedOut ? "badge-danger" : "badge-muted"}>{row.lockedOut ? "Yes" : "No"}</Badge>
                </td>
                <td>{formatUtcDateTime(row.createdAt)}</td>
                <td>
                  <div className="table-actions">
                    <Link href={`/admin/users/${encodeURIComponent(row.id)}`}>
                      <Button type="button" variant="ghost">
                        View
                      </Button>
                    </Link>
                    <Link href={`/admin/users/${encodeURIComponent(row.id)}/sessions`}>
                      <Button type="button" variant="ghost">
                        Sessions
                      </Button>
                    </Link>
                    {row.lockedOut ? (
                      <Button type="button" variant="secondary" onClick={() => unlockUser(row)} disabled={pending}>
                        Unlock
                      </Button>
                    ) : (
                      <Button type="button" variant="danger" onClick={() => setLockTarget(row)} disabled={pending}>
                        Lock
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </DataTable>

          <div className="toolbar" style={{ marginTop: "0.8rem", justifyContent: "space-between" }}>
            <span className="inline-message">Page {page} of {totalPages}</span>
            <div className="toolbar">
              <Button type="button" variant="ghost" onClick={() => setPage((value) => Math.max(1, value - 1))}>
                Previous
              </Button>
              <Button
                type="button"
                variant="ghost"
                onClick={() => setPage((value) => Math.min(totalPages, value + 1))}
              >
                Next
              </Button>
            </div>
          </div>
        </div>
      </section>

      <ConfirmDialog
        open={Boolean(lockTarget)}
        title="Lock user account"
        description="User will be locked out until manually unlocked."
        confirmLabel={pending ? "Locking..." : "Lock"}
        pending={pending}
        onCancel={() => setLockTarget(null)}
        onConfirm={lockUser}
      />

      <Dialog
        open={createOpen}
        title="Create User"
        description="Register a new user account. The user will be assigned the default role."
        onClose={() => setCreateOpen(false)}
      >
        <div className="form-grid" style={{ gap: "0.75rem" }}>
          {/* Hidden inputs so the browser fills these instead of the real Create User fields */}
          <div
            style={{
              position: "absolute",
              left: "-9999px",
              width: 1,
              height: 1,
              overflow: "hidden",
              opacity: 0,
              pointerEvents: "none"
            }}
            aria-hidden="true"
          >
            <input type="text" tabIndex={-1} autoComplete="username" />
            <input type="password" tabIndex={-1} autoComplete="current-password" />
          </div>

          <div className="form-row">
            <label htmlFor="create-user-login">Username</label>
            <Input
              id="create-user-login"
              name="new_user_login"
              type="text"
              autoComplete="off"
              value={createForm.userName}
              placeholder="e.g. jdoe"
              maxLength={255}
              onChange={(e) => setCreateForm((s) => ({ ...s, userName: e.target.value }))}
            />
          </div>
          <div className="form-row">
            <label htmlFor="create-user-secret">Password</label>
            <Input
              id="create-user-secret"
              name="new_user_secret"
              type="password"
              autoComplete="new-password"
              value={createForm.password}
              placeholder="e.g. 8+ characters, no spaces"
              onChange={(e) => setCreateForm((s) => ({ ...s, password: e.target.value }))}
            />
          </div>
          <div className="form-row">
            <label htmlFor="create-firstname">First name</label>
            <Input
              id="create-firstname"
              type="text"
              value={createForm.firstName}
              placeholder="Jane"
              maxLength={255}
              onChange={(e) => setCreateForm((s) => ({ ...s, firstName: e.target.value }))}
            />
          </div>
          <div className="form-row">
            <label htmlFor="create-lastname">Last name</label>
            <Input
              id="create-lastname"
              type="text"
              value={createForm.lastName}
              placeholder="Doe"
              maxLength={255}
              onChange={(e) => setCreateForm((s) => ({ ...s, lastName: e.target.value }))}
            />
          </div>
          <div className="form-row">
            <label htmlFor="create-email">Email</label>
            <Input
              id="create-email"
              type="email"
              value={createForm.email}
              placeholder="jane@example.com"
              maxLength={255}
              onChange={(e) => setCreateForm((s) => ({ ...s, email: e.target.value }))}
            />
          </div>
          <div className="form-row">
            <label htmlFor="create-phone">Phone number (optional)</label>
            <Input
              id="create-phone"
              type="text"
              value={createForm.phoneNumber}
              placeholder="+1 555 000 0000"
              maxLength={15}
              onChange={(e) => setCreateForm((s) => ({ ...s, phoneNumber: e.target.value }))}
            />
          </div>
          <div className="toolbar" style={{ marginTop: "0.5rem", justifyContent: "flex-end", gap: "0.5rem" }}>
            <Button type="button" variant="secondary" onClick={() => setCreateOpen(false)} disabled={pending}>
              Cancel
            </Button>
            <Button type="button" variant="primary" onClick={onSubmitCreate} disabled={pending}>
              {pending ? "Creating..." : "Create User"}
            </Button>
          </div>
        </div>
      </Dialog>

      <Dialog
        open={helpOpen}
        title="About users & roles"
        description="How users, roles, and assignments work in HCL.CS.SF."
        onClose={() => setHelpOpen(false)}
      >
        <div className="form-grid">
          <p className="text-body">
            Each user can have one or more roles. Roles contain claims that drive authorization decisions across APIs
            and applications.
          </p>
          <p className="text-caption">
            You can assign or remove roles from a user from the User detail page. Changes take effect immediately for
            new sessions.
          </p>
        </div>
      </Dialog>
    </>
  );
}
