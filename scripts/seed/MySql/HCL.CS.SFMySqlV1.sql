CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_ApiResources` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `DisplayName` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Enabled` tinyint(1) NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_ApiResources` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_AuditTrail` (
        `Id` char(36) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ActionType` int NOT NULL,
        `TableName` varchar(255) CHARACTER SET utf8mb4 NULL,
        `OldValue` longtext CHARACTER SET utf8mb4 NULL,
        `NewValue` longtext CHARACTER SET utf8mb4 NULL,
        `AffectedColumn` longtext CHARACTER SET utf8mb4 NULL,
        `ActionName` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_HCL.CS.SF_AuditTrail` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_Clients` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `ClientId` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
        `ClientName` varchar(255) CHARACTER SET utf8mb4 NULL,
        `ClientUri` longtext CHARACTER SET utf8mb4 NULL,
        `ClientIdIssuedAt` bigint NOT NULL,
        `ClientSecretExpiresAt` bigint NOT NULL,
        `ClientSecret` varchar(128) CHARACTER SET utf8mb4 NULL,
        `LogoUri` longtext CHARACTER SET utf8mb4 NULL,
        `TermsOfServiceUri` longtext CHARACTER SET utf8mb4 NULL,
        `PolicyUri` longtext CHARACTER SET utf8mb4 NULL,
        `RefreshTokenExpiration` int NOT NULL,
        `AccessTokenExpiration` int NOT NULL,
        `IdentityTokenExpiration` int NOT NULL,
        `LogoutTokenExpiration` int NOT NULL,
        `AuthorizationCodeExpiration` int NOT NULL,
        `AccessTokenType` int NOT NULL,
        `RequirePkce` tinyint(1) NOT NULL,
        `IsPkceTextPlain` tinyint(1) NOT NULL,
        `RequireClientSecret` tinyint(1) NOT NULL,
        `IsFirstPartyApp` tinyint(1) NOT NULL,
        `AllowOfflineAccess` tinyint(1) NOT NULL,
        `AllowedScopes` longtext CHARACTER SET utf8mb4 NULL,
        `AllowAccessTokensViaBrowser` tinyint(1) NOT NULL,
        `ApplicationType` int NOT NULL,
        `AllowedSigningAlgorithm` longtext CHARACTER SET utf8mb4 NULL,
        `SupportedGrantTypes` longtext CHARACTER SET utf8mb4 NULL,
        `SupportedResponseTypes` longtext CHARACTER SET utf8mb4 NULL,
        `FrontChannelLogoutSessionRequired` tinyint(1) NOT NULL,
        `FrontChannelLogoutUri` longtext CHARACTER SET utf8mb4 NULL,
        `BackChannelLogoutSessionRequired` tinyint(1) NOT NULL,
        `BackChannelLogoutUri` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_HCL.CS.SF_Clients` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_IdentityResources` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `DisplayName` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Enabled` tinyint(1) NOT NULL,
        `Required` tinyint(1) NOT NULL,
        `Emphasize` tinyint(1) NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_IdentityResources` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_Roles` (
        `Id` char(36) NOT NULL,
        `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `NormalizedName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ConcurrencyStamp` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_HCL.CS.SF_Roles` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_SecurityQuestions` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `Question` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_SecurityQuestions` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_SecurityTokens` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Key` longtext CHARACTER SET utf8mb4 NULL,
        `TokenType` longtext CHARACTER SET utf8mb4 NULL,
        `TokenValue` longtext CHARACTER SET utf8mb4 NULL,
        `ClientId` longtext CHARACTER SET utf8mb4 NULL,
        `SessionId` longtext CHARACTER SET utf8mb4 NULL,
        `SubjectId` longtext CHARACTER SET utf8mb4 NULL,
        `CreationTime` datetime(6) NOT NULL,
        `ExpiresAt` int NOT NULL,
        `ConsumedTime` datetime(6) NULL,
        `ConsumedAt` datetime(6) NULL,
        `TokenReuseDetected` tinyint(1) NOT NULL DEFAULT FALSE,
        CONSTRAINT `PK_HCL.CS.SF_SecurityTokens` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_Users` (
        `Id` char(36) NOT NULL,
        `UserName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `NormalizedUserName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `NormalizedEmail` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `EmailConfirmed` tinyint(1) NOT NULL,
        `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SecurityStamp` varchar(255) CHARACTER SET utf8mb4 NULL,
        `ConcurrencyStamp` varchar(255) CHARACTER SET utf8mb4 NULL,
        `PhoneNumber` varchar(15) CHARACTER SET utf8mb4 NULL,
        `PhoneNumberConfirmed` tinyint(1) NOT NULL,
        `TwoFactorEnabled` tinyint(1) NOT NULL,
        `LockoutEnd` datetime(6) NULL,
        `LockoutEnabled` tinyint(1) NOT NULL,
        `AccessFailedCount` int NOT NULL,
        `FirstName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `LastName` varchar(255) CHARACTER SET utf8mb4 NULL,
        `DateOfBirth` datetime(6) NULL,
        `TwoFactorType` int NOT NULL,
        `LastPasswordChangedDate` datetime(6) NULL,
        `RequiresDefaultPasswordChange` tinyint(1) NULL,
        `LastLoginDateTime` datetime(6) NULL,
        `LastLogoutDateTime` datetime(6) NULL,
        `IdentityProviderType` int NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_HCL.CS.SF_Users` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_ApiResourceClaims` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `ApiResourceId` char(36) NOT NULL,
        `Type` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_ApiResourceClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_ApiResourceClaims_HCL.CS.SF_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `HCL.CS.SF_ApiResources` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_ApiScopes` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `ApiResourceId` char(36) NOT NULL,
        `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `DisplayName` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Required` tinyint(1) NOT NULL,
        `Emphasize` tinyint(1) NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_ApiScopes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_ApiScopes_HCL.CS.SF_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `HCL.CS.SF_ApiResources` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_ClientPostLogoutRedirectUris` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `ClientId` char(36) NOT NULL,
        `PostLogoutRedirectUri` varchar(510) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_ClientPostLogoutRedirectUris` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_ClientPostLogoutRedirectUris_HCL.CS.SF_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `HCL.CS.SF_Clients` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_ClientRedirectUris` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `ClientId` char(36) NOT NULL,
        `RedirectUri` varchar(510) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_ClientRedirectUris` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_ClientRedirectUris_HCL.CS.SF_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `HCL.CS.SF_Clients` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_IdentityClaims` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `IdentityResourceId` char(36) NOT NULL,
        `Type` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `AliasType` varchar(255) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_HCL.CS.SF_IdentityClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_IdentityClaims_HCL.CS.SF_IdentityResources_IdentityResourceId` FOREIGN KEY (`IdentityResourceId`) REFERENCES `HCL.CS.SF_IdentityResources` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_RoleClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `RoleId` char(36) NOT NULL,
        `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
        `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        CONSTRAINT `PK_HCL.CS.SF_RoleClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_RoleClaims_HCL.CS.SF_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `HCL.CS.SF_Roles` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_Notification` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `UserId` char(36) NOT NULL,
        `MessageId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Type` int NOT NULL,
        `Activity` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Status` int NOT NULL,
        `Sender` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Recipient` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_Notification` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_Notification_HCL.CS.SF_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_PasswordHistory` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `UserID` char(36) NOT NULL,
        `ChangedOn` datetime(6) NOT NULL,
        `PasswordHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_PasswordHistory` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_PasswordHistory_HCL.CS.SF_Users_UserID` FOREIGN KEY (`UserID`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_UserClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` char(36) NOT NULL,
        `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
        `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
        `IsAdminClaim` tinyint(1) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        CONSTRAINT `PK_HCL.CS.SF_UserClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_UserClaims_HCL.CS.SF_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_UserLogins` (
        `LoginProvider` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `ProviderKey` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `UserId` char(36) NOT NULL,
        `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_HCL.CS.SF_UserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`, `UserId`),
        CONSTRAINT `FK_HCL.CS.SF_UserLogins_HCL.CS.SF_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_UserRoles` (
        `UserId` char(36) NOT NULL,
        `RoleId` char(36) NOT NULL,
        `Id` char(36) NOT NULL,
        `ValidFrom` datetime(6) NULL,
        `ValidTo` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        CONSTRAINT `PK_HCL.CS.SF_UserRoles` PRIMARY KEY (`Id`, `UserId`, `RoleId`),
        CONSTRAINT `FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `HCL.CS.SF_Roles` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_UserSecurityQuestions` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `UserId` char(36) NOT NULL,
        `SecurityQuestionId` char(36) NOT NULL,
        `Answer` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_UserSecurityQuestions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_SecurityQuestions_SecurityQuesti~` FOREIGN KEY (`SecurityQuestionId`) REFERENCES `HCL.CS.SF_SecurityQuestions` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_UserTokens` (
        `UserId` char(36) NOT NULL,
        `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Value` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_UserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
        CONSTRAINT `FK_HCL.CS.SF_UserTokens_HCL.CS.SF_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE TABLE `HCL.CS.SF_ApiScopeClaims` (
        `Id` char(36) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedOn` datetime(6) NOT NULL,
        `ModifiedOn` datetime(6) NULL,
        `CreatedBy` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ModifiedBy` varchar(255) CHARACTER SET utf8mb4 NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        `ApiScopeId` char(36) NOT NULL,
        `Type` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_HCL.CS.SF_ApiScopeClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HCL.CS.SF_ApiScopeClaims_HCL.CS.SF_ApiScopes_ApiScopeId` FOREIGN KEY (`ApiScopeId`) REFERENCES `HCL.CS.SF_ApiScopes` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_APIRES_CLM_RESID_TYPE` ON `HCL.CS.SF_ApiResourceClaims` (`ApiResourceId`, `Type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_APIRES_NAME` ON `HCL.CS.SF_ApiResources` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_APISCO_CLM_SCOID_TYPE` ON `HCL.CS.SF_ApiScopeClaims` (`ApiScopeId`, `Type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_APISCO_SCOID_NAME` ON `HCL.CS.SF_ApiScopes` (`ApiResourceId`, `Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_AUD_CBBY_ACTY` ON `HCL.CS.SF_AuditTrail` (`CreatedBy`, `ActionType`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_AUD_CRON_ACTY` ON `HCL.CS.SF_AuditTrail` (`CreatedOn`, `ActionType`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_AUD_CRON_CBBY` ON `HCL.CS.SF_AuditTrail` (`CreatedOn`, `CreatedBy`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_HCL.CS.SF_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectU~` ON `HCL.CS.SF_ClientPostLogoutRedirectUris` (`ClientId`, `PostLogoutRedirectUri`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_HCL.CS.SF_ClientRedirectUris_ClientId_RedirectUri` ON `HCL.CS.SF_ClientRedirectUris` (`ClientId`, `RedirectUri`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_SECTOK_TOKTYPE_KEY` ON `HCL.CS.SF_SecurityTokens` (`TokenType`(64), `Key`(255));

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_CLI_CLID_CLSEC` ON `HCL.CS.SF_Clients` (`ClientId`, `ClientSecret`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_IDRESCLM_IDRESID_TYPE` ON `HCL.CS.SF_IdentityClaims` (`IdentityResourceId`, `Type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_IDRES_NAME` ON `HCL.CS.SF_IdentityResources` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_NOTI_TYPE` ON `HCL.CS.SF_Notification` (`Type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_HCL.CS.SF_Notification_UserId` ON `HCL.CS.SF_Notification` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_HCL.CS.SF_PasswordHistory_UserID` ON `HCL.CS.SF_PasswordHistory` (`UserID`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_HCL.CS.SF_RoleClaims_RoleId` ON `HCL.CS.SF_RoleClaims` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `RoleNameIndex` ON `HCL.CS.SF_Roles` (`NormalizedName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_SEC_QUESTION` ON `HCL.CS.SF_SecurityQuestions` (`Question`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_HCL.CS.SF_UserClaims_UserId` ON `HCL.CS.SF_UserClaims` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_HCL.CS.SF_UserLogins_UserId` ON `HCL.CS.SF_UserLogins` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_HCL.CS.SF_UserRoles_RoleId` ON `HCL.CS.SF_UserRoles` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_HCL.CS.SF_UserRoles_UserId` ON `HCL.CS.SF_UserRoles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `EmailIndex` ON `HCL.CS.SF_Users` (`NormalizedEmail`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `UserNameIndex` ON `HCL.CS.SF_Users` (`NormalizedUserName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE INDEX `IX_USRSEC_QUEID` ON `HCL.CS.SF_UserSecurityQuestions` (`SecurityQuestionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    CREATE UNIQUE INDEX `IX_USRSEC_UID_QUEID` ON `HCL.CS.SF_UserSecurityQuestions` (`UserId`, `SecurityQuestionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20220722123951_HCL.CS.SFMySqlV1') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20220722123951_HCL.CS.SFMySqlV1', '3.1.27');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

