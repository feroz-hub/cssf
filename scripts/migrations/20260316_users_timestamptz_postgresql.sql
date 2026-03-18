DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'HCL.CS.SF_Users'
          AND column_name = 'CreatedOn'
          AND data_type = 'timestamp without time zone'
    ) THEN
        ALTER TABLE "HCL.CS.SF_Users"
            ALTER COLUMN "DateOfBirth" TYPE timestamp with time zone USING "DateOfBirth" AT TIME ZONE 'UTC',
            ALTER COLUMN "LastPasswordChangedDate" TYPE timestamp with time zone USING "LastPasswordChangedDate" AT TIME ZONE 'UTC',
            ALTER COLUMN "LastLoginDateTime" TYPE timestamp with time zone USING "LastLoginDateTime" AT TIME ZONE 'UTC',
            ALTER COLUMN "LastLogoutDateTime" TYPE timestamp with time zone USING "LastLogoutDateTime" AT TIME ZONE 'UTC',
            ALTER COLUMN "CreatedOn" TYPE timestamp with time zone USING "CreatedOn" AT TIME ZONE 'UTC',
            ALTER COLUMN "ModifiedOn" TYPE timestamp with time zone USING "ModifiedOn" AT TIME ZONE 'UTC';
    END IF;
END $$;
