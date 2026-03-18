import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import { type FrameworkResult, type IdentityClaimsModel, type IdentityResourcesModel, type UUID } from "@/lib/types/HCL.CS.SF";

export async function listIdentityResources(): Promise<IdentityResourcesModel[]> {
  return HCLCSSFPostWithSession<IdentityResourcesModel[]>(ApiRoutes.identityResource.getAllIdentityResources, "");
}

export async function getIdentityResourceById(id: UUID): Promise<IdentityResourcesModel> {
  return HCLCSSFPostWithSession<IdentityResourcesModel, UUID>(ApiRoutes.identityResource.getIdentityResourceById, id);
}

export async function getIdentityResourceByName(name: string): Promise<IdentityResourcesModel> {
  return HCLCSSFPostWithSession<IdentityResourcesModel, string>(ApiRoutes.identityResource.getIdentityResourceByName, name);
}

export async function createIdentityResource(model: IdentityResourcesModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, IdentityResourcesModel>(ApiRoutes.identityResource.addIdentityResource, model);
}

export async function updateIdentityResource(model: IdentityResourcesModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, IdentityResourcesModel>(ApiRoutes.identityResource.updateIdentityResource, model);
}

export async function deleteIdentityResourceById(id: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.identityResource.deleteIdentityResourceById, id);
}

export async function deleteIdentityResourceByName(name: string): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.identityResource.deleteIdentityResourceByName, name);
}

export async function listIdentityResourceClaims(resourceId: UUID): Promise<IdentityClaimsModel[]> {
  return HCLCSSFPostWithSession<IdentityClaimsModel[], UUID>(ApiRoutes.identityResource.getIdentityResourceClaims, resourceId);
}

export async function addIdentityResourceClaim(model: IdentityClaimsModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, IdentityClaimsModel>(ApiRoutes.identityResource.addIdentityResourceClaim, model);
}

export async function deleteIdentityResourceClaimById(id: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.identityResource.deleteIdentityResourceClaimById, id);
}

export async function deleteIdentityResourceClaimByResourceId(resourceId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(
    ApiRoutes.identityResource.deleteIdentityResourceClaimByResourceIdAsync,
    resourceId
  );
}

export async function deleteIdentityResourceClaimModel(model: IdentityClaimsModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, IdentityClaimsModel>(ApiRoutes.identityResource.deleteIdentityResourceClaimModel, model);
}

