import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostAnonymous } from "@/lib/api/client";
import type { FrameworkResult, GeneratePasswordResetTokenRequest, ResetPasswordRequest } from "@/lib/types/HCL.CS.SF";

/** NotificationTypes: 1 = Email, 2 = SMS */
const NOTIFICATION_TYPE_EMAIL = 1;

export async function generatePasswordResetTokenAnonymous(username: string): Promise<FrameworkResult> {
  const payload: GeneratePasswordResetTokenRequest = {
    user_name: username.trim(),
    notification_type: NOTIFICATION_TYPE_EMAIL
  };
  return HCLCSSFPostAnonymous<FrameworkResult, GeneratePasswordResetTokenRequest>(
    ApiRoutes.user.generatePasswordResetToken,
    payload
  );
}

export async function resetPasswordAnonymous(payload: ResetPasswordRequest): Promise<FrameworkResult> {
  return HCLCSSFPostAnonymous<FrameworkResult, ResetPasswordRequest>(ApiRoutes.user.resetPassword, payload);
}
