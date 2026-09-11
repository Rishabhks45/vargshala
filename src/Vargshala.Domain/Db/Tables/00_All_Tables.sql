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


-- ============================================================================
-- 09. Subjects Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Subjects"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,

    "Name" VARCHAR(150) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" TEXT,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Subjects"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Subjects_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_Subjects_OrganizationId_Code"
        UNIQUE ("OrganizationId", "Code")
);

CREATE INDEX IF NOT EXISTS "IX_Subjects_OrganizationId"
    ON public."Subjects" ("OrganizationId");

-- ============================================================================
-- 10. Classes Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Classes"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BranchId" UUID NOT NULL,

    "Name" VARCHAR(150) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" TEXT,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Classes"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Classes_Branches_BranchId"
        FOREIGN KEY ("BranchId")
        REFERENCES public."Branches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_Classes_BranchId_Code"
        UNIQUE ("BranchId", "Code")
);

CREATE INDEX IF NOT EXISTS "IX_Classes_BranchId"
    ON public."Classes" ("BranchId");


-- ============================================================================
-- 11. Batches Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Batches"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "ClassId" UUID NOT NULL,
    "SubjectId" UUID NOT NULL,

    "Name" VARCHAR(150) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "StartTime" TIME,
    "EndTime" TIME,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Batches"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Batches_Classes_ClassId"
        FOREIGN KEY ("ClassId")
        REFERENCES public."Classes" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Batches_Subjects_SubjectId"
        FOREIGN KEY ("SubjectId")
        REFERENCES public."Subjects" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_Batches_ClassId_Code"
        UNIQUE ("ClassId", "Code")
);

CREATE INDEX IF NOT EXISTS "IX_Batches_ClassId"
    ON public."Batches" ("ClassId");

CREATE INDEX IF NOT EXISTS "IX_Batches_SubjectId"
    ON public."Batches" ("SubjectId");


-- ============================================================================
-- 12. BatchTeachers Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."BatchTeachers"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "TeacherId" UUID NOT NULL,

    "AssignedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "RemovedAt" TIMESTAMPTZ,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_BatchTeachers"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_BatchTeachers_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_BatchTeachers_Teachers_TeacherId"
        FOREIGN KEY ("TeacherId")
        REFERENCES public."Teachers" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_BatchTeachers_BatchId_TeacherId"
        UNIQUE ("BatchId", "TeacherId")
);

CREATE INDEX IF NOT EXISTS "IX_BatchTeachers_BatchId"
    ON public."BatchTeachers" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_BatchTeachers_TeacherId"
    ON public."BatchTeachers" ("TeacherId");


-- ============================================================================
-- 13. BatchStudents Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."BatchStudents"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,

    "IsPrimary" BOOLEAN NOT NULL DEFAULT TRUE,
    "EnrollmentType" VARCHAR(50) NOT NULL DEFAULT 'Regular',

    "JoinedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "LeftAt" TIMESTAMPTZ,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_BatchStudents"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_BatchStudents_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_BatchStudents_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_BatchStudents_BatchId_StudentId"
        UNIQUE ("BatchId", "StudentId")
);

CREATE INDEX IF NOT EXISTS "IX_BatchStudents_BatchId"
    ON public."BatchStudents" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_BatchStudents_StudentId"
    ON public."BatchStudents" ("StudentId");

COMMIT;


-- =============================================
-- 14. Batch Schedules Table
-- =============================================
-- Table: public.BatchSchedules

CREATE TABLE IF NOT EXISTS public."BatchSchedules"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "DayOfWeek" INTEGER NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "RoomOrLocation" VARCHAR(150),

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_BatchSchedules"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_BatchSchedules_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_BatchSchedules_BatchId"
    ON public."BatchSchedules" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_BatchSchedules_DayOfWeek"
    ON public."BatchSchedules" ("DayOfWeek");


-- =============================================
-- 15. Class Sessions Table
-- =============================================
-- Table: public.ClassSessions

CREATE TABLE IF NOT EXISTS public."ClassSessions"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "BatchScheduleId" UUID,
    "TeacherId" UUID,

    "SessionDate" DATE NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,

    "Topic" VARCHAR(250),
    "Notes" TEXT,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Scheduled',

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_ClassSessions"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_ClassSessions_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_ClassSessions_BatchSchedules_BatchScheduleId"
        FOREIGN KEY ("BatchScheduleId")
        REFERENCES public."BatchSchedules" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "FK_ClassSessions_Teachers_TeacherId"
        FOREIGN KEY ("TeacherId")
        REFERENCES public."Teachers" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_BatchId"
    ON public."ClassSessions" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_SessionDate"
    ON public."ClassSessions" ("SessionDate");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_TeacherId"
    ON public."ClassSessions" ("TeacherId");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_BatchScheduleId"
    ON public."ClassSessions" ("BatchScheduleId");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_BatchId_SessionDate"
    ON public."ClassSessions" ("BatchId", "SessionDate");


