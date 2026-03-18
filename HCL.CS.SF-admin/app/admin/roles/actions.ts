"use server";

import { z } from "zod";

import {
  addRoleClaim,
  addRoleClaims,
  createRole,
  deleteRole,
  getRole,
  removeRoleClaim,
  updateRole
} from "@/lib/api/roles";
import { getActor } from "@/lib/actor";
import { auth } from "@/lib/auth";
import { MAX_LENGTH_255 } from "@/lib/constants";
import { type ActionResult, type RoleModel } from "@/lib/types/HCL.CS.SF";

const ZERO_GUID = "00000000-0000-0000-0000-000000000000";

const roleSchema = z.object({
  id: z.string().default(ZERO_GUID),
  name: z.string().min(2).max(MAX_LENGTH_255),
  description: z.string().default("")
});

const deleteSchema = z.object({
  id: z.string().min(1)
});

const claimSchema = z.object({
  roleId: z.string().min(1),
  claimType: z.string().min(1).max(MAX_LENGTH_255),
  claimValue: z.string().min(1).max(MAX_LENGTH_255)
});

const removeClaimSchema = z.object({
  claimId: z.number().int().positive()
});

const bulkClaimsSchema = z.object({
  roleId: z.string().uuid(),
  claimType: z.string().min(1).max(MAX_LENGTH_255),
  claimValuesText: z.string().min(1)
});

export async function createRoleAction(input: z.infer<typeof roleSchema>): Promise<ActionResult> {
  const parsed = roleSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid role input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const session = await auth();
  const actor = getActor(session);

  const model: RoleModel = {
    Id: ZERO_GUID,
    Name: parsed.data.name,
    Description: parsed.data.description,
    RoleClaims: [],
    CreatedBy: actor,
    CreatedOn: new Date().toISOString(),
    IsDeleted: false
  };

  try {
    await createRole(model);
    return { ok: true, message: "Role created." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Role create failed." };
  }
}

export async function updateRoleAction(input: z.infer<typeof roleSchema>): Promise<ActionResult> {
  const parsed = roleSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid role input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const existing = await getRole(parsed.data.id);
    const session = await auth();
    const actor = getActor(session);

    const model: RoleModel = {
      ...existing,
      Name: parsed.data.name,
      Description: parsed.data.description,
      ModifiedBy: actor,
      ModifiedOn: new Date().toISOString()
    };

    await updateRole(model);
    return { ok: true, message: "Role updated." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Role update failed." };
  }
}

export async function deleteRoleAction(input: { id: string }): Promise<ActionResult> {
  const parsed = deleteSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid role identifier." };
  }

  try {
    await deleteRole(parsed.data.id);
    return { ok: true, message: "Role deleted." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Role delete failed." };
  }
}

export async function addRoleClaimAction(input: z.infer<typeof claimSchema>): Promise<ActionResult> {
  const parsed = claimSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid claim input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const session = await auth();
  const actor = getActor(session);

  try {
    await addRoleClaim({
      Id: 0,
      RoleId: parsed.data.roleId,
      ClaimType: parsed.data.claimType,
      ClaimValue: parsed.data.claimValue,
      CreatedBy: actor,
      CreatedOn: new Date().toISOString(),
      IsDeleted: false
    });

    return { ok: true, message: "Role claim added." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Add role claim failed." };
  }
}

export async function removeRoleClaimAction(input: { claimId: number }): Promise<ActionResult> {
  const parsed = removeClaimSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid claim identifier." };
  }

  try {
    await removeRoleClaim(parsed.data.claimId);
    return { ok: true, message: "Role claim removed." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Remove role claim failed." };
  }
}

/**
 * Add multiple role claims at once. claimValuesText is split by newlines or commas; each non-empty trimmed line is one claim value.
 */
export async function addRoleClaimsBulkAction(
  input: z.infer<typeof bulkClaimsSchema>
): Promise<ActionResult> {
  const parsed = bulkClaimsSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid bulk claim input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const values = parsed.data.claimValuesText
    .split(/[\n,]+/)
    .map((v) => v.trim())
    .filter(Boolean);
  if (values.length === 0) {
    return { ok: false, message: "Enter at least one claim value (one per line or comma-separated)." };
  }

  const session = await auth();
  const actor = getActor(session);
  const now = new Date().toISOString();

  const models = values.map((claimValue) => ({
    Id: 0,
    RoleId: parsed.data.roleId,
    ClaimType: parsed.data.claimType,
    ClaimValue: claimValue,
    CreatedBy: actor,
    CreatedOn: now,
    IsDeleted: false
  }));

  try {
    await addRoleClaims(models);
    return { ok: true, message: `Added ${models.length} claim(s).` };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Bulk add role claims failed."
    };
  }
}
