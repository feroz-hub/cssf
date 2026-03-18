DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'HCL.CS.SF_SecurityTokens'
          AND column_name = 'CreationTime'
          AND data_type = 'timestamp without time zone'
    ) THEN
        ALTER TABLE "HCL.CS.SF_SecurityTokens"
            ALTER COLUMN "CreatedOn" TYPE timestamp with time zone USING "CreatedOn" AT TIME ZONE 'UTC',
            ALTER COLUMN "ModifiedOn" TYPE timestamp with time zone USING "ModifiedOn" AT TIME ZONE 'UTC',
            ALTER COLUMN "CreationTime" TYPE timestamp with time zone USING "CreationTime" AT TIME ZONE 'UTC',
            ALTER COLUMN "ConsumedTime" TYPE timestamp with time zone USING "ConsumedTime" AT TIME ZONE 'UTC',
            ALTER COLUMN "ConsumedAt" TYPE timestamp with time zone USING "ConsumedAt" AT TIME ZONE 'UTC';
    END IF;
END $$;
