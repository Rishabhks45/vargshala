-- ============================================================================
-- Vargshala - Full Unified Database Schema Script (PostgreSQL)
-- All 8 Core Entities & Multi-Tenant Architecture
-- 1. Organizations
-- 2. Users
-- 3. Branches
-- 4. UserBranchAccess
-- 5. Students
-- 6. Teachers
-- 7. EmailTemplates
-- 8. Coupons
-- ============================================================================

START TRANSACTION;

-- ============================================================================
-- 1. ORGANIZATIONS (Root Multi-Tenant Entity)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Organizations" (
    "Id"                 UUID                     NOT NULL,
    "Name"               VARCHAR(200)             NOT NULL,
    "Code"               VARCHAR(50)              NOT NULL,
    "LogoUrl"            TEXT                     NULL,
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
    ON public."Organizations" ("Code") 
    WHERE "IsDeleted" = false;

-- ============================================================================
-- 2. USERS (Identity & Authentication)
-- Role Enum:
--   1001 = SuperAdmin (Platform Level)
--   1002 = BackOffice Staff
--   1    = OrganizationAdmin (Institute Admin)
--   2    = Teacher
--   3    = Student
--   4    = BranchAdmin (Head of Branch)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Users" (
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
    "ProfilePictureUrl"      TEXT                     NULL,
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
        REFERENCES public."Organizations" ("Id") 
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email_OrganizationId" 
    ON public."Users" ("Email", "OrganizationId") 
    WHERE "Email" IS NOT NULL AND "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Users_OrganizationId" 
    ON public."Users" ("OrganizationId");

-- ============================================================================
-- 3. BRANCHES (Institute Campuses & Locations)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Branches" (
    "Id"                 UUID                     NOT NULL,
    "OrganizationId"     UUID                     NOT NULL,
    "Name"               VARCHAR(200)             NOT NULL,
    "Code"               VARCHAR(50)              NOT NULL,
    "LogoUrl"            TEXT                     NULL,
    "Email"              VARCHAR(150)             NULL,
    "Mobile"             VARCHAR(20)              NULL,
    "AlternateMobile"    VARCHAR(20)              NULL,
    "Address"            VARCHAR(500)             NULL,
    "City"               VARCHAR(100)             NULL,
    "State"              VARCHAR(100)             NULL,
    "Pincode"            VARCHAR(10)              NULL,
    "Country"            VARCHAR(100)             NULL,
    "IsMainBranch"       BOOLEAN                  NOT NULL DEFAULT FALSE,
    "UseBranchName"      BOOLEAN                  NOT NULL DEFAULT TRUE,
    "IsActive"           BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"          UUID                     NULL,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"          UUID                     NULL,
    "UpdatedAt"          TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"          BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"          UUID                     NULL,
    "DeletedAt"          TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "PK_Branches" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Branches_OrganizationId_Code" UNIQUE ("OrganizationId", "Code"),
    CONSTRAINT "FK_Branches_Organizations_OrganizationId" 
        FOREIGN KEY ("OrganizationId") 
        REFERENCES public."Organizations" ("Id") 
        ON UPDATE CASCADE 
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_Branches_OrganizationId" 
    ON public."Branches" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_Branches_City" 
    ON public."Branches" ("City");

