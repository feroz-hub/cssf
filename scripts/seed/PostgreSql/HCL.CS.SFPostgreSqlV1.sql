CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);


DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_ApiResources" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "Name" character varying(255) NOT NULL,
        "DisplayName" character varying(255) NULL,
        "Description" text NULL,
        "Enabled" boolean NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_ApiResources" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_AuditTrail" (
        "Id" uuid NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ActionType" integer NOT NULL,
        "TableName" character varying(255) NULL,
        "OldValue" text NULL,
        "NewValue" text NULL,
        "AffectedColumn" text NULL,
        "ActionName" text NULL,
        CONSTRAINT "PK_HCL.CS.SF_AuditTrail" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_Clients" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "ClientId" character varying(128) NOT NULL,
        "ClientName" character varying(255) NULL,
        "ClientUri" text NULL,
        "ClientIdIssuedAt" bigint NOT NULL,
        "ClientSecretExpiresAt" bigint NOT NULL,
        "ClientSecret" character varying(128) NULL,
        "LogoUri" text NULL,
        "TermsOfServiceUri" text NULL,
        "PolicyUri" text NULL,
        "RefreshTokenExpiration" integer NOT NULL,
        "AccessTokenExpiration" integer NOT NULL,
        "IdentityTokenExpiration" integer NOT NULL,
        "LogoutTokenExpiration" integer NOT NULL,
        "AuthorizationCodeExpiration" integer NOT NULL,
        "AccessTokenType" integer NOT NULL,
        "RequirePkce" boolean NOT NULL,
        "IsPkceTextPlain" boolean NOT NULL,
        "RequireClientSecret" boolean NOT NULL,
        "IsFirstPartyApp" boolean NOT NULL,
        "AllowOfflineAccess" boolean NOT NULL,
        "AllowedScopes" text NULL,
        "AllowAccessTokensViaBrowser" boolean NOT NULL,
        "ApplicationType" integer NOT NULL,
        "AllowedSigningAlgorithm" text NULL,
        "SupportedGrantTypes" text NULL,
        "SupportedResponseTypes" text NULL,
        "FrontChannelLogoutSessionRequired" boolean NOT NULL,
        "FrontChannelLogoutUri" text NULL,
        "BackChannelLogoutSessionRequired" boolean NOT NULL,
        "BackChannelLogoutUri" text NULL,
        CONSTRAINT "PK_HCL.CS.SF_Clients" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_IdentityResources" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "Name" character varying(255) NOT NULL,
        "DisplayName" character varying(255) NULL,
        "Description" text NULL,
        "Enabled" boolean NOT NULL,
        "Required" boolean NOT NULL,
        "Emphasize" boolean NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_IdentityResources" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_Roles" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "NormalizedName" character varying(255) NOT NULL,
        "ConcurrencyStamp" character varying(255) NOT NULL,
        "Description" text NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        CONSTRAINT "PK_HCL.CS.SF_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_SecurityQuestions" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "Question" character varying(255) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_SecurityQuestions" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_SecurityTokens" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp with time zone NOT NULL,
        "ModifiedOn" timestamp with time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "Key" text NULL,
        "TokenType" text NULL,
        "TokenValue" text NULL,
        "ClientId" text NULL,
        "SessionId" text NULL,
        "SubjectId" text NULL,
        "CreationTime" timestamp with time zone NOT NULL,
        "ExpiresAt" integer NOT NULL,
        "ConsumedTime" timestamp with time zone NULL,
        "ConsumedAt" timestamp with time zone NULL,
        "TokenReuseDetected" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_HCL.CS.SF_SecurityTokens" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_Users" (
        "Id" uuid NOT NULL,
        "UserName" character varying(255) NOT NULL,
        "NormalizedUserName" character varying(255) NOT NULL,
        "Email" character varying(255) NOT NULL,
        "NormalizedEmail" character varying(255) NOT NULL,
        "EmailConfirmed" boolean NOT NULL,
        "PasswordHash" text NOT NULL,
        "SecurityStamp" character varying(255) NULL,
        "ConcurrencyStamp" character varying(255) NULL,
        "PhoneNumber" character varying(15) NULL,
        "PhoneNumberConfirmed" boolean NOT NULL,
        "TwoFactorEnabled" boolean NOT NULL,
        "LockoutEnd" timestamp with time zone NULL,
        "LockoutEnabled" boolean NOT NULL,
        "AccessFailedCount" integer NOT NULL,
        "FirstName" character varying(255) NOT NULL,
        "LastName" character varying(255) NULL,
        "DateOfBirth" timestamp with time zone NULL,
        "TwoFactorType" integer NOT NULL,
        "LastPasswordChangedDate" timestamp with time zone NULL,
        "RequiresDefaultPasswordChange" boolean NULL,
        "LastLoginDateTime" timestamp with time zone NULL,
        "LastLogoutDateTime" timestamp with time zone NULL,
        "IdentityProviderType" integer NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp with time zone NOT NULL,
        "ModifiedOn" timestamp with time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        CONSTRAINT "PK_HCL.CS.SF_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_ApiResourceClaims" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "ApiResourceId" uuid NOT NULL,
        "Type" character varying(255) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_ApiResourceClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_ApiResourceClaims_HCL.CS.SF_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "HCL.CS.SF_ApiResources" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_ApiScopes" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "ApiResourceId" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "DisplayName" character varying(255) NULL,
        "Description" text NULL,
        "Required" boolean NOT NULL,
        "Emphasize" boolean NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_ApiScopes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_ApiScopes_HCL.CS.SF_ApiResources_ApiResourceId" FOREIGN KEY ("ApiResourceId") REFERENCES "HCL.CS.SF_ApiResources" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_ClientPostLogoutRedirectUris" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "ClientId" uuid NOT NULL,
        "PostLogoutRedirectUri" character varying(510) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_ClientPostLogoutRedirectUris" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_ClientPostLogoutRedirectUris_HCL.CS.SF_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "HCL.CS.SF_Clients" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_ClientRedirectUris" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "ClientId" uuid NOT NULL,
        "RedirectUri" character varying(510) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_ClientRedirectUris" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_ClientRedirectUris_HCL.CS.SF_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "HCL.CS.SF_Clients" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_IdentityClaims" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "IdentityResourceId" uuid NOT NULL,
        "Type" character varying(255) NOT NULL,
        "AliasType" character varying(255) NULL,
        CONSTRAINT "PK_HCL.CS.SF_IdentityClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_IdentityClaims_HCL.CS.SF_IdentityResources_IdentityResourceId" FOREIGN KEY ("IdentityResourceId") REFERENCES "HCL.CS.SF_IdentityResources" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_RoleClaims" (
        "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
        "RoleId" uuid NOT NULL,
        "ClaimType" text NULL,
        "ClaimValue" text NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        CONSTRAINT "PK_HCL.CS.SF_RoleClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_RoleClaims_HCL.CS.SF_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "HCL.CS.SF_Roles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_Notification" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "UserId" uuid NOT NULL,
        "MessageId" character varying(255) NOT NULL,
        "Type" integer NOT NULL,
        "Activity" character varying(255) NULL,
        "Status" integer NOT NULL,
        "Sender" character varying(255) NOT NULL,
        "Recipient" character varying(255) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_Notification" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_Notification_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_PasswordHistory" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "UserID" uuid NOT NULL,
        "ChangedOn" timestamp without time zone NOT NULL,
        "PasswordHash" character varying(255) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_PasswordHistory" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_PasswordHistory_HCL.CS.SF_Users_UserID" FOREIGN KEY ("UserID") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_UserClaims" (
        "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
        "UserId" uuid NOT NULL,
        "ClaimType" text NULL,
        "ClaimValue" text NULL,
        "IsAdminClaim" boolean NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        CONSTRAINT "PK_HCL.CS.SF_UserClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_UserClaims_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_UserLogins" (
        "LoginProvider" character varying(256) NOT NULL,
        "ProviderKey" character varying(256) NOT NULL,
        "UserId" uuid NOT NULL,
        "ProviderDisplayName" text NULL,
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        CONSTRAINT "PK_HCL.CS.SF_UserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey", "UserId"),
        CONSTRAINT "FK_HCL.CS.SF_UserLogins_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_UserRoles" (
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "Id" uuid NOT NULL,
        "ValidFrom" timestamp without time zone NULL,
        "ValidTo" timestamp without time zone NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        CONSTRAINT "PK_HCL.CS.SF_UserRoles" PRIMARY KEY ("Id", "UserId", "RoleId"),
        CONSTRAINT "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "HCL.CS.SF_Roles" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_UserSecurityQuestions" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "UserId" uuid NOT NULL,
        "SecurityQuestionId" uuid NOT NULL,
        "Answer" character varying(255) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_UserSecurityQuestions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_SecurityQuestions_SecurityQuest~" FOREIGN KEY ("SecurityQuestionId") REFERENCES "HCL.CS.SF_SecurityQuestions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_UserTokens" (
        "UserId" uuid NOT NULL,
        "LoginProvider" character varying(255) NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Value" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_UserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
        CONSTRAINT "FK_HCL.CS.SF_UserTokens_HCL.CS.SF_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE TABLE "HCL.CS.SF_ApiScopeClaims" (
        "Id" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "ModifiedOn" timestamp without time zone NULL,
        "CreatedBy" character varying(255) NOT NULL,
        "ModifiedBy" character varying(255) NULL,
        "ApiScopeId" uuid NOT NULL,
        "Type" character varying(255) NOT NULL,
        CONSTRAINT "PK_HCL.CS.SF_ApiScopeClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HCL.CS.SF_ApiScopeClaims_HCL.CS.SF_ApiScopes_ApiScopeId" FOREIGN KEY ("ApiScopeId") REFERENCES "HCL.CS.SF_ApiScopes" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_APIRES_CLM_RESID_TYPE" ON "HCL.CS.SF_ApiResourceClaims" ("ApiResourceId", "Type");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_APIRES_NAME" ON "HCL.CS.SF_ApiResources" ("Name");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_APISCO_CLM_SCOID_TYPE" ON "HCL.CS.SF_ApiScopeClaims" ("ApiScopeId", "Type");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_APISCO_SCOID_NAME" ON "HCL.CS.SF_ApiScopes" ("ApiResourceId", "Name");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_AUD_CBBY_ACTY" ON "HCL.CS.SF_AuditTrail" ("CreatedBy", "ActionType");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_AUD_CRON_ACTY" ON "HCL.CS.SF_AuditTrail" ("CreatedOn", "ActionType");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_AUD_CRON_CBBY" ON "HCL.CS.SF_AuditTrail" ("CreatedOn", "CreatedBy");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_HCL.CS.SF_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirect~" ON "HCL.CS.SF_ClientPostLogoutRedirectUris" ("ClientId", "PostLogoutRedirectUri");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_HCL.CS.SF_ClientRedirectUris_ClientId_RedirectUri" ON "HCL.CS.SF_ClientRedirectUris" ("ClientId", "RedirectUri");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_SECTOK_TOKTYPE_KEY" ON "HCL.CS.SF_SecurityTokens" ("TokenType", "Key");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_CLI_CLID_CLSEC" ON "HCL.CS.SF_Clients" ("ClientId", "ClientSecret");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_IDRESCLM_IDRESID_TYPE" ON "HCL.CS.SF_IdentityClaims" ("IdentityResourceId", "Type");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_IDRES_NAME" ON "HCL.CS.SF_IdentityResources" ("Name");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_NOTI_TYPE" ON "HCL.CS.SF_Notification" ("Type");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_HCL.CS.SF_Notification_UserId" ON "HCL.CS.SF_Notification" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_HCL.CS.SF_PasswordHistory_UserID" ON "HCL.CS.SF_PasswordHistory" ("UserID");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_HCL.CS.SF_RoleClaims_RoleId" ON "HCL.CS.SF_RoleClaims" ("RoleId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "RoleNameIndex" ON "HCL.CS.SF_Roles" ("NormalizedName");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_SEC_QUESTION" ON "HCL.CS.SF_SecurityQuestions" ("Question");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_HCL.CS.SF_UserClaims_UserId" ON "HCL.CS.SF_UserClaims" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_HCL.CS.SF_UserLogins_UserId" ON "HCL.CS.SF_UserLogins" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_HCL.CS.SF_UserRoles_RoleId" ON "HCL.CS.SF_UserRoles" ("RoleId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_HCL.CS.SF_UserRoles_UserId" ON "HCL.CS.SF_UserRoles" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "EmailIndex" ON "HCL.CS.SF_Users" ("NormalizedEmail");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "UserNameIndex" ON "HCL.CS.SF_Users" ("NormalizedUserName");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE INDEX "IX_USRSEC_QUEID" ON "HCL.CS.SF_UserSecurityQuestions" ("SecurityQuestionId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    CREATE UNIQUE INDEX "IX_USRSEC_UID_QUEID" ON "HCL.CS.SF_UserSecurityQuestions" ("UserId", "SecurityQuestionId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220726113011_HCL.CS.SFPostgreSqlV1') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220726113011_HCL.CS.SFPostgreSqlV1', '3.1.27');
    END IF;
END $$;
