BEGIN IMMEDIATE;

-- Align the existing local admin client with the current HCL.CS.SF admin app.
UPDATE Zentra_Clients
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
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE ClientId = 'REDbgSB6J3jb3N9uolHJH74HzZDKM0wrcxZYf+Xve+I='
   OR AllowedScopes LIKE '%zentra.%';

UPDATE Zentra_ClientRedirectUris
SET RedirectUri = REPLACE(
        REPLACE(RedirectUri, 'https://admin.localhost:3001', 'https://localhost:3001'),
        '/api/auth/callback/zentra',
        '/api/auth/callback/HCL.CS.SF'
    ),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE RedirectUri LIKE '%admin.localhost%'
   OR RedirectUri LIKE '%/api/auth/callback/zentra';

UPDATE Zentra_ClientPostLogoutRedirectUris
SET PostLogoutRedirectUri = REPLACE(PostLogoutRedirectUri, 'https://admin.localhost:3001', 'https://localhost:3001'),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE PostLogoutRedirectUri LIKE '%admin.localhost%';

-- Bring persisted API resource and scope names in line with the HCL codebase.
UPDATE Zentra_ApiResources
SET Name = REPLACE(Name, 'zentra.', 'HCL.CS.SF.'),
    DisplayName = REPLACE(REPLACE(DisplayName, 'ZENTRA.', 'HCL.CS.SF.'), 'Zentra.', 'HCL.CS.SF.'),
    Description = REPLACE(Description, 'Zentra', 'HCL.CS.SF'),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE Name LIKE 'zentra.%'
   OR DisplayName LIKE 'ZENTRA.%'
   OR DisplayName LIKE 'Zentra.%'
   OR Description LIKE '%Zentra%';

UPDATE Zentra_ApiScopes
SET Name = REPLACE(Name, 'zentra.', 'HCL.CS.SF.'),
    DisplayName = REPLACE(REPLACE(DisplayName, 'ZENTRA.', 'HCL.CS.SF.'), 'Zentra.', 'HCL.CS.SF.'),
    Description = REPLACE(Description, 'Zentra', 'HCL.CS.SF'),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE Name LIKE 'zentra.%'
   OR DisplayName LIKE 'ZENTRA.%'
   OR DisplayName LIKE 'Zentra.%'
   OR Description LIKE '%Zentra%';

-- Normalize persisted role permissions so admin authorization matches HCL constants.
UPDATE Zentra_RoleClaims
SET ClaimValue = REPLACE(
        REPLACE(ClaimValue, 'zentra.apiResource', 'HCL.CS.SF.apiresource'),
        'zentra.',
        'HCL.CS.SF.'
    ),
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE ClaimValue LIKE 'zentra.%';

-- Update built-in role names to the current HCL naming.
UPDATE Zentra_Roles
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

-- Ensure persisted provider sender metadata is HCL-branded too.
UPDATE Zentra_NotificationProviderConfig
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
