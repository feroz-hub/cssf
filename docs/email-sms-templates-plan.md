# Email, SMS & Mail Templates – Configuration & Flow

This document describes how **Email**, **SMS**, and **notification templates** are configured and used across the HCL.CS.SF Identity platform.

---

## 1. Overview

- **Email**: SMTP via **MailKit**; config in `SystemSettings.EmailConfig`; templates in `NotificationTemplateSettings.EmailTemplateCollection`.
- **SMS**: **Twilio**; config in `SystemSettings.SMSConfig`; templates in `NotificationTemplateSettings.SMSTemplateCollection`.
- **Templates**: JSON-defined (name, subject for email, body/format); placeholders like `{USERNAME}`, `{TOKEN}` are replaced at send time. Template **names** are fixed constants used by the application; body/subject can be edited in JSON.

---

## 2. Where Things Are Configured

### 2.1 Configuration Files

| Purpose | File(s) | Section / content |
|--------|---------|-------------------|
| **System (SMTP + SMS)** | `src/Identity/HCL.CS.SF.Identity.Infrastructure.Resources/Settings/SystemSettings.json` | `SystemSettings.EmailConfig`, `SystemSettings.SMSConfig` |
| **Templates (email + SMS)** | `src/Identity/HCL.CS.SF.Identity.Infrastructure.Resources/Settings/NotificationTemplateSettings.json` | `NotificationTemplateSettings.EmailTemplateCollection`, `SMSTemplateCollection` |
| **Demo server** | `demos/HCL.CS.SF.Demo.Server/Configurations/` | Same structure; `Program.cs` loads `NotificationTemplateSettings.json` and system settings |
| **Integration tests** | `tests/HCL.CS.SF.IntegrationTests/Configurations/NotificationTemplateSettings.json` | Same structure; `HCL.CS.SFFakeSetup` loads it |

Config is **deserialized** in `HCL.CS.SFExtension.DeserializeConfiguration`: it builds an `IConfiguration` from the system, notification template, and token JSON paths, then binds `SystemSettings`, `NotificationTemplateSettings`, and `TokenSettings` into a single `HCL.CS.SFConfig` used for the rest of the app.

### 2.2 Domain Model (Config Types)

- **`HCL.CS.SFConfig`** (`HCL.CS.SF.Identity.Domain/HCL.CS.SFConfig.cs`): Holds `SystemSettings`, `NotificationTemplateSettings`, `TokenSettings`.
- **`SystemConfig.cs`** (`HCL.CS.SF.Identity.Domain/Configurations/Api/SystemConfig.cs`):
  - **EmailConfig**: `SmtpServer`, `Port` (int), `UserName`, `Password`, `EmailNotificationType` (Token/Link), `SecureSocketOptions` (bool).
  - **SMSConfig**: `SMSAccountIdentification`, `SMSAccountPassword`, `SMSAccountFrom`, `SMSStatusCallbackURL`.
  - **EmailTemplate**: `Name`, `Subject`, `FromAddress`, `FromName`, `CC`, `TemplateFormat`.
  - **SMSTemplate**: `Name`, `TemplateFormat`.

### 2.3 Environment / Placeholders

In `SystemSettings.json`, secrets are often placeholders:

- **Email**: `SmtpServer`, `Port`, `UserName`, `Password`, `SecureSocketOptions`.
  - Example placeholders: `"UserName": "${HCL.CS.SF_SMTP_USERNAME}"`, `"Password": "${HCL.CS.SF_SMTP_PASSWORD}"`.
- **SMS**: `SMSAccountIdentification`, `SMSAccountPassword`, `SMSAccountFrom`; `SMSStatusCallbackURL` may be a fixed URL (e.g. `https://localhost:50001/account/SMSCallback/`).

These placeholders are typically replaced by the host (e.g. env vars or a config layer) before binding; the code only sees the final string/int values.

---

## 3. Email Configuration & Sending

### 3.1 EmailConfig (SystemSettings)

