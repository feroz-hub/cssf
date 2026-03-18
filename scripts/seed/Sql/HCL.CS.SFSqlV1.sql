IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_ApiResources] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [Name] nvarchar(255) NOT NULL,
        [DisplayName] nvarchar(255) NULL,
        [Description] nvarchar(max) NULL,
        [Enabled] bit NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_ApiResources] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_AuditTrail] (
        [Id] uniqueidentifier NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ActionType] int NOT NULL,
        [TableName] nvarchar(255) NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [AffectedColumn] nvarchar(max) NULL,
        [ActionName] nvarchar(max) NULL,
        CONSTRAINT [PK_HCL.CS.SF_AuditTrail] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_Clients] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [ClientId] nvarchar(128) NOT NULL,
        [ClientName] nvarchar(255) NULL,
        [ClientUri] nvarchar(max) NULL,
        [ClientIdIssuedAt] bigint NOT NULL,
        [ClientSecretExpiresAt] bigint NOT NULL,
        [ClientSecret] nvarchar(128) NULL,
        [LogoUri] nvarchar(max) NULL,
        [TermsOfServiceUri] nvarchar(max) NULL,
        [PolicyUri] nvarchar(max) NULL,
        [RefreshTokenExpiration] int NOT NULL,
        [AccessTokenExpiration] int NOT NULL,
        [IdentityTokenExpiration] int NOT NULL,
        [LogoutTokenExpiration] int NOT NULL,
        [AuthorizationCodeExpiration] int NOT NULL,
        [AccessTokenType] int NOT NULL,
        [RequirePkce] bit NOT NULL,
        [IsPkceTextPlain] bit NOT NULL,
        [RequireClientSecret] bit NOT NULL,
        [IsFirstPartyApp] bit NOT NULL,
        [AllowOfflineAccess] bit NOT NULL,
        [AllowedScopes] nvarchar(max) NULL,
        [AllowAccessTokensViaBrowser] bit NOT NULL,
        [ApplicationType] int NOT NULL,
        [AllowedSigningAlgorithm] nvarchar(max) NULL,
        [SupportedGrantTypes] nvarchar(max) NULL,
        [SupportedResponseTypes] nvarchar(max) NULL,
        [FrontChannelLogoutSessionRequired] bit NOT NULL,
        [FrontChannelLogoutUri] nvarchar(max) NULL,
        [BackChannelLogoutSessionRequired] bit NOT NULL,
        [BackChannelLogoutUri] nvarchar(max) NULL,
        CONSTRAINT [PK_HCL.CS.SF_Clients] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_IdentityResources] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [Name] nvarchar(255) NOT NULL,
        [DisplayName] nvarchar(255) NULL,
        [Description] nvarchar(max) NULL,
        [Enabled] bit NOT NULL,
        [Required] bit NOT NULL,
        [Emphasize] bit NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_IdentityResources] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [NormalizedName] nvarchar(255) NOT NULL,
        [ConcurrencyStamp] nvarchar(255) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        CONSTRAINT [PK_HCL.CS.SF_Roles] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_SecurityQuestions] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [Question] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_SecurityQuestions] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_SecurityTokens] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [Key] nvarchar(max) NULL,
        [TokenType] nvarchar(max) NULL,
        [TokenValue] nvarchar(max) NULL,
        [ClientId] nvarchar(max) NULL,
        [SessionId] nvarchar(max) NULL,
        [SubjectId] nvarchar(max) NULL,
        [CreationTime] datetime2 NOT NULL,
        [ExpiresAt] int NOT NULL,
        [ConsumedTime] datetime2 NULL,
        [ConsumedAt] datetime2 NULL,
        [TokenReuseDetected] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_HCL.CS.SF_SecurityTokens] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_Users] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(255) NOT NULL,
        [NormalizedUserName] nvarchar(255) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [NormalizedEmail] nvarchar(255) NOT NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [SecurityStamp] nvarchar(255) NULL,
        [ConcurrencyStamp] nvarchar(255) NULL,
        [PhoneNumber] nvarchar(15) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [FirstName] nvarchar(255) NOT NULL,
        [LastName] nvarchar(255) NULL,
        [DateOfBirth] datetime2 NULL,
        [TwoFactorType] int NOT NULL,
        [LastPasswordChangedDate] datetime2 NULL,
        [RequiresDefaultPasswordChange] bit NULL,
        [LastLoginDateTime] datetime2 NULL,
        [LastLogoutDateTime] datetime2 NULL,
        [IdentityProviderType] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        CONSTRAINT [PK_HCL.CS.SF_Users] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_ApiResourceClaims] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [ApiResourceId] uniqueidentifier NOT NULL,
        [Type] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_ApiResourceClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_ApiResourceClaims_HCL.CS.SF_ApiResources_ApiResourceId] FOREIGN KEY ([ApiResourceId]) REFERENCES [HCL.CS.SF_ApiResources] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_ApiScopes] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [ApiResourceId] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [DisplayName] nvarchar(255) NULL,
        [Description] nvarchar(max) NULL,
        [Required] bit NOT NULL,
        [Emphasize] bit NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_ApiScopes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_ApiScopes_HCL.CS.SF_ApiResources_ApiResourceId] FOREIGN KEY ([ApiResourceId]) REFERENCES [HCL.CS.SF_ApiResources] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_ClientPostLogoutRedirectUris] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [ClientId] uniqueidentifier NOT NULL,
        [PostLogoutRedirectUri] nvarchar(510) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_ClientPostLogoutRedirectUris] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_ClientPostLogoutRedirectUris_HCL.CS.SF_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [HCL.CS.SF_Clients] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_ClientRedirectUris] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [ClientId] uniqueidentifier NOT NULL,
        [RedirectUri] nvarchar(510) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_ClientRedirectUris] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_ClientRedirectUris_HCL.CS.SF_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [HCL.CS.SF_Clients] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_IdentityClaims] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [IdentityResourceId] uniqueidentifier NOT NULL,
        [Type] nvarchar(255) NOT NULL,
        [AliasType] nvarchar(255) NULL,
        CONSTRAINT [PK_HCL.CS.SF_IdentityClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_IdentityClaims_HCL.CS.SF_IdentityResources_IdentityResourceId] FOREIGN KEY ([IdentityResourceId]) REFERENCES [HCL.CS.SF_IdentityResources] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_RoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_HCL.CS.SF_RoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_RoleClaims_HCL.CS.SF_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [HCL.CS.SF_Roles] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_Notification] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [UserId] uniqueidentifier NOT NULL,
        [MessageId] nvarchar(255) NOT NULL,
        [Type] int NOT NULL,
        [Activity] nvarchar(255) NULL,
        [Status] int NOT NULL,
        [Sender] nvarchar(255) NOT NULL,
        [Recipient] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_Notification] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_Notification_HCL.CS.SF_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_PasswordHistory] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [UserID] uniqueidentifier NOT NULL,
        [ChangedOn] datetime2 NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_PasswordHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_PasswordHistory_HCL.CS.SF_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_UserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        [IsAdminClaim] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_HCL.CS.SF_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_UserClaims_HCL.CS.SF_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_UserLogins] (
        [LoginProvider] nvarchar(256) NOT NULL,
        [ProviderKey] nvarchar(256) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        CONSTRAINT [PK_HCL.CS.SF_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey], [UserId]),
        CONSTRAINT [FK_HCL.CS.SF_UserLogins_HCL.CS.SF_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_UserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [Id] uniqueidentifier NOT NULL,
        [ValidFrom] datetime2 NULL,
        [ValidTo] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_HCL.CS.SF_UserRoles] PRIMARY KEY ([Id], [UserId], [RoleId]),
        CONSTRAINT [FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [HCL.CS.SF_Roles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_UserSecurityQuestions] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [UserId] uniqueidentifier NOT NULL,
        [SecurityQuestionId] uniqueidentifier NOT NULL,
        [Answer] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_UserSecurityQuestions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_SecurityQuestions_SecurityQuestionId] FOREIGN KEY ([SecurityQuestionId]) REFERENCES [HCL.CS.SF_SecurityQuestions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_UserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(255) NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_HCL.CS.SF_UserTokens_HCL.CS.SF_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE TABLE [HCL.CS.SF_ApiScopeClaims] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion NULL,
        [ApiScopeId] uniqueidentifier NOT NULL,
        [Type] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_HCL.CS.SF_ApiScopeClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HCL.CS.SF_ApiScopeClaims_HCL.CS.SF_ApiScopes_ApiScopeId] FOREIGN KEY ([ApiScopeId]) REFERENCES [HCL.CS.SF_ApiScopes] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_APIRES_CLM_RESID_TYPE] ON [HCL.CS.SF_ApiResourceClaims] ([ApiResourceId], [Type]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_APIRES_NAME] ON [HCL.CS.SF_ApiResources] ([Name]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_APISCO_CLM_SCOID_TYPE] ON [HCL.CS.SF_ApiScopeClaims] ([ApiScopeId], [Type]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_APISCO_SCOID_NAME] ON [HCL.CS.SF_ApiScopes] ([ApiResourceId], [Name]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_AUD_CBBY_ACTY] ON [HCL.CS.SF_AuditTrail] ([CreatedBy], [ActionType]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_AUD_CRON_ACTY] ON [HCL.CS.SF_AuditTrail] ([CreatedOn], [ActionType]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_AUD_CRON_CBBY] ON [HCL.CS.SF_AuditTrail] ([CreatedOn], [CreatedBy]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_HCL.CS.SF_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri] ON [HCL.CS.SF_ClientPostLogoutRedirectUris] ([ClientId], [PostLogoutRedirectUri]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_HCL.CS.SF_ClientRedirectUris_ClientId_RedirectUri] ON [HCL.CS.SF_ClientRedirectUris] ([ClientId], [RedirectUri]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_SECTOK_TOKTYPE_KEY] ON [HCL.CS.SF_SecurityTokens] ([TokenType], [Key]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_CLI_CLID_CLSEC] ON [HCL.CS.SF_Clients] ([ClientId], [ClientSecret]) WHERE [ClientId] IS NOT NULL AND [ClientSecret] IS NOT NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_IDRESCLM_IDRESID_TYPE] ON [HCL.CS.SF_IdentityClaims] ([IdentityResourceId], [Type]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_IDRES_NAME] ON [HCL.CS.SF_IdentityResources] ([Name]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_NOTI_TYPE] ON [HCL.CS.SF_Notification] ([Type]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_HCL.CS.SF_Notification_UserId] ON [HCL.CS.SF_Notification] ([UserId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_HCL.CS.SF_PasswordHistory_UserID] ON [HCL.CS.SF_PasswordHistory] ([UserID]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_HCL.CS.SF_RoleClaims_RoleId] ON [HCL.CS.SF_RoleClaims] ([RoleId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [RoleNameIndex] ON [HCL.CS.SF_Roles] ([NormalizedName]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_SEC_QUESTION] ON [HCL.CS.SF_SecurityQuestions] ([Question]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_HCL.CS.SF_UserClaims_UserId] ON [HCL.CS.SF_UserClaims] ([UserId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_HCL.CS.SF_UserLogins_UserId] ON [HCL.CS.SF_UserLogins] ([UserId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_HCL.CS.SF_UserRoles_RoleId] ON [HCL.CS.SF_UserRoles] ([RoleId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_HCL.CS.SF_UserRoles_UserId] ON [HCL.CS.SF_UserRoles] ([UserId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [EmailIndex] ON [HCL.CS.SF_Users] ([NormalizedEmail]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [UserNameIndex] ON [HCL.CS.SF_Users] ([NormalizedUserName]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE INDEX [IX_USRSEC_QUEID] ON [HCL.CS.SF_UserSecurityQuestions] ([SecurityQuestionId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    CREATE UNIQUE INDEX [IX_USRSEC_UID_QUEID] ON [HCL.CS.SF_UserSecurityQuestions] ([UserId], [SecurityQuestionId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220722123632_HCL.CS.SFSqlV1')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220722123632_HCL.CS.SFSqlV1', N'3.1.27');
END;

GO

