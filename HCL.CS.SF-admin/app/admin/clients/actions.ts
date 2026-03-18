"use server";

import { z } from "zod";

import {
  createClient,
  deleteClient,
  getClient,
  rotateClientSecret,
  updateClient
} from "@/lib/api/clients";
import { isUnauthorizedError } from "@/lib/api/client";
import { listClientScopes } from "@/lib/api/clientScopes";
import { auth } from "@/lib/auth";
import { type ActionResult, type ApplicationType, type ClientsModel } from "@/lib/types/HCL.CS.SF";

const ZERO_GUID = "00000000-0000-0000-0000-000000000000";

function isValidUrl(s: string): boolean {
  if (!s || !s.trim()) return false;
  try {
    const u = new URL(s.trim());
    return u.protocol === "http:" || u.protocol === "https:";
  } catch {
    return false;
  }
}

const MAX_CLIENT_NAME_LENGTH = 255;
const MAX_URI_LENGTH = 2048;
const MAX_CREATED_BY_LENGTH = 255;

function usesAuthorizationCodeGrant(grantTypes: string[]): boolean {
  return grantTypes.includes("authorization_code");
}

function buildSupportedResponseTypes(grantTypes: string[]): string[] {
  return usesAuthorizationCodeGrant(grantTypes) ? ["code"] : [];
}

function normalizeItems(items: string[]): string[] {
  return [...new Set(items.map((item) => item.trim()).filter(Boolean))];
}

const allowedGrantTypeSchema = z.enum(["authorization_code", "refresh_token", "client_credentials", "password"]);

const formSchema = z.object({
  clientId: z.string().optional().default("").transform((value) => value.trim()),
  name: z
    .string()
    .transform((value) => value.trim())
    .pipe(
      z
        .string()
        .min(1, "Client name is required.")
        .max(MAX_CLIENT_NAME_LENGTH, `Client name must be ${MAX_CLIENT_NAME_LENGTH} characters or fewer.`)
    ),
  type: z.enum(["1", "2", "3", "4"]).default("1"),
  allowedGrantTypes: z.array(allowedGrantTypeSchema).min(1, "Select at least one grant type."),
  redirectUris: z
    .array(z.string())
    .refine((arr) => arr.every((u) => !u.trim() || isValidUrl(u)), "Invalid redirect URI.")
    .refine((arr) => arr.every((u) => !u.trim() || u.trim().length <= MAX_URI_LENGTH), "Each redirect URI must be 2048 characters or fewer.")
    .transform((arr) => arr.map((u) => u.trim()).filter(Boolean)),
  postLogoutUris: z
    .array(z.string())
    .default([])
    .refine((arr) => arr.every((u) => !u.trim() || u.trim().length <= MAX_URI_LENGTH), "Each post-logout URI must be 2048 characters or fewer.")
    .transform((arr) => arr.map((u) => u.trim()).filter(Boolean))
    .refine((arr) => arr.every((u) => isValidUrl(u)), "Invalid post logout URI."),
  allowedScopes: z
    .array(z.string())
    .transform((items) => normalizeItems(items))
    .refine((items) => items.length > 0, "At least one scope is required."),
  accessTokenLifetime: z.coerce.number().int().min(60).max(900),
  refreshTokenLifetime: z.coerce.number().int().min(300).max(86400),
  identityTokenLifetime: z.coerce.number().int().min(60).max(3600),
  logoutTokenLifetime: z.coerce.number().int().min(1800).max(86400),
  authorizationCodeLifetime: z.coerce.number().int().min(60).max(600),
  logoUri: z.string().optional().default("").transform((s) => (s?.trim() || "https://localhost:3000/logo")),
  clientUri: z.string().optional().default("").transform((s) => (s?.trim() || "https://localhost:3000")),
  termsOfServiceUri: z.string().optional().default("").transform((s) => (s?.trim() || "https://localhost:3000/terms")),
  policyUri: z.string().optional().default("").transform((s) => (s?.trim() || "https://localhost:3000/policy")),
  preferredAudience: z.string().max(300).optional().default("").transform((value) => value.trim())
}).superRefine((data, ctx) => {
  if (usesAuthorizationCodeGrant(data.allowedGrantTypes) && data.redirectUris.length === 0) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ["redirectUris"],
      message: "At least one redirect URI required when authorization_code grant is selected."
    });
  }
});

const deleteSchema = z.object({
  clientId: z.string().min(1)
});

type ClientFormInput = {
  clientId?: string;
  name: string;
  type: "1" | "2" | "3" | "4";
  allowedGrantTypes: string[];
  redirectUris: string[];
  postLogoutUris: string[];
  allowedScopes: string[];
  accessTokenLifetime: number;
  refreshTokenLifetime: number;
  identityTokenLifetime: number;
  logoutTokenLifetime: number;
  authorizationCodeLifetime: number;
  logoUri?: string;
  clientUri?: string;
  termsOfServiceUri?: string;
  policyUri?: string;
  preferredAudience?: string;
};

