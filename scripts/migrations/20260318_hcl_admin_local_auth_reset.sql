BEGIN IMMEDIATE;

UPDATE "HCL.CS.SF_Clients"
SET AllowedScopes = 'openid profile email offline_access phone HCL.CS.SF.apiresource.manage HCL.CS.SF.client.manage HCL.CS.SF.user.read HCL.CS.SF.user.write HCL.CS.SF.role.manage HCL.CS.SF.identityresource.manage HCL.CS.SF.adminuser.manage HCL.CS.SF.securitytoken.manage HCL.CS.SF.notification.read HCL.CS.SF.notification.manage',
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE ClientId = 'REDbgSB6J3jb3N9uolHJH74HzZDKM0wrcxZYf+Xve+I=';

UPDATE "HCL.CS.SF_Users"
SET PasswordHash = '$argon2i$v=19$m=32768,t=10,p=5$hSqlKREs+ocJYJYV7Brv5A$2zeyg8EeBdt986mjhKToD9NSjSM',
    FirstName = 'HCL',
    LastName = 'Admin',
    AccessFailedCount = 0,
    LockoutEnd = NULL,
    LockoutEnabled = 1,
    RequiresDefaultPasswordChange = 0,
    LastPasswordChangedDate = CURRENT_TIMESTAMP,
    ModifiedOn = CURRENT_TIMESTAMP,
    ModifiedBy = 'HCLCSSFAdmin'
WHERE UserName = 'HCLCSSFAdmin';

DELETE FROM "HCL.CS.SF_SecurityTokens";

COMMIT;