- **SmtpServer**, **Port**, **UserName**, **Password**: SMTP connection.
- **EmailNotificationType**: `Token` vs `Link` – used to choose **which email template** is used for “email verification” (see Template name resolution).
- **SecureSocketOptions**: `true` = SSL; `false` = StartTls (used at connect time in `EmailService` and in startup validation).

### 3.2 Validation at Startup

In `HCL.CS.SFExtension.ValidateEmailConfiguration`:

- Requires: `SmtpServer`, `UserName`, `Password` non-empty and `Port > 0`.
- Optionally connects with MailKit (SSL or StartTls) and sets `GlobalConfiguration.IsEmailConfigurationValid = true`. If any check fails, errors are collected and can cause startup to fail.

### 3.3 Sending Flow (EmailService)

1. **Entry**: `IEmailService.SendEmailAsync(NotificationInfoModel)`.
2. **Template resolution**: Template is looked up by `message.TemplateName` in `configSettings.NotificationTemplateSettings.EmailTemplateCollection`.
3. **Placeholders**: Body (and subject if needed) are passed to `UpdateNotificationTemplatePlaceholder(template, users, message.Parameters)` (see Placeholders below).
4. **Build/send**: A `MimeMessage` is built (from address from template or config, subject, HTML body); MailKit’s `SmtpClient` (wrapped by `SmtpClientWrapper`) sends it using `EmailConfig` (SmtpServer, Port, SecureSocketOptions).
5. **Persistence**: A `Notification` entity is stored (e.g. for audit) via `emailRepository`.

Implemented in: `src/Identity/HCL.CS.SF.Identity.Infrastructure/Implementation/EmailService.cs`. Dependency: `HCL.CS.SFConfig` (for `NotificationTemplateSettings` and `SystemSettings.EmailConfig`).

---

## 4. SMS Configuration & Sending

### 4.1 SMSConfig (SystemSettings)

- **SMSAccountIdentification**, **SMSAccountPassword**, **SMSAccountFrom**: Twilio credentials and “from” number.
- **SMSStatusCallbackURL**: Required by current validation (see below).

### 4.2 Validation at Startup

In `HCL.CS.SFExtension.ValidateSmsConfiguration`:

- All four properties must be non-empty to set `GlobalConfiguration.IsSmsConfigurationValid = true`. So **SMSStatusCallbackURL is required** for SMS to be considered configured.

### 4.3 Sending Flow (SmsService)

1. **Entry**: `ISmsService.SendSmsAsync(NotificationInfoModel)`; `ToAddress` = phone number.
2. **Template resolution**: Template looked up by `message.TemplateName` in `NotificationTemplateSettings.SMSTemplateCollection`.
3. **Placeholders**: Same `UpdateNotificationTemplatePlaceholder` pattern (template body, user, parameters).
4. **Send**: Twilio is initialized from `SMSConfig`; `MessageResource.CreateAsync` sends to `ToAddress` from `SMSAccountFrom` with the resolved body; optional status callback URL from config.
5. **Persistence**: A `Notification` record is saved.

Implemented in: `src/Identity/HCL.CS.SF.Identity.Infrastructure/Implementation/SmsService.cs`.

---

## 5. Notification Templates

### 5.1 Template Collections

- **EmailTemplateCollection**: Each item: `Name`, `Subject`, `FromAddress`, `FromName`, `CC`, `TemplateFormat` (HTML).
- **SMSTemplateCollection**: Each item: `Name`, `TemplateFormat` (plain text).

Template **names** are fixed in code (see Constants and Template name resolution). Changing a name in JSON without updating code will break that flow. **Subject**, **FromAddress**, **FromName**, **CC**, and **TemplateFormat** are safe to change.

### 5.2 Supported Placeholders

Documented in `NotificationTemplateSettings.json` and implemented in `TemplateExtension.UpdateNotificationTemplatePlaceholder`:

- `{USERNAME}` → user’s first name (note: same as FIRSTNAME in current code).
- `{FIRSTNAME}` → user’s first name.
- `{LASTNAME}` → user’s last name.
- `{FULLNAME}` → `FirstName + " " + LastName`.
- `{TOKEN}` → from `message.Parameters["{TOKEN}"]` (e.g. verification or reset token).
- `{USERID}` → user’s Id (string).
- `{EMAIL}` → user’s email.