type ParsedClientFormInput = z.output<typeof formSchema>;

type RotateInput = {
  clientId: string;
};

function actorName(session: Awaited<ReturnType<typeof auth>>): string {
  return session?.user?.name ?? session?.user?.email ?? "HCL.CS.SF-admin";
}

function toApplicationType(type: string): ApplicationType {
  return Number(type) as ApplicationType;
}

async function validateAllowedScopes(allowedScopes: string[]): Promise<string | null> {
  const knownScopes = await listClientScopes();
  if (knownScopes.length === 0) {
    return "No scopes are currently available for client registration. Reload the page and try again.";
  }

  const knownScopeSet = new Set(knownScopes);
  const invalidScopes = normalizeItems(allowedScopes).filter((scope) => !knownScopeSet.has(scope));
  if (invalidScopes.length > 0) {
    return `Unknown or unavailable scopes selected: ${invalidScopes.join(", ")}. Reload the page and try again.`;
  }

  return null;
}

function buildCreateModel(input: ParsedClientFormInput, actor: string): ClientsModel {
  const now = new Date().toISOString();
  const supportedGrantTypes = normalizeItems(input.allowedGrantTypes);

  return {
    Id: ZERO_GUID,
    ClientId: input.clientId,
    ClientName: input.name,
    ClientUri: input.clientUri,
    ClientIdIssuedAt: now,
    ClientSecretExpiresAt: now,
    ClientSecret: "placeholder",
    LogoUri: input.logoUri,
    TermsOfServiceUri: input.termsOfServiceUri,
    PolicyUri: input.policyUri,
    RefreshTokenExpiration: input.refreshTokenLifetime,
    AccessTokenExpiration: input.accessTokenLifetime,
    IdentityTokenExpiration: input.identityTokenLifetime,
    LogoutTokenExpiration: input.logoutTokenLifetime,
    AuthorizationCodeExpiration: input.authorizationCodeLifetime,
    AccessTokenType: 1,
    RequirePkce: true,
    IsPkceTextPlain: false,
    RequireClientSecret: true,
    IsFirstPartyApp: true,
    AllowOfflineAccess: true,
    AllowAccessTokensViaBrowser: false,
    ApplicationType: toApplicationType(input.type),
    AllowedSigningAlgorithm: "RS256",
    FrontChannelLogoutSessionRequired: true,
    FrontChannelLogoutUri: "",
    BackChannelLogoutSessionRequired: false,
    BackChannelLogoutUri: "",
    SupportedGrantTypes: supportedGrantTypes,
    SupportedResponseTypes: buildSupportedResponseTypes(supportedGrantTypes),
    AllowedScopes: normalizeItems(input.allowedScopes),
    PreferredAudience: input.preferredAudience?.trim() || undefined,
    RedirectUris: normalizeItems(input.redirectUris).map((uri) => ({
      Id: ZERO_GUID,
      ClientId: ZERO_GUID,
      RedirectUri: uri,
      CreatedBy: actor,
      CreatedOn: now,
      IsDeleted: false
    })),
    PostLogoutRedirectUris: normalizeItems(input.postLogoutUris).map((uri) => ({
      Id: ZERO_GUID,
      ClientId: ZERO_GUID,
      PostLogoutRedirectUri: uri,
      CreatedBy: actor,
      CreatedOn: now,
      IsDeleted: false
    })),
    CreatedBy: actor,
    CreatedOn: now,
    IsDeleted: false
  };
}

function mergeClientModel(existing: ClientsModel, input: ParsedClientFormInput, actor: string): ClientsModel {
  const now = new Date().toISOString();
  const supportedGrantTypes = normalizeItems(input.allowedGrantTypes);
  const redirectUris = normalizeItems(input.redirectUris);
  const postLogoutUris = normalizeItems(input.postLogoutUris);

  return {
    ...existing,
    ClientName: input.name,
    ClientUri: input.clientUri,
    LogoUri: input.logoUri,
    TermsOfServiceUri: input.termsOfServiceUri,
    PolicyUri: input.policyUri,
    RefreshTokenExpiration: input.refreshTokenLifetime,
    AccessTokenExpiration: input.accessTokenLifetime,
    IdentityTokenExpiration: input.identityTokenLifetime,
    LogoutTokenExpiration: input.logoutTokenLifetime,
    AuthorizationCodeExpiration: input.authorizationCodeLifetime,
    ApplicationType: toApplicationType(input.type),
    SupportedGrantTypes: supportedGrantTypes,
    SupportedResponseTypes: buildSupportedResponseTypes(supportedGrantTypes),
    AllowedScopes: normalizeItems(input.allowedScopes),
    PreferredAudience: input.preferredAudience?.trim() || undefined,
    RedirectUris: redirectUris.map((uri) => {
      const matched = existing.RedirectUris.find((entry) => entry.RedirectUri === uri);
      if (matched) {
        return {
          ...matched,
          RedirectUri: uri,
          ModifiedBy: actor,
          ModifiedOn: now
        };
      }

      return {
        Id: ZERO_GUID,
        ClientId: existing.Id,
        RedirectUri: uri,
        CreatedBy: actor,
        CreatedOn: now,
        IsDeleted: false
      };
    }),
    PostLogoutRedirectUris: postLogoutUris.map((uri) => {
      const matched = existing.PostLogoutRedirectUris.find((entry) => entry.PostLogoutRedirectUri === uri);
      if (matched) {
        return {
          ...matched,
          PostLogoutRedirectUri: uri,
          ModifiedBy: actor,
          ModifiedOn: now
        };
      }

      return {
        Id: ZERO_GUID,
        ClientId: existing.Id,
        PostLogoutRedirectUri: uri,
        CreatedBy: actor,
        CreatedOn: now,
        IsDeleted: false
      };
    }),
    ModifiedBy: actor,
    ModifiedOn: now
  };
}

