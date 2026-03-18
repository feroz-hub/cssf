IF COL_LENGTH('HCL.CS.SF_SecurityTokens', 'ConsumedAt') IS NULL
BEGIN
    ALTER TABLE [HCL.CS.SF_SecurityTokens] ADD [ConsumedAt] datetime2 NULL;
END;
GO

IF COL_LENGTH('HCL.CS.SF_SecurityTokens', 'TokenReuseDetected') IS NULL
BEGIN
    ALTER TABLE [HCL.CS.SF_SecurityTokens] ADD [TokenReuseDetected] bit NOT NULL CONSTRAINT [DF_HCL.CS.SF_SecurityTokens_TokenReuseDetected] DEFAULT (0);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SECTOK_TOKTYPE_KEY' AND object_id = OBJECT_ID('HCL.CS.SF_SecurityTokens'))
BEGIN
    CREATE INDEX [IX_SECTOK_TOKTYPE_KEY] ON [HCL.CS.SF_SecurityTokens] ([TokenType], [Key]);
END;
GO
