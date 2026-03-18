CREATE TABLE IF NOT EXISTS "HCL.CS.SF_ExternalIdentities" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedOn" timestamp without time zone NOT NULL,
    "ModifiedOn" timestamp without time zone NULL,
    "CreatedBy" varchar(255) NOT NULL,
    "ModifiedBy" varchar(255) NULL,
    "RowVersion" bytea NULL,
    "UserId" uuid NOT NULL,
    "TenantId" varchar(128) NULL,
    "Provider" varchar(64) NOT NULL,
    "Issuer" varchar(256) NOT NULL,
    "Subject" varchar(256) NOT NULL,
    "Email" varchar(255) NOT NULL,
    "EmailVerified" boolean NOT NULL,
    "LinkedAt" timestamp without time zone NOT NULL,
    "LastSignInAt" timestamp without time zone NULL,
    CONSTRAINT "FK_HCL.CS.SF_ExternalIdentities_HCL.CS.SF_Users_UserId"
        FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_EXTID_PROVIDER_ISSUER_SUBJECT"
    ON "HCL.CS.SF_ExternalIdentities" ("Provider", "Issuer", "Subject");

CREATE INDEX IF NOT EXISTS "IX_EXTID_USERID"
    ON "HCL.CS.SF_ExternalIdentities" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_EXTID_TENANT_EMAIL"
    ON "HCL.CS.SF_ExternalIdentities" ("TenantId", "Email");
