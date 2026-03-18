PRAGMA foreign_keys = OFF;

BEGIN IMMEDIATE;

-- Rename the legacy SQLite schema so the current HCL.CS.SF EF mappings can resolve tables.
ALTER TABLE "Zentra_ApiResourceClaims" RENAME TO "HCL.CS.SF_ApiResourceClaims";
ALTER TABLE "Zentra_ApiResources" RENAME TO "HCL.CS.SF_ApiResources";
ALTER TABLE "Zentra_ApiScopeClaims" RENAME TO "HCL.CS.SF_ApiScopeClaims";
ALTER TABLE "Zentra_ApiScopes" RENAME TO "HCL.CS.SF_ApiScopes";
ALTER TABLE "Zentra_AuditTrail" RENAME TO "HCL.CS.SF_AuditTrail";
ALTER TABLE "Zentra_ClientPostLogoutRedirectUris" RENAME TO "HCL.CS.SF_ClientPostLogoutRedirectUris";
ALTER TABLE "Zentra_ClientRedirectUris" RENAME TO "HCL.CS.SF_ClientRedirectUris";
ALTER TABLE "Zentra_Clients" RENAME TO "HCL.CS.SF_Clients";
ALTER TABLE "Zentra_ExternalAuthProviderConfig" RENAME TO "HCL.CS.SF_ExternalAuthProviderConfig";
ALTER TABLE "Zentra_ExternalIdentities" RENAME TO "HCL.CS.SF_ExternalIdentities";
ALTER TABLE "Zentra_IdentityClaims" RENAME TO "HCL.CS.SF_IdentityClaims";
ALTER TABLE "Zentra_IdentityResources" RENAME TO "HCL.CS.SF_IdentityResources";
ALTER TABLE "Zentra_MigrationHistory" RENAME TO "HCL.CS.SF_MigrationHistory";
ALTER TABLE "Zentra_Notification" RENAME TO "HCL.CS.SF_Notification";
ALTER TABLE "Zentra_NotificationProviderConfig" RENAME TO "HCL.CS.SF_NotificationProviderConfig";
ALTER TABLE "Zentra_PasswordHistory" RENAME TO "HCL.CS.SF_PasswordHistory";
ALTER TABLE "Zentra_RoleClaims" RENAME TO "HCL.CS.SF_RoleClaims";
ALTER TABLE "Zentra_Roles" RENAME TO "HCL.CS.SF_Roles";
ALTER TABLE "Zentra_SecurityQuestions" RENAME TO "HCL.CS.SF_SecurityQuestions";
ALTER TABLE "Zentra_SecurityTokens" RENAME TO "HCL.CS.SF_SecurityTokens";
ALTER TABLE "Zentra_UserClaims" RENAME TO "HCL.CS.SF_UserClaims";
ALTER TABLE "Zentra_UserLogins" RENAME TO "HCL.CS.SF_UserLogins";
ALTER TABLE "Zentra_UserRoles" RENAME TO "HCL.CS.SF_UserRoles";
ALTER TABLE "Zentra_UserSecurityQuestions" RENAME TO "HCL.CS.SF_UserSecurityQuestions";
ALTER TABLE "Zentra_UserTokens" RENAME TO "HCL.CS.SF_UserTokens";
ALTER TABLE "Zentra_Users" RENAME TO "HCL.CS.SF_Users";

-- Align the existing local admin client with the current HCL.CS.SF admin app.
UPDATE "HCL.CS.SF_Clients"
SET ClientName = CASE
        WHEN ClientId = 'REDbgSB6J3jb3N9uolHJH74HzZDKM0wrcxZYf+Xve+I=' THEN 'HCL.CS.SF Admin'
        ELSE ClientName
    END,
    ClientUri = CASE
        WHEN ClientId = 'REDbgSB6J3jb3N9uolHJH74HzZDKM0wrcxZYf+Xve+I='
             AND ClientUri = 'https://admin.localhost:3001' THEN 'https://localhost:3001'
        ELSE ClientUri
    END,
    AllowedScopes = REPLACE(AllowedScopes, 'zentra.', 'HCL.CS.SF.'),
    PreferredAudience = CASE
        WHEN ClientId = 'REDbgSB6J3jb3N9uolHJH74HzZDKM0wrcxZYf+Xve+I='
             AND (PreferredAudience IS NULL OR PreferredAudience = '' OR PreferredAudience = 'zentra.api')
            THEN 'HCL.CS.SF.api'
        ELSE REPLACE(COALESCE(PreferredAudience, ''), 'zentra.api', 'HCL.CS.SF.api')
    END,
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE ClientId = 'REDbgSB6J3jb3N9uolHJH74HzZDKM0wrcxZYf+Xve+I='
   OR AllowedScopes LIKE '%zentra.%'
   OR COALESCE(PreferredAudience, '') LIKE '%zentra.api%';

