IF OBJECT_ID('HCL.CS.SF_ExternalIdentities', 'U') IS NULL
BEGIN
    CREATE TABLE [HCL.CS.SF_ExternalIdentities] (
        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_HCL.CS.SF_ExternalIdentities] PRIMARY KEY,
        [IsDeleted] bit NOT NULL CONSTRAINT [DF_HCL.CS.SF_ExternalIdentities_IsDeleted] DEFAULT (0),
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(255) NOT NULL,
        [ModifiedBy] nvarchar(255) NULL,
        [RowVersion] rowversion,
        [UserId] uniqueidentifier NOT NULL,
        [TenantId] nvarchar(128) NULL,
        [Provider] nvarchar(64) NOT NULL,
        [Issuer] nvarchar(256) NOT NULL,
        [Subject] nvarchar(256) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [EmailVerified] bit NOT NULL,
        [LinkedAt] datetime2 NOT NULL,
        [LastSignInAt] datetime2 NULL,
        CONSTRAINT [FK_HCL.CS.SF_ExternalIdentities_HCL.CS.SF_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EXTID_PROVIDER_ISSUER_SUBJECT' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalIdentities'))
BEGIN
    CREATE UNIQUE INDEX [IX_EXTID_PROVIDER_ISSUER_SUBJECT]
        ON [HCL.CS.SF_ExternalIdentities] ([Provider], [Issuer], [Subject]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EXTID_USERID' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalIdentities'))
BEGIN
    CREATE INDEX [IX_EXTID_USERID]
        ON [HCL.CS.SF_ExternalIdentities] ([UserId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EXTID_TENANT_EMAIL' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalIdentities'))
BEGIN
    CREATE INDEX [IX_EXTID_TENANT_EMAIL]
        ON [HCL.CS.SF_ExternalIdentities] ([TenantId], [Email]);
END;
GO
