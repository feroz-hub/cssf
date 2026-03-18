import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import {
  type ApiResourceClaimsModel,
  type ApiResourcesModel,
  type ApiScopeClaimsModel,
  type ApiScopesModel,
  type FrameworkResult,
  type UUID
} from "@/lib/types/HCL.CS.SF";

export async function listApiResources(): Promise<ApiResourcesModel[]> {
  return HCLCSSFPostWithSession<ApiResourcesModel[]>(ApiRoutes.resource.getAllApiResources, "");
}

export async function listApiResourcesByScopes(payload: unknown): Promise<ApiResourcesModel[]> {
  return HCLCSSFPostWithSession<ApiResourcesModel[], unknown>(ApiRoutes.resource.getAllApiResourcesByScopesAsync, payload);
}

export async function getApiResource(resourceId: UUID): Promise<ApiResourcesModel> {
  return HCLCSSFPostWithSession<ApiResourcesModel, UUID>(ApiRoutes.resource.getApiResourceById, resourceId);
}

export async function getApiResourceByName(name: string): Promise<ApiResourcesModel> {
  return HCLCSSFPostWithSession<ApiResourcesModel, string>(ApiRoutes.resource.getApiResourceByName, name);
}

export async function createApiResource(model: ApiResourcesModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiResourcesModel>(ApiRoutes.resource.addApiResource, model);
}

export async function updateApiResource(model: ApiResourcesModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiResourcesModel>(ApiRoutes.resource.updateApiResource, model);
}

export async function deleteApiResource(resourceId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.resource.deleteApiResourceById, resourceId);
}

export async function deleteApiResourceByName(name: string): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.resource.deleteApiResourceByName, name);
}

export async function createApiScope(model: ApiScopesModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiScopesModel>(ApiRoutes.resource.addApiScope, model);
}

export async function updateApiScope(model: ApiScopesModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiScopesModel>(ApiRoutes.resource.updateApiScope, model);
}

export async function deleteApiScope(scopeId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.resource.deleteApiScopeById, scopeId);
}

export async function getApiScope(scopeId: UUID): Promise<ApiScopesModel> {
  return HCLCSSFPostWithSession<ApiScopesModel, UUID>(ApiRoutes.resource.getApiScopeById, scopeId);
}

export async function getApiScopeByName(name: string): Promise<ApiScopesModel> {
  return HCLCSSFPostWithSession<ApiScopesModel, string>(ApiRoutes.resource.getApiScopeByName, name);
}

export async function listApiScopes(): Promise<ApiScopesModel[]> {
  return HCLCSSFPostWithSession<ApiScopesModel[]>(ApiRoutes.resource.getAllApiScopes, "");
}

export async function deleteApiScopeByName(name: string): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.resource.deleteApiScopeByName, name);
}

export async function addApiResourceClaim(model: ApiResourceClaimsModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiResourceClaimsModel>(
    ApiRoutes.resource.addApiResourceClaim,
    model
  );
}

export async function deleteApiResourceClaim(claimId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.resource.deleteApiResourceClaimById, claimId);
}

export async function deleteApiResourceClaimByResourceId(resourceId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.resource.deleteApiResourceClaimByResourceIdAsync, resourceId);
}

export async function deleteApiResourceClaimModel(model: ApiResourceClaimsModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiResourceClaimsModel>(ApiRoutes.resource.deleteApiResourceClaimModel, model);
}

export async function addApiScopeClaim(model: ApiScopeClaimsModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiScopeClaimsModel>(ApiRoutes.resource.addApiScopeClaim, model);
}

export async function deleteApiScopeClaim(claimId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.resource.deleteApiScopeClaimById, claimId);
}

export async function deleteApiScopeClaimByScopeId(scopeId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.resource.deleteApiScopeClaimByScopeId, scopeId);
}

export async function deleteApiScopeClaimModel(model: ApiScopeClaimsModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, ApiScopeClaimsModel>(ApiRoutes.resource.deleteApiScopeClaimModel, model);
}

export async function getApiResourceClaimsById(resourceId: UUID): Promise<ApiResourceClaimsModel[]> {
  return HCLCSSFPostWithSession<ApiResourceClaimsModel[], UUID>(ApiRoutes.resource.getApiResourceClaimsById, resourceId);
}

export async function getApiScopeClaims(scopeId: UUID): Promise<ApiScopeClaimsModel[]> {
  return HCLCSSFPostWithSession<ApiScopeClaimsModel[], UUID>(ApiRoutes.resource.getApiScopeClaims, scopeId);
}

