"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";

import {
  addRoleClaimAction,
  addRoleClaimsBulkAction,
  createRoleAction,
  deleteRoleAction,
  removeRoleClaimAction,
  updateRoleAction
} from "@/app/admin/roles/actions";
import { removeUserRoleAction } from "@/app/admin/users/actions";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { AlertDialog } from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/toaster";
import { type RoleModel, type UserModel } from "@/lib/types/HCL.CS.SF";

type RoleRow = {
  role: RoleModel;
  claimsCount: number;
  usersCount: number;
  usersInRole: UserModel[];
};

type Props = {
  rows: RoleRow[];
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

type RoleForm = {
  id: string;
  name: string;
  description: string;
};

const defaultRoleForm: RoleForm = {
  id: "00000000-0000-0000-0000-000000000000",
  name: "",
  description: ""
};

export function RolesModule({ rows, loadError, loadErrorIsUnauthorized }: Props) {
  const router = useRouter();
  const { notify } = useToast();

  const [expandedRoleId, setExpandedRoleId] = useState<string | null>(null);
  const [expandedUsersRoleId, setExpandedUsersRoleId] = useState<string | null>(null);
  const [roleModalOpen, setRoleModalOpen] = useState(false);
  const [roleForm, setRoleForm] = useState<RoleForm>(defaultRoleForm);
  const [deleteRoleId, setDeleteRoleId] = useState<string | null>(null);
  const [deleteClaimId, setDeleteClaimId] = useState<number | null>(null);
  const [removeUserFromRole, setRemoveUserFromRole] = useState<{ userId: string; roleId: string; userName: string; roleName: string } | null>(null);
  const [claimInputs, setClaimInputs] = useState<Record<string, { type: string; value: string }>>({});
  const [bulkClaimInputs, setBulkClaimInputs] = useState<
    Record<string, { claimType: string; claimValuesText: string }>
  >({});
  const [pending, startTransition] = useTransition();
  const [helpOpen, setHelpOpen] = useState(false);

  const openCreate = () => {
    setRoleForm(defaultRoleForm);
    setRoleModalOpen(true);
  };

  const openEdit = (role: RoleModel) => {
    setRoleForm({
      id: role.Id,
      name: role.Name,
      description: role.Description
    });
    setRoleModalOpen(true);
  };

  const saveRole = () => {
    startTransition(async () => {
      const action =
        roleForm.id === "00000000-0000-0000-0000-000000000000" ? createRoleAction : updateRoleAction;

      const result = await action(roleForm);
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setRoleModalOpen(false);
      router.refresh();
    });
  };

  const submitClaim = (roleId: string) => {
    const claim = claimInputs[roleId];
    if (!claim || !claim.type || !claim.value) {
      notify("Claim type and value are required.", "error");
      return;
    }

    startTransition(async () => {
      const result = await addRoleClaimAction({
        roleId,
        claimType: claim.type,
        claimValue: claim.value
      });

      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setClaimInputs((state) => ({
        ...state,
        [roleId]: { type: "", value: "" }
      }));
      router.refresh();
    });
  };

  const submitBulkClaims = (roleId: string) => {
    const bulk = bulkClaimInputs[roleId];
    if (!bulk?.claimType?.trim() || !bulk?.claimValuesText?.trim()) {
      notify("Claim type and at least one claim value are required.", "error");
      return;
    }

    startTransition(async () => {
      const result = await addRoleClaimsBulkAction({
        roleId,
        claimType: bulk.claimType.trim(),
        claimValuesText: bulk.claimValuesText
      });

      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setBulkClaimInputs((state) => ({
        ...state,
        [roleId]: { claimType: "capabilities", claimValuesText: "" }
      }));
      router.refresh();
    });
  };

  const confirmDeleteRole = () => {
    if (!deleteRoleId) {
      return;
    }

    startTransition(async () => {
      const result = await deleteRoleAction({ id: deleteRoleId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setDeleteRoleId(null);
      router.refresh();
    });
  };

  const confirmDeleteClaim = () => {
    if (!deleteClaimId) {
      return;
    }

    startTransition(async () => {
      const result = await removeRoleClaimAction({ claimId: deleteClaimId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setDeleteClaimId(null);
      router.refresh();
    });
  };

  const confirmRemoveUserFromRole = () => {
    if (!removeUserFromRole) {
      return;
    }

    startTransition(async () => {
      const result = await removeUserRoleAction({
        userId: removeUserFromRole.userId,
        roleId: removeUserFromRole.roleId
      });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setRemoveUserFromRole(null);
      router.refresh();
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
        <header className="card-head">
          <div>
            <h2>Roles & Claims</h2>
            <p className="inline-message">Manage roles and attached claim mappings.</p>
          </div>
          <div className="toolbar">
            <Button type="button" variant="ghost" onClick={() => setHelpOpen(true)}>
              ?
            </Button>
            <Button type="button" variant="secondary" onClick={openCreate}>
              Create Role
            </Button>
          </div>
        </header>

        <div className="card-body">
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Claims Count</th>
                  <th>Users Count</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {(rows ?? []).map((row) => {
                  const expanded = expandedRoleId === row.role.Id;

                  return (
                    <tr key={row.role.Id} className="table-row">
                      <td>{row.role.Name}</td>
                      <td>{row.role.Description}</td>
                      <td>{row.claimsCount}</td>
                      <td>{row.usersCount}</td>
                      <td>
                        <div className="table-actions">
                          <Button
                            type="button"
                            variant="ghost"
                            onClick={() => setExpandedRoleId(expanded ? null : row.role.Id)}
                          >
                            {expanded ? "Hide Claims" : "Claims"}
                          </Button>
                          <Button
                            type="button"
                            variant="ghost"
                            onClick={() =>
                              setExpandedUsersRoleId(expandedUsersRoleId === row.role.Id ? null : row.role.Id)
                            }
                          >
                            {expandedUsersRoleId === row.role.Id ? "Hide Users" : "Users"}
                          </Button>
                          <Button type="button" variant="ghost" onClick={() => openEdit(row.role)}>
                            Edit
                          </Button>
                          <Button type="button" variant="danger" onClick={() => setDeleteRoleId(row.role.Id)}>
                            Delete
                          </Button>
                        </div>

                        {expanded ? (
                          <div className="card" style={{ marginTop: "0.8rem" }}>
                            <div className="card-head">
                              <h3>Claims</h3>
                              <Badge>{row.role.RoleClaims?.length ?? 0}</Badge>
                            </div>
                            <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
                              <div className="table-wrap">
                                <table className="table">
                                  <thead>
                                    <tr className="table-row">
                                      <th>Type</th>
                                      <th>Value</th>
                                      <th>Action</th>
                                    </tr>
                                  </thead>
                                  <tbody>
                                    {(row.role.RoleClaims ?? []).map((claim) => (
                                      <tr key={claim.Id} className="table-row">
                                        <td>{claim.ClaimType}</td>
                                        <td>{claim.ClaimValue}</td>
                                        <td>
                                          <Button
                                            type="button"
                                            variant="danger"
                                            onClick={() => setDeleteClaimId(claim.Id)}
                                          >
                                            Remove
                                          </Button>
                                        </td>
                                      </tr>
                                    ))}
                                  </tbody>
                                </table>
                              </div>

                              <div className="form-grid">
                                <div className="form-row">
                                  <label>Claim Type</label>
                                  <Input
                                    value={claimInputs[row.role.Id]?.type ?? ""}
                                    onChange={(event) =>
                                      setClaimInputs((state) => ({
                                        ...state,
                                        [row.role.Id]: {
                                          type: event.target.value,
                                          value: state[row.role.Id]?.value ?? ""
                                        }
                                      }))
                                    }
                                  />
                                </div>
                                <div className="form-row">
                                  <label>Claim Value</label>
                                  <Input
                                    value={claimInputs[row.role.Id]?.value ?? ""}
                                    onChange={(event) =>
                                      setClaimInputs((state) => ({
                                        ...state,
                                        [row.role.Id]: {
                                          type: state[row.role.Id]?.type ?? "",
                                          value: event.target.value
                                        }
                                      }))
                                    }
                                  />
                                </div>
                                <div>
                                  <Button
                                    type="button"
                                    variant="secondary"
                                    onClick={() => submitClaim(row.role.Id)}
                                  >
                                    Add Claim
                                  </Button>
                                </div>
                              </div>

                              <hr style={{ margin: "0.8rem 0" }} />
                              <div>
                                <h4 style={{ marginBottom: "0.5rem" }}>Bulk add claims</h4>
                                <p className="text-muted" style={{ marginBottom: "0.5rem", fontSize: "0.9rem" }}>
                                  One claim value per line, or comma-separated (e.g. capabilities like <code>health:read</code>, <code>tenant:read</code>).
                                </p>
                                <div className="form-grid" style={{ gap: "0.5rem" }}>
                                  <div className="form-row">
                                    <label>Claim type</label>
                                    <Input
                                      placeholder="e.g. capabilities"
                                      value={bulkClaimInputs[row.role.Id]?.claimType ?? "capabilities"}
                                      onChange={(e) =>
                                        setBulkClaimInputs((s) => ({
                                          ...s,
                                          [row.role.Id]: {
                                            claimType: e.target.value,
                                            claimValuesText: s[row.role.Id]?.claimValuesText ?? ""
                                          }
                                        }))
                                      }
                                    />
                                  </div>
                                  <div className="form-row">
                                    <label>Claim values</label>
                                    <Textarea
                                      placeholder={"health:read\ntenant:read\ntenant:write"}
                                      rows={4}
                                      value={bulkClaimInputs[row.role.Id]?.claimValuesText ?? ""}
                                      onChange={(e) =>
                                        setBulkClaimInputs((s) => ({
                                          ...s,
                                          [row.role.Id]: {
                                            claimType: s[row.role.Id]?.claimType ?? "capabilities",
                                            claimValuesText: e.target.value
                                          }
                                        }))
                                      }
                                    />
                                  </div>
                                  <div>
                                    <Button
                                      type="button"
                                      variant="secondary"
                                      disabled={pending}
                                      onClick={() => submitBulkClaims(row.role.Id)}
                                    >
                                      Add all
                                    </Button>
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        ) : null}

                        {expandedUsersRoleId === row.role.Id ? (
                          <div className="card" style={{ marginTop: "0.8rem" }}>
                            <div className="card-head">
                              <h3>Users in this role</h3>
                              <Badge>{(row.usersInRole ?? []).length}</Badge>
                            </div>
                            <div className="card-body">
                              {(row.usersInRole ?? []).length === 0 ? (
                                <p className="inline-message">No users assigned to this role.</p>
                              ) : (
                                <div className="table-wrap">
                                  <table className="table">
                                    <thead>
                                      <tr className="table-row">
                                        <th>User</th>
                                        <th>Action</th>
                                      </tr>
                                    </thead>
                                    <tbody>
                                      {(row.usersInRole ?? []).map((user) => (
                                        <tr key={user.Id} className="table-row">
                                          <td>
                                            <Link href={`/admin/users/${encodeURIComponent(user.Id)}`}>
                                              {user.UserName}
                                            </Link>
                                            {user.Email ? (
                                              <span className="inline-message" style={{ marginLeft: "0.5rem" }}>
                                                {user.Email}
                                              </span>
                                            ) : null}
                                          </td>
                                          <td>
                                            <Button
                                              type="button"
                                              variant="danger"
                                              onClick={() =>
                                                setRemoveUserFromRole({
                                                  userId: user.Id,
                                                  roleId: row.role.Id,
                                                  userName: user.UserName,
                                                  roleName: row.role.Name
                                                })
                                              }
                                            >
                                              Remove from role
                                            </Button>
                                          </td>
                                        </tr>
                                      ))}
                                    </tbody>
                                  </table>
                                </div>
                              )}
                            </div>
                          </div>
                        ) : null}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <Dialog
        open={roleModalOpen}
        title={roleForm.id === "00000000-0000-0000-0000-000000000000" ? "Create Role" : "Edit Role"}
        onClose={() => setRoleModalOpen(false)}
      >
        <div className="form-grid">
          <div className="form-row">
            <label>Name</label>
            <Input
              value={roleForm.name}
              onChange={(event) => setRoleForm((state) => ({ ...state, name: event.target.value }))}
            />
          </div>
          <div className="form-row">
            <label>Description</label>
            <Textarea
              value={roleForm.description}
              onChange={(event) => setRoleForm((state) => ({ ...state, description: event.target.value }))}
            />
          </div>
          <div className="dialog-actions">
            <Button type="button" onClick={saveRole} disabled={pending}>
              {pending ? "Saving..." : "Save"}
            </Button>
          </div>
        </div>
      </Dialog>

      <Dialog
        open={helpOpen}
        title="About roles & claims"
        description="How roles and claims work in HCL.CS.SF."
        onClose={() => setHelpOpen(false)}
      >
        <div className="form-grid">
          <p className="text-body">
            Roles group permissions. Claims are key/value pairs attached to roles and users. When you assign a role to a
            user, the user inherits the role&apos;s claims and any policies that depend on them.
          </p>
          <p className="text-caption">
            Tip: Keep roles coarse-grained (e.g. Admin, Auditor, Support) and use claims for fine-grained checks.
          </p>
        </div>
      </Dialog>

      <AlertDialog
        open={Boolean(deleteRoleId)}
        title="Delete role"
        description={
          deleteRoleId
            ? (() => {
                const roleToDelete = (rows ?? []).find((row) => row.role.Id === deleteRoleId);
                if (!roleToDelete) return "This removes the role mapping from future assignments.";

                const users = roleToDelete.usersCount;
                const claims = roleToDelete.claimsCount;
                return `This role is currently assigned to ${users} user${users === 1 ? "" : "s"} and has ${claims} claim${
                  claims === 1 ? "" : "s"
                }. Deleting it will remove these assignments and claims.`;
              })()
            : "This removes the role mapping from future assignments."
        }
        confirmLabel={pending ? "Deleting..." : "Delete"}
        onCancel={() => setDeleteRoleId(null)}
        onConfirm={confirmDeleteRole}
      />

      <AlertDialog
        open={Boolean(deleteClaimId)}
        title="Remove role claim"
        description="Claim entry will be deleted from this role."
        confirmLabel={pending ? "Removing..." : "Remove"}
        onCancel={() => setDeleteClaimId(null)}
        onConfirm={confirmDeleteClaim}
      />

      <AlertDialog
        open={Boolean(removeUserFromRole)}
        title="Remove user from role"
        description={
          removeUserFromRole
            ? `Remove "${removeUserFromRole.userName}" from role "${removeUserFromRole.roleName}"?`
            : "Remove this user from the role?"
        }
        confirmLabel={pending ? "Removing..." : "Remove"}
        onCancel={() => setRemoveUserFromRole(null)}
        onConfirm={confirmRemoveUserFromRole}
      />
    </>
  );
}
