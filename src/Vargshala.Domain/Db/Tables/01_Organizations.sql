-- ============================================================================
-- Table: Organizations
-- Description: Core multi-tenant organization entity for coaching/institutes
-- ============================================================================

CREATE TABLE IF NOT EXISTS "Organizations" (
    "Id"                 UUID                     NOT NULL,
    "Name"               VARCHAR(200)             NOT NULL,
    "Code"               VARCHAR(50)              NOT NULL,
    "LogoUrl"            VARCHAR(500)             NULL,
    "Email"              VARCHAR(150)             NULL,
    "Mobile"             VARCHAR(20)              NULL,
    "Address"            VARCHAR(500)             NULL,
    "City"               VARCHAR(100)             NULL,
    "State"              VARCHAR(100)             NULL,
    "Pincode"            VARCHAR(10)              NULL,
    "AcademicSession"    VARCHAR(20)              NULL,

    -- Status & Audit Fields (BaseEntity)
    "IsActive"           BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"          UUID                     NULL,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"          UUID                     NULL,
    "UpdatedAt"          TIMESTAMP WITH TIME ZONE NULL,

    -- Soft Delete (BaseEntity)
    "IsDeleted"          BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"          UUID                     NULL,
    "DeletedAt"          TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_Organizations" PRIMARY KEY ("Id")
);

-- Unique Organization Code (ignoring soft-deleted records)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Organizations_Code" 
    ON "Organizations" ("Code") 
    WHERE "IsDeleted" = false;
