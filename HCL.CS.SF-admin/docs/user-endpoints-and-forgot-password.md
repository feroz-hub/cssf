# User endpoints and Forgot Password flow

## 1. User-related API endpoints (Gateway)

All under base path `/Security/Api/User/`. Called via POST with JSON body. Many require a valid Bearer token (session); a few are **anonymous** (no token).

| Endpoint | Path | Auth | Purpose |
|----------|------|------|--------|
| RegisterUser | `/RegisterUser` | Anonymous | Create new user (self-registration or admin) |
| GeneratePasswordResetToken | `/GeneratePasswordResetToken` | Anonymous | Request password reset; server sends token by email (or SMS) |
| ResetPassword | `/ResetPassword` | Anonymous | Reset password using token from email |
| GetUserById, GetUserByName, UpdateUser, DeleteUser, LockUser, UnLockUser, AddUserRole, etc. | Various | Token required | Admin user management |
| ChangePassword | `/ChangePassword` | Token required | Change password when already signed in |

**Request shapes (anonymous forgot/reset):**

- **GeneratePasswordResetToken:** `{ "user_name": "<username>", "notification_type": 1 }` (1 = Email, 2 = SMS).
- **ResetPassword:** `{ "user_name": "<username>", "password_reset_token": "<token from email>", "new_password": "<new password>" }`.

Server flow for **GeneratePasswordResetToken**: finds user by username, generates a password-reset token, sends it via email (template `ResetPasswordUsingToken`) or SMS. Token validity is configured (e.g. `PasswordResetTokenExpiry` minutes). **ResetPassword** validates the token and updates the password.

---

## 2. Forgot Password flow in HCL.CS.SF-admin

1. **Login page** (`/login`)  
   - User sees “Forgot password?” link.

2. **Forgot password page** (`/login/forgot-password`)  
   - User enters **username** only.  
   - Submit calls server action `requestForgotPasswordAction(username)`, which uses **anonymous** POST to `GeneratePasswordResetToken` (no session).  
   - UI shows a single message: “If an account exists for that username, password reset instructions have been sent to the registered email address.” (No disclosure of whether the user exists.)

3. **Email**  
   - User receives email from HCL.CS.SF (SMTP) with the reset token (or link containing it), per server notification config.

4. **Reset password page** (`/login/reset-password`)  
   - User opens this page (e.g. from link in email or manually).  
   - Enters **username**, **reset token** (from email), **new password**, **confirm password**.  
   - Submit calls server action `resetPasswordAction(...)`, which uses **anonymous** POST to `ResetPassword`.  
   - On success: “Password has been reset. You can sign in with your new password.” and link back to sign in.

5. **Sign in**  
   - User goes to `/login` and signs in with the new password.

---

## 3. Implementation details (admin)

- **Anonymous calls:** `lib/api/client.ts` exposes `HCL.CS.SFPostAnonymous(path, payload)` (no `Authorization` header). Used by `lib/api/passwordReset.ts` for `generatePasswordResetTokenAnonymous` and `resetPasswordAnonymous`.
- **Server actions:** `app/(auth)/login/actions.ts` implements `requestForgotPasswordAction` and `resetPasswordAction` so the API is called from the server (env/api base URL available).
- **Pages:**  
  - `app/(auth)/login/page.tsx` – “Forgot password?” link to `/login/forgot-password`.  
  - `app/(auth)/login/forgot-password/page.tsx` – form: username → send reset.  
  - `app/(auth)/login/reset-password/page.tsx` – form: username, token, new password, confirm → reset.

---

## 4. Server-side reference (source)

- **GeneratePasswordResetToken:**  
  Gateway: `UserAccountServiceRoute.GeneratePasswordResetToken` → `UserAccountService.GeneratePasswordResetTokenAsync(username, notificationType)`.  
  Identity: `UserTokenService.GeneratePasswordResetTokenAsync` (generates token, sends via `SendNotification` with `ResetPasswordUsingToken` template).
- **ResetPassword:**  
  Gateway: `UserAccountServiceRoute.ResetPassword` → `UserAccountService.ResetPasswordAsync(username, passwordResetToken, newPassword)`.  
  Identity: validates token and calls `UserManager.ResetPasswordAsync`.
- Both are in `ProxyConstants.AnonymousApis`, so no Bearer token is required.
