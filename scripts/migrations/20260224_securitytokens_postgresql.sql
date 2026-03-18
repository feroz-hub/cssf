ALTER TABLE "HCL.CS.SF_SecurityTokens"
    ADD COLUMN IF NOT EXISTS "ConsumedAt" timestamp with time zone NULL;

ALTER TABLE "HCL.CS.SF_SecurityTokens"
    ADD COLUMN IF NOT EXISTS "TokenReuseDetected" boolean NOT NULL DEFAULT FALSE;

CREATE INDEX IF NOT EXISTS "IX_SECTOK_TOKTYPE_KEY"
    ON "HCL.CS.SF_SecurityTokens" ("TokenType", "Key");
