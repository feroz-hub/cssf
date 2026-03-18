"use client";

import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition } from "react";

import {
  assignUserRoleAction,
  lockUserAction,
  removeUserRoleAction,
  unlockUserAction,
  updateUserProfileAction
} from "@/app/admin/users/actions";
import { AlertDialog } from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { useToast } from "@/components/ui/toaster";
import { notifyClientActionError } from "@/lib/clientActionErrors";
import { type RoleModel, type UserModel } from "@/lib/types/HCL.CS.SF";
import { formatUtcDateTime } from "@/lib/utils";

type Props = {
  user: UserModel;
  roles: RoleModel[];
  userRoles: string[] | null | undefined;
};

export function UserDetailModule({ user, roles, userRoles }: Props) {
  const router = useRouter();
  const { notify } = useToast();

  const [form, setForm] = useState({
    firstName: user.FirstName,
    lastName: user.LastName,
    email: user.Email,
    phoneNumber: user.PhoneNumber,
    twoFactorEnabled: user.TwoFactorEnabled,
    forcePasswordReset: Boolean(user.RequiresDefaultPasswordChange)
  });
  const [selectedRoleId, setSelectedRoleId] = useState("");
  const [lockConfirmOpen, setLockConfirmOpen] = useState(false);
  const [removeRoleName, setRemoveRoleName] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const safeUserRoles = Array.isArray(userRoles) ? userRoles : [];

  const rolesByName = useMemo(() => {
    const map = new Map<string, string>();
    for (const role of roles) {
      map.set(role.Name, role.Id);
    }

    return map;
  }, [roles]);

  const availableRoles = roles.filter((role) => !safeUserRoles.includes(role.Name));

  const saveProfile = () => {
    startTransition(async () => {
      try {
        const result = await updateUserProfileAction({
          id: user.Id,
          ...form
        });

        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "User update failed.");
      }
    });
  };

  const assignRole = () => {
    if (!selectedRoleId) {
      notify("Select a role first.", "error");
      return;
    }

    startTransition(async () => {
      try {
        const result = await assignUserRoleAction({
          userId: user.Id,
          roleId: selectedRoleId
        });

        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        setSelectedRoleId("");
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Assign role failed.");
      }
    });
  };

  const confirmRemoveRole = () => {
    if (!removeRoleName) {
      return;
    }

    const roleId = rolesByName.get(removeRoleName);
    if (!roleId) {
      notify("Role identifier not found.", "error");
      setRemoveRoleName(null);
      return;
    }

    startTransition(async () => {
      try {
        const result = await removeUserRoleAction({
          userId: user.Id,
          roleId
        });

        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        setRemoveRoleName(null);
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Remove role failed.");
      }
    });
  };

  const lockUser = () => {
    startTransition(async () => {
      try {
        const result = await lockUserAction({ userId: user.Id });
        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        setLockConfirmOpen(false);
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Lock user failed.");
      }
    });
  };

  const unlockUser = () => {
    startTransition(async () => {
      try {
        const result = await unlockUserAction({ userName: user.UserName });
        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Unlock user failed.");
      }
    });
  };

  return (
    <>
      <section className="card">
        <header className="card-head">
          <div>
            <h2>{user.UserName}</h2>
            <p className="inline-message">User ID: {user.Id}</p>
          </div>
          <div className="toolbar">
            {user.LockoutEnabled ? (
              <Button type="button" variant="secondary" onClick={unlockUser} disabled={pending}>
                Unlock
              </Button>
            ) : (
              <Button type="button" variant="danger" onClick={() => setLockConfirmOpen(true)} disabled={pending}>
                Lock
              </Button>
            )}
          </div>
        </header>

        <div className="card-body" style={{ display: "grid", gap: "1rem" }}>
          <div className="form-grid">
            <div className="form-row">
              <label>First Name</label>
              <Input
                value={form.firstName}
                onChange={(event) => setForm((state) => ({ ...state, firstName: event.target.value }))}
              />
            </div>
            <div className="form-row">
              <label>Last Name</label>
              <Input
                value={form.lastName}
                onChange={(event) => setForm((state) => ({ ...state, lastName: event.target.value }))}
              />
            </div>
            <div className="form-row">
              <label>Email</label>
              <Input
                value={form.email}
                onChange={(event) => setForm((state) => ({ ...state, email: event.target.value }))}
              />
            </div>
            <div className="form-row">
              <label>Phone Number</label>
              <Input
                value={form.phoneNumber}
                maxLength={15}
                onChange={(event) => setForm((state) => ({ ...state, phoneNumber: event.target.value }))}
              />
            </div>
            <label>
              <input
                type="checkbox"
                checked={form.twoFactorEnabled}
                onChange={(event) => setForm((state) => ({ ...state, twoFactorEnabled: event.target.checked }))}
              />{" "}
              Two factor enabled
            </label>
            <label>
              <input
                type="checkbox"
                checked={form.forcePasswordReset}
                onChange={(event) => setForm((state) => ({ ...state, forcePasswordReset: event.target.checked }))}
              />{" "}
              Force password reset
            </label>
            <div>
              <Button type="button" onClick={saveProfile} disabled={pending}>
                {pending ? "Saving..." : "Save Profile"}
              </Button>
            </div>
          </div>

          <div className="card">
            <div className="card-head">
              <h3>Role assignment</h3>
              {safeUserRoles.length > 0 ? (
                <Badge>{safeUserRoles.length} role{safeUserRoles.length !== 1 ? "s" : ""}</Badge>
              ) : null}
            </div>
            <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
              {safeUserRoles.length === 0 ? (
                <p className="inline-message">No roles assigned. Use the form below to assign a role.</p>
              ) : (
                <div className="toolbar">
                  {safeUserRoles.map((roleName) => (
                    <span key={roleName} className="badge">
                      {roleName}
                      <button
                        type="button"
                        onClick={() => setRemoveRoleName(roleName)}
                        style={{ marginLeft: "0.5rem", border: 0, background: "transparent", cursor: "pointer" }}
                      >
                        ×
                      </button>
                    </span>
                  ))}
                </div>
              )}

              {availableRoles.length === 0 ? (
                <p className="inline-message">All roles are already assigned to this user.</p>
              ) : (
                <div className="toolbar">
                  <Select value={selectedRoleId} onChange={(event) => setSelectedRoleId(event.target.value)}>
                    <option value="">Select role to assign</option>
                    {availableRoles.map((role) => (
                      <option key={role.Id} value={role.Id}>
                        {role.Name}
                      </option>
                    ))}
                  </Select>
                  <Button type="button" variant="secondary" onClick={assignRole} disabled={pending || !selectedRoleId}>
                    Add role
                  </Button>
                </div>
              )}
            </div>
          </div>

          <div className="toolbar">
            <Badge className={user.LockoutEnabled ? "badge-danger" : "badge-muted"}>
              Locked out: {user.LockoutEnabled ? "Yes" : "No"}
            </Badge>
            <Badge>MFA Type: {user.TwoFactorType}</Badge>
            <Badge>Created: {formatUtcDateTime(user.CreatedOn)}</Badge>
          </div>
        </div>
      </section>

      <AlertDialog
        open={lockConfirmOpen}
        title="Lock user"
        description="The user account will be disabled until manually unlocked."
        confirmLabel={pending ? "Locking..." : "Lock"}
        onCancel={() => setLockConfirmOpen(false)}
        onConfirm={lockUser}
      />

      <AlertDialog
        open={Boolean(removeRoleName)}
        title="Remove role assignment"
        description={
          removeRoleName
            ? `Remove role "${removeRoleName}" from this user?`
            : "Remove this role assignment from the user?"
        }
        confirmLabel={pending ? "Removing..." : "Remove"}
        onCancel={() => setRemoveRoleName(null)}
        onConfirm={confirmRemoveRole}
      />
    </>
  );
}