function formatValidationMessage(err: z.ZodError): string {
  const flat = err.flatten();
  const first = flat.fieldErrors && Object.values(flat.fieldErrors).flat().filter(Boolean)[0];
  return (typeof first === "string" ? first : flat.formErrors?.[0]) || "Validation failed for client creation.";
}

export async function createClientAction(input: ClientFormInput): Promise<ActionResult<{ clientId: string; secret: string }>> {
  const parsed = formSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: formatValidationMessage(parsed.error),
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const session = await auth();
  const actor = actorName(session);

  if (actor.length > MAX_CREATED_BY_LENGTH) {
    return {
      ok: false,
      message: `Created by value is too long (max ${MAX_CREATED_BY_LENGTH} characters). Use a shorter display name or email.`
    };
  }

  try {
    const scopeValidationMessage = await validateAllowedScopes(parsed.data.allowedScopes);
    if (scopeValidationMessage) {
      return {
        ok: false,
        message: scopeValidationMessage
      };
    }

    const model = buildCreateModel(parsed.data, actor);
    const result = await createClient(model);
    return {
      ok: true,
      message: "Client created successfully.",
      data: {
        clientId: result.ClientId,
        secret: result.ClientSecret
      }
    };
  } catch (error) {
    return {
      ok: false,
      message: isUnauthorizedError(error)
        ? "Session expired. Please sign in again. If this persists after re-login, check server logs."
        : error instanceof Error
          ? error.message
          : "Client creation failed."
    };
  }
}

export async function updateClientAction(input: ClientFormInput): Promise<ActionResult> {
  const parsed = formSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Validation failed for client update.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  if (!parsed.data.clientId) {
    return {
      ok: false,
      message: "Client ID is required for update."
    };
  }

  const session = await auth();
  const actor = actorName(session);

  if (actor.length > MAX_CREATED_BY_LENGTH) {
    return {
      ok: false,
      message: `Modified by value is too long (max ${MAX_CREATED_BY_LENGTH} characters). Use a shorter display name or email.`
    };
  }

  try {
    const scopeValidationMessage = await validateAllowedScopes(parsed.data.allowedScopes);
    if (scopeValidationMessage) {
      return {
        ok: false,
        message: scopeValidationMessage
      };
    }

    const existing = await getClient(parsed.data.clientId);
    const model = mergeClientModel(existing, parsed.data, actor);
    await updateClient(model);

    return {
      ok: true,
      message: "Client updated successfully."
    };
  } catch (error) {
    return {
      ok: false,
      message: isUnauthorizedError(error)
        ? "Session expired. Please sign in again. If this persists after re-login, check server logs."
        : error instanceof Error
          ? error.message
          : "Client update failed."
    };
  }
}

export async function deleteClientAction(input: { clientId: string }): Promise<ActionResult> {
  const parsed = deleteSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid client delete request.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    await deleteClient(parsed.data.clientId);
    return {
      ok: true,
      message: "Client deleted successfully."
    };
  } catch (error) {
    return {
      ok: false,
      message: isUnauthorizedError(error)
        ? "Session expired. Please sign in again. If this persists after re-login, check server logs."
        : error instanceof Error
          ? error.message
          : "Client deletion failed."
    };
  }
}

export async function rotateClientSecretAction(input: RotateInput): Promise<ActionResult<{ secret: string }>> {
  const parsed = deleteSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid client identifier.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const result = await rotateClientSecret(parsed.data.clientId);
    return {
      ok: true,
      message: "Client secret rotated. Copy the value now; it will not be shown again.",
      data: {
        secret: result.ClientSecret
      }
    };
  } catch (error) {
    return {
      ok: false,
      message: isUnauthorizedError(error)
        ? "Session expired. Please sign in again. If this persists after re-login, check server logs."
        : error instanceof Error
          ? error.message
          : "Client secret rotation failed."
    };
  }
}
