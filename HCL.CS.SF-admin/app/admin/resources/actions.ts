"use server";

import { z } from "zod";

import { HCLCSSFApiError } from "@/lib/api/client";
import { listDetailedClients } from "@/lib/api/clients";
import {
  addApiResourceClaim,
  addApiScopeClaim,
  createApiResource,
  createApiScope,
  deleteApiResource,
  deleteApiResourceClaim,
  deleteApiScope,
  deleteApiScopeClaim,
  getApiResourceClaimsById,
  getApiResource,
  getApiScope,
  getApiScopeClaims,
  updateApiResource,
  updateApiScope
} from "@/lib/api/resources";
import { getActor } from "@/lib/actor";
import { auth } from "@/lib/auth";
import { MAX_LENGTH_255 } from "@/lib/constants";
import {
  type ApiResourceClaimsModel,
  type ActionResult,
  type ApiResourcesModel,
  type ApiScopeClaimsModel,
  type ApiScopesModel
} from "@/lib/types/HCL.CS.SF";

const CONCURRENCY_MESSAGE =
  "This record was changed elsewhere (e.g. another tab or user). Refresh the page and try again.";

function isConcurrencyError(error: unknown): boolean {
  if (error instanceof HCLCSSFApiError) {
    if (error.message.toLowerCase().includes("concurrency")) return true;
    const d = error.details as { Errors?: { Code?: string }[] } | undefined;
    return d?.Errors?.some((e) => e.Code === "CONCURRENCY_FAILURE") ?? false;
  }
  return false;
}

const ZERO_GUID = "00000000-0000-0000-0000-000000000000";

const resourceSchema = z.object({
  id: z.string().default(ZERO_GUID),
  name: z.string().min(2).max(MAX_LENGTH_255),
  displayName: z.string().min(2).max(MAX_LENGTH_255),
  description: z.string().min(1),
  enabled: z.boolean().default(true)
});

const scopeSchema = z.object({
  id: z.string().default(ZERO_GUID),
  resourceId: z.string().min(1),
  name: z.string().min(2).max(MAX_LENGTH_255),
  displayName: z.string().min(2).max(MAX_LENGTH_255),
  description: z.string().default(""),
  required: z.boolean().default(false),
  emphasize: z.boolean().default(false)
});

const deleteSchema = z.object({
  id: z.string().min(1)
});

