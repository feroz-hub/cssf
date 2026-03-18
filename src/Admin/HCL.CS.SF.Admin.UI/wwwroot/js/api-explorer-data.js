var ApiExplorerData = (() => {
  var __defProp = Object.defineProperty;
  var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
  var __getOwnPropNames = Object.getOwnPropertyNames;
  var __hasOwnProp = Object.prototype.hasOwnProperty;
  var __export = (target, all) => {
    for (var name in all)
      __defProp(target, name, { get: all[name], enumerable: true });
  };
  var __copyProps = (to, from, except, desc) => {
    if (from && typeof from === "object" || typeof from === "function") {
      for (let key of __getOwnPropNames(from))
        if (!__hasOwnProp.call(to, key) && key !== except)
          __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
    }
    return to;
  };
  var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

  // src/Admin/HCL.CS.SF.Admin.UI/wwwroot/js/api-explorer-data-entry.ts
  var api_explorer_data_entry_exports = {};
  __export(api_explorer_data_entry_exports, {
    ApiRoutes: () => ApiRoutes,
    endpointSchemas: () => endpointSchemas
  });

  // HCL.CS.SF-admin/lib/api/endpoint-schemas.ts
  var guidId = (name, description) => ({
    name,
    type: "guid",
    required: true,
    placeholder: "e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6",
    description
  });
  var pagingFields = [
    { name: "page.currentPage", type: "number", placeholder: "1", description: "Current page number" },
    { name: "page.itemsPerPage", type: "number", placeholder: "20", description: "Items per page" }
  ];
  var userClaimFields = [
    guidId("userId", "Target user ID"),
    { name: "claimType", type: "string", required: true, placeholder: "e.g. permission" },
    { name: "claimValue", type: "string", required: true, placeholder: "e.g. admin.read" },
    { name: "isAdminClaim", type: "boolean", defaultValue: "false", description: "Whether this is an admin-level claim" }
  ];
  var roleClaimFields = [
    guidId("roleId", "Target role ID"),
    { name: "claimType", type: "string", required: true, placeholder: "e.g. permission" },
    { name: "claimValue", type: "string", required: true, placeholder: "e.g. users.manage" }
  ];
  var userRoleFields = [
    guidId("userId", "Target user ID"),
    guidId("roleId", "Role ID to assign"),
    { name: "validFrom", type: "date", description: "Role assignment start date (optional)" },
    { name: "validTo", type: "date", description: "Role assignment end date (optional)" }
  ];
  var userSecurityQuestionFields = [
    guidId("userId", "Target user ID"),
    guidId("securityQuestionId", "Security question ID"),
    { name: "answer", type: "string", required: true, placeholder: "Security question answer" }
  ];
  var endpointSchemas = {
    // =========================================================================
    // USER
    // =========================================================================
    "/Security/Api/User/RegisterUser": {
      method: "POST",
      description: "Register a new user account",
      fields: [
        { name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "password", type: "string", required: true, placeholder: "Strong password" },
        { name: "firstName", type: "string", required: true, placeholder: "e.g. John" },
        { name: "lastName", type: "string", required: true, placeholder: "e.g. Doe" },
        { name: "email", type: "string", required: true, placeholder: "e.g. john@example.com" },
        { name: "phoneNumber", type: "string", placeholder: "e.g. +1234567890" },
        { name: "emailConfirmed", type: "boolean", defaultValue: "false" },
        { name: "phoneNumberConfirmed", type: "boolean", defaultValue: "false" },
        { name: "twoFactorEnabled", type: "boolean", defaultValue: "false" },
        { name: "twoFactorType", type: "enum", defaultValue: "0", enumValues: ["0 - None", "1 - Email", "2 - Sms", "3 - AuthenticatorApp"], description: "Two-factor authentication type" },
        { name: "lockoutEnabled", type: "boolean", defaultValue: "false" }
      ]
    },
    "/Security/Api/User/UpdateUser": {
      method: "POST",
      description: "Update an existing user account",
      fields: [
        guidId("id", "User ID to update"),
        { name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "password", type: "string", placeholder: "New password (leave empty to keep current)" },
        { name: "firstName", type: "string", required: true, placeholder: "e.g. John" },
        { name: "lastName", type: "string", required: true, placeholder: "e.g. Doe" },
        { name: "email", type: "string", required: true, placeholder: "e.g. john@example.com" },
        { name: "phoneNumber", type: "string", placeholder: "e.g. +1234567890" },
        { name: "emailConfirmed", type: "boolean", defaultValue: "false" },
        { name: "phoneNumberConfirmed", type: "boolean", defaultValue: "false" },
        { name: "twoFactorEnabled", type: "boolean", defaultValue: "false" },
        { name: "twoFactorType", type: "enum", defaultValue: "0", enumValues: ["0 - None", "1 - Email", "2 - Sms", "3 - AuthenticatorApp"] },
        { name: "lockoutEnabled", type: "boolean", defaultValue: "false" }
      ]
    },
    "/Security/Api/User/GetAllUsers": {
      method: "POST",
      description: "Retrieve all users",
      fields: []
    },
    "/Security/Api/User/GetUserById": {
      method: "POST",
      description: "Get a user by their ID",
      fields: [guidId("id", "User ID")]
    },
    "/Security/Api/User/GetUserByName": {
      method: "POST",
      description: "Get a user by username",
      fields: [{ name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/GetUserByEmail": {
      method: "POST",
      description: "Get a user by email address",
      fields: [{ name: "email", type: "string", required: true, placeholder: "e.g. john@example.com" }]
    },
    "/Security/Api/User/DeleteUserById": {
      method: "POST",
      description: "Delete a user by their ID",
      fields: [guidId("id", "User ID to delete")]
    },
    "/Security/Api/User/DeleteUserByName": {
      method: "POST",
      description: "Delete a user by username",
      fields: [{ name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/IsUserExistsById": {
      method: "POST",
      description: "Check if a user exists by ID",
      fields: [guidId("id", "User ID to check")]
    },
    "/Security/Api/User/IsUserExistsByName": {
      method: "POST",
      description: "Check if a user exists by username",
      fields: [{ name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/IsUserExistsByClaimPrincipal": {
      method: "POST",
      description: "Check if a user exists by claims principal (requires active session)",
      fields: []
    },
    "/Security/Api/User/LockUser": {
      method: "POST",
      description: "Lock a user account indefinitely",
      fields: [guidId("id", "User ID to lock")]
    },
    "/Security/Api/lockuserwithenddate": {
      method: "POST",
      description: "Lock a user account until a specific date",
      fields: [
        guidId("userId", "User ID to lock"),
        { name: "enddate", type: "date", required: true, description: "Lockout end date" }
      ]
    },
    "/Security/Api/User/UnLockUser": {
      method: "POST",
      description: "Unlock a user account by username",
      fields: [{ name: "username", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/UnLockUserByToken": {
      method: "POST",
      description: "Unlock a user account using a verification token",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "token", type: "string", required: true, placeholder: "Unlock token" },
        { name: "purpose", type: "string", required: true, placeholder: "e.g. AccountUnlock" }
      ]
    },
    "/Security/Api/User/UnLockUserByuserSecurityQuestions": {
      method: "POST",
      description: "Unlock a user account by answering security questions",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "securityQuestions", type: "array", description: "Array of {securityQuestionId, answer} objects", nested: userSecurityQuestionFields }
      ]
    },
    "/Security/Api/User/ChangePassword": {
      method: "POST",
      description: "Change a user's password",
      fields: [
        guidId("userId", "User ID"),
        { name: "currentPassword", type: "string", required: true, placeholder: "Current password" },
        { name: "newPassword", type: "string", required: true, placeholder: "New password" }
      ]
    },
    "/Security/Api/User/GeneratePasswordResetToken": {
      method: "POST",
      description: "Generate a password reset token and send via email/SMS",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "notificatype", type: "enum", required: true, enumValues: ["1 - Email", "2 - SMS"], description: "Notification delivery channel" }
      ]
    },
    "/Security/Api/User/ResetPassword": {
      method: "POST",
      description: "Reset a user's password using a reset token",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "passwordResetToken", type: "string", required: true, placeholder: "Token from GeneratePasswordResetToken" },
        { name: "newPassword", type: "string", required: true, placeholder: "New password" }
      ]
    },
    "/Security/Api/User/GenerateUserTokenAsync": {
      method: "POST",
      description: "Generate a generic user token for a specific purpose",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "purpose", type: "string", required: true, placeholder: "e.g. EmailConfirmation" },
        { name: "template", type: "string", required: true, placeholder: "Notification template name" },
        { name: "notificatype", type: "enum", required: true, enumValues: ["1 - Email", "2 - SMS"], description: "Notification delivery channel" }
      ]
    },
    "/Security/Api/User/VerifyUserToken": {
      method: "POST",
      description: "Verify a generic user token",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "purpose", type: "string", required: true, placeholder: "e.g. EmailConfirmation" },
        { name: "token", type: "string", required: true, placeholder: "Token to verify" }
      ]
    },
    "/Security/Api/User/GenerateEmailConfirmationToken": {
      method: "POST",
      description: "Generate an email confirmation token",
      fields: [{ name: "username", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/VerifyEmailConfirmationToken": {
      method: "POST",
      description: "Verify an email confirmation token",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "emailToken", type: "string", required: true, placeholder: "Email confirmation token" }
      ]
    },
    "/Security/Api/User/GeneratePhoneNumberConfirmationToken": {
      method: "POST",
      description: "Generate a phone number confirmation token",
      fields: [{ name: "username", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/VerifyPhoneNumberConfirmationToken": {
      method: "POST",
      description: "Verify a phone number confirmation token",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "smsToken", type: "string", required: true, placeholder: "SMS verification token" }
      ]
    },
    "/Security/Api/User/GenerateEmailTwoFactorToken": {
      method: "POST",
      description: "Generate a two-factor authentication token via email",
      fields: [{ name: "username", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/VerifyEmailTwoFactorToken": {
      method: "POST",
      description: "Verify a two-factor authentication email token",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "emailToken", type: "string", required: true, placeholder: "2FA email token" }
      ]
    },
    "/Security/Api/User/GenerateSmsTwoFactorToken": {
      method: "POST",
      description: "Generate a two-factor authentication token via SMS",
      fields: [{ name: "username", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/VerifySmsTwoFactorToken": {
      method: "POST",
      description: "Verify a two-factor authentication SMS token",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "smsToken", type: "string", required: true, placeholder: "2FA SMS token" }
      ]
    },
    "/Security/Api/User/GetAllTwoFactorType": {
      method: "POST",
      description: "Get all available two-factor authentication types",
      fields: []
    },
    "/Security/Api/User/SetTwoFactorEnabled": {
      method: "POST",
      description: "Enable or disable two-factor authentication for a user",
      fields: [
        guidId("userId", "User ID"),
        { name: "enabled", type: "boolean", required: true, description: "Enable (true) or disable (false) 2FA" }
      ]
    },
    "/Security/Api/User/UpdateUserTwoFactorType": {
      method: "POST",
      description: "Update the two-factor authentication type for a user",
      fields: [
        guidId("userId", "User ID"),
        { name: "twoFactorType", type: "enum", required: true, enumValues: ["0 - None", "1 - Email", "2 - Sms", "3 - AuthenticatorApp"], description: "New 2FA type" }
      ]
    },
    "/Security/Api/User/GetUserRoles": {
      method: "POST",
      description: "Get all roles assigned to a user",
      fields: [guidId("id", "User ID")]
    },
    "/Security/Api/User/AddUserRole": {
      method: "POST",
      description: "Assign a role to a user",
      fields: userRoleFields
    },
    "/Security/Api/User/AddUserRolesList": {
      method: "POST",
      description: "Assign multiple roles to a user (array of UserRoleModel)",
      fields: userRoleFields
    },
    "/Security/Api/User/RemoveUserRole": {
      method: "POST",
      description: "Remove a role from a user",
      fields: [
        guidId("userId", "User ID"),
        guidId("roleId", "Role ID to remove")
      ]
    },
    "/Security/Api/User/RemoveUserRoleList": {
      method: "POST",
      description: "Remove multiple roles from a user (array of UserRoleModel)",
      fields: [
        guidId("userId", "User ID"),
        guidId("roleId", "Role ID to remove")
      ]
    },
    "/Security/Api/User/GetUserRoleClaimsById": {
      method: "POST",
      description: "Get role claims for a user by user ID",
      fields: [guidId("id", "User ID")]
    },
    "/Security/Api/User/GetUserRoleClaimsByName": {
      method: "POST",
      description: "Get role claims for a user by username",
      fields: [{ name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" }]
    },
    "/Security/Api/User/GetUsersInRole": {
      method: "POST",
      description: "Get all users assigned to a specific role",
      fields: [{ name: "roleName", type: "string", required: true, placeholder: "e.g. Administrator" }]
    },
    "/Security/Api/User/GetClaims": {
      method: "POST",
      description: "Get all claims for a user (via identity claims principal)",
      fields: [guidId("id", "User ID")]
    },
    "/Security/Api/User/GetUserClaims": {
      method: "POST",
      description: "Get user claims by user ID",
      fields: [guidId("id", "User ID")]
    },
    "/Security/Api/User/GetUsersForClaim": {
      method: "POST",
      description: "Get all users that have a specific claim",
      fields: [
        { name: "claimType", type: "string", required: true, placeholder: "e.g. permission" },
        { name: "claimValue", type: "string", required: true, placeholder: "e.g. admin.read" }
      ]
    },
    "/Security/Api/User/AddClaim": {
      method: "POST",
      description: "Add a claim to a user",
      fields: userClaimFields
    },
    "/Security/Api/User/AddClaimList": {
      method: "POST",
      description: "Add multiple claims to a user (array of UserClaimModel)",
      fields: userClaimFields
    },
    "/Security/Api/User/RemoveClaim": {
      method: "POST",
      description: "Remove a claim from a user",
      fields: userClaimFields
    },
    "/Security/Api/User/RemoveClaimList": {
      method: "POST",
      description: "Remove multiple claims from a user (array of UserClaimModel)",
      fields: userClaimFields
    },
    "/Security/Api/User/ReplaceClaim": {
      method: "POST",
      description: "Replace an existing claim with a new one",
      fields: [
        { name: "claim.userId", type: "guid", required: true, placeholder: "User ID" },
        { name: "claim.claimType", type: "string", required: true, placeholder: "Existing claim type" },
        { name: "claim.claimValue", type: "string", required: true, placeholder: "Existing claim value" },
        { name: "newClaim.userId", type: "guid", required: true, placeholder: "User ID" },
        { name: "newClaim.claimType", type: "string", required: true, placeholder: "New claim type" },
        { name: "newClaim.claimValue", type: "string", required: true, placeholder: "New claim value" }
      ]
    },
    "/Security/Api/User/GetAdminUserClaims": {
      method: "POST",
      description: "Get admin-level claims for a user",
      fields: [guidId("id", "User ID")]
    },
    "/Security/Api/User/AddAdminClaim": {
      method: "POST",
      description: "Add an admin-level claim to a user",
      fields: userClaimFields
    },
    "/Security/Api/User/AddAdminClaimList": {
      method: "POST",
      description: "Add multiple admin-level claims to a user (array)",
      fields: userClaimFields
    },
    "/Security/Api/User/RemoveAdminClaim": {
      method: "POST",
      description: "Remove an admin-level claim from a user",
      fields: userClaimFields
    },
    "/Security/Api/User/RemoveAdminClaimList": {
      method: "POST",
      description: "Remove multiple admin-level claims from a user (array)",
      fields: userClaimFields
    },
    "/Security/Api/User/GetAllSecurityQuestions": {
      method: "POST",
      description: "Get all available security questions",
      fields: []
    },
    "/Security/Api/User/AddSecurityQuestion": {
      method: "POST",
      description: "Add a new security question",
      fields: [{ name: "question", type: "string", required: true, placeholder: "e.g. What is your pet's name?" }]
    },
    "/Security/Api/User/UpdateSecurityQuestion": {
      method: "POST",
      description: "Update an existing security question",
      fields: [
        guidId("id", "Security question ID"),
        { name: "question", type: "string", required: true, placeholder: "Updated question text" }
      ]
    },
    "/Security/Api/User/DeleteSecurityQuestion": {
      method: "POST",
      description: "Delete a security question by ID",
      fields: [guidId("id", "Security question ID")]
    },
    "/Security/Api/User/GetUserSecurityQuestions": {
      method: "POST",
      description: "Get security questions for a user",
      fields: [guidId("id", "User ID")]
    },
    "/Security/Api/User/AddUserSecurityQuestion": {
      method: "POST",
      description: "Add a security question answer for a user",
      fields: userSecurityQuestionFields
    },
    "/Security/Api/User/AddUserSecurityQuestionList": {
      method: "POST",
      description: "Add multiple security question answers for a user (array)",
      fields: userSecurityQuestionFields
    },
    "/Security/Api/User/UpdateUserSecurityQuestion": {
      method: "POST",
      description: "Update a user's security question answer",
      fields: [
        guidId("id", "User security question record ID"),
        ...userSecurityQuestionFields
      ]
    },
    "/Security/Api/User/DeleteUserSecurityQuestion": {
      method: "POST",
      description: "Delete a user's security question answer",
      fields: [
        guidId("id", "User security question record ID"),
        guidId("userId", "User ID"),
        guidId("securityQuestionId", "Security question ID")
      ]
    },
    "/Security/Api/User/DeleteUserSecurityQuestionList": {
      method: "POST",
      description: "Delete multiple user security question answers (array)",
      fields: userSecurityQuestionFields
    },
    // =========================================================================
    // CLIENT
    // =========================================================================
    "/Security/Api/Client/RegisterClient": {
      method: "POST",
      description: "Register a new OAuth client application",
      fields: [
        { name: "clientId", type: "string", required: true, placeholder: "e.g. my-web-app" },
        { name: "clientName", type: "string", required: true, placeholder: "e.g. My Web Application" },
        { name: "clientUri", type: "string", placeholder: "e.g. https://myapp.com" },
        { name: "clientSecret", type: "string", placeholder: "Client secret (auto-generated if blank)" },
        { name: "requireClientSecret", type: "boolean", defaultValue: "true" },
        { name: "requirePkce", type: "boolean", defaultValue: "false", description: "Require PKCE for authorization code flow" },
        { name: "isPkceTextPlain", type: "boolean", defaultValue: "false" },
        { name: "isFirstPartyApp", type: "boolean", defaultValue: "true" },
        { name: "applicationType", type: "enum", defaultValue: "1", enumValues: ["1 - RegularWeb", "2 - SinglePageApp", "3 - Native", "4 - Service"] },
        { name: "accessTokenType", type: "enum", defaultValue: "1", enumValues: ["1 - JWT"] },
        { name: "allowOfflineAccess", type: "boolean", defaultValue: "false", description: "Allow refresh tokens" },
        { name: "allowAccessTokensViaBrowser", type: "boolean", defaultValue: "false" },
        { name: "allowedSigningAlgorithm", type: "string", defaultValue: "RS256", placeholder: "e.g. RS256" },
        { name: "accessTokenExpiration", type: "number", defaultValue: "900", description: "Access token lifetime in seconds" },
        { name: "refreshTokenExpiration", type: "number", defaultValue: "86400", description: "Refresh token lifetime in seconds" },
        { name: "identityTokenExpiration", type: "number", defaultValue: "3600", description: "Identity token lifetime in seconds" },
        { name: "logoutTokenExpiration", type: "number", defaultValue: "1800" },
        { name: "authorizationCodeExpiration", type: "number", defaultValue: "600" },
        { name: "supportedGrantTypes", type: "array", placeholder: '["client_credentials","authorization_code"]', description: "Supported OAuth grant types" },
        { name: "supportedResponseTypes", type: "array", placeholder: '["code","token"]' },
        { name: "allowedScopes", type: "array", placeholder: '["openid","profile","api"]', description: "Allowed scopes" },
        { name: "redirectUris", type: "array", placeholder: '[{"redirectUri":"https://myapp.com/callback"}]', description: "Array of {redirectUri} objects" },
        { name: "postLogoutRedirectUris", type: "array", placeholder: '[{"postLogoutRedirectUri":"https://myapp.com"}]', description: "Array of {postLogoutRedirectUri} objects" },
        { name: "preferredAudience", type: "string", placeholder: "e.g. api://my-resource" },
        { name: "frontChannelLogoutUri", type: "string", placeholder: "e.g. https://myapp.com/signout" },
        { name: "frontChannelLogoutSessionRequired", type: "boolean", defaultValue: "false" },
        { name: "backChannelLogoutUri", type: "string" },
        { name: "backChannelLogoutSessionRequired", type: "boolean", defaultValue: "false" }
      ]
    },
    "/Security/Api/Client/UpdateClient": {
      method: "POST",
      description: "Update an existing OAuth client application",
      fields: [
        guidId("id", "Internal client record ID"),
        { name: "clientId", type: "string", required: true, placeholder: "e.g. my-web-app" },
        { name: "clientName", type: "string", required: true, placeholder: "e.g. My Web Application" },
        { name: "clientUri", type: "string", placeholder: "e.g. https://myapp.com" },
        { name: "clientSecret", type: "string", placeholder: "Client secret" },
        { name: "requireClientSecret", type: "boolean", defaultValue: "true" },
        { name: "requirePkce", type: "boolean", defaultValue: "false" },
        { name: "applicationType", type: "enum", defaultValue: "1", enumValues: ["1 - RegularWeb", "2 - SinglePageApp", "3 - Native", "4 - Service"] },
        { name: "accessTokenType", type: "enum", defaultValue: "1", enumValues: ["1 - JWT"] },
        { name: "allowOfflineAccess", type: "boolean", defaultValue: "false" },
        { name: "accessTokenExpiration", type: "number", defaultValue: "900" },
        { name: "refreshTokenExpiration", type: "number", defaultValue: "86400" },
        { name: "identityTokenExpiration", type: "number", defaultValue: "3600" },
        { name: "supportedGrantTypes", type: "array", placeholder: '["client_credentials"]' },
        { name: "allowedScopes", type: "array", placeholder: '["openid","profile"]' },
        { name: "redirectUris", type: "array", placeholder: '[{"redirectUri":"https://..."}]' },
        { name: "postLogoutRedirectUris", type: "array", placeholder: '[{"postLogoutRedirectUri":"https://..."}]' }
      ]
    },
    "/Security/Api/Client/GetAllClient": {
      method: "POST",
      description: "Retrieve all registered clients",
      fields: []
    },
    "/Security/Api/Client/GetClient": {
      method: "POST",
      description: "Get a client by client ID string",
      fields: [{ name: "clientId", type: "string", required: true, placeholder: "e.g. my-web-app" }]
    },
    "/Security/Api/Client/DeleteClient": {
      method: "POST",
      description: "Delete a client by client ID string",
      fields: [{ name: "clientId", type: "string", required: true, placeholder: "e.g. my-web-app" }]
    },
    "/Security/Api/Client/GenerateClientSecret": {
      method: "POST",
      description: "Generate a new secret for a client",
      fields: [{ name: "clientId", type: "string", required: true, placeholder: "e.g. my-web-app" }]
    },
    // =========================================================================
    // ROLE
    // =========================================================================
    "/Security/Api/Role/CreateRole": {
      method: "POST",
      description: "Create a new role",
      fields: [
        { name: "name", type: "string", required: true, placeholder: "e.g. Administrator" },
        { name: "description", type: "string", placeholder: "e.g. Full system access" }
      ]
    },
    "/Security/Api/Role/UpdateRole": {
      method: "POST",
      description: "Update an existing role",
      fields: [
        guidId("id", "Role ID to update"),
        { name: "name", type: "string", required: true, placeholder: "e.g. Administrator" },
        { name: "description", type: "string", placeholder: "e.g. Full system access" }
      ]
    },
    "/Security/Api/Role/DeleteRoleById": {
      method: "POST",
      description: "Delete a role by ID",
      fields: [guidId("id", "Role ID to delete")]
    },
    "/Security/Api/Role/DeleteRoleByName": {
      method: "POST",
      description: "Delete a role by name",
      fields: [{ name: "roleName", type: "string", required: true, placeholder: "e.g. Administrator" }]
    },
    "/Security/Api/Role/GetRoleById": {
      method: "POST",
      description: "Get a role by ID",
      fields: [guidId("id", "Role ID")]
    },
    "/Security/Api/Role/GetRoleByName": {
      method: "POST",
      description: "Get a role by name",
      fields: [{ name: "roleName", type: "string", required: true, placeholder: "e.g. Administrator" }]
    },
    "/Security/Api/Role/GetAllRoles": {
      method: "POST",
      description: "Retrieve all roles",
      fields: []
    },
    "/Security/Api/Role/AddRoleClaim": {
      method: "POST",
      description: "Add a claim to a role",
      fields: roleClaimFields
    },
    "/Security/Api/Role/AddRoleClaimList": {
      method: "POST",
      description: "Add multiple claims to a role (array of RoleClaimModel)",
      fields: roleClaimFields
    },
    "/Security/Api/Role/GetRoleClaim": {
      method: "POST",
      description: "Get claims for a role",
      fields: [guidId("roleId", "Role ID")]
    },
    "/Security/Api/Role/RemoveRoleClaim": {
      method: "POST",
      description: "Remove a claim from a role",
      fields: roleClaimFields
    },
    "/Security/Api/Role/RemoveRoleClaimsById": {
      method: "POST",
      description: "Remove all claims from a role by role ID",
      fields: [guidId("roleId", "Role ID")]
    },
    "/Security/Api/Role/RemoveRoleClaimsList": {
      method: "POST",
      description: "Remove multiple claims from a role (array of RoleClaimModel)",
      fields: roleClaimFields
    },
    // =========================================================================
    // API RESOURCE
    // =========================================================================
    "/Security/Api/ApiResource/AddApiResource": {
      method: "POST",
      description: "Add a new API resource",
      fields: [
        { name: "name", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.apiresource" },
        { name: "displayName", type: "string", required: true, placeholder: "e.g. HCL.CS.SF API" },
        { name: "description", type: "string", placeholder: "Resource description" },
        { name: "enabled", type: "boolean", defaultValue: "true" }
      ]
    },
    "/Security/Api/ApiResource/UpdateApiResource": {
      method: "POST",
      description: "Update an existing API resource",
      fields: [
        guidId("id", "API resource ID"),
        { name: "name", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.apiresource" },
        { name: "displayName", type: "string", required: true, placeholder: "e.g. HCL.CS.SF API" },
        { name: "description", type: "string", placeholder: "Resource description" },
        { name: "enabled", type: "boolean", defaultValue: "true" }
      ]
    },
    "/Security/Api/ApiResource/DeleteApiResourceById": {
      method: "POST",
      description: "Delete an API resource by ID",
      fields: [guidId("id", "API resource ID")]
    },
    "/Security/Api/ApiResource/DeleteApiResourceByName": {
      method: "POST",
      description: "Delete an API resource by name",
      fields: [{ name: "apiResourceName", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.apiresource" }]
    },
    "/Security/Api/ApiResource/GetApiResourceById": {
      method: "POST",
      description: "Get an API resource by ID",
      fields: [guidId("id", "API resource ID")]
    },
    "/Security/Api/ApiResource/GetApiResourceByName": {
      method: "POST",
      description: "Get an API resource by name",
      fields: [{ name: "apiResourceName", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.apiresource" }]
    },
    "/Security/Api/ApiResource/GetAllApiResources": {
      method: "POST",
      description: "Retrieve all API resources",
      fields: []
    },
    "/Security/Api/ApiResource/GetAllApiResourcesByScopesAsync": {
      method: "POST",
      description: "Get API resources that contain specific scopes",
      fields: [{ name: "apiScopeNames", type: "array", required: true, placeholder: '["scope1","scope2"]', description: "List of scope names" }]
    },
    "/Security/Api/ApiResource/GetApiResourceClaimsById": {
      method: "POST",
      description: "Get claims for an API resource",
      fields: [guidId("apiResourceId", "API resource ID")]
    },
    "/Security/Api/ApiResource/AddApiResourceClaim": {
      method: "POST",
      description: "Add a claim to an API resource",
      fields: [
        guidId("apiResourceId", "API resource ID"),
        { name: "type", type: "string", required: true, placeholder: "e.g. role", description: "Claim type" }
      ]
    },
    "/Security/Api/ApiResource/DeleteApiResourceClaimById": {
      method: "POST",
      description: "Delete an API resource claim by claim ID",
      fields: [guidId("id", "API resource claim ID")]
    },
    "/Security/Api/ApiResource/DeleteApiResourceClaimByResourceIdAsync": {
      method: "POST",
      description: "Delete all claims for an API resource",
      fields: [guidId("apiResourceId", "API resource ID")]
    },
    "/Security/Api/ApiResource/DeleteApiResourceClaimModel": {
      method: "POST",
      description: "Delete a specific API resource claim by model",
      fields: [
        guidId("apiResourceId", "API resource ID"),
        { name: "type", type: "string", required: true, placeholder: "e.g. role" }
      ]
    },
    "/Security/Api/ApiResource/AddApiScope": {
      method: "POST",
      description: "Add a new API scope",
      fields: [
        guidId("apiResourceId", "Parent API resource ID"),
        { name: "name", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.read" },
        { name: "displayName", type: "string", required: true, placeholder: "e.g. Read Access" },
        { name: "description", type: "string", placeholder: "Scope description" },
        { name: "required", type: "boolean", defaultValue: "false" },
        { name: "emphasize", type: "boolean", defaultValue: "false" }
      ]
    },
    "/Security/Api/ApiResource/UpdateApiScope": {
      method: "POST",
      description: "Update an existing API scope",
      fields: [
        guidId("id", "API scope ID"),
        guidId("apiResourceId", "Parent API resource ID"),
        { name: "name", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.read" },
        { name: "displayName", type: "string", required: true, placeholder: "e.g. Read Access" },
        { name: "description", type: "string", placeholder: "Scope description" },
        { name: "required", type: "boolean", defaultValue: "false" },
        { name: "emphasize", type: "boolean", defaultValue: "false" }
      ]
    },
    "/Security/Api/ApiResource/DeleteApiScopeById": {
      method: "POST",
      description: "Delete an API scope by ID",
      fields: [guidId("id", "API scope ID")]
    },
    "/Security/Api/ApiResource/DeleteApiScopeByName": {
      method: "POST",
      description: "Delete an API scope by name",
      fields: [{ name: "apiScopeName", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.read" }]
    },
    "/Security/Api/ApiResource/GetApiScopeById": {
      method: "POST",
      description: "Get an API scope by ID",
      fields: [guidId("id", "API scope ID")]
    },
    "/Security/Api/ApiResource/GetApiScopeByName": {
      method: "POST",
      description: "Get an API scope by name",
      fields: [{ name: "apiScopeName", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.read" }]
    },
    "/Security/Api/ApiResource/GetAllApiScopes": {
      method: "POST",
      description: "Retrieve all API scopes",
      fields: []
    },
    "/Security/Api/ApiResource/GetApiScopeClaims": {
      method: "POST",
      description: "Get claims for an API scope",
      fields: [guidId("apiScopeId", "API scope ID")]
    },
    "/Security/Api/ApiResource/AddApiScopeClaim": {
      method: "POST",
      description: "Add a claim to an API scope",
      fields: [
        guidId("apiScopeId", "API scope ID"),
        { name: "type", type: "string", required: true, placeholder: "e.g. role", description: "Claim type" }
      ]
    },
    "/Security/Api/ApiResource/DeleteApiScopeClaimById": {
      method: "POST",
      description: "Delete an API scope claim by claim ID",
      fields: [guidId("id", "API scope claim ID")]
    },
    "/Security/Api/ApiResource/DeleteApiScopeClaimByScopeId": {
      method: "POST",
      description: "Delete all claims for an API scope",
      fields: [guidId("apiScopeId", "API scope ID")]
    },
    "/Security/Api/ApiResource/DeleteApiScopeClaimModel": {
      method: "POST",
      description: "Delete a specific API scope claim by model",
      fields: [
        guidId("apiScopeId", "API scope ID"),
        { name: "type", type: "string", required: true, placeholder: "e.g. role" }
      ]
    },
    // =========================================================================
    // IDENTITY RESOURCE
    // =========================================================================
    "/Security/Api/IdentityResource/AddIdentityResource": {
      method: "POST",
      description: "Add a new identity resource",
      fields: [
        { name: "name", type: "string", required: true, placeholder: "e.g. openid" },
        { name: "displayName", type: "string", required: true, placeholder: "e.g. OpenID" },
        { name: "description", type: "string", placeholder: "Resource description" },
        { name: "enabled", type: "boolean", defaultValue: "true" },
        { name: "required", type: "boolean", defaultValue: "false" },
        { name: "emphasize", type: "boolean", defaultValue: "false" }
      ]
    },
    "/Security/Api/IdentityResource/UpdateIdentityResource": {
      method: "POST",
      description: "Update an existing identity resource",
      fields: [
        guidId("id", "Identity resource ID"),
        { name: "name", type: "string", required: true, placeholder: "e.g. openid" },
        { name: "displayName", type: "string", required: true, placeholder: "e.g. OpenID" },
        { name: "description", type: "string", placeholder: "Resource description" },
        { name: "enabled", type: "boolean", defaultValue: "true" },
        { name: "required", type: "boolean", defaultValue: "false" },
        { name: "emphasize", type: "boolean", defaultValue: "false" }
      ]
    },
    "/Security/Api/IdentityResource/DeleteIdentityResourceById": {
      method: "POST",
      description: "Delete an identity resource by ID",
      fields: [guidId("id", "Identity resource ID")]
    },
    "/Security/Api/IdentityResource/DeleteIdentityResourceByName": {
      method: "POST",
      description: "Delete an identity resource by name",
      fields: [{ name: "identityResourceName", type: "string", required: true, placeholder: "e.g. openid" }]
    },
    "/Security/Api/IdentityResource/GetAllIdentityResources": {
      method: "POST",
      description: "Retrieve all identity resources",
      fields: []
    },
    "/Security/Api/IdentityResource/GetIdentityResourceById": {
      method: "POST",
      description: "Get an identity resource by ID",
      fields: [guidId("id", "Identity resource ID")]
    },
    "/Security/Api/IdentityResource/GetIdentityResourceByName": {
      method: "POST",
      description: "Get an identity resource by name",
      fields: [{ name: "identityResourceName", type: "string", required: true, placeholder: "e.g. openid" }]
    },
    "/Security/Api/IdentityResource/GetIdentityResourceClaims": {
      method: "POST",
      description: "Get claims for an identity resource",
      fields: [guidId("identityResourceId", "Identity resource ID")]
    },
    "/Security/Api/IdentityResource/AddIdentityResourceClaim": {
      method: "POST",
      description: "Add a claim to an identity resource",
      fields: [
        guidId("identityResourceId", "Identity resource ID"),
        { name: "type", type: "string", required: true, placeholder: "e.g. sub", description: "Claim type" },
        { name: "aliasType", type: "string", placeholder: "Optional alias for the claim type" }
      ]
    },
    "/Security/Api/IdentityResource/DeleteIdentityResourceClaimById": {
      method: "POST",
      description: "Delete an identity resource claim by claim ID",
      fields: [guidId("id", "Identity resource claim ID")]
    },
    "/Security/Api/IdentityResource/DeleteIdentityResourceClaimByResourceIdAsync": {
      method: "POST",
      description: "Delete all claims for an identity resource",
      fields: [guidId("identityResourceId", "Identity resource ID")]
    },
    "/Security/Api/IdentityResource/DeleteIdentityResourceClaimModel": {
      method: "POST",
      description: "Delete a specific identity resource claim by model",
      fields: [
        guidId("identityResourceId", "Identity resource ID"),
        { name: "type", type: "string", required: true, placeholder: "e.g. sub" }
      ]
    },
    // =========================================================================
    // AUTHENTICATION
    // =========================================================================
    "/Security/Api/Authentication/PasswordSignIn": {
      method: "POST",
      description: "Sign in with username and password",
      fields: [
        { name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "password", type: "string", required: true, placeholder: "Password" }
      ]
    },
    "/Security/Api/Authentication/PasswordSignInByTwoFactorAuthenticatorToken": {
      method: "POST",
      description: "Sign in with password and two-factor authenticator token",
      fields: [
        { name: "userName", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "password", type: "string", required: true, placeholder: "Password" },
        { name: "twofactorToken", type: "string", required: true, placeholder: "6-digit authenticator code" }
      ]
    },
    "/Security/Api/Authentication/TwoFactorAuthenticatorAppSignIn": {
      method: "POST",
      description: "Complete 2FA sign-in with authenticator app code",
      fields: [{ name: "code", type: "string", required: true, placeholder: "6-digit authenticator code" }]
    },
    "/Security/Api/Authentication/TwoFactorEmailSignIn": {
      method: "POST",
      description: "Complete 2FA sign-in with email code",
      fields: [{ name: "code", type: "string", required: true, placeholder: "Email verification code" }]
    },
    "/Security/Api/Authentication/TwoFactorRecoveryCodeSignIn": {
      method: "POST",
      description: "Complete 2FA sign-in with a recovery code",
      fields: [{ name: "code", type: "string", required: true, placeholder: "Recovery code" }]
    },
    "/Security/Api/Authentication/TwoFactorSmsSignInAsync": {
      method: "POST",
      description: "Complete 2FA sign-in with SMS code",
      fields: [{ name: "code", type: "string", required: true, placeholder: "SMS verification code" }]
    },
    "/Security/Api/Authentication/RopValidateCredentials": {
      method: "POST",
      description: "Validate credentials using Resource Owner Password flow",
      fields: [
        { name: "username", type: "string", required: true, placeholder: "e.g. john.doe" },
        { name: "password", type: "string", required: true, placeholder: "Password" },
        { name: "client_id", type: "string", required: true, placeholder: "e.g. my-client" },
        { name: "client_secret", type: "string", required: true, placeholder: "Client secret" },
        { name: "scope", type: "string", required: true, placeholder: "e.g. HCL.CS.SF.apiresource HCL.CS.SF.client" }
      ]
    },
    "/Security/Api/Authentication/IsUserSignedIn": {
      method: "POST",
      description: "Check if the current user is signed in (uses session)",
      fields: []
    },
    "/Security/Api/Authentication/SignOut": {
      method: "POST",
      description: "Sign out the current user",
      fields: []
    },
    "/Security/Api/Authentication/SetupAuthenticatorApp": {
      method: "POST",
      description: "Set up authenticator app for a user (returns QR code data)",
      fields: [
        guidId("userId", "User ID"),
        { name: "applicationName", type: "string", required: true, placeholder: "e.g. HCL.CS.SF Identity" }
      ]
    },
    "/Security/Api/Authentication/VerifyAuthenticatorAppSetup": {
      method: "POST",
      description: "Verify authenticator app setup with a test token",
      fields: [
        guidId("userId", "User ID"),
        { name: "token", type: "string", required: true, placeholder: "6-digit authenticator code" }
      ]
    },
    "/Security/Api/Authentication/ResetAuthenticatorApp": {
      method: "POST",
      description: "Reset authenticator app for a user",
      fields: [guidId("userId", "User ID")]
    },
    "/Security/Api/Authentication/GenerateRecoveryCodes": {
      method: "POST",
      description: "Generate new recovery codes for a user",
      fields: [guidId("userId", "User ID")]
    },
    "/Security/Api/Authentication/CountRecoveryCodesAsync": {
      method: "POST",
      description: "Count remaining recovery codes for a user",
      fields: [guidId("userId", "User ID")]
    },
    // =========================================================================
    // SECURITY TOKEN
    // =========================================================================
    "/Security/Api/SecurityToken/GetActiveSecurityTokensByClientIds": {
      method: "POST",
      description: "Get active security tokens for specific clients",
      fields: [
        { name: "clientsList", type: "array", required: true, placeholder: '["client-id-1","client-id-2"]', description: "List of client ID strings" },
        ...pagingFields
      ]
    },
    "/Security/Api/SecurityToken/GetActiveSecurityTokensByUserIds": {
      method: "POST",
      description: "Get active security tokens for specific users",
      fields: [
        { name: "userList", type: "array", required: true, placeholder: '["user-id-1","user-id-2"]', description: "List of user ID strings" },
        ...pagingFields
      ]
    },
    "/Security/Api/SecurityToken/GetActiveSecurityTokensBetweenDates": {
      method: "POST",
      description: "Get active security tokens within a date range",
      fields: [
        { name: "fromdate", type: "date", required: true, description: "Start date" },
        { name: "todate", type: "date", required: true, description: "End date" },
        ...pagingFields
      ]
    },
    "/Security/Api/SecurityToken/GetAllSecurityTokensBetweenDates": {
      method: "POST",
      description: "Get all security tokens (active and expired) within a date range",
      fields: [
        { name: "fromdate", type: "date", required: true, description: "Start date" },
        { name: "todate", type: "date", required: true, description: "End date" },
        ...pagingFields
      ]
    },
    // =========================================================================
    // AUDIT
    // =========================================================================
    "/Security/Api/Audittrail/GetAuditDetails": {
      method: "POST",
      description: "Search audit trail logs with filters",
      fields: [
        { name: "actionType", type: "enum", enumValues: ["0 - None", "1 - Create", "2 - Update", "3 - Delete"], description: "Filter by action type" },
        { name: "createdBy", type: "string", placeholder: "e.g. admin" },
        { name: "fromDate", type: "date", description: "Start date filter" },
        { name: "toDate", type: "date", description: "End date filter" },
        { name: "searchValue", type: "string", placeholder: "Search text" },
        ...pagingFields
      ]
    },
    "/Security/Api/Audittrail/AddAuditTrail": {
      method: "POST",
      description: "Add an audit trail entry",
      fields: [
        { name: "actionType", type: "enum", required: true, enumValues: ["0 - None", "1 - Create", "2 - Update", "3 - Delete"] },
        { name: "tableName", type: "string", required: true, placeholder: "e.g. Users" },
        { name: "actionName", type: "string", required: true, placeholder: "e.g. UserCreated" },
        { name: "oldValue", type: "string", placeholder: "Previous value (JSON)" },
        { name: "newValue", type: "string", placeholder: "New value (JSON)" },
        { name: "affectedColumn", type: "string", placeholder: "e.g. Email" }
      ]
    },
    "/Security/Api/Audittrail/AddAuditTrailModel": {
      method: "POST",
      description: "Add an audit trail entry (model variant)",
      fields: [
        { name: "actionType", type: "enum", required: true, enumValues: ["0 - None", "1 - Create", "2 - Update", "3 - Delete"] },
        { name: "tableName", type: "string", required: true, placeholder: "e.g. Users" },
        { name: "actionName", type: "string", required: true, placeholder: "e.g. UserCreated" },
        { name: "oldValue", type: "string", placeholder: "Previous value (JSON)" },
        { name: "newValue", type: "string", placeholder: "New value (JSON)" },
        { name: "affectedColumn", type: "string", placeholder: "e.g. Email" }
      ]
    },
    // =========================================================================
    // NOTIFICATION
    // =========================================================================
    "/Security/Api/Notification/GetNotificationLogs": {
      method: "POST",
      description: "Search notification delivery logs",
      fields: [
        { name: "type", type: "enum", enumValues: ["1 - Email", "2 - SMS"], description: "Notification channel" },
        { name: "status", type: "enum", enumValues: [
          "1 - Initiated",
          "2 - Delivered",
          "3 - Failed",
          "4 - Delayed",
          "5 - Relayed",
          "6 - Expanded",
          "7 - Queued",
          "8 - Sending",
          "9 - Sent",
          "10 - Undelivered",
          "11 - Receiving",
          "12 - Received",
          "13 - Accepted",
          "14 - Scheduled",
          "15 - Read",
          "16 - Partially"
        ], description: "Delivery status filter" },
        { name: "fromDate", type: "string", placeholder: "e.g. 2024-01-01", description: "Start date (string)" },
        { name: "toDate", type: "string", placeholder: "e.g. 2024-12-31", description: "End date (string)" },
        { name: "searchValue", type: "string", placeholder: "Search text" },
        ...pagingFields
      ]
    },
    "/Security/Api/Notification/GetNotificationTemplates": {
      method: "POST",
      description: "Get all notification templates (email and SMS)",
      fields: []
    },
    "/Security/Api/Notification/GetProviderConfig": {
      method: "POST",
      description: "Get a notification provider configuration by ID",
      fields: [guidId("id", "Provider config ID")]
    },
    "/Security/Api/Notification/GetAllProviderConfigs": {
      method: "POST",
      description: "Get all notification provider configurations",
      fields: []
    },
    "/Security/Api/Notification/SaveProviderConfig": {
      method: "POST",
      description: "Create or update a notification provider configuration",
      fields: [
        { name: "id", type: "guid", placeholder: "Leave empty for new, provide for update", description: "Provider config ID (optional for create)" },
        { name: "providerName", type: "string", required: true, placeholder: "e.g. SendGrid" },
        { name: "channelType", type: "enum", required: true, enumValues: ["1 - Email", "2 - SMS"], description: "Notification channel" },
        { name: "isActive", type: "boolean", defaultValue: "false" },
        { name: "settings", type: "object", required: true, placeholder: '{"apiKey":"...","fromEmail":"..."}', description: "Provider-specific settings (key-value pairs)" }
      ]
    },
    "/Security/Api/Notification/SetActiveProvider": {
      method: "POST",
      description: "Set a notification provider as the active one",
      fields: [guidId("id", "Provider config ID to activate")]
    },
    "/Security/Api/Notification/DeleteProviderConfig": {
      method: "POST",
      description: "Delete a notification provider configuration",
      fields: [guidId("id", "Provider config ID to delete")]
    },
    "/Security/Api/Notification/GetProviderFieldDefinitions": {
      method: "POST",
      description: "Get field definitions for all notification providers",
      fields: []
    },
    "/Security/Api/Notification/SendTestNotification": {
      method: "POST",
      description: "Send a test notification (email or SMS)",
      fields: [
        { name: "type", type: "enum", required: true, enumValues: ["1 - Email", "2 - SMS"], description: "Notification channel" },
        { name: "recipient", type: "string", required: true, placeholder: "e.g. test@example.com or +1234567890" },
        { name: "providerConfigId", type: "guid", placeholder: "Optional: specific provider config to test" }
      ]
    },
    // =========================================================================
    // EXTERNAL AUTH
    // =========================================================================
    "/Security/Api/ExternalAuth/GetAllExternalAuthProviders": {
      method: "POST",
      description: "Get all external authentication providers",
      fields: []
    },
    "/Security/Api/ExternalAuth/GetExternalAuthProvider": {
      method: "POST",
      description: "Get an external auth provider by ID",
      fields: [guidId("id", "External auth provider ID")]
    },
    "/Security/Api/ExternalAuth/SaveExternalAuthProvider": {
      method: "POST",
      description: "Create or update an external authentication provider",
      fields: [
        { name: "id", type: "guid", placeholder: "Leave empty for new, provide for update" },
        { name: "providerName", type: "string", required: true, placeholder: "e.g. Google" },
        { name: "providerType", type: "number", required: true, placeholder: "e.g. 1", description: "Provider type identifier" },
        { name: "isEnabled", type: "boolean", defaultValue: "true" },
        { name: "settings", type: "object", required: true, placeholder: '{"clientId":"...","clientSecret":"..."}', description: "Provider-specific settings" },
        { name: "autoProvisionEnabled", type: "boolean", defaultValue: "false", description: "Auto-create user accounts on first login" },
        { name: "allowedDomains", type: "string", placeholder: "e.g. example.com,corp.com", description: "Comma-separated allowed email domains" }
      ]
    },
    "/Security/Api/ExternalAuth/DeleteExternalAuthProvider": {
      method: "POST",
      description: "Delete an external authentication provider",
      fields: [guidId("id", "External auth provider ID")]
    },
    "/Security/Api/ExternalAuth/TestExternalAuthProvider": {
      method: "POST",
      description: "Test connectivity for an external auth provider",
      fields: [guidId("id", "External auth provider ID to test")]
    },
    "/Security/Api/ExternalAuth/GetExternalAuthFieldDefinitions": {
      method: "POST",
      description: "Get field definitions for all external auth provider types",
      fields: []
    },
    // =========================================================================
    // OPENID CONNECT ENDPOINTS
    // =========================================================================
    "/security/token": {
      method: "POST",
      description: "OAuth 2.0 token endpoint \u2014 exchange credentials for tokens",
      fields: [
        { name: "grant_type", type: "enum", required: true, enumValues: ["client_credentials", "authorization_code", "refresh_token", "password"], description: "OAuth grant type" },
        { name: "client_id", type: "string", required: true, placeholder: "e.g. my-client" },
        { name: "client_secret", type: "string", placeholder: "Client secret" },
        { name: "scope", type: "string", placeholder: "e.g. openid profile HCL.CS.SF.apiresource" },
        { name: "code", type: "string", placeholder: "Authorization code (for authorization_code grant)" },
        { name: "redirect_uri", type: "string", placeholder: "e.g. https://myapp.com/callback" },
        { name: "refresh_token", type: "string", placeholder: "Refresh token (for refresh_token grant)" },
        { name: "username", type: "string", placeholder: "Username (for password grant)" },
        { name: "password", type: "string", placeholder: "Password (for password grant)" }
      ]
    },
    "/security/introspect": {
      method: "POST",
      description: "OAuth 2.0 token introspection \u2014 check if a token is active",
      fields: [
        { name: "token", type: "string", required: true, placeholder: "Access or refresh token" },
        { name: "token_type_hint", type: "enum", enumValues: ["access_token", "refresh_token"], description: "Type of token" },
        { name: "client_id", type: "string", required: true, placeholder: "e.g. my-client" },
        { name: "client_secret", type: "string", required: true, placeholder: "Client secret" }
      ]
    },
    "/security/revocation": {
      method: "POST",
      description: "OAuth 2.0 token revocation \u2014 revoke an active token",
      fields: [
        { name: "token", type: "string", required: true, placeholder: "Access or refresh token to revoke" },
        { name: "token_type_hint", type: "enum", enumValues: ["access_token", "refresh_token"], description: "Type of token" },
        { name: "client_id", type: "string", required: true, placeholder: "e.g. my-client" },
        { name: "client_secret", type: "string", required: true, placeholder: "Client secret" }
      ]
    },
    "/security/authorize": {
      method: "GET",
      description: "OAuth 2.0 authorization endpoint (browser redirect)",
      fields: [
        { name: "client_id", type: "string", required: true, placeholder: "e.g. my-client" },
        { name: "response_type", type: "enum", required: true, enumValues: ["code", "token", "id_token"] },
        { name: "redirect_uri", type: "string", required: true, placeholder: "e.g. https://myapp.com/callback" },
        { name: "scope", type: "string", placeholder: "e.g. openid profile" },
        { name: "state", type: "string", placeholder: "CSRF state value" },
        { name: "nonce", type: "string", placeholder: "Nonce for ID token" },
        { name: "code_challenge", type: "string", placeholder: "PKCE code challenge" },
        { name: "code_challenge_method", type: "enum", enumValues: ["S256", "plain"] }
      ]
    },
    "/security/authorize/authorizecallback": {
      method: "GET",
      description: "Authorization callback endpoint",
      fields: []
    },
    "/security/endsession": {
      method: "GET",
      description: "OpenID Connect end session (logout) endpoint",
      fields: [
        { name: "id_token_hint", type: "string", placeholder: "ID token" },
        { name: "post_logout_redirect_uri", type: "string", placeholder: "e.g. https://myapp.com" },
        { name: "state", type: "string", placeholder: "State value" }
      ]
    },
    "/security/endsession/callback": {
      method: "GET",
      description: "End session callback endpoint",
      fields: []
    },
    "/security/userinfo": {
      method: "GET",
      description: "OpenID Connect UserInfo endpoint \u2014 returns claims for the authenticated user",
      fields: []
    },
    "/.well-known/openid-configuration/jwks": {
      method: "GET",
      description: "JSON Web Key Set \u2014 public keys for token verification",
      fields: []
    },
    "/.well-known/openid-configuration": {
      method: "GET",
      description: "OpenID Connect Discovery document",
      fields: []
    },
    // =========================================================================
    // HEALTH
    // =========================================================================
    "/health": {
      method: "GET",
      description: "Health check endpoint",
      fields: []
    },
    "/health/live": {
      method: "GET",
      description: "Liveness probe \u2014 is the service running?",
      fields: []
    },
    "/health/ready": {
      method: "GET",
      description: "Readiness probe \u2014 is the service ready to accept traffic?",
      fields: []
    },
    // =========================================================================
    // DEMO EXTERNAL AUTH
    // =========================================================================
    "/auth/external/google/start": {
      method: "GET",
      description: "Start Google OAuth sign-in flow (demo)",
      fields: []
    },
    "/auth/external/google/callback": {
      method: "GET",
      description: "Google OAuth callback (demo)",
      fields: []
    },
    "/auth/external/link/google": {
      method: "POST",
      description: "Link Google account to existing user (demo)",
      fields: []
    },
    "/auth/external/unlink/google": {
      method: "POST",
      description: "Unlink Google account from user (demo)",
      fields: []
    },
    // =========================================================================
    // INSTALLER
    // =========================================================================
    "/": {
      method: "GET",
      description: "Installer root page",
      fields: []
    },
    "/setup": {
      method: "GET",
      description: "Installer setup page / provider configuration",
      fields: []
    },
    "/setup/provider": {
      method: "POST",
      description: "Submit database provider configuration",
      fields: [
        { name: "provider", type: "string", required: true, placeholder: "e.g. PostgreSQL, SqlServer" }
      ]
    },
    "/setup/connection": {
      method: "POST",
      description: "Submit database connection string",
      fields: [
        { name: "connectionString", type: "string", required: true, placeholder: "e.g. Host=localhost;Database=HCL.CS.SF;..." }
      ]
    },
    "/setup/validate": {
      method: "POST",
      description: "Validate database connection",
      fields: []
    },
    "/setup/migrate": {
      method: "POST",
      description: "Run database migrations",
      fields: []
    },
    "/setup/seed": {
      method: "POST",
      description: "Seed initial data",
      fields: []
    },
    "/complete": {
      method: "GET",
      description: "Installation complete page",
      fields: []
    },
    "/installed": {
      method: "GET",
      description: "Check if already installed",
      fields: []
    },
    "/error": {
      method: "GET",
      description: "Installer error page",
      fields: []
    }
  };

  // HCL.CS.SF-admin/lib/api/routes.ts
  var ApiRoutes = {
    client: {
      deleteClient: "/Security/Api/Client/DeleteClient",
      generateClientSecret: "/Security/Api/Client/GenerateClientSecret",
      getAllClient: "/Security/Api/Client/GetAllClient",
      getClient: "/Security/Api/Client/GetClient",
      registerClient: "/Security/Api/Client/RegisterClient",
      updateClient: "/Security/Api/Client/UpdateClient"
    },
    resource: {
      addApiResource: "/Security/Api/ApiResource/AddApiResource",
      addApiResourceClaim: "/Security/Api/ApiResource/AddApiResourceClaim",
      updateApiResource: "/Security/Api/ApiResource/UpdateApiResource",
      deleteApiResourceById: "/Security/Api/ApiResource/DeleteApiResourceById",
      deleteApiResourceByName: "/Security/Api/ApiResource/DeleteApiResourceByName",
      getApiResourceById: "/Security/Api/ApiResource/GetApiResourceById",
      getApiResourceByName: "/Security/Api/ApiResource/GetApiResourceByName",
      getAllApiResources: "/Security/Api/ApiResource/GetAllApiResources",
      getAllApiResourcesByScopesAsync: "/Security/Api/ApiResource/GetAllApiResourcesByScopesAsync",
      getApiResourceClaimsById: "/Security/Api/ApiResource/GetApiResourceClaimsById",
      addApiScope: "/Security/Api/ApiResource/AddApiScope",
      addApiScopeClaim: "/Security/Api/ApiResource/AddApiScopeClaim",
      updateApiScope: "/Security/Api/ApiResource/UpdateApiScope",
      deleteApiScopeById: "/Security/Api/ApiResource/DeleteApiScopeById",
      deleteApiScopeByName: "/Security/Api/ApiResource/DeleteApiScopeByName",
      getApiScopeById: "/Security/Api/ApiResource/GetApiScopeById",
      getApiScopeByName: "/Security/Api/ApiResource/GetApiScopeByName",
      getAllApiScopes: "/Security/Api/ApiResource/GetAllApiScopes",
      getApiScopeClaims: "/Security/Api/ApiResource/GetApiScopeClaims",
      deleteApiResourceClaimById: "/Security/Api/ApiResource/DeleteApiResourceClaimById",
      deleteApiResourceClaimByResourceIdAsync: "/Security/Api/ApiResource/DeleteApiResourceClaimByResourceIdAsync",
      deleteApiResourceClaimModel: "/Security/Api/ApiResource/DeleteApiResourceClaimModel",
      deleteApiScopeClaimById: "/Security/Api/ApiResource/DeleteApiScopeClaimById",
      deleteApiScopeClaimByScopeId: "/Security/Api/ApiResource/DeleteApiScopeClaimByScopeId",
      deleteApiScopeClaimModel: "/Security/Api/ApiResource/DeleteApiScopeClaimModel"
    },
    identityResource: {
      addIdentityResource: "/Security/Api/IdentityResource/AddIdentityResource",
      addIdentityResourceClaim: "/Security/Api/IdentityResource/AddIdentityResourceClaim",
      updateIdentityResource: "/Security/Api/IdentityResource/UpdateIdentityResource",
      deleteIdentityResourceById: "/Security/Api/IdentityResource/DeleteIdentityResourceById",
      deleteIdentityResourceByName: "/Security/Api/IdentityResource/DeleteIdentityResourceByName",
      deleteIdentityResourceClaimById: "/Security/Api/IdentityResource/DeleteIdentityResourceClaimById",
      deleteIdentityResourceClaimByResourceIdAsync: "/Security/Api/IdentityResource/DeleteIdentityResourceClaimByResourceIdAsync",
      deleteIdentityResourceClaimModel: "/Security/Api/IdentityResource/DeleteIdentityResourceClaimModel",
      getAllIdentityResources: "/Security/Api/IdentityResource/GetAllIdentityResources",
      getIdentityResourceById: "/Security/Api/IdentityResource/GetIdentityResourceById",
      getIdentityResourceByName: "/Security/Api/IdentityResource/GetIdentityResourceByName",
      getIdentityResourceClaims: "/Security/Api/IdentityResource/GetIdentityResourceClaims"
    },
    role: {
      createRole: "/Security/Api/Role/CreateRole",
      updateRole: "/Security/Api/Role/UpdateRole",
      deleteRoleById: "/Security/Api/Role/DeleteRoleById",
      deleteRoleByName: "/Security/Api/Role/DeleteRoleByName",
      getRoleById: "/Security/Api/Role/GetRoleById",
      getRoleByName: "/Security/Api/Role/GetRoleByName",
      getAllRoles: "/Security/Api/Role/GetAllRoles",
      addRoleClaim: "/Security/Api/Role/AddRoleClaim",
      addRoleClaimList: "/Security/Api/Role/AddRoleClaimList",
      getRoleClaim: "/Security/Api/Role/GetRoleClaim",
      removeRoleClaim: "/Security/Api/Role/RemoveRoleClaim",
      removeRoleClaimsById: "/Security/Api/Role/RemoveRoleClaimsById",
      removeRoleClaimsList: "/Security/Api/Role/RemoveRoleClaimsList"
    },
    user: {
      registerUser: "/Security/Api/User/RegisterUser",
      getAllUsers: "/Security/Api/User/GetAllUsers",
      getUserById: "/Security/Api/User/GetUserById",
      getUserByName: "/Security/Api/User/GetUserByName",
      getUserByEmail: "/Security/Api/User/GetUserByEmail",
      updateUser: "/Security/Api/User/UpdateUser",
      deleteUserById: "/Security/Api/User/DeleteUserById",
      deleteUserByName: "/Security/Api/User/DeleteUserByName",
      isUserExistsById: "/Security/Api/User/IsUserExistsById",
      isUserExistsByName: "/Security/Api/User/IsUserExistsByName",
      isUserExistsByClaimPrincipal: "/Security/Api/User/IsUserExistsByClaimPrincipal",
      lockUser: "/Security/Api/User/LockUser",
      lockUserWithEndDatePath: "/Security/Api/lockuserwithenddate",
      unLockUser: "/Security/Api/User/UnLockUser",
      unLockUserByToken: "/Security/Api/User/UnLockUserByToken",
      unLockUserByuserSecurityQuestions: "/Security/Api/User/UnLockUserByuserSecurityQuestions",
      changePassword: "/Security/Api/User/ChangePassword",
      generatePasswordResetToken: "/Security/Api/User/GeneratePasswordResetToken",
      resetPassword: "/Security/Api/User/ResetPassword",
      generateUserTokenAsync: "/Security/Api/User/GenerateUserTokenAsync",
      verifyUserToken: "/Security/Api/User/VerifyUserToken",
      generateEmailConfirmationToken: "/Security/Api/User/GenerateEmailConfirmationToken",
      verifyEmailConfirmationToken: "/Security/Api/User/VerifyEmailConfirmationToken",
      generatePhoneNumberConfirmationToken: "/Security/Api/User/GeneratePhoneNumberConfirmationToken",
      verifyPhoneNumberConfirmationToken: "/Security/Api/User/VerifyPhoneNumberConfirmationToken",
      generateEmailTwoFactorToken: "/Security/Api/User/GenerateEmailTwoFactorToken",
      verifyEmailTwoFactorToken: "/Security/Api/User/VerifyEmailTwoFactorToken",
      generateSmsTwoFactorToken: "/Security/Api/User/GenerateSmsTwoFactorToken",
      verifySmsTwoFactorToken: "/Security/Api/User/VerifySmsTwoFactorToken",
      getAllTwoFactorType: "/Security/Api/User/GetAllTwoFactorType",
      setTwoFactorEnabled: "/Security/Api/User/SetTwoFactorEnabled",
      updateUserTwoFactorType: "/Security/Api/User/UpdateUserTwoFactorType",
      getUserRoles: "/Security/Api/User/GetUserRoles",
      addUserRole: "/Security/Api/User/AddUserRole",
      addUserRolesList: "/Security/Api/User/AddUserRolesList",
      removeUserRole: "/Security/Api/User/RemoveUserRole",
      removeUserRoleList: "/Security/Api/User/RemoveUserRoleList",
      getUserRoleClaimsById: "/Security/Api/User/GetUserRoleClaimsById",
      getUserRoleClaimsByName: "/Security/Api/User/GetUserRoleClaimsByName",
      getUsersInRole: "/Security/Api/User/GetUsersInRole",
      getClaims: "/Security/Api/User/GetClaims",
      getUserClaims: "/Security/Api/User/GetUserClaims",
      getUsersForClaim: "/Security/Api/User/GetUsersForClaim",
      addClaim: "/Security/Api/User/AddClaim",
      addClaimList: "/Security/Api/User/AddClaimList",
      removeClaim: "/Security/Api/User/RemoveClaim",
      removeClaimList: "/Security/Api/User/RemoveClaimList",
      replaceClaim: "/Security/Api/User/ReplaceClaim",
      getAdminUserClaims: "/Security/Api/User/GetAdminUserClaims",
      addAdminClaim: "/Security/Api/User/AddAdminClaim",
      addAdminClaimList: "/Security/Api/User/AddAdminClaimList",
      removeAdminClaim: "/Security/Api/User/RemoveAdminClaim",
      removeAdminClaimList: "/Security/Api/User/RemoveAdminClaimList",
      getAllSecurityQuestions: "/Security/Api/User/GetAllSecurityQuestions",
      addSecurityQuestion: "/Security/Api/User/AddSecurityQuestion",
      updateSecurityQuestion: "/Security/Api/User/UpdateSecurityQuestion",
      deleteSecurityQuestion: "/Security/Api/User/DeleteSecurityQuestion",
      getUserSecurityQuestions: "/Security/Api/User/GetUserSecurityQuestions",
      addUserSecurityQuestion: "/Security/Api/User/AddUserSecurityQuestion",
      addUserSecurityQuestionList: "/Security/Api/User/AddUserSecurityQuestionList",
      updateUserSecurityQuestion: "/Security/Api/User/UpdateUserSecurityQuestion",
      deleteUserSecurityQuestion: "/Security/Api/User/DeleteUserSecurityQuestion",
      deleteUserSecurityQuestionList: "/Security/Api/User/DeleteUserSecurityQuestionList"
    },
    authentication: {
      passwordSignIn: "/Security/Api/Authentication/PasswordSignIn",
      passwordSignInByTwoFactorAuthenticatorToken: "/Security/Api/Authentication/PasswordSignInByTwoFactorAuthenticatorToken",
      twoFactorAuthenticatorAppSignIn: "/Security/Api/Authentication/TwoFactorAuthenticatorAppSignIn",
      twoFactorEmailSignIn: "/Security/Api/Authentication/TwoFactorEmailSignIn",
      twoFactorRecoveryCodeSignIn: "/Security/Api/Authentication/TwoFactorRecoveryCodeSignIn",
      twoFactorSmsSignInAsync: "/Security/Api/Authentication/TwoFactorSmsSignInAsync",
      ropValidateCredentials: "/Security/Api/Authentication/RopValidateCredentials",
      isUserSignedIn: "/Security/Api/Authentication/IsUserSignedIn",
      signOut: "/Security/Api/Authentication/SignOut",
      setupAuthenticatorApp: "/Security/Api/Authentication/SetupAuthenticatorApp",
      verifyAuthenticatorAppSetup: "/Security/Api/Authentication/VerifyAuthenticatorAppSetup",
      resetAuthenticatorApp: "/Security/Api/Authentication/ResetAuthenticatorApp",
      generateRecoveryCodes: "/Security/Api/Authentication/GenerateRecoveryCodes",
      countRecoveryCodesAsync: "/Security/Api/Authentication/CountRecoveryCodesAsync"
    },
    securityToken: {
      getByClientIds: "/Security/Api/SecurityToken/GetActiveSecurityTokensByClientIds",
      getByUserIds: "/Security/Api/SecurityToken/GetActiveSecurityTokensByUserIds",
      getByDateRange: "/Security/Api/SecurityToken/GetActiveSecurityTokensBetweenDates",
      getAllByDateRange: "/Security/Api/SecurityToken/GetAllSecurityTokensBetweenDates"
    },
    audit: {
      getAuditDetails: "/Security/Api/Audittrail/GetAuditDetails",
      addAuditTrail: "/Security/Api/Audittrail/AddAuditTrail",
      addAuditTrailModel: "/Security/Api/Audittrail/AddAuditTrailModel"
    },
    endpoint: {
      authorize: "/security/authorize",
      authorizeCallback: "/security/authorize/authorizecallback",
      token: "/security/token",
      introspect: "/security/introspect",
      endsession: "/security/endsession",
      endsessionCallback: "/security/endsession/callback",
      revocation: "/security/revocation",
      userinfo: "/security/userinfo",
      jwks: "/.well-known/openid-configuration/jwks",
      discovery: "/.well-known/openid-configuration"
    },
    installer: {
      root: "/",
      setup: "/setup",
      providerGet: "/setup",
      providerPost: "/setup/provider",
      connectionGet: "/setup/connection",
      connectionPost: "/setup/connection",
      validateGet: "/setup/validate",
      validatePost: "/setup/validate",
      migrateGet: "/setup/migrate",
      migratePost: "/setup/migrate",
      seedGet: "/setup/seed",
      seedPost: "/setup/seed",
      complete: "/complete",
      installed: "/installed",
      error: "/error"
    },
    demoExternalAuth: {
      googleStart: "/auth/external/google/start",
      googleCallback: "/auth/external/google/callback",
      linkGoogle: "/auth/external/link/google",
      unlinkGoogle: "/auth/external/unlink/google"
    },
    health: {
      health: "/health",
      live: "/health/live",
      ready: "/health/ready"
    },
    externalAuth: {
      getAllProviders: "/Security/Api/ExternalAuth/GetAllExternalAuthProviders",
      getProvider: "/Security/Api/ExternalAuth/GetExternalAuthProvider",
      saveProvider: "/Security/Api/ExternalAuth/SaveExternalAuthProvider",
      deleteProvider: "/Security/Api/ExternalAuth/DeleteExternalAuthProvider",
      testProvider: "/Security/Api/ExternalAuth/TestExternalAuthProvider",
      getFieldDefinitions: "/Security/Api/ExternalAuth/GetExternalAuthFieldDefinitions"
    },
    notification: {
      getNotificationLogs: "/Security/Api/Notification/GetNotificationLogs",
      getNotificationTemplates: "/Security/Api/Notification/GetNotificationTemplates",
      getProviderConfig: "/Security/Api/Notification/GetProviderConfig",
      getAllProviderConfigs: "/Security/Api/Notification/GetAllProviderConfigs",
      saveProviderConfig: "/Security/Api/Notification/SaveProviderConfig",
      setActiveProvider: "/Security/Api/Notification/SetActiveProvider",
      deleteProviderConfig: "/Security/Api/Notification/DeleteProviderConfig",
      getProviderFieldDefinitions: "/Security/Api/Notification/GetProviderFieldDefinitions",
      sendTestNotification: "/Security/Api/Notification/SendTestNotification"
    }
  };
  return __toCommonJS(api_explorer_data_entry_exports);
})();
