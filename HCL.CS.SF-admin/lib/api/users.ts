import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import {
  type FrameworkResult,
  type PagingModel,
  type TokenModel,
  type UserClaimModel,
  type UserDisplayModel,
  type UserModel,
  type UserRoleModel,
  type UUID
} from "@/lib/types/HCL.CS.SF";

type UserTokenLookupRequest = {
  user_list: string[];
  paging_model?: PagingModel;
};

export async function listUsers(): Promise<UserDisplayModel[]> {
  return HCLCSSFPostWithSession<UserDisplayModel[]>(ApiRoutes.user.getAllUsers, "");
}

export async function registerUser(user: UserModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserModel>(ApiRoutes.user.registerUser, user);
}

export async function getUser(userId: UUID): Promise<UserModel> {
  return HCLCSSFPostWithSession<UserModel, UUID>(ApiRoutes.user.getUserById, userId);
}

export async function getUserByName(userName: string): Promise<UserModel> {
  return HCLCSSFPostWithSession<UserModel, string>(ApiRoutes.user.getUserByName, userName);
}

export async function getUserByEmail(email: string): Promise<UserModel> {
  return HCLCSSFPostWithSession<UserModel, string>(ApiRoutes.user.getUserByEmail, email);
}

export async function updateUser(user: UserModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserModel>(ApiRoutes.user.updateUser, user);
}

export async function deleteUserById(userId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.user.deleteUserById, userId);
}

export async function deleteUserByName(userName: string): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.user.deleteUserByName, userName);
}

export async function isUserExistsById(userId: UUID): Promise<boolean> {
  return HCLCSSFPostWithSession<boolean, UUID>(ApiRoutes.user.isUserExistsById, userId);
}

export async function isUserExistsByName(userName: string): Promise<boolean> {
  return HCLCSSFPostWithSession<boolean, string>(ApiRoutes.user.isUserExistsByName, userName);
}

export async function lockUser(userId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.user.lockUser, userId);
}

export async function unlockUser(userName: string): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.user.unLockUser, userName);
}

export async function unlockUserByToken(payload: unknown): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, unknown>(ApiRoutes.user.unLockUserByToken, payload);
}

export async function unlockUserBySecurityQuestions(payload: unknown): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, unknown>(ApiRoutes.user.unLockUserByuserSecurityQuestions, payload);
}

export async function getUserRoles(userId: UUID): Promise<string[]> {
  return HCLCSSFPostWithSession<string[], UUID>(ApiRoutes.user.getUserRoles, userId);
}

export async function addUserRole(model: UserRoleModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserRoleModel>(ApiRoutes.user.addUserRole, model);
}

export async function addUserRoles(models: UserRoleModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserRoleModel[]>(ApiRoutes.user.addUserRolesList, models);
}

export async function removeUserRole(model: UserRoleModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserRoleModel>(ApiRoutes.user.removeUserRole, model);
}

export async function removeUserRoles(models: UserRoleModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserRoleModel[]>(ApiRoutes.user.removeUserRoleList, models);
}

export async function getAdminUserClaims(userId: UUID): Promise<UserClaimModel[]> {
  return HCLCSSFPostWithSession<UserClaimModel[], UUID>(ApiRoutes.user.getAdminUserClaims, userId);
}

export async function addAdminUserClaim(model: UserClaimModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel>(ApiRoutes.user.addAdminClaim, model);
}

export async function addAdminUserClaims(models: UserClaimModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel[]>(ApiRoutes.user.addAdminClaimList, models);
}

export async function removeAdminUserClaim(model: UserClaimModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel>(ApiRoutes.user.removeAdminClaim, model);
}

export async function removeAdminUserClaims(models: UserClaimModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel[]>(ApiRoutes.user.removeAdminClaimList, models);
}

export async function getUserClaims(userId: UUID): Promise<UserClaimModel[]> {
  return HCLCSSFPostWithSession<UserClaimModel[], UUID>(ApiRoutes.user.getUserClaims, userId);
}

export async function addUserClaim(model: UserClaimModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel>(ApiRoutes.user.addClaim, model);
}

export async function addUserClaims(models: UserClaimModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel[]>(ApiRoutes.user.addClaimList, models);
}

export async function removeUserClaim(model: UserClaimModel): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel>(ApiRoutes.user.removeClaim, model);
}

export async function removeUserClaims(models: UserClaimModel[]): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UserClaimModel[]>(ApiRoutes.user.removeClaimList, models);
}

export async function replaceUserClaim(model: unknown): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, unknown>(ApiRoutes.user.replaceClaim, model);
}

export async function listUserActiveTokens(
  userIds: string[],
  paging: PagingModel | undefined = undefined
): Promise<TokenModel[]> {
  const payload: UserTokenLookupRequest = {
    user_list: userIds
  };

  if (paging) {
    payload.paging_model = paging;
  }

  return HCLCSSFPostWithSession<TokenModel[], UserTokenLookupRequest>(ApiRoutes.securityToken.getByUserIds, payload);
}