export async function createResourceAction(
  input: z.infer<typeof resourceSchema>
): Promise<ActionResult> {
  const parsed = resourceSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Validation failed for resource create.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const session = await auth();
  const actor = getActor(session);
  const now = new Date().toISOString();

  const model: ApiResourcesModel = {
    Id: ZERO_GUID,
    Name: parsed.data.name,
    DisplayName: parsed.data.displayName,
    Description: parsed.data.description,
    Enabled: parsed.data.enabled,
    ApiResourceClaims: [],
    ApiScopes: [],
    CreatedBy: actor,
    CreatedOn: now,
    IsDeleted: false
  };

  try {
    await createApiResource(model);
    return { ok: true, message: "API resource created." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Create failed." };
  }
}

export async function updateResourceAction(
  input: z.infer<typeof resourceSchema>
): Promise<ActionResult> {
  const parsed = resourceSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Validation failed for resource update.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const session = await auth();
  const actor = getActor(session);

  const buildResourceModel = (existing: ApiResourcesModel): ApiResourcesModel => ({
    ...existing,
    Name: parsed.data.name,
    DisplayName: parsed.data.displayName,
    Description: parsed.data.description,
    Enabled: parsed.data.enabled,
    ModifiedBy: actor,
    ModifiedOn: new Date().toISOString()
  });

  try {
    const existing = await getApiResource(parsed.data.id);
    const model = buildResourceModel(existing);
    await updateApiResource(model);
    return { ok: true, message: "API resource updated." };
  } catch (error) {
    if (isConcurrencyError(error)) {
      try {
        const existingAgain = await getApiResource(parsed.data.id);
        const modelAgain = buildResourceModel(existingAgain);
        await updateApiResource(modelAgain);
        return { ok: true, message: "API resource updated." };
      } catch (retryError) {
        return { ok: false, message: CONCURRENCY_MESSAGE };
      }
    }
    return { ok: false, message: error instanceof Error ? error.message : "Update failed." };
  }
}

export async function deleteResourceAction(input: { id: string }): Promise<ActionResult> {
  const parsed = deleteSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid resource identifier." };
  }

  try {
    const [resource, clients] = await Promise.all([getApiResource(parsed.data.id), listDetailedClients()]);

    const scopedNames = new Set<string>([resource.Name, ...resource.ApiScopes.map((scope) => scope.Name)]);
    const dependentClients = clients.filter((client) =>
      client.AllowedScopes.some((scope) => scopedNames.has(scope))
    );

    if (dependentClients.length > 0) {
      return {
        ok: false,
        message: `Cannot delete resource. Assigned to ${dependentClients.length} client(s). Remove scope assignments first.`
      };
    }

    await deleteApiResource(parsed.data.id);
    return { ok: true, message: "API resource deleted." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Delete failed." };
  }
}

export async function createScopeAction(input: z.infer<typeof scopeSchema>): Promise<ActionResult> {
  const parsed = scopeSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Validation failed for scope create.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const session = await auth();
  const actor = getActor(session);
  const now = new Date().toISOString();

  const model: ApiScopesModel = {
    Id: ZERO_GUID,
    ApiResourceId: parsed.data.resourceId,
    Name: parsed.data.name,
    DisplayName: parsed.data.displayName,
    Description: parsed.data.description,
    Required: parsed.data.required,
    Emphasize: parsed.data.emphasize,
    ApiScopeClaims: [],
    CreatedBy: actor,
    CreatedOn: now,
    IsDeleted: false
  };

  try {
    await createApiScope(model);
    return { ok: true, message: "API scope created." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Create scope failed." };
  }
}

export async function updateScopeAction(input: z.infer<typeof scopeSchema>): Promise<ActionResult> {
  const parsed = scopeSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Validation failed for scope update.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  const session = await auth();
  const actor = getActor(session);

  const buildScopeModel = (existing: ApiScopesModel): ApiScopesModel => ({
    ...existing,
    ApiResourceId: parsed.data.resourceId,
    Name: parsed.data.name,
    DisplayName: parsed.data.displayName,
    Description: parsed.data.description,
    Required: parsed.data.required,
    Emphasize: parsed.data.emphasize,
    ModifiedBy: actor,
    ModifiedOn: new Date().toISOString()
  });

  try {
    const existing = await getApiScope(parsed.data.id);
    const model = buildScopeModel(existing);
    await updateApiScope(model);
    return { ok: true, message: "API scope updated." };
  } catch (error) {
    if (isConcurrencyError(error)) {
      try {
        const existingAgain = await getApiScope(parsed.data.id);
        const modelAgain = buildScopeModel(existingAgain);
        await updateApiScope(modelAgain);
        return { ok: true, message: "API scope updated." };
      } catch (retryError) {
        return { ok: false, message: CONCURRENCY_MESSAGE };
      }
    }
    return { ok: false, message: error instanceof Error ? error.message : "Update scope failed." };
  }
}

export async function deleteScopeAction(input: { id: string }): Promise<ActionResult> {
  const parsed = deleteSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid scope identifier." };
  }

  try {
    const [scope, clients] = await Promise.all([getApiScope(parsed.data.id), listDetailedClients()]);

    const dependentClients = clients.filter((client) => client.AllowedScopes.includes(scope.Name));
    if (dependentClients.length > 0) {
      return {
        ok: false,
        message: `Cannot delete scope. Assigned to ${dependentClients.length} client(s). Remove scope assignments first.`
      };
    }

    await deleteApiScope(parsed.data.id);
    return { ok: true, message: "API scope deleted." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Delete scope failed." };
  }
}

const resourceClaimSchema = z.object({
  resourceId: z.string().uuid(),
  type: z.string().min(1).max(MAX_LENGTH_255)
});

const scopeClaimSchema = z.object({
  scopeId: z.string().uuid(),
  type: z.string().min(1).max(MAX_LENGTH_255)
});

export async function addApiResourceClaimAction(input: z.infer<typeof resourceClaimSchema>): Promise<ActionResult> {
  const parsed = resourceClaimSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid resource or claim type." };
  }
  try {
    const session = await auth();
    const actor = getActor(session);
    const now = new Date().toISOString();
    await addApiResourceClaim({
      Id: "00000000-0000-0000-0000-000000000000",
      ApiResourceId: parsed.data.resourceId,
      Type: parsed.data.type,
      CreatedBy: actor,
      CreatedOn: now,
      IsDeleted: false
    });
    return { ok: true, message: "Claim type added to resource." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Add resource claim failed." };
  }
}

export async function deleteApiResourceClaimAction(input: { claimId: string }): Promise<ActionResult> {
  if (!input.claimId) {
    return { ok: false, message: "Claim id required." };
  }
  try {
    await deleteApiResourceClaim(input.claimId);
    return { ok: true, message: "Resource claim type removed." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Remove resource claim failed." };
  }
}

export async function addApiScopeClaimAction(input: z.infer<typeof scopeClaimSchema>): Promise<ActionResult> {
  const parsed = scopeClaimSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid scope or claim type." };
  }
  try {
    const session = await auth();
    const actor = getActor(session);
    const now = new Date().toISOString();
    await addApiScopeClaim({
      Id: "00000000-0000-0000-0000-000000000000",
      ApiScopeId: parsed.data.scopeId,
      Type: parsed.data.type,
      CreatedBy: actor,
      CreatedOn: now,
      IsDeleted: false
    } as ApiScopeClaimsModel);
    return { ok: true, message: "Claim type added to scope." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Add scope claim failed." };
  }
}

export async function deleteApiScopeClaimAction(input: { claimId: string }): Promise<ActionResult> {
  if (!input.claimId) {
    return { ok: false, message: "Claim id required." };
  }
  try {
    await deleteApiScopeClaim(input.claimId);
    return { ok: true, message: "Scope claim type removed." };
  } catch (error) {
    return { ok: false, message: error instanceof Error ? error.message : "Remove scope claim failed." };
  }
}

export async function getApiResourceClaimsAction(resourceId: string): Promise<ApiResourceClaimsModel[]> {
  try {
    return await getApiResourceClaimsById(resourceId);
  } catch {
    return [];
  }
}

export async function getApiScopeClaimsAction(scopeId: string): Promise<ApiScopeClaimsModel[]> {
  try {
    return await getApiScopeClaims(scopeId);
  } catch {
    return [];
  }
}
