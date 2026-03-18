ALTER TABLE `HCL.CS.SF_SecurityTokens`
    ADD COLUMN IF NOT EXISTS `ConsumedAt` datetime(6) NULL;

ALTER TABLE `HCL.CS.SF_SecurityTokens`
    ADD COLUMN IF NOT EXISTS `TokenReuseDetected` tinyint(1) NOT NULL DEFAULT FALSE;

CREATE INDEX `IX_SECTOK_TOKTYPE_KEY`
    ON `HCL.CS.SF_SecurityTokens` (`TokenType`(64), `Key`(255));
