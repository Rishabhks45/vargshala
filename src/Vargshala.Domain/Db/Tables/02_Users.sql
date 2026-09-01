-- ============================================================================
-- Table: Users
-- Description: Platform and organization users (SuperAdmin, OrgAdmin, Teacher, Student)
-- ============================================================================

CREATE TABLE IF NOT EXISTS "Users" (
    "Id"                     UUID                     NOT NULL,
    "OrganizationId"         UUID                     NULL,
    "FirstName"              VARCHAR(100)             NOT NULL,
    "LastName"               VARCHAR(100)             NOT NULL,
    "Email"                  VARCHAR(150)             NULL,
    "Mobile"                 VARCHAR(20)              NULL,
    "PasswordHash"           VARCHAR(500)             NOT NULL,
    "Role"                   INTEGER                  NOT NULL, -- 1: SuperAdmin, 2: OrganizationAdmin, 3: Teacher, 4: Student
    "EmailVerified"          BOOLEAN                  NOT NULL DEFAULT FALSE,
    "MobileVerified"         BOOLEAN                  NOT NULL DEFAULT FALSE,
    "LastLoginAt"            TIMESTAMP WITH TIME ZONE NULL,
    
    -- Authentication Tokens
    "RefreshToken"           VARCHAR(500)             NULL,
    "RefreshTokenExpiryTime" TIMESTAMP WITH TIME ZONE NULL,

    -- Status & Audit Fields (BaseEntity)
    "IsActive"               BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"              UUID                     NULL,
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"              UUID                     NULL,
    "UpdatedAt"              TIMESTAMP WITH TIME ZONE NULL,

    -- Soft Delete (BaseEntity)
    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Users_Organizations_OrganizationId" 
        FOREIGN KEY ("OrganizationId") 
        REFERENCES "Organizations" ("Id") 
        ON DELETE RESTRICT
);

-- Indexes
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email_OrganizationId" 
    ON "Users" ("Email", "OrganizationId") 
    WHERE "Email" IS NOT NULL AND "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Users_OrganizationId" 
    ON "Users" ("OrganizationId");
