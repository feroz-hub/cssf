CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

CREATE TABLE "HCL.CS.SF_ApiResources" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ApiResources" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "Name" TEXT NOT NULL,
    "DisplayName" TEXT NULL,
    "Description" TEXT NULL,
    "Enabled" INTEGER NOT NULL
);

CREATE TABLE "HCL.CS.SF_AuditTrail" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_AuditTrail" PRIMARY KEY,
    "CreatedOn" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ActionType" INTEGER NOT NULL,
    "TableName" TEXT NULL,
    "OldValue" TEXT NULL,
    "NewValue" TEXT NULL,
    "AffectedColumn" TEXT NULL,
    "ActionName" TEXT NULL
);

CREATE TABLE "HCL.CS.SF_Clients" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_Clients" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "ClientId" TEXT NOT NULL,
    "ClientName" TEXT NULL,
    "ClientUri" TEXT NULL,
    "ClientIdIssuedAt" INTEGER NOT NULL,
    "ClientSecretExpiresAt" INTEGER NOT NULL,
    "ClientSecret" TEXT NULL,
    "LogoUri" TEXT NULL,
    "TermsOfServiceUri" TEXT NULL,
    "PolicyUri" TEXT NULL,
    "RefreshTokenExpiration" INTEGER NOT NULL,
    "AccessTokenExpiration" INTEGER NOT NULL,
    "IdentityTokenExpiration" INTEGER NOT NULL,
    "LogoutTokenExpiration" INTEGER NOT NULL,
    "AuthorizationCodeExpiration" INTEGER NOT NULL,
    "AccessTokenType" INTEGER NOT NULL,
    "RequirePkce" INTEGER NOT NULL,
    "IsPkceTextPlain" INTEGER NOT NULL,
    "RequireClientSecret" INTEGER NOT NULL,
    "IsFirstPartyApp" INTEGER NOT NULL,
    "AllowOfflineAccess" INTEGER NOT NULL,
    "AllowedScopes" TEXT NULL,
    "AllowAccessTokensViaBrowser" INTEGER NOT NULL,
    "ApplicationType" INTEGER NOT NULL,
    "AllowedSigningAlgorithm" TEXT NULL,
    "SupportedGrantTypes" TEXT NULL,
    "SupportedResponseTypes" TEXT NULL,
    "FrontChannelLogoutSessionRequired" INTEGER NOT NULL,
    "FrontChannelLogoutUri" TEXT NULL,
    "BackChannelLogoutSessionRequired" INTEGER NOT NULL,
    "BackChannelLogoutUri" TEXT NULL
);

CREATE TABLE "HCL.CS.SF_IdentityResources" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_IdentityResources" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "Name" TEXT NOT NULL,
    "DisplayName" TEXT NULL,
    "Description" TEXT NULL,
    "Enabled" INTEGER NOT NULL,
    "Required" INTEGER NOT NULL,
    "Emphasize" INTEGER NOT NULL
);

CREATE TABLE "HCL.CS.SF_Roles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_Roles" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "NormalizedName" TEXT NOT NULL,
    "ConcurrencyStamp" TEXT NOT NULL,
    "Description" TEXT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL
);

CREATE TABLE "HCL.CS.SF_SecurityQuestions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_SecurityQuestions" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "Question" TEXT NOT NULL
);

CREATE TABLE "HCL.CS.SF_SecurityTokens" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_SecurityTokens" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "Key" TEXT NULL,
    "TokenType" TEXT NULL,
    "TokenValue" TEXT NULL,
    "ClientId" TEXT NULL,
    "SessionId" TEXT NULL,
    "SubjectId" TEXT NULL,
    "CreationTime" TEXT NOT NULL,
    "ExpiresAt" INTEGER NOT NULL,
    "ConsumedTime" TEXT NULL,
    "ConsumedAt" TEXT NULL,
    "TokenReuseDetected" INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE "HCL.CS.SF_Users" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_Users" PRIMARY KEY,
    "UserName" TEXT NOT NULL,
    "NormalizedUserName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "NormalizedEmail" TEXT NOT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NULL,
    "DateOfBirth" TEXT NULL,
    "TwoFactorType" INTEGER NOT NULL,
    "LastPasswordChangedDate" TEXT NULL,
    "RequiresDefaultPasswordChange" INTEGER NULL,
    "LastLoginDateTime" TEXT NULL,
    "LastLogoutDateTime" TEXT NULL,
    "IdentityProviderType" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL
);