-- ============================================================================
-- 4. USER BRANCH ACCESS (Many-to-Many RBAC & Multi-Branch Mapping)
-- Maps Users (OrgAdmin, BranchAdmin, Teachers, Students) to Authorized Branches
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."UserBranchAccess" (
    "Id"                 UUID                     NOT NULL,
    "UserId"             UUID                     NOT NULL,
    "BranchId"           UUID                     NOT NULL,
    "IsActive"           BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"          UUID                     NULL,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"          UUID                     NULL,
    "UpdatedAt"          TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "PK_UserBranchAccess" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_UserBranchAccess_UserId_BranchId" UNIQUE ("UserId", "BranchId"),
    CONSTRAINT "FK_UserBranchAccess_Users_UserId" 
        FOREIGN KEY ("UserId") 
        REFERENCES public."Users" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_UserBranchAccess_Branches_BranchId" 
        FOREIGN KEY ("BranchId") 
        REFERENCES public."Branches" ("Id") 
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserBranchAccess_UserId" 
    ON public."UserBranchAccess" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_UserBranchAccess_BranchId" 
    ON public."UserBranchAccess" ("BranchId");

-- ============================================================================
-- 5. STUDENTS (Profile, Academic & Guardian Information)
-- One-to-One linked with Users table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Students" (
    "Id"                       UUID                     NOT NULL,
    "UserId"                   UUID                     NOT NULL,
    "Gender"                   VARCHAR(20)              NULL,
    "DateOfBirth"              DATE                     NULL,
    "BloodGroup"               VARCHAR(10)              NULL,
    "Nationality"              VARCHAR(50)              NULL,
    "StudentCode"              VARCHAR(50)              NULL,
    "EnrollmentDate"           DATE                     NULL,
    "ClassName"                VARCHAR(100)             NULL,
    "Section"                  VARCHAR(50)              NULL,
    "RollNumber"               VARCHAR(50)              NULL,
    "FatherName"               VARCHAR(150)             NULL,
    "FatherMobile"             VARCHAR(20)              NULL,
    "FatherAlternateMobile"    VARCHAR(20)              NULL,
    "MotherName"               VARCHAR(150)             NULL,
    "Address"                  TEXT                     NULL,
    "City"                     VARCHAR(100)             NULL,
    "State"                    VARCHAR(100)             NULL,
    "PostalCode"               VARCHAR(20)              NULL,
    "Country"                  VARCHAR(100)             NULL,
    "EmergencyContactName"     VARCHAR(150)             NULL,
    "EmergencyContactMobile"   VARCHAR(20)              NULL,
    "EmergencyContactRelation" VARCHAR(50)              NULL,
    "AadharNumber"             VARCHAR(20)              NULL,
    "PreviousInstitute"        VARCHAR(200)             NULL,
    "MedicalNotes"             TEXT                     NULL,
    "IsActive"                 BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"                UUID                     NULL,
    "CreatedAt"                TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"                UUID                     NULL,
    "UpdatedAt"                TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"                BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"                UUID                     NULL,
    "DeletedAt"                TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "Students_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Students_UserId" UNIQUE ("UserId"),
    CONSTRAINT "FK_Students_Users" 
        FOREIGN KEY ("UserId") 
        REFERENCES public."Users" ("Id") 
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Students_StudentCode" 
    ON public."Students" ("StudentCode") 
    WHERE "StudentCode" IS NOT NULL AND "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Students_ClassName_Section" 
    ON public."Students" ("ClassName", "Section");

-- ============================================================================
-- 6. TEACHERS (Faculty & Staff Profile)
-- One-to-One linked with Users table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Teachers" (
    "Id"                       UUID                     NOT NULL,
    "UserId"                   UUID                     NOT NULL,
    "EmployeeCode"             VARCHAR(50)              NULL,
    "JoiningDate"              DATE                     NULL,
    "Department"               VARCHAR(100)             NULL,
    "Designation"              VARCHAR(100)             NULL,
    "HighestQualification"     VARCHAR(150)             NULL,
    "Specialization"           VARCHAR(150)             NULL,
    "TeachingExperienceYears"  NUMERIC(5, 2)            NULL,
    "Address"                  TEXT                     NULL,
    "City"                     VARCHAR(100)             NULL,
    "State"                    VARCHAR(100)             NULL,
    "PostalCode"               VARCHAR(20)              NULL,
    "Country"                  VARCHAR(100)             NULL,
    "AadharNumber"             VARCHAR(20)              NULL,
    "PreviousInstitute"        VARCHAR(200)             NULL,
    "Bio"                      TEXT                     NULL,
    "IsActive"                 BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"                UUID                     NULL,
    "CreatedAt"                TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"                UUID                     NULL,
    "UpdatedAt"                TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"                BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"                UUID                     NULL,
    "DeletedAt"                TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "Teachers_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Teachers_UserId" UNIQUE ("UserId"),
    CONSTRAINT "FK_Teachers_Users" 
        FOREIGN KEY ("UserId") 
        REFERENCES public."Users" ("Id") 
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Teachers_EmployeeCode" 
    ON public."Teachers" ("EmployeeCode") 
    WHERE "EmployeeCode" IS NOT NULL AND "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Teachers_Department" 
    ON public."Teachers" ("Department");

CREATE INDEX IF NOT EXISTS "IX_Teachers_Designation" 
    ON public."Teachers" ("Designation");

-- ============================================================================
-- 7. EMAIL TEMPLATES (Transactional & Automated Email System)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."EmailTemplates" (
    "Id"                    UUID                     NOT NULL,
    "OrganizationId"        UUID                     NULL,     -- NULL for Platform/Global Templates
    "Category"              INTEGER                  NOT NULL DEFAULT 1, -- 1=Onboarding, 2=Auth, 3=Billing, 4=System
    "TargetRole"            INTEGER                  NULL,     -- NULL=All, 1001=SuperAdmin, 1=OrgAdmin, 2=Teacher, 3=Student, 4=BranchAdmin
    "Code"                  VARCHAR(50)              NOT NULL,
    "Name"                  VARCHAR(150)             NOT NULL,
    "Subject"               VARCHAR(250)             NOT NULL,
    "AvailablePlaceholders" VARCHAR(1000)            NULL,
    "BodyHtml"              TEXT                     NOT NULL,
    "Description"           VARCHAR(500)             NULL,
    "IsActive"              BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"             UUID                     NULL,
    "CreatedAt"             TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"             UUID                     NULL,
    "UpdatedAt"             TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"             BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"             UUID                     NULL,
    "DeletedAt"             TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "PK_EmailTemplates" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmailTemplates_Organizations_OrganizationId" 
        FOREIGN KEY ("OrganizationId") 
        REFERENCES public."Organizations" ("Id") 
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_EmailTemplates_Org_Code" 
    ON public."EmailTemplates" (COALESCE("OrganizationId", '00000000-0000-0000-0000-000000000000'), "Code") 
    WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_EmailTemplates_Category" 
    ON public."EmailTemplates" ("Category");

-- ============================================================================
-- 8. COUPONS & DISCOUNTS (SaaS Subscription Promo Codes)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Coupons" (
    "Id"                 UUID                     NOT NULL,
    "OrganizationId"     UUID                     NULL,
    "Code"               VARCHAR(50)              NOT NULL,
    "Description"        VARCHAR(500)             NULL,
    "Category"           INTEGER                  NOT NULL DEFAULT 1,
    "DiscountType"       INTEGER                  NOT NULL DEFAULT 1, -- 1=Percentage, 2=FixedAmount
    "DiscountValue"      NUMERIC(18, 2)           NOT NULL,
    "MinOrderAmount"     NUMERIC(18, 2)           NULL,
    "MaxDiscountAmount"  NUMERIC(18, 2)           NULL,
    "ApplicablePlan"     INTEGER                  NOT NULL DEFAULT 0,
    "UsedCount"          INTEGER                  NOT NULL DEFAULT 0,
    "MaxUses"            INTEGER                  NOT NULL DEFAULT 100,
    "MaxUsesPerUser"     INTEGER                  NULL,
    "StartDate"          TIMESTAMP WITH TIME ZONE NULL,
    "ExpiryDate"         TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsActive"           BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"          UUID                     NULL,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"          UUID                     NULL,
    "UpdatedAt"          TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"          BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"          UUID                     NULL,
    "DeletedAt"          TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Coupons_Organizations" 
        FOREIGN KEY ("OrganizationId") 
        REFERENCES public."Organizations" ("Id") 
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Coupons_Code" 
    ON public."Coupons" ("Code") 
    WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Coupons_Organization_Active" 
    ON public."Coupons" ("OrganizationId", "IsActive") 
    WHERE "IsDeleted" = false;

COMMIT;
