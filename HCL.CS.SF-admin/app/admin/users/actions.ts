"use server";

import { z } from "zod";

import { bulkRevokeTokens, revokeToken } from "@/lib/api/revocation";
import {
  addUserRole,
  getUser,
  listUserActiveTokens,
  lockUser,
  registerUser,
  removeUserRole,
  unlockUser,
  updateUser
} from "@/lib/api/users";
import { getActor } from "@/lib/actor";
import { auth } from "@/lib/auth";
import { MAX_LENGTH_255 } from "@/lib/constants";
import { type ActionResult, type UserModel, type UserRoleModel } from "@/lib/types/HCL.CS.SF";

const ZERO_GUID = "00000000-0000-0000-0000-000000000000";
const MAX_PHONE_NUMBER_LENGTH = 15;

const createUserSchema = z.object({
  userName: z.string().min(2, "Username must be at least 2 characters").max(MAX_LENGTH_255),
  password: z
    .string()
    .min(8, "Password must be at least 8 characters")
    .max(128)
    .refine((val) => !val.includes(" "), "Password must not contain spaces"),
  firstName: z.string().min(1, "First name is required").max(MAX_LENGTH_255),
  lastName: z.string().max(MAX_LENGTH_255).default(""),
  email: z.string().email("Invalid email").max(MAX_LENGTH_255),
  phoneNumber: z.string().max(MAX_PHONE_NUMBER_LENGTH).default("")
});

const updateSchema = z.object({
  id: z.string().min(1),
  firstName: z.string().min(1),
  lastName: z.string().default(""),
  email: z.string().email().max(MAX_LENGTH_255),
  phoneNumber: z.string().max(MAX_PHONE_NUMBER_LENGTH).default(""),
  twoFactorEnabled: z.boolean().default(false),
  forcePasswordReset: z.boolean().default(false)
});

const lockSchema = z.object({
  userId: z.string().min(1)
});

const unlockSchema = z.object({
  userName: z.string().min(1)
});

const roleSchema = z.object({
  userId: z.string().min(1),
  roleId: z.string().min(1)
});

const revokeSchema = z.object({
  token: z.string().min(1),
  tokenTypeHint: z.enum(["access_token", "refresh_token"])
});

function buildUserModelForCreate(
  input: z.infer<typeof createUserSchema>,
  createdBy: string
): UserModel {
  const now = new Date().toISOString();
  return {
    Id: ZERO_GUID,
    UserName: input.userName,
    Password: input.password,
    FirstName: input.firstName,
    LastName: input.lastName ?? "",
    DateOfBirth: null,
    Email: input.email,
    EmailConfirmed: false,
    PhoneNumber: input.phoneNumber ?? "",
    PhoneNumberConfirmed: false,
    TwoFactorEnabled: false,
    TwoFactorType: 0,
    LockoutEnd: null,
    LockoutEnabled: false,
    AccessFailedCount: 0,
    LastPasswordChangedDate: null,
    RequiresDefaultPasswordChange: false,
    LastLoginDateTime: null,
    LastLogoutDateTime: null,
    IdentityProviderType: 1,
    UserSecurityQuestion: [],
    UserClaims: [],
    CreatedBy: createdBy,
    CreatedOn: now,
    ModifiedBy: undefined,
    ModifiedOn: null,
    IsDeleted: false
  };
}

export async function createUserAction(input: z.infer<typeof createUserSchema>): Promise<ActionResult> {
  const parsed = createUserSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid user input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const session = await auth();
    const actor = getActor(session);
    const userModel = buildUserModelForCreate(parsed.data, actor);
    const result = await registerUser(userModel);
    const succeeded = result.Status === "Succeeded" || result.Status === 0;
    if (!succeeded) {
      const msg = result.Errors?.[0]?.Description ?? result.Errors?.[0]?.Code ?? "Registration failed.";
      return { ok: false, message: msg };
    }
    return { ok: true, message: "User created successfully." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Create user failed." };
  }
}

export async function updateUserProfileAction(input: z.infer<typeof updateSchema>): Promise<ActionResult> {
  const parsed = updateSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid user profile input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const existing = await getUser(parsed.data.id);
    const session = await auth();
    const actor = getActor(session);

    const updated = {
      ...existing,
      FirstName: parsed.data.firstName,
      LastName: parsed.data.lastName,
      Email: parsed.data.email,
      PhoneNumber: parsed.data.phoneNumber,
      TwoFactorEnabled: parsed.data.twoFactorEnabled,
      RequiresDefaultPasswordChange: parsed.data.forcePasswordReset,
      ModifiedBy: actor,
      ModifiedOn: new Date().toISOString()
    };

    await updateUser(updated);
    return { ok: true, message: "User profile updated." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "User update failed." };
  }
}

export async function lockUserAction(input: { userId: string }): Promise<ActionResult> {
  const parsed = lockSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid user identifier." };
  }

  try {
    await lockUser(parsed.data.userId);
    return { ok: true, message: "User account locked." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Lock user failed." };
  }
}

export async function unlockUserAction(input: { userName: string }): Promise<ActionResult> {
  const parsed = unlockSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid user name." };
  }

  try {
    await unlockUser(parsed.data.userName);
    return { ok: true, message: "User account unlocked." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Unlock user failed." };
  }
}

function buildUserRole(userId: string, roleId: string, actor: string): UserRoleModel {
  return {
    Id: ZERO_GUID,
    UserId: userId,
    RoleId: roleId,
    ValidFrom: new Date().toISOString(),
    ValidTo: null,
    CreatedBy: actor,
    CreatedOn: new Date().toISOString(),
    IsDeleted: false
  };
}

export async function assignUserRoleAction(input: z.infer<typeof roleSchema>): Promise<ActionResult> {
  const parsed = roleSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid role assignment.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const session = await auth();
    const actor = getActor(session);

    await addUserRole(buildUserRole(parsed.data.userId, parsed.data.roleId, actor));
    return { ok: true, message: "Role assigned to user." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Assign role failed." };
  }
}

export async function removeUserRoleAction(input: z.infer<typeof roleSchema>): Promise<ActionResult> {
  const parsed = roleSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid role removal input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const session = await auth();
    const actor = getActor(session);

    await removeUserRole(buildUserRole(parsed.data.userId, parsed.data.roleId, actor));
    return { ok: true, message: "Role removed from user." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Remove role failed." };
  }
}

export async function revokeSessionAction(input: z.infer<typeof revokeSchema>): Promise<ActionResult> {
  const parsed = revokeSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid revoke request.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    await revokeToken(parsed.data.token, parsed.data.tokenTypeHint);
    return { ok: true, message: "Session token revoked." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Session revoke failed." };
  }
}

export async function revokeAllUserSessionsAction(input: { userId: string }): Promise<ActionResult> {
  const parsed = lockSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid user identifier." };
  }

  try {
    const tokens = await listUserActiveTokens([parsed.data.userId]);
    if (tokens.length === 0) {
      return { ok: true, message: "No active sessions found for this user." };
    }

    const count = await bulkRevokeTokens(tokens);
    return { ok: true, message: `${count} session token(s) revoked.` };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Bulk revoke failed." };
  }
}
