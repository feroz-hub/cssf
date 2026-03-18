"use server";

import { generatePasswordResetTokenAnonymous, resetPasswordAnonymous } from "@/lib/api/passwordReset";

export type ForgotResult = { ok: boolean; message: string };
export type ResetResult = { ok: boolean; message: string };

export async function requestForgotPasswordAction(username: string): Promise<ForgotResult> {
  const trimmed = username?.trim() ?? "";
  if (!trimmed) {
    return { ok: false, message: "Username is required." };
  }

  try {
    await generatePasswordResetTokenAnonymous(trimmed);
    return {
      ok: true,
      message:
        "If an account exists for that username, password reset instructions have been sent to the registered email address."
    };
  } catch (error) {
    const message = error instanceof Error ? error.message : "Request failed. Please try again.";
    return { ok: false, message };
  }
}

export async function resetPasswordAction(
  username: string,
  token: string,
  newPassword: string,
  confirmPassword: string
): Promise<ResetResult> {
  const trimmedUser = username?.trim() ?? "";
  const trimmedToken = token?.trim() ?? "";
  if (!trimmedUser) return { ok: false, message: "Username is required." };
  if (!trimmedToken) return { ok: false, message: "Reset token is required." };
  if (!newPassword) return { ok: false, message: "New password is required." };
  if (newPassword !== confirmPassword) return { ok: false, message: "Passwords do not match." };
  if (newPassword.length < 8) return { ok: false, message: "Password must be at least 8 characters." };
  if (newPassword.includes(" ")) return { ok: false, message: "Password must not contain spaces." };

  try {
    await resetPasswordAnonymous({
      user_name: trimmedUser,
      password_reset_token: trimmedToken,
      new_password: newPassword
    });
    return { ok: true, message: "Password has been reset. You can sign in with your new password." };
  } catch (error) {
    const message = error instanceof Error ? error.message : "Reset failed. Check the token and try again.";
    return { ok: false, message };
  }
}
