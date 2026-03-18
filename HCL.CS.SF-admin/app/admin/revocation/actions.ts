"use server";

import { z } from "zod";

import {
  bulkRevokeTokens,
  defaultSearchWindow,
  listClientActiveTokens,
  listTokensByDateRange,
  listUserActiveTokens,
  revokeToken
} from "@/lib/api/revocation";
import { type ActionResult, type TokenModel } from "@/lib/types/HCL.CS.SF";

const searchSchema = z.object({
  subject: z.string().optional().default(""),
  clientId: z.string().optional().default(""),
  tokenJti: z.string().optional().default(""),
  fromDate: z.string().optional().default(""),
  toDate: z.string().optional().default("")
});

const revokeSchema = z.object({
  token: z.string().min(1),
  tokenTypeHint: z.enum(["access_token", "refresh_token"])
});

const bulkUserSchema = z.object({
  subject: z.string().min(1)
});

const bulkClientSchema = z.object({
  clientId: z.string().min(1)
});

export async function searchRevocationAction(
  input: z.input<typeof searchSchema>
): Promise<ActionResult<TokenModel[]>> {
  const parsed = searchSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid revocation search input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    let tokens: TokenModel[] = [];

    if (parsed.data.subject) {
      tokens = await listUserActiveTokens([parsed.data.subject]);
    } else if (parsed.data.clientId) {
      tokens = await listClientActiveTokens([parsed.data.clientId]);
    } else {
      const window = defaultSearchWindow();
      const fromDate = parsed.data.fromDate || window.fromDate;
      const toDate = parsed.data.toDate || window.toDate;
      tokens = await listTokensByDateRange(fromDate, toDate, true);
    }

    if (parsed.data.tokenJti) {
      tokens = tokens.filter((token) => token.Token.includes(parsed.data.tokenJti));
    }

    return {
      ok: true,
      message: `${tokens.length} token(s) found.`,
      data: tokens
    };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Revocation search failed."
    };
  }
}

export async function revokeTokenAction(input: z.infer<typeof revokeSchema>): Promise<ActionResult> {
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
    return { ok: true, message: "Token revoked." };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Token revoke failed."
    };
  }
}

export async function bulkRevokeByUserAction(input: z.infer<typeof bulkUserSchema>): Promise<ActionResult> {
  const parsed = bulkUserSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid subject.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const tokens = await listUserActiveTokens([parsed.data.subject]);
    const count = await bulkRevokeTokens(tokens);
    return { ok: true, message: `${count} token(s) revoked for subject.` };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Bulk revoke by subject failed."
    };
  }
}

export async function bulkRevokeByClientAction(input: z.infer<typeof bulkClientSchema>): Promise<ActionResult> {
  const parsed = bulkClientSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid client id.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const tokens = await listClientActiveTokens([parsed.data.clientId]);
    const count = await bulkRevokeTokens(tokens);
    return { ok: true, message: `${count} token(s) revoked for client.` };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Bulk revoke by client failed."
    };
  }
}