-- =============================================
-- 16_Attendances.sql
-- =============================================
CREATE TABLE IF NOT EXISTS public."Attendances"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),

    "ClassSessionId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,

    "Status" VARCHAR(20) NOT NULL DEFAULT 'Present',

    "MarkedAt" TIMESTAMPTZ,
    "Remarks" TEXT,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Attendances"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Attendances_ClassSessions_ClassSessionId"
        FOREIGN KEY ("ClassSessionId")
        REFERENCES public."ClassSessions" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Attendances_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_Attendances_ClassSessionId_StudentId"
        UNIQUE ("ClassSessionId", "StudentId"),

    CONSTRAINT "CK_Attendances_Status"
        CHECK ("Status" IN ('Present', 'Absent', 'Late', 'Excused'))
);

CREATE INDEX IF NOT EXISTS "IX_Attendances_ClassSessionId"
    ON public."Attendances" ("ClassSessionId");

CREATE INDEX IF NOT EXISTS "IX_Attendances_StudentId"
    ON public."Attendances" ("StudentId");


-- =============================================
-- 17_FeeStructures.sql
-- =============================================
CREATE TABLE IF NOT EXISTS public."FeeStructures"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,
    "BranchId" UUID NOT NULL,

    "Name" VARCHAR(150) NOT NULL,
    "Description" TEXT,
    "TotalAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "AcademicSession" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_FeeStructures"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_FeeStructures_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_FeeStructures_Branches_BranchId"
        FOREIGN KEY ("BranchId")
        REFERENCES public."Branches" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "CK_FeeStructures_TotalAmount"
        CHECK ("TotalAmount" >= 0)
);

CREATE INDEX IF NOT EXISTS "IX_FeeStructures_OrganizationId"
    ON public."FeeStructures" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_FeeStructures_BranchId"
    ON public."FeeStructures" ("BranchId");

CREATE INDEX IF NOT EXISTS "IX_FeeStructures_Org_Branch"
    ON public."FeeStructures" ("OrganizationId", "BranchId");

CREATE INDEX IF NOT EXISTS "IX_FeeStructures_Session_IsActive"
    ON public."FeeStructures" ("AcademicSession", "IsActive");


