import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import {
  type AuthenticatorAppResponseModel,
  type AuthenticatorAppSetupResponseModel,
  type FrameworkResult,
  type PasswordSignInByTwoFactorAuthenticatorTokenRequest,
  type PasswordSignInRequest,
  type SetupAuthenticatorAppRequest,
  type SignInResponseModel,
  type UUID,
  type VerifyAuthenticatorAppSetupRequest
} from "@/lib/types/HCL.CS.SF";

export async function passwordSignIn(payload: PasswordSignInRequest): Promise<SignInResponseModel> {
  return HCLCSSFPostWithSession<SignInResponseModel, PasswordSignInRequest>(ApiRoutes.authentication.passwordSignIn, payload);
}

export async function passwordSignInByTwoFactorAuthenticatorToken(
  payload: PasswordSignInByTwoFactorAuthenticatorTokenRequest
): Promise<SignInResponseModel> {
  return HCLCSSFPostWithSession<SignInResponseModel, PasswordSignInByTwoFactorAuthenticatorTokenRequest>(
    ApiRoutes.authentication.passwordSignInByTwoFactorAuthenticatorToken,
    payload
  );
}

export async function twoFactorAuthenticatorAppSignIn(code: string): Promise<SignInResponseModel> {
  return HCLCSSFPostWithSession<SignInResponseModel, string>(ApiRoutes.authentication.twoFactorAuthenticatorAppSignIn, code);
}

export async function twoFactorEmailSignIn(code: string): Promise<SignInResponseModel> {
  return HCLCSSFPostWithSession<SignInResponseModel, string>(ApiRoutes.authentication.twoFactorEmailSignIn, code);
}

export async function twoFactorRecoveryCodeSignIn(code: string): Promise<SignInResponseModel> {
  return HCLCSSFPostWithSession<SignInResponseModel, string>(ApiRoutes.authentication.twoFactorRecoveryCodeSignIn, code);
}

export async function twoFactorSmsSignIn(code: string): Promise<SignInResponseModel> {
  return HCLCSSFPostWithSession<SignInResponseModel, string>(ApiRoutes.authentication.twoFactorSmsSignInAsync, code);
}

export async function ropValidateCredentials(payload: unknown): Promise<unknown> {
  return HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.authentication.ropValidateCredentials, payload);
}

export async function isUserSignedIn(payload: unknown): Promise<unknown> {
  return HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.authentication.isUserSignedIn, payload);
}

export async function signOut(): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.authentication.signOut, "");
}

export async function setupAuthenticatorApp(payload: SetupAuthenticatorAppRequest): Promise<AuthenticatorAppSetupResponseModel> {
  return HCLCSSFPostWithSession<AuthenticatorAppSetupResponseModel, SetupAuthenticatorAppRequest>(
    ApiRoutes.authentication.setupAuthenticatorApp,
    payload
  );
}

export async function verifyAuthenticatorAppSetup(payload: VerifyAuthenticatorAppSetupRequest): Promise<AuthenticatorAppResponseModel> {
  return HCLCSSFPostWithSession<AuthenticatorAppResponseModel, VerifyAuthenticatorAppSetupRequest>(
    ApiRoutes.authentication.verifyAuthenticatorAppSetup,
    payload
  );
}

export async function resetAuthenticatorApp(userId: UUID): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, UUID>(ApiRoutes.authentication.resetAuthenticatorApp, userId);
}

export async function generateRecoveryCodes(userId: UUID): Promise<string[]> {
  return HCLCSSFPostWithSession<string[], UUID>(ApiRoutes.authentication.generateRecoveryCodes, userId);
}

export async function countRecoveryCodes(userId: UUID): Promise<number> {
  return HCLCSSFPostWithSession<number, UUID>(ApiRoutes.authentication.countRecoveryCodesAsync, userId);
}