Replacement order: first the key/value pairs in `parameters`, then the fixed user placeholders above. So custom parameters can override if needed.

File: `src/Identity/HCL.CS.SF.Identity.Infrastructure/Extension/TemplateExtension.cs`.

### 5.3 Template Name Constants & Resolution

**Constants** (`HCL.CS.SF.Identity.Domain/Constants/Constants.cs` – `NotificationConstants`):

- `EmailVerification`, `PhoneNumberVerification`, `GenerateTwoFactorToken`, `ResetPasswordUsingToken`
- `EmailVerificationUsingLink`, `EmailVerificationUsingToken`, `PhoneNumberVerificationToken`
- `DefaultTemplate`

**Resolution** (`NotificationUtil.GetTemplateName`):

- **EmailVerification** → if `EmailNotificationType == Link` then `EmailVerificationUsingLink`, else `EmailVerificationUsingToken`.
- **PhoneNumberVerification** → `PhoneNumberVerificationToken`.
- **GenerateTwoFactorToken** → `GenerateTwoFactorToken`.
- **ResetPasswordUsingToken** → `ResetPasswordUsingToken`.
- **default** → use `purpose` as template name (allows custom purposes that match a template name).

So: the **purpose** (e.g. from `UserTokenService` / account flows) is mapped to a **template name**; that name must exist in the corresponding JSON collection.

---

## 6. End-to-End: Who Sends What

### 6.1 Call Chain

1. **User/account operations** (e.g. `UserAccountService`, `UserTokenService`) need to send email or SMS.
2. They call **NotificationUtil**: `SendEmailAsync(user, purpose, token)` or `SendSmsAsync(user, purpose, token)`.
3. **NotificationUtil**:
   - Checks `GlobalConfiguration.IsEmailConfigurationValid` / `IsSmsConfigurationValid`.
   - Builds `NotificationInfoModel` (UserId, TemplateName from `GetTemplateName(purpose)`, ToAddress = user email or phone, Activity = purpose, Parameters e.g. `{TOKEN}`).
   - Calls `IEmailService.SendEmailAsync` or `ISmsService.SendSmsAsync`.

4. **EmailService** / **SmsService**: Resolve template by name, replace placeholders, send, persist `Notification`.

### 6.2 Feature → Template Mapping (from code)

| Feature | Notification type | Purpose (constant) | Resolved template name |
|--------|--------------------|--------------------|-------------------------|
| Email verification | Email | `EmailVerification` | `EmailVerificationUsingLink` or `EmailVerificationUsingToken` |
| Phone verification | SMS | `PhoneNumberVerification` | `PhoneNumberVerificationToken` |
| Password reset | Email or SMS | `ResetPasswordUsingToken` | `ResetPasswordUsingToken` |
| 2FA token | Email or SMS (by user’s TwoFactorType) | `GenerateTwoFactorToken` | `GenerateTwoFactorToken` |
| Custom / fallback | Email or SMS | (passed through) | purpose used as template name; fallback `DefaultTemplate` in one path |

Other template names present in JSON (e.g. `ChangeEmailAddress`, `DisableUser`, `UnlockUser`, `RoleModification`) are available for flows that pass that purpose (or same-named template) when calling `SendNotification` / `NotificationUtil`.

---

## 7. How to Add or Change Templates

### 7.1 Changing content or SMTP/SMS settings

- **Email/SMS config**: Edit `SystemSettings.json` (or host-specific override). Ensure env placeholders are substituted (e.g. `HCL.CS.SF_SMTP_*`, `HCL.CS.SF_SMS_*`). Port in JSON can be string `"25"`; binder maps it to `EmailConfig.Port` (int).
- **Template body/subject**: Edit `NotificationTemplateSettings.json` (or demo/test copies). Change `Subject`, `TemplateFormat`, `FromAddress`, `FromName`, `CC` only; keep **Name** in sync with constants used in code.

### 7.2 Adding a new template

