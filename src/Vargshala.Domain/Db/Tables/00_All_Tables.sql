-- ============================================================================
-- Vargshala - Full Database Schema Script (Phase 1 Baseline)
-- Database: PostgreSQL
-- ============================================================================

START TRANSACTION;

-- 1. Organizations
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
    "IsActive"           BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"          UUID                     NULL,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"          UUID                     NULL,
    "UpdatedAt"          TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"          BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"          UUID                     NULL,
    "DeletedAt"          TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "PK_Organizations" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Organizations_Code" 
    ON "Organizations" ("Code") 
    WHERE "IsDeleted" = false;

-- 2. Users
CREATE TABLE IF NOT EXISTS "Users" (
    "Id"                     UUID                     NOT NULL,
    "OrganizationId"         UUID                     NULL,
    "FirstName"              VARCHAR(100)             NOT NULL,
    "LastName"               VARCHAR(100)             NOT NULL,
    "Email"                  VARCHAR(150)             NULL,
    "Mobile"                 VARCHAR(20)              NULL,
    "PasswordHash"           VARCHAR(500)             NOT NULL,
    "Role"                   INTEGER                  NOT NULL,
    "EmailVerified"          BOOLEAN                  NOT NULL DEFAULT FALSE,
    "MobileVerified"         BOOLEAN                  NOT NULL DEFAULT FALSE,
    "LastLoginAt"            TIMESTAMP WITH TIME ZONE NULL,
    "RefreshToken"           VARCHAR(500)             NULL,
    "RefreshTokenExpiryTime" TIMESTAMP WITH TIME ZONE NULL,
    "IsActive"               BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"              UUID                     NULL,
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"              UUID                     NULL,
    "UpdatedAt"              TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Users_Organizations_OrganizationId" 
        FOREIGN KEY ("OrganizationId") 
        REFERENCES "Organizations" ("Id") 
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email_OrganizationId" 
    ON "Users" ("Email", "OrganizationId") 
    WHERE "Email" IS NOT NULL AND "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Users_OrganizationId" 
    ON "Users" ("OrganizationId");

COMMIT;
