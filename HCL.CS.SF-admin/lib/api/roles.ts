import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import {
  type FrameworkResult,
  type RoleClaimModel,
  type RoleModel,
  type UserModel,
  type UUID
} from "@/lib/types/HCL.CS.SF";

export async function listRoles(): Promise<RoleModel[]> {
  return HCLCSSFPostWithSession<RoleModel[]>(ApiRoutes.role.getAllRoles, "");
}

export async function getRole(roleId: UUID): Promise<RoleModel> {
  return HCLCSSFPostWithSession<RoleModel, UUID>(ApiRoutes.role.getRoleById, roleId);
}

export async function createRole(role: RoleModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, RoleModel>(ApiRoutes.role.createRole, role);
}

export async function updateRole(role: RoleModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, RoleModel>(ApiRoutes.role.updateRole, role);
}

export async function deleteRole(roleId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.role.deleteRoleById, roleId);
}

export async function deleteRoleByName(roleName: string): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.role.deleteRoleByName, roleName);
}

export async function addRoleClaim(model: RoleClaimModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, RoleClaimModel>(ApiRoutes.role.addRoleClaim, model);
}

export async function addRoleClaims(models: RoleClaimModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, RoleClaimModel[]>(ApiRoutes.role.addRoleClaimList, models);
}

export async function getRoleByName(roleName: string): Promise<RoleModel> {
  return HCLCSSFPostWithSession<RoleModel, string>(ApiRoutes.role.getRoleByName, roleName);
}

export async function getRoleClaim(roleId: UUID): Promise<RoleClaimModel[]> {
  return HCLCSSFPostWithSession<RoleClaimModel[], UUID>(ApiRoutes.role.getRoleClaim, roleId);
}

export async function removeRoleClaim(claimId: number): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, number>(ApiRoutes.role.removeRoleClaimsById, claimId);
}

export async function removeRoleClaimByModel(model: RoleClaimModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, RoleClaimModel>(ApiRoutes.role.removeRoleClaim, model);
}

export async function removeRoleClaims(models: RoleClaimModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, RoleClaimModel[]>(ApiRoutes.role.removeRoleClaimsList, models);
}

export async function listUsersInRole(roleName: string): Promise<UserModel[]> {
  return HCLCSSFPostWithSession<UserModel[], string>(ApiRoutes.user.getUsersInRole, roleName);
}