UPDATE "HCL.CS.SF_ClientRedirectUris"
SET RedirectUri = REPLACE(
        REPLACE(RedirectUri, 'https://admin.localhost:3001', 'https://localhost:3001'),
        '/api/auth/callback/zentra',
        '/api/auth/callback/HCL.CS.SF'
    ),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE RedirectUri LIKE '%admin.localhost%'
   OR RedirectUri LIKE '%/api/auth/callback/zentra';

UPDATE "HCL.CS.SF_ClientPostLogoutRedirectUris"
SET PostLogoutRedirectUri = REPLACE(PostLogoutRedirectUri, 'https://admin.localhost:3001', 'https://localhost:3001'),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE PostLogoutRedirectUri LIKE '%admin.localhost%';

-- Bring persisted API resource and scope names in line with the HCL codebase.
UPDATE "HCL.CS.SF_ApiResources"
SET Name = REPLACE(Name, 'zentra.', 'HCL.CS.SF.'),
    DisplayName = REPLACE(REPLACE(DisplayName, 'ZENTRA.', 'HCL.CS.SF.'), 'Zentra.', 'HCL.CS.SF.'),
    Description = REPLACE(Description, 'Zentra', 'HCL.CS.SF'),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE Name LIKE 'zentra.%'
   OR DisplayName LIKE 'ZENTRA.%'
   OR DisplayName LIKE 'Zentra.%'
   OR Description LIKE '%Zentra%';

UPDATE "HCL.CS.SF_ApiScopes"
SET Name = REPLACE(Name, 'zentra.', 'HCL.CS.SF.'),
    DisplayName = REPLACE(REPLACE(DisplayName, 'ZENTRA.', 'HCL.CS.SF.'), 'Zentra.', 'HCL.CS.SF.'),
    Description = REPLACE(Description, 'Zentra', 'HCL.CS.SF'),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE Name LIKE 'zentra.%'
   OR DisplayName LIKE 'ZENTRA.%'
   OR DisplayName LIKE 'Zentra.%'
   OR Description LIKE '%Zentra%';

UPDATE "HCL.CS.SF_RoleClaims"
SET ClaimValue = REPLACE(
        REPLACE(ClaimValue, 'zentra.apiResource', 'HCL.CS.SF.apiresource'),
        'zentra.',
        'HCL.CS.SF.'
    ),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE ClaimValue LIKE 'zentra.%';

UPDATE "HCL.CS.SF_Roles"
SET Name = CASE
        WHEN Name = 'ZentraAdmin' THEN 'HCLCSSFAdmin'
        WHEN Name = 'ZentraUser' THEN 'HCLCSSFUser'
        ELSE Name
    END,
    NormalizedName = CASE
        WHEN Name = 'ZentraAdmin' THEN 'HCLCSSFADMIN'
        WHEN Name = 'ZentraUser' THEN 'HCLCSSFUSER'
        ELSE NormalizedName
    END,
    Description = CASE
        WHEN Description = 'ZentraAdmin' THEN 'HCL.CS.SF Admin'
        WHEN Description = 'ZentraUser' THEN 'HCL.CS.SF User'
        ELSE REPLACE(Description, 'Zentra', 'HCL.CS.SF')
    END,
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE Name IN ('ZentraAdmin', 'ZentraUser')
   OR Description IN ('ZentraAdmin', 'ZentraUser')
   OR Description LIKE '%Zentra%';

-- Clean the remaining runtime user branding so local data is HCL-only.
UPDATE "HCL.CS.SF_Users"
SET UserName = 'HCLCSSFAdmin',
    NormalizedUserName = 'HCLCSSFADMIN',
    FirstName = 'HCL',
    LastName = 'Admin',
    Email = 'admin@HCL.CS.SF.com',
    NormalizedEmail = 'ADMIN@HCL.CS.SF.COM',
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE UserName = 'ZentraAdmin';

UPDATE "HCL.CS.SF_Users"
SET FirstName = 'HCL',
    LastName = 'Admin',
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE UserName = 'HCLCSSFAdmin'
  AND (FirstName LIKE '%Zentra%' OR LastName LIKE '%Zentra%');

UPDATE "HCL.CS.SF_Users"
SET Email = REPLACE(Email, 'futurebeyondtech.com', 'HCL.CS.SF.com'),
    NormalizedEmail = UPPER(REPLACE(Email, 'futurebeyondtech.com', 'HCL.CS.SF.com')),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE Email LIKE '%futurebeyondtech.com';

UPDATE "HCL.CS.SF_NotificationProviderConfig"
SET ConfigJson = REPLACE(
        REPLACE(
            REPLACE(ConfigJson, 'zentra@futurebeyondtech.com', 'no-reply@HCL.CS.SF.com'),
            'futurebeyondtech.com',
            'HCL.CS.SF.com'
        ),
        'Zentra',
        'HCL.CS.SF'
    ),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE ConfigJson LIKE '%futurebeyondtech%'
   OR ConfigJson LIKE '%zentra%';

COMMIT;

PRAGMA foreign_keys = ON;