CREATE TABLE "HCL.CS.SF_ExternalIdentities" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ExternalIdentities" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "UserId" TEXT NOT NULL,
    "TenantId" TEXT NULL,
    "Provider" TEXT NOT NULL,
    "Issuer" TEXT NOT NULL,
    "Subject" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "EmailVerified" INTEGER NOT NULL,
    "LinkedAt" TEXT NOT NULL,
    "LastSignInAt" TEXT NULL,
    CONSTRAINT "FK_HCL.CS.SF_ExternalIdentities_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_ApiResourceClaims" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ApiResourceClaims" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "ApiResourceId" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_ApiResourceClaims_HCL.CS.SF_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "HCL.CS.SF_ApiResources" ("Id") ON DELETE CASCADE
);

CREATE TABLE "HCL.CS.SF_ApiScopes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ApiScopes" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "ApiResourceId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "DisplayName" TEXT NULL,
    "Description" TEXT NULL,
    "Required" INTEGER NOT NULL,
    "Emphasize" INTEGER NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_ApiScopes_HCL.CS.SF_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "HCL.CS.SF_ApiResources" ("Id") ON DELETE CASCADE
);

CREATE TABLE "HCL.CS.SF_ClientPostLogoutRedirectUris" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ClientPostLogoutRedirectUris" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "ClientId" TEXT NOT NULL,
    "PostLogoutRedirectUri" TEXT NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_ClientPostLogoutRedirectUris_HCL.CS.SF_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "HCL.CS.SF_Clients" ("Id") ON DELETE CASCADE
);

CREATE TABLE "HCL.CS.SF_ClientRedirectUris" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ClientRedirectUris" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "ClientId" TEXT NOT NULL,
    "RedirectUri" TEXT NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_ClientRedirectUris_HCL.CS.SF_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "HCL.CS.SF_Clients" ("Id") ON DELETE CASCADE
);

CREATE TABLE "HCL.CS.SF_IdentityClaims" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_IdentityClaims" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "IdentityResourceId" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "AliasType" TEXT NULL,
    CONSTRAINT "FK_HCL.CS.SF_IdentityClaims_HCL.CS.SF_IdentityResources_IdentityResourceId" FOREIGN KEY ("IdentityResourceId") REFERENCES "HCL.CS.SF_IdentityResources" ("Id") ON DELETE CASCADE
);

CREATE TABLE "HCL.CS.SF_RoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_HCL.CS.SF_RoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    CONSTRAINT "FK_HCL.CS.SF_RoleClaims_HCL.CS.SF_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "HCL.CS.SF_Roles" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_Notification" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_Notification" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "UserId" TEXT NOT NULL,
    "MessageId" TEXT NOT NULL,
    "Type" INTEGER NOT NULL,
    "Activity" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Sender" TEXT NOT NULL,
    "Recipient" TEXT NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_Notification_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_PasswordHistory" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_PasswordHistory" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "UserID" TEXT NOT NULL,
    "ChangedOn" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_PasswordHistory_HCL.CS.SF_Users_UserID" FOREIGN KEY ("UserID") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_UserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_HCL.CS.SF_UserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    "IsAdminClaim" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    CONSTRAINT "FK_HCL.CS.SF_UserClaims_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_UserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "Id" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    CONSTRAINT "PK_HCL.CS.SF_UserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey", "UserId"),
    CONSTRAINT "FK_HCL.CS.SF_UserLogins_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_UserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    "Id" TEXT NOT NULL,
    "ValidFrom" TEXT NULL,
    "ValidTo" TEXT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    CONSTRAINT "PK_HCL.CS.SF_UserRoles" PRIMARY KEY ("Id", "UserId", "RoleId"),
    CONSTRAINT "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "HCL.CS.SF_Roles" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_UserSecurityQuestions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_UserSecurityQuestions" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "UserId" TEXT NOT NULL,
    "SecurityQuestionId" TEXT NOT NULL,
    "Answer" TEXT NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_SecurityQuestions_SecurityQuestionId" FOREIGN KEY ("SecurityQuestionId") REFERENCES "HCL.CS.SF_SecurityQuestions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_UserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    CONSTRAINT "PK_HCL.CS.SF_UserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_HCL.CS.SF_UserTokens_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "HCL.CS.SF_ApiScopeClaims" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ApiScopeClaims" PRIMARY KEY,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedOn" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "RowVersion" BLOB NULL,
    "ApiScopeId" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    CONSTRAINT "FK_HCL.CS.SF_ApiScopeClaims_HCL.CS.SF_ApiScopes_ApiScopeId" FOREIGN KEY ("ApiScopeId") REFERENCES "HCL.CS.SF_ApiScopes" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_APIRES_CLM_RESID_TYPE" ON "HCL.CS.SF_ApiResourceClaims" ("ApiResourceId", "Type");

