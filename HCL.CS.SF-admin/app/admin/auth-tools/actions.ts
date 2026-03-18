"use server";

import { z } from "zod";

import {
  countRecoveryCodes,
  generateRecoveryCodes,
  passwordSignIn,
  resetAuthenticatorApp,
  setupAuthenticatorApp,
  verifyAuthenticatorAppSetup
} from "@/lib/api/authentication";
import { type ActionResult } from "@/lib/types/HCL.CS.SF";

const signInSchema = z.object({
  user_name: z.string().min(1),
  password: z.string().min(1)
});

const userIdSchema = z.object({
  user_id: z.string().uuid()
});

const setupSchema = z.object({
  user_id: z.string().uuid(),
  application_name: z.string().min(1)
});

const verifySchema = z.object({
  user_id: z.string().uuid(),
  user_token: z.string().min(1)
});

export async function passwordSignInAction(input: z.infer<typeof signInSchema>): Promise<ActionResult<unknown>> {
  const parsed = signInSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const result = await passwordSignIn(parsed.data);
    return { ok: true, message: "Password sign-in executed.", data: result };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function setupAuthenticatorAppAction(
  input: z.infer<typeof setupSchema>
): Promise<ActionResult<unknown>> {
  const parsed = setupSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const result = await setupAuthenticatorApp(parsed.data);
    return { ok: true, message: "Authenticator app setup executed.", data: result };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function verifyAuthenticatorAppSetupAction(
  input: z.infer<typeof verifySchema>
): Promise<ActionResult<unknown>> {
  const parsed = verifySchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Validation failed.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const result = await verifyAuthenticatorAppSetup(parsed.data);
    return { ok: true, message: "Authenticator app verification executed.", data: result };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function resetAuthenticatorAppAction(
  input: z.infer<typeof userIdSchema>
): Promise<ActionResult> {
  const parsed = userIdSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid user id.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    await resetAuthenticatorApp(parsed.data.user_id);
    return { ok: true, message: "Authenticator app reset executed." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function generateRecoveryCodesAction(input: z.infer<typeof userIdSchema>): Promise<ActionResult<unknown>> {
  const parsed = userIdSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid user id.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const codes = await generateRecoveryCodes(parsed.data.user_id);
    return { ok: true, message: "Recovery codes generated.", data: codes };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

export async function countRecoveryCodesAction(input: z.infer<typeof userIdSchema>): Promise<ActionResult<number>> {
  const parsed = userIdSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid user id.", errors: parsed.error.flatten().fieldErrors };
  }

  try {
    const count = await countRecoveryCodes(parsed.data.user_id);
    return { ok: true, message: "Recovery codes counted.", data: count };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Request failed." };
  }
}