-- =============================================
-- 18_StudentFees.sql
-- =============================================
CREATE TABLE IF NOT EXISTS public."StudentFees"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,
    "FeeStructureId" UUID NOT NULL,

    "OriginalAmount" NUMERIC(12, 2) NOT NULL,
    "DiscountAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "FinalAmount" NUMERIC(12, 2) NOT NULL,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Pending',
    "AssignedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_StudentFees"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_StudentFees_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_StudentFees_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_StudentFees_FeeStructures_FeeStructureId"
        FOREIGN KEY ("FeeStructureId")
        REFERENCES public."FeeStructures" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "CK_StudentFees_Amounts"
        CHECK ("FinalAmount" = ("OriginalAmount" - "DiscountAmount")),

    CONSTRAINT "CK_StudentFees_Status"
        CHECK ("Status" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS "IX_StudentFees_OrganizationId"
    ON public."StudentFees" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_StudentFees_StudentId"
    ON public."StudentFees" ("StudentId");

CREATE INDEX IF NOT EXISTS "IX_StudentFees_FeeStructureId"
    ON public."StudentFees" ("FeeStructureId");

CREATE INDEX IF NOT EXISTS "IX_StudentFees_Org_Status"
    ON public."StudentFees" ("OrganizationId", "Status");


-- =============================================
-- 19_FeeDiscounts.sql
-- =============================================
CREATE TABLE IF NOT EXISTS public."FeeDiscounts"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "StudentFeeId" UUID NOT NULL,

    "DiscountType" VARCHAR(50) NOT NULL,
    "Value" NUMERIC(12, 2) NOT NULL,
    "Amount" NUMERIC(12, 2) NOT NULL,
    "Reason" VARCHAR(255),
    "ApprovedBy" UUID,
    "ApprovedAt" TIMESTAMPTZ DEFAULT NOW(),

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_FeeDiscounts"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_FeeDiscounts_StudentFees_StudentFeeId"
        FOREIGN KEY ("StudentFeeId")
        REFERENCES public."StudentFees" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_FeeDiscounts_Users_ApprovedBy"
        FOREIGN KEY ("ApprovedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "CK_FeeDiscounts_Amount"
        CHECK ("Amount" >= 0),

    CONSTRAINT "CK_FeeDiscounts_Value"
        CHECK ("Value" >= 0)
);

CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_StudentFeeId"
    ON public."FeeDiscounts" ("StudentFeeId");

CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_ApprovedBy"
    ON public."FeeDiscounts" ("ApprovedBy");


-- =============================================
-- 20_FeeInstallments.sql
-- =============================================
CREATE TABLE IF NOT EXISTS public."FeeInstallments"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "StudentFeeId" UUID NOT NULL,

    "InstallmentNumber" INT NOT NULL,
    "Amount" NUMERIC(12, 2) NOT NULL,
    "DueDate" DATE NOT NULL,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Pending',

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_FeeInstallments"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_FeeInstallments_StudentFees_StudentFeeId"
        FOREIGN KEY ("StudentFeeId")
        REFERENCES public."StudentFees" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "UQ_FeeInstallments_StudentFeeId_InstallmentNumber"
        UNIQUE ("StudentFeeId", "InstallmentNumber"),

    CONSTRAINT "CK_FeeInstallments_Amount"
        CHECK ("Amount" > 0),

    CONSTRAINT "CK_FeeInstallments_Status"
        CHECK ("Status" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_StudentFeeId"
    ON public."FeeInstallments" ("StudentFeeId");

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_DueDate"
    ON public."FeeInstallments" ("DueDate");

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_Status"
    ON public."FeeInstallments" ("Status");

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_StudentFee_DueDate"
    ON public."FeeInstallments" ("StudentFeeId", "DueDate");


-- =============================================
-- 21_Payments.sql
-- =============================================
CREATE TABLE IF NOT EXISTS public."Payments"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,

    "Amount" NUMERIC(12, 2) NOT NULL,
    "PaymentDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "PaymentMethod" VARCHAR(50) NOT NULL,
    "TransactionReference" VARCHAR(100),
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Completed',
    "Remarks" TEXT,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Payments"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Payments_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_Payments_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_Payments_Users_CreatedBy"
        FOREIGN KEY ("CreatedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "CK_Payments_Amount"
        CHECK ("Amount" > 0),

    CONSTRAINT "CK_Payments_Status"
        CHECK ("Status" IN ('Completed', 'Pending', 'Failed', 'Refunded'))
);

CREATE INDEX IF NOT EXISTS "IX_Payments_OrganizationId"
    ON public."Payments" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_Payments_StudentId"
    ON public."Payments" ("StudentId");

CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentDate"
    ON public."Payments" ("PaymentDate");

CREATE INDEX IF NOT EXISTS "IX_Payments_TransactionReference"
    ON public."Payments" ("TransactionReference");

CREATE INDEX IF NOT EXISTS "IX_Payments_Org_Date"
    ON public."Payments" ("OrganizationId", "PaymentDate");


-- =============================================
-- 22_PaymentAllocations.sql
-- =============================================
CREATE TABLE IF NOT EXISTS public."PaymentAllocations"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "PaymentId" UUID NOT NULL,
    "FeeInstallmentId" UUID NOT NULL,

    "AllocatedAmount" NUMERIC(12, 2) NOT NULL,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_PaymentAllocations"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_PaymentAllocations_Payments_PaymentId"
        FOREIGN KEY ("PaymentId")
        REFERENCES public."Payments" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_PaymentAllocations_FeeInstallments_FeeInstallmentId"
        FOREIGN KEY ("FeeInstallmentId")
        REFERENCES public."FeeInstallments" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_PaymentAllocations_PaymentId_FeeInstallmentId"
        UNIQUE ("PaymentId", "FeeInstallmentId"),

    CONSTRAINT "CK_PaymentAllocations_Amount"
        CHECK ("AllocatedAmount" > 0)
);

CREATE INDEX IF NOT EXISTS "IX_PaymentAllocations_PaymentId"
    ON public."PaymentAllocations" ("PaymentId");

CREATE INDEX IF NOT EXISTS "IX_PaymentAllocations_FeeInstallmentId"
    ON public."PaymentAllocations" ("FeeInstallmentId");