CREATE UNIQUE INDEX "IX_APIRES_NAME" ON "HCL.CS.SF_ApiResources" ("Name");

CREATE UNIQUE INDEX "IX_APISCO_CLM_SCOID_TYPE" ON "HCL.CS.SF_ApiScopeClaims" ("ApiScopeId", "Type");

CREATE UNIQUE INDEX "IX_APISCO_SCOID_NAME" ON "HCL.CS.SF_ApiScopes" ("ApiResourceId", "Name");

CREATE INDEX "IX_AUD_CBBY_ACTY" ON "HCL.CS.SF_AuditTrail" ("CreatedBy", "ActionType");

CREATE INDEX "IX_AUD_CRON_ACTY" ON "HCL.CS.SF_AuditTrail" ("CreatedOn", "ActionType");

CREATE INDEX "IX_AUD_CRON_CBBY" ON "HCL.CS.SF_AuditTrail" ("CreatedOn", "CreatedBy");

CREATE UNIQUE INDEX "IX_HCL.CS.SF_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri" ON "HCL.CS.SF_ClientPostLogoutRedirectUris" ("ClientId", "PostLogoutRedirectUri");

CREATE UNIQUE INDEX "IX_HCL.CS.SF_ClientRedirectUris_ClientId_RedirectUri" ON "HCL.CS.SF_ClientRedirectUris" ("ClientId", "RedirectUri");

CREATE UNIQUE INDEX "IX_CLI_CLID_CLSEC" ON "HCL.CS.SF_Clients" ("ClientId", "ClientSecret");

CREATE INDEX "IX_SECTOK_TOKTYPE_KEY" ON "HCL.CS.SF_SecurityTokens" ("TokenType", "Key");

CREATE UNIQUE INDEX "IX_IDRESCLM_IDRESID_TYPE" ON "HCL.CS.SF_IdentityClaims" ("IdentityResourceId", "Type");

CREATE UNIQUE INDEX "IX_IDRES_NAME" ON "HCL.CS.SF_IdentityResources" ("Name");

CREATE INDEX "IX_NOTI_TYPE" ON "HCL.CS.SF_Notification" ("Type");

CREATE INDEX "IX_HCL.CS.SF_Notification_UserId" ON "HCL.CS.SF_Notification" ("UserId");

CREATE INDEX "IX_HCL.CS.SF_PasswordHistory_UserID" ON "HCL.CS.SF_PasswordHistory" ("UserID");

CREATE INDEX "IX_HCL.CS.SF_RoleClaims_RoleId" ON "HCL.CS.SF_RoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "HCL.CS.SF_Roles" ("NormalizedName");

CREATE UNIQUE INDEX "IX_SEC_QUESTION" ON "HCL.CS.SF_SecurityQuestions" ("Question");

CREATE INDEX "IX_HCL.CS.SF_UserClaims_UserId" ON "HCL.CS.SF_UserClaims" ("UserId");

CREATE UNIQUE INDEX "IX_EXTID_PROVIDER_ISSUER_SUBJECT" ON "HCL.CS.SF_ExternalIdentities" ("Provider", "Issuer", "Subject");

CREATE INDEX "IX_EXTID_USERID" ON "HCL.CS.SF_ExternalIdentities" ("UserId");

CREATE INDEX "IX_EXTID_TENANT_EMAIL" ON "HCL.CS.SF_ExternalIdentities" ("TenantId", "Email");

CREATE INDEX "IX_HCL.CS.SF_UserLogins_UserId" ON "HCL.CS.SF_UserLogins" ("UserId");

CREATE INDEX "IX_HCL.CS.SF_UserRoles_RoleId" ON "HCL.CS.SF_UserRoles" ("RoleId");

CREATE INDEX "IX_HCL.CS.SF_UserRoles_UserId" ON "HCL.CS.SF_UserRoles" ("UserId");

CREATE INDEX "EmailIndex" ON "HCL.CS.SF_Users" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "HCL.CS.SF_Users" ("NormalizedUserName");

CREATE INDEX "IX_USRSEC_QUEID" ON "HCL.CS.SF_UserSecurityQuestions" ("SecurityQuestionId");

CREATE UNIQUE INDEX "IX_USRSEC_UID_QUEID" ON "HCL.CS.SF_UserSecurityQuestions" ("UserId", "SecurityQuestionId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220802110433_HCL.CS.SFSqliteV1', '3.1.27');
