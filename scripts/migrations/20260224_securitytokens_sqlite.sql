-- Apply once per SQLite database.
ALTER TABLE "HCL.CS.SF_SecurityTokens" ADD COLUMN "ConsumedAt" TEXT NULL;
ALTER TABLE "HCL.CS.SF_SecurityTokens" ADD COLUMN "TokenReuseDetected" INTEGER NOT NULL DEFAULT 0;
CREATE INDEX IF NOT EXISTS "IX_SECTOK_TOKTYPE_KEY" ON "HCL.CS.SF_SecurityTokens" ("TokenType", "Key");