1. **Add entry** to `EmailTemplateCollection` and/or `SMSTemplateCollection` in `NotificationTemplateSettings.json` (same `Name` in both if used for both channels).
2. **Use a new constant** in `NotificationConstants` (optional but recommended) for the new name.
3. **Use it in code**: Either:
   - Add a case in `NotificationUtil.GetTemplateName` for a new purpose and call `SendNotification(..., purpose, token)` with that purpose, or
   - Call the notification layer with that purpose/template name where the new flow is implemented.
4. **Placeholders**: Use only the supported set (`{USERNAME}`, `{TOKEN}`, etc.); custom keys can be added via `Parameters` and will be replaced by `UpdateNotificationTemplatePlaceholder` if the template contains them.

### 7.3 Where config is loaded

- **Main API / host**: `HCL.CS.SFExtension.AddHCL.CS.SF(systemSettingsJsonPath, tokenConfigSettingsJsonPath, notificationTemplateSettingsJsonPath)` → `DeserializeConfiguration` loads and binds the three JSON files.
- **Demo**: `demos/HCL.CS.SF.Demo.Server/Program.cs` loads system + token + `./Configurations/NotificationTemplateSettings.json` and passes the three objects to `AddHCL.CS.SF(...)`.
- **Integration tests**: `HCL.CS.SFFakeSetup.LoadNotificationTemplateSettings(integrationRootPath)` loads `NotificationTemplateSettings.json` under the integration root; that settings object is used when building the test host/config.

---

## 8. Summary Diagram

```
Host / Demo / Tests
  → Load SystemSettings.json + NotificationTemplateSettings.json (+ TokenSettings)
  → HCL.CS.SFConfig (SystemSettings, NotificationTemplateSettings, TokenSettings)
  → ValidateEmailConfiguration / ValidateSmsConfiguration
  → GlobalConfiguration.IsEmailConfigurationValid / IsSmsConfigurationValid

User/Account flows (e.g. UserTokenService, UserAccountService)
  → NotificationUtil.SendEmailAsync(user, purpose, token) or SendSmsAsync(...)
  → GetTemplateName(purpose) → template name
  → NotificationInfoModel { TemplateName, ToAddress, Parameters }
  → IEmailService.SendEmailAsync(model) or ISmsService.SendSmsAsync(model)

EmailService / SmsService
  → Look up template by name in NotificationTemplateSettings
  → UpdateNotificationTemplatePlaceholder(template, user, parameters)
  → Build MimeMessage (email) or Twilio message (SMS)
  → Send via MailKit (SMTP) / Twilio
  → Persist Notification to DB
```

---

## 9. Key File Reference

| Area | File |
|------|------|
| Config (system) | `HCL.CS.SF.Identity.Infrastructure.Resources/Settings/SystemSettings.json` |
| Config (templates) | `HCL.CS.SF.Identity.Infrastructure.Resources/Settings/NotificationTemplateSettings.json` |
| Config types | `HCL.CS.SF.Identity.Domain/Configurations/Api/SystemConfig.cs`, `HCL.CS.SF.Identity.Domain/HCL.CS.SFConfig.cs` |
| Load & validate | `HCL.CS.SF.Identity.API/Extensions/HCL.CS.SFExtension.cs` |
| Template names | `HCL.CS.SF.Identity.Domain/Constants/Constants.cs` (NotificationConstants) |
| Purpose → template | `HCL.CS.SF.Identity.Application/.../Api/Utils/NotificationUtil.cs` |
| Placeholders | `HCL.CS.SF.Identity.Infrastructure/Extension/TemplateExtension.cs` |
| Email send | `HCL.CS.SF.Identity.Infrastructure/Implementation/EmailService.cs` |
| SMS send | `HCL.CS.SF.Identity.Infrastructure/Implementation/SmsService.cs` |
| Callers | `HCL.CS.SF.Identity.Application/.../Api/Services/UserTokenService.cs`, `UserAccountService` (partial) |

This completes the plan and analysis of how Email, SMS, and mail/notification templates are configured and used in the project.
