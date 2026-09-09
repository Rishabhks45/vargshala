-- Table: public.Classes

-- DROP TABLE IF EXISTS public."Classes";

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
