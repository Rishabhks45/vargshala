-- Table: public.FeeStructures

-- DROP TABLE IF EXISTS public."FeeStructures";

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
